using UnityEngine;

public class EnemyDamageDealer : MonoBehaviour
{
    [SerializeField] float     weaponDamage = 10f;
    [SerializeField] LayerMask playerLayer = 1 << 8; // Layer 8 = Player

    float weaponRadius;
    bool  canDealDamage;
    bool  hasDealtDamage;

    void Update()
    {
        if (!canDealDamage || hasDealtDamage) return;

        var hits = Physics.OverlapSphere(transform.position, weaponRadius, playerLayer);
        foreach (var col in hits)
        {
            IDamageable target = col.GetComponent<IDamageable>()
                              ?? col.GetComponentInParent<IDamageable>();

            if (target != null && !target.IsDead)
            {
                target.TakeDamage(weaponDamage);
                target.HitVFX(transform.position);
                hasDealtDamage = true;
                return;
            }
        }
    }

    public void StartDealDamage(float range)
    {
        weaponRadius  = range;
        canDealDamage = true;
    }

    public void EndDealDamage()
    {
        canDealDamage  = false;
        hasDealtDamage = false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, weaponRadius);
    }
}
