using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.AI;

public class State
{
    public enum STATE
    {
        IDLE,
        PATROL,
        PURSUE,
        ATTACK,
        RUNAWAY,
        SLEEP
    };

    public enum EVENT
    {
        ENTER,
        UPDATE,
        EXIT
    }

    public STATE Name;
    protected EVENT Stage;
    protected GameObject Npc;
    protected NavMeshAgent Agent;
    protected Animator Anim;
    protected Transform Player;
    protected State NextState;

    private readonly float _visDist = 10.0f;
    private readonly float _visAngle = 30.0f;
    private readonly float _shootDist = 7.0f;
    private readonly float _tagDist = 2.0f;
    private readonly float _tagAngle = -30.0f;
    

    public State(GameObject npc, NavMeshAgent agent, Animator anim, Transform player)
    {
        Npc = npc;
        Agent = agent;
        Anim = anim;
        Stage = EVENT.ENTER;
        Player = player;
    }

    public virtual void Enter()
    {
        Stage = EVENT.UPDATE;
    }

    public virtual void Update()
    {
        Stage = EVENT.UPDATE;
    }

    public virtual void Exit()
    {
        Stage = EVENT.EXIT;
    }

    public State Process()
    {
        switch (Stage)
        {
            case EVENT.ENTER:
            {
                Enter();
                break;
            }
            case EVENT.UPDATE:
            {
                Update();
                break;
            }
            case EVENT.EXIT:
            {
                Exit();
                return NextState;
            }
        }
        return this;
    }

    public bool CanSeePlayer()
    { 
        Vector3 toPlayer = Player.position - Npc.transform.position;
        if (toPlayer.sqrMagnitude <= _visDist*_visDist)
        {
            float dot = Vector3.Dot(Npc.transform.forward, toPlayer.normalized);
            float minVisionDot = Mathf.Cos(_visAngle * Mathf.Deg2Rad);
            
            return dot > minVisionDot;
        }
        return false;
    }

    public bool CanAttackPlayer()
    {
        Vector3 toPlayer = Player.position - Npc.transform.position;
        return toPlayer.sqrMagnitude < _shootDist * _shootDist;
    }
    
    public bool IsPlayerTagging()
    { 
        Vector3 toPlayer = Player.position - Npc.transform.position;
        if (toPlayer.sqrMagnitude <= _tagDist*_tagDist)
        {
            float dot = Vector3.Dot(-Npc.transform.forward, toPlayer.normalized);
            float minVisionDot = Mathf.Cos(_tagAngle * Mathf.Deg2Rad);
            
            return dot > minVisionDot;
        }
        return false;
    }
}

public class Idle : State
{
    private readonly int _idleChance = 10;
    
    public Idle(GameObject npc, NavMeshAgent agent, Animator anim, Transform player) : base(npc, agent, anim, player)
    {
        Name = STATE.IDLE;
    }

    public override void Enter()
    {
        Anim.SetTrigger("isIdle");
        base.Enter();
    }
    
    public override void Update()
    {
        if (CanSeePlayer())
        {
            NextState = new Pursue(Npc, Agent, Anim, Player);
            Stage = EVENT.EXIT;
        }
        if (Random.Range(0, 100) < _idleChance)
        {
            NextState = new Patrol(Npc, Agent, Anim, Player);
            Stage = EVENT.EXIT;
        }
    }

    public override void Exit()
    {
        Anim.ResetTrigger("isIdle");
        base.Exit();
    }
}

public class Patrol : State
{
    private int _currentWaypointIndex = -1;
    private readonly int _patrolStateSpeed = 2;
    public Patrol(GameObject npc, NavMeshAgent agent, Animator anim, Transform player) : base(npc, agent, anim, player)
    {
        Name = STATE.PATROL;
    }

    public override void Enter()
    {
        float shortestDistance = Mathf.Infinity;
        for (int foundWaypointIndex = 0; foundWaypointIndex < GameEnvironment.Singleton.Checkpoints.Count; foundWaypointIndex++)
        {
            GameObject foundWaypoint = GameEnvironment.Singleton.Checkpoints[foundWaypointIndex];
            float distanceToWaypoint = Vector3.Distance(Npc.transform.position, foundWaypoint.transform.position);
            if (distanceToWaypoint < shortestDistance)
            {
                _currentWaypointIndex = foundWaypointIndex;
                shortestDistance = distanceToWaypoint;
            }
        }

        if (_currentWaypointIndex >= 0)
        {
            Agent.SetDestination(GameEnvironment.Singleton.Checkpoints[_currentWaypointIndex].transform.position);
        }
        
        Agent.isStopped = false;
        Agent.speed = _patrolStateSpeed;
        Anim.SetTrigger("isWalking");
        base.Enter();
    }
    
    public override void Update()
    {
        if (Agent.remainingDistance <= Agent.stoppingDistance)
        {
            if (_currentWaypointIndex >= GameEnvironment.Singleton.Checkpoints.Count - 1)
            {
                _currentWaypointIndex = 0;
            }
            else
            {
                _currentWaypointIndex++;
            }

            Agent.SetDestination(GameEnvironment.Singleton.Checkpoints[_currentWaypointIndex].transform.position);
        }
        if (CanSeePlayer())
        {
            NextState = new Pursue(Npc, Agent, Anim, Player);
            Stage = EVENT.EXIT;
        }
        else if (IsPlayerTagging())
        {
            NextState = new RunAway(Npc, Agent, Anim, Player);
            Stage = EVENT.EXIT;
        }
    }

    public override void Exit()
    {
        Anim.ResetTrigger("isWalking");
        base.Exit();
    }
}

public class Pursue : State
{
    private readonly int _pursueStateSpeed = 5;
    
    public Pursue(GameObject npc, NavMeshAgent agent, Animator anim, Transform player) : base(npc, agent, anim, player)
    {
        Name = STATE.PURSUE;
    }

    public override void Enter()
    {
        Agent.isStopped = false;
        Agent.speed = _pursueStateSpeed;
        Anim.SetTrigger("isRunning");
        base.Enter();
    }
    
    public override void Update()
    {
        Agent.SetDestination(Player.position);
        if (Agent.hasPath)
        {
            if (CanAttackPlayer())
            {
                NextState = new Attack(Npc, Agent, Anim, Player);
                Stage = EVENT.EXIT;
            }
            else if (!CanSeePlayer())
            {
                NextState = new Patrol(Npc, Agent, Anim, Player);
                Stage = EVENT.EXIT;
            }
        }
    }

    public override void Exit()
    {
        Anim.ResetTrigger("isRunning");
        base.Exit();
    }
}

public class Attack : State
{
    private const float RotationSpeed = 2.0f;
    private AudioSource _shoot;
    
    public Attack(GameObject npc, NavMeshAgent agent, Animator anim, Transform player) : base(npc, agent, anim, player)
    {
        Name = STATE.ATTACK;
    }

    public override void Enter()
    {
        Agent.isStopped = true;
        _shoot = Npc.GetComponent<AudioSource>();   //Get Component is costly, this could be optimized by instead passing the AudioSource through.
        _shoot.Play();
        Anim.SetTrigger("isShooting");
        base.Enter();
    }
    
    public override void Update()
    {
        Vector3 toPlayer = Player.position - Npc.transform.position;
        toPlayer.y = 0; // Constrain the vector the the xz plane to avoid tilting.
        Npc.transform.rotation = Quaternion.Slerp(Npc.transform.rotation, Quaternion.LookRotation(toPlayer),
            Time.deltaTime * RotationSpeed);
        if (!CanAttackPlayer())
        {
            NextState = new Idle(Npc, Agent, Anim, Player);
            Stage = EVENT.EXIT;
        }

    }

    public override void Exit()
    {
        _shoot.Stop();
        Anim.ResetTrigger("isShooting");
        base.Exit();
    }
}

public class RunAway : State
{
    private int _currentSafeSpotIndex = -1;
    private readonly int _runAwayStateSpeed = 5;
    
    public RunAway(GameObject npc, NavMeshAgent agent, Animator anim, Transform player) : base(npc, agent, anim, player)
    {
        Name = STATE.RUNAWAY;
    }

    public override void Enter()
    {
        float shortestDistance = Mathf.Infinity;
        for(int safeSpotIndex = 0; safeSpotIndex < GameEnvironment.Singleton.SafeSpots.Count; safeSpotIndex++)
        {
            float distanceToSafeSpot = Vector3.Distance(Npc.transform.position, GameEnvironment.Singleton.SafeSpots[safeSpotIndex].transform.position);
            if (distanceToSafeSpot < shortestDistance)
            {
                _currentSafeSpotIndex = safeSpotIndex;
                shortestDistance = distanceToSafeSpot;
            }
        }

        if (_currentSafeSpotIndex >= 0)
        {
            Agent.SetDestination(GameEnvironment.Singleton.SafeSpots[_currentSafeSpotIndex].transform.position);
        }
        
        Agent.isStopped = false;
        Agent.speed = _runAwayStateSpeed;
        Anim.SetTrigger("isRunning");
        base.Enter();
    }
    
    public override void Update()
    {
        if (Agent.remainingDistance <= Agent.stoppingDistance)
        {
            NextState = new Idle(Npc, Agent, Anim, Player);
            Stage = EVENT.EXIT;
        }
    }

    public override void Exit()
    {
        Anim.ResetTrigger("isRunning");
        base.Exit();
    }
}