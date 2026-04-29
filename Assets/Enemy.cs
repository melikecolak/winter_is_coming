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
    [SerializeField] float minSeparation  = 1.2f; // içinden geçmeyi önleyen minimum mesafe

    public bool IsDead => health <= 0;

    GameObject        player;
    NavMeshAgent      agent;
    Animator          animator;
    CharacterController playerCC;
    float             attackTimer;
    float             destTimer = 0.5f;
    bool              isAttacking;

    void Start()
    {
        agent    = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
        player   = GameObject.FindGameObjectWithTag("Player");
        playerCC = player?.GetComponent<CharacterController>();

        agent.updateRotation   = false;
        agent.stoppingDistance = stoppingDist;
    }

    void Update()
    {
        if (IsDead || player == null) return;

        float dist = Vector3.Distance(player.transform.position, transform.position);

        animator.SetFloat("speed", agent.velocity.magnitude / agent.speed);

        // Düşmanın player'ın içine girmesini kodla önle
        PreventOverlap();

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

    // Her iki tarafı da iter: enemy transform + player CharacterController
    void PreventOverlap()
    {
        Vector3 diff = transform.position - player.transform.position;
        diff.y = 0f;
        float dist = diff.magnitude;

        if (dist >= minSeparation || dist < 0.01f) return;

        float overlap    = minSeparation - dist;
        Vector3 pushDir  = diff.normalized;

        // Düşmanı ağırlıklı geri it (NavMesh sorumluluğu)
        transform.position += pushDir * (overlap * 0.6f);
        agent.nextPosition  = transform.position;

        // Player CharacterController'ı hafifçe geri it
        playerCC?.Move(pushDir * -(overlap * 0.4f));
    }

    void StartAttack()
    {
        attackTimer     = 0f;
        isAttacking     = true;
        agent.isStopped = true;
        animator.SetTrigger("attack");
        StartCoroutine(AttackResetFallback());
    }

    IEnumerator AttackResetFallback()
    {
        yield return new WaitForSeconds(attackDuration);
        if (isAttacking) ResumeAfterAttack();
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

    // ── Animation Events ────────────────────────────────────────────────

    public void StartDealDamage() =>
        GetComponentInChildren<EnemyDamageDealer>()?.StartDealDamage();

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
        agent.isStopped = true;
        agent.ResetPath();

        if (ragdoll != null)
        {
            Instantiate(ragdoll, transform.position, transform.rotation);
            Destroy(gameObject);
        }
        else
        {
            var ice = GetComponent<IceShatterEffect>();
            if (ice != null) ice.Shatter();
            var smr = GetComponentInChildren<SkinnedMeshRenderer>();
            if (smr != null) smr.enabled = false;
            Destroy(gameObject, 5f);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, aggroRange);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, minSeparation);
    }
}
