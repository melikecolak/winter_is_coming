using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class Enemy : MonoBehaviour, IDamageable
{
    // ── Living-enemy counter ─────────────────────────────────────────────
    static int livingCount;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Init()
    {
        livingCount = 0;
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    static void OnSceneLoaded(Scene scene, LoadSceneMode mode) => livingCount = 0;

    // ── Fields ───────────────────────────────────────────────────────────
    [SerializeField] float health = 100f;
    [SerializeField] GameObject hitVFXPrefab;
    [SerializeField] GameObject ragdoll; // Artık kullanılmıyor, diriliş sistemiyle uyumsuz

    [Header("Combat")]
    [SerializeField] float attackCD       = 3f;
    [SerializeField] float attackDuration = 1.5f;
    [SerializeField] float attackRange    = 2.5f;
    [SerializeField] float aggroRange     = 15f;
    [SerializeField] float rotationSpeed  = 6f;
    [SerializeField] float stoppingDist   = 2f;

    [Header("Resurrection")]
    [SerializeField] float resurrectDelay = 15f;

    [Header("Ses")]
    [SerializeField] AudioClip  zombieDyingClip;
    [SerializeField] [Range(0f, 3f)] float zombieDyingVolume = 1.5f;

    public bool IsDead   => health <= 0f;
    public bool IsBurned => isBurned;

    bool  isBurned = false;
    float maxHealth;

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
        livingCount++;
        maxHealth = health;

        agent    = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
        player   = GameObject.FindGameObjectWithTag("Player");

        agent.updateRotation    = false;
        agent.stoppingDistance  = stoppingDist;
        agent.avoidancePriority = Random.Range(30, 70);
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
        StopAllCoroutines();
        GetComponentInChildren<EnemyDamageDealer>()?.EndDealDamage();
        isAttacking     = false;
        agent.isStopped = true;
        agent.ResetPath();
        animator.SetFloat("speed", 0f);
        animator.ResetTrigger("attack");
        player = null;
    }

    // ── Animation Events ─────────────────────────────────────────────────

    public void StartDealDamage() =>
        GetComponentInChildren<EnemyDamageDealer>()?.StartDealDamage(attackRange);

    public void EndDealDamage()
    {
        StopAllCoroutines();
        GetComponentInChildren<EnemyDamageDealer>()?.EndDealDamage();
        ResumeAfterAttack();
    }

    // ── IDamageable ──────────────────────────────────────────────────────

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

    // ── Death & Resurrection ─────────────────────────────────────────────

    void Die()
    {
        livingCount--;

        StopAllCoroutines();
        isAttacking     = false;
        agent.isStopped = true;
        agent.enabled   = false;

        // Trigger'a çevir: player ceset üzerinden geçebilsin ve yakma etkileşimi çalışsın
        var col = GetComponent<CapsuleCollider>();
        if (col != null) col.isTrigger = true;

        animator.SetTrigger("death");

        if (livingCount <= 0)
            GameEvents.RaiseAllEnemiesDied();

        StartCoroutine(ResurrectionCountdown());
    }

    IEnumerator ResurrectionCountdown()
    {
        yield return new WaitForSeconds(resurrectDelay);
        if (!isBurned) Resurrect();
    }

    void Resurrect()
    {
        health = maxHealth * 0.5f;
        livingCount++;

        var col = GetComponent<CapsuleCollider>();
        if (col != null) col.isTrigger = false;

        agent.enabled   = true;
        agent.isStopped = false;
        isAttacking     = false;
        attackTimer     = 0f;
        destTimer       = 0.5f;

        player = GameObject.FindGameObjectWithTag("Player");
        animator.SetTrigger("resurrect");
    }

    // ── Burning (InteractionSystem tarafından çağrılır) ──────────────────

    public void Burn()
    {
        if (isBurned) return;
        isBurned = true;
        StopAllCoroutines(); // diriliş sayacını iptal et
        StartCoroutine(BurnSequence());
    }

    IEnumerator BurnSequence()
    {
        if (zombieDyingClip != null)
        {
            var src = gameObject.AddComponent<AudioSource>();
            src.spatialBlend = 1f;
            src.playOnAwake  = false;
            src.PlayOneShot(zombieDyingClip, zombieDyingVolume);
        }
        yield return new WaitForSeconds(8f);
        Destroy(gameObject);
    }

    // ── Gizmos ───────────────────────────────────────────────────────────

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, aggroRange);
    }
}
