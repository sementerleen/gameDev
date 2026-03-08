using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Ela'nın paranormal dünyasındaki varlıkların yapay zekası.
/// NavMesh kullanarak Ela'yı takip eder; yakaladığında GameManager'ı bilgilendirir.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class EntityAI : MonoBehaviour
{
    public enum State { Idle, Patrol, Chase, Caught }

    [Header("Detection")]
    [SerializeField] private float chaseRadius = 12f;
    [SerializeField] private float catchRadius = 1.2f;
    [SerializeField] private float fieldOfView = 110f;

    [Header("Speed")]
    [SerializeField] private float patrolSpeed = 1.8f;
    [SerializeField] private float chaseSpeed = 4.5f;

    [Header("Patrol")]
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private float patrolWaitTime = 2f;

    private NavMeshAgent _agent;
    private Transform _elaTransform;
    private State _state = State.Idle;
    private int _patrolIndex;
    private float _waitTimer;

    void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
    }

    void Start()
    {
        GameObject elaObj = GameObject.FindGameObjectWithTag("Ela");
        if (elaObj != null) _elaTransform = elaObj.transform;

        if (patrolPoints != null && patrolPoints.Length > 0)
            SetState(State.Patrol);
        else
            SetState(State.Idle);
    }

    void Update()
    {
        if (_state == State.Caught) return;

        switch (_state)
        {
            case State.Idle:
                IdleTick();
                break;
            case State.Patrol:
                PatrolTick();
                break;
            case State.Chase:
                ChaseTick();
                break;
        }
    }

    private void IdleTick()
    {
        if (CanSeeEla())
            SetState(State.Chase);
    }

    private void PatrolTick()
    {
        if (CanSeeEla())
        {
            SetState(State.Chase);
            return;
        }

        if (!_agent.pathPending && _agent.remainingDistance < 0.5f)
        {
            _waitTimer -= Time.deltaTime;
            if (_waitTimer <= 0f)
                GoToNextPatrolPoint();
        }
    }

    private void ChaseTick()
    {
        if (_elaTransform == null) return;

        float dist = Vector3.Distance(transform.position, _elaTransform.position);

        if (dist > chaseRadius * 1.5f)
        {
            SetState(patrolPoints != null && patrolPoints.Length > 0 ? State.Patrol : State.Idle);
            return;
        }

        _agent.SetDestination(_elaTransform.position);

        if (dist <= catchRadius)
            CatchEla();
    }

    private bool CanSeeEla()
    {
        if (_elaTransform == null) return false;

        float dist = Vector3.Distance(transform.position, _elaTransform.position);
        if (dist > chaseRadius) return false;

        Vector3 dirToEla = (_elaTransform.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, dirToEla);
        if (angle > fieldOfView * 0.5f) return false;

        // Engel var mı?
        if (Physics.Raycast(transform.position + Vector3.up, dirToEla, dist))
            return false; // raycast bir şeye çarptı (duvar vb.)

        return true;
    }

    private void CatchEla()
    {
        SetState(State.Caught);
        _agent.isStopped = true;
        GameManager.Instance?.TriggerLoss();
        Debug.Log($"[EntityAI] {gameObject.name} Ela'yı yakaladı!");
    }

    private void SetState(State newState)
    {
        _state = newState;
        switch (newState)
        {
            case State.Patrol:
                _agent.speed = patrolSpeed;
                _agent.isStopped = false;
                GoToNextPatrolPoint();
                break;
            case State.Chase:
                _agent.speed = chaseSpeed;
                _agent.isStopped = false;
                break;
            case State.Idle:
                _agent.isStopped = true;
                break;
        }
    }

    private void GoToNextPatrolPoint()
    {
        if (patrolPoints == null || patrolPoints.Length == 0) return;

        _agent.SetDestination(patrolPoints[_patrolIndex].position);
        _patrolIndex = (_patrolIndex + 1) % patrolPoints.Length;
        _waitTimer = patrolWaitTime;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, chaseRadius);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, catchRadius);
    }
}
