using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour, IDamageable
{
    [SerializeField] float health = 100f;
    [SerializeField] GameObject hitVFXPrefab;
    [SerializeField] GameObject ragdoll;

    [Header("Combat")]
    [SerializeField] float attackCD       = 3f;
    [SerializeField] float attackDuration = 1.5f;
    [SerializeField] float attackRange    = 2.5f;
    [SerializeField] float aggroRange     = 15f;
    [SerializeField] float rotationSpeed  = 6f;
    [SerializeField] float stoppingDist   = 2f;

    public bool IsDead => health <= 0;

    GameObject   player;
    NavMeshAgent agent;
    Animator     animator;
    float        attackTimer;
    float        destTimer = 0.5f;
    bool         isAttacking;

    void OnEnable()  => GameEvents.OnPlayerDied += OnPlayerDied;
    void OnDisable() => GameEvents.OnPlayerDied -= OnPlayerDied;

    void Start()
    {
        agent    = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
        player   = GameObject.FindGameObjectWithTag("Player");

        agent.updateRotation      = false;
        agent.stoppingDistance    = stoppingDist;
        agent.avoidancePriority   = Random.Range(30, 70);
    }

    void Update()
    {
        if (IsDead || player == null) return;

        float dist = Vector3.Distance(player.transform.position, transform.position);

        animator.SetFloat("speed", agent.velocity.magnitude / agent.speed);

        if (dist <= aggroRange)
        {
            SmoothRotateTowardPlayer();

            attackTimer += Time.deltaTime;
            if (!isAttacking && attackTimer >= attackCD && dist <= attackRange)
                StartAttack();

            if (!isAttacking)
            {
                if (dist > stoppingDist + 0.3f)
                {
                    agent.isStopped = false;
                    destTimer -= Time.deltaTime;
                    if (destTimer <= 0f)
                    {
                        destTimer = 0.5f;
                        agent.SetDestination(player.transform.position);
                    }
                }
                else
                {
                    agent.isStopped = true;
                    agent.ResetPath();
                }
            }
        }
        else
        {
            agent.isStopped = false;
            agent.ResetPath();
            destTimer = 0.5f;
        }
    }

    void StartAttack()
    {
        attackTimer     = 0f;
        isAttacking     = true;
        agent.isStopped = true;
        agent.ResetPath();
        animator.SetTrigger("attack");
        StartCoroutine(AttackResetFallback());
    }

    IEnumerator AttackResetFallback()
    {
        yield return new WaitForSeconds(attackDuration);
        if (isAttacking)
        {
            GetComponentInChildren<EnemyDamageDealer>()?.EndDealDamage();
            ResumeAfterAttack();
        }
    }

    void SmoothRotateTowardPlayer()
    {
        Vector3 dir = player.transform.position - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.01f) return;
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            Quaternion.LookRotation(dir),
            rotationSpeed * Time.deltaTime);
    }

    void ResumeAfterAttack()
    {
        isAttacking     = false;
        agent.isStopped = false;
    }

    // ── Player Death Event ───────────────────────────────────────────────

    void OnPlayerDied()
    {
        if (IsDead) return;

        // Stop any in-progress attack coroutine and damage window.
        StopAllCoroutines();
        GetComponentInChildren<EnemyDamageDealer>()?.EndDealDamage();
        isAttacking     = false;
        agent.isStopped = true;
        agent.ResetPath();
        animator.SetFloat("speed", 0f);
        animator.ResetTrigger("attack");

        // TODO (multiplayer): call FindNextTarget() here instead of nulling player.
        // FindNextTarget() should scan for other living players and assign one;
        // only go idle when none remain.
        player = null;
    }

    // ── Animation Events ────────────────────────────────────────────────

    public void StartDealDamage() =>
        GetComponentInChildren<EnemyDamageDealer>()?.StartDealDamage(attackRange);

    public void EndDealDamage()
    {
        StopAllCoroutines();
        GetComponentInChildren<EnemyDamageDealer>()?.EndDealDamage();
        ResumeAfterAttack();
    }

    // ── IDamageable ─────────────────────────────────────────────────────

    public void TakeDamage(float amount)
    {
        if (IsDead) return;
        health -= amount;
        animator.SetTrigger("damage");
        CameraShake.Instance?.ShakeCamera(2f, 0.2f);
        if (health <= 0f) Die();
    }

    public void HitVFX(Vector3 hitPosition)
    {
        if (hitVFXPrefab == null) return;
        var vfx = Instantiate(hitVFXPrefab, hitPosition, Quaternion.identity);
        Destroy(vfx, 3f);
    }

    void Die()
    {
        StopAllCoroutines();

        if (ragdoll != null)
        {
            var r = Instantiate(ragdoll, transform.position, transform.rotation);
            Destroy(r, 7f);
            Destroy(gameObject);
        }
        else
        {
            // Ragdoll atanmamışsa eski yol: death animasyonu + gecikmiş destroy
            agent.isStopped = true;
            agent.enabled = false;
            GetComponent<CapsuleCollider>().enabled = false;
            animator.SetTrigger("death");
            Destroy(gameObject, 9.2f);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, aggroRange);
    }
}
