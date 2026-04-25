using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [Header("Saldırı")]
    public float attackDamage = 25f;
    public float attackRange = 2f;
    public LayerMask enemyLayer;

    [Header("Combo")]
    public float comboWindow = 0.9f;
    public float attackCooldown = 0.3f;

    [Header("Hit Noktası")]
    public Transform hitOrigin; // Kılıç ucundaki boş obje

    private Animator animator;
    private int comboStep = 0;
    private float lastAttackTime = -99f;
    private float comboTimer = 0f;
    private bool comboBuffered = false;

    private static readonly int HashAttack1 = Animator.StringToHash("Attack1");
    private static readonly int HashAttack2 = Animator.StringToHash("Attack2");

    private void Start()
    {
        animator = GetComponentInChildren<Animator>();
        if (hitOrigin == null) hitOrigin = transform;
    }

    private void Update()
    {
        HandleComboTimer();

        if (Input.GetMouseButtonDown(0))
            TryAttack();
    }

    private void TryAttack()
    {
        if (Time.time - lastAttackTime < attackCooldown) return;

        if (comboStep == 0)
        {
            comboStep = 1;
            lastAttackTime = Time.time;
            comboTimer = comboWindow;
            animator.SetTrigger(HashAttack1);
        }
        else if (comboStep == 1 && comboTimer > 0f)
        {
            comboBuffered = true;
        }
    }

    private void HandleComboTimer()
    {
        if (comboStep == 0) return;

        comboTimer -= Time.deltaTime;

        if (comboBuffered)
        {
            comboBuffered = false;
            comboStep = 0;
            lastAttackTime = Time.time;
            comboTimer = 0f;
            animator.SetTrigger(HashAttack2);
        }
        else if (comboTimer <= 0f)
        {
            comboStep = 0;
        }
    }

    // Bu metodu Animation Event çağıracak — ismi TAM bu olmalı
    public void OnHitFrame()
{
    Debug.Log("[Combat] OnHitFrame çağrıldı!");

    Collider[] hits = Physics.OverlapSphere(hitOrigin.position, attackRange, enemyLayer);
    
    Debug.Log($"[Combat] Bulunan collider sayısı: {hits.Length}");

    foreach (Collider hit in hits)
    {
        Debug.Log($"[Combat] Hit: {hit.gameObject.name}, Layer: {hit.gameObject.layer}");
        
        IDamageable target = hit.GetComponent<IDamageable>()
                          ?? hit.GetComponentInParent<IDamageable>();

        Debug.Log($"[Combat] IDamageable bulundu mu: {target != null}");

        if (target != null && !target.IsDead)
        {
            target.TakeDamage(attackDamage);
        }
    }
}

    // Scene'de kırmızı küre ile saldırı menzilini gösterir
    private void OnDrawGizmosSelected()
    {
        if (hitOrigin == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(hitOrigin.position, attackRange);
    }
}