using UnityEngine;
using UnityEngine.AI;

public class AI : MonoBehaviour
{
    private NavMeshAgent _agent;
    private Animator _animator;
    public Transform playerTransform;
    private State _currentState;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();
        _currentState = new Idle(gameObject, _agent, _animator, playerTransform);
    }

    // Update is called once per frame
    private void Update()
    {
        _currentState = _currentState.Process();
    }
}
