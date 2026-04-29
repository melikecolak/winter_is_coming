using UnityEngine;

public class EnemyDamageDealer : MonoBehaviour
{
    [SerializeField] float     weaponLength = 1f;
    [SerializeField] float     weaponDamage = 10f;
    [SerializeField] LayerMask playerLayer;

    bool canDealDamage;
    bool hasDealtDamage;

    void Update()
    {
        if (!canDealDamage || hasDealtDamage) return;

        if (Physics.Raycast(transform.position, -transform.up, out RaycastHit hit, weaponLength, playerLayer))
        {
            IDamageable target = hit.collider.GetComponent<IDamageable>()
                              ?? hit.collider.GetComponentInParent<IDamageable>();

            if (target != null && !target.IsDead)
            {
                target.TakeDamage(weaponDamage);
                target.HitVFX(hit.point); // Fix 3: IDamageable interface üzerinden
                hasDealtDamage = true;
            }
        }
    }

    public void StartDealDamage()
    {
        canDealDamage  = true;
        hasDealtDamage = false;
    }

    public void EndDealDamage() => canDealDamage = false;

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position - transform.up * weaponLength);
    }
}
