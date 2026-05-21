using UnityEngine;
using UnityEngine.AI;

public class AI : MonoBehaviour
{
    private NavMeshAgent agent;
    private Animator _animator;
    public Transform playerTransform;
    private State currentState;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();
        currentState = new Idle(gameObject, agent, _animator, playerTransform);
    }

    // Update is called once per frame
    void Update()
    {
        currentState = currentState.Process();
    }
}
