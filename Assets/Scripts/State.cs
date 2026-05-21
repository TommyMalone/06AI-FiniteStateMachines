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

    private float _visDist = 10.0f;
    private float _visAngle = 30.0f;
    private float _shootDist = 7.0f;
    

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
        Stage = EVENT.ENTER;
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
}

public class Idle : State
{
    private int _idleChance = 10;
    
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
        if (Random.Range(0, 100) < _idleChance)
        {
            NextState = new Patrol(Npc, Agent, Anim, Player);
            Stage = EVENT.EXIT;
        }
        base.Update();
    }

    public override void Exit()
    {
        Anim.ResetTrigger("isIdle");
        base.Exit();
    }
}

public class Patrol : State
{
    private int currentWaypointIndex = -1;
    public Patrol(GameObject npc, NavMeshAgent agent, Animator anim, Transform player) : base(npc, agent, anim, player)
    {
        Name = STATE.PATROL;
        agent.speed = 2;
        agent.isStopped = false;
    }

    public override void Enter()
    {
        currentWaypointIndex = 0;
        Anim.SetTrigger("isWalking");
        base.Enter();
    }
    
    public override void Update()
    {
        if (Agent.remainingDistance <= Agent.stoppingDistance)
        {
            if (currentWaypointIndex >= GameEnvironment.Singleton.Checkpoints.Count - 1)
            {
                currentWaypointIndex = 0;
            }
            else
            {
                currentWaypointIndex++;
            }

            Agent.SetDestination(GameEnvironment.Singleton.Checkpoints[currentWaypointIndex].transform.position);
        }
        
        base.Update();
    }

    public override void Exit()
    {
        Anim.ResetTrigger("isWalking");
        base.Exit();
    }
}