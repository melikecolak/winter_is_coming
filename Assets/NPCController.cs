using UnityEngine;
using UnityEngine.AI;

public class NPCController : MonoBehaviour
{
    [Header("Hedef")]
    public Transform target;

    [Header("Mesafe Ayarları")]
    public float chaseRange = 20f;
    public float stopDistance = 2f;

    [Header("Hız")]
    public float runSpeed = 5f;
    public float walkSpeed = 2f;

    [Header("Saldırı (Yakında)")]
    public float attackRange = 1.5f;

    [Header("Ölüm")]
    public float deathAnimDuration = 2f; // Death animasyonu kaç saniye sürsün

    private NavMeshAgent agent;
    private Animator animator;
    private bool isDead = false;
    private Health health;

    public enum NPCState { Idle, Chasing, Attacking, Dead }
    public NPCState currentState = NPCState.Idle;

    private void Start()
    {
        health = GetComponent<Health>();
        if (health != null)
            health.onDeath.AddListener(Die);
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();

        agent.stoppingDistance = stopDistance;
        agent.speed = runSpeed;
    }

    private void Update()
    {
        if (isDead) return;

        if (target == null) return;

        float distanceToTarget = Vector3.Distance(transform.position, target.position);

        switch (currentState)
        {
            case NPCState.Idle:     HandleIdle(distanceToTarget);    break;
            case NPCState.Chasing:  HandleChasing(distanceToTarget); break;
            case NPCState.Attacking: break;
        }

        UpdateAnimations();
    }

    private void HandleIdle(float distance)
    {
        if (distance <= chaseRange)
            currentState = NPCState.Chasing;
    }

    private void HandleChasing(float distance)
    {
        if (distance > chaseRange)
        {
            currentState = NPCState.Idle;
            agent.ResetPath();
            return;
        }

        agent.SetDestination(target.position);
    }

    private void UpdateAnimations()
    {
        bool isRunning = currentState == NPCState.Chasing
                     && !agent.pathPending
                     && agent.hasPath
                     && agent.remainingDistance > agent.stoppingDistance + 0.05f;

        animator.SetBool("isRunning", isRunning);
    }

    public void Die()
{
    if (isDead) return;
    isDead = true;
    currentState = NPCState.Dead;

    agent.isStopped = true;
    agent.ResetPath();

    IceShatterEffect ice = GetComponent<IceShatterEffect>();
    if (ice != null) ice.Shatter();

    SkinnedMeshRenderer smr = GetComponentInChildren<SkinnedMeshRenderer>();
    if (smr != null) smr.enabled = false;

    Destroy(gameObject, 5f);
}
}


