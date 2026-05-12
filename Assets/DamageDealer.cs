using System.Collections.Generic;
using UnityEngine;

public class DamageDealer : MonoBehaviour
{
    [SerializeField] float weaponLength = 1.5f;
    [SerializeField] float weaponDamage = 25f;
    [SerializeField] LayerMask enemyLayer;

    bool canDealDamage;
    List<GameObject> hasDealtDamage = new();

    void Start()
    {
        canDealDamage = false;
    }

    void Update()
    {
        if (!canDealDamage) return;

        if (Physics.Raycast(transform.position, -transform.up, out RaycastHit hit, weaponLength, enemyLayer))
        {
            if (hasDealtDamage.Contains(hit.transform.gameObject)) return;

            IDamageable target = hit.collider.GetComponent<IDamageable>()
                              ?? hit.collider.GetComponentInParent<IDamageable>();

            if (target != null && !target.IsDead)
            {
                target.TakeDamage(weaponDamage);
                target.HitVFX(hit.point);
                hasDealtDamage.Add(hit.transform.gameObject);
            }
        }
    }

    public void StartDealDamage()
    {
        canDealDamage = true;
        hasDealtDamage.Clear();
    }

    public void EndDealDamage()
    {
        canDealDamage = false;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position - transform.up * weaponLength);
    }
}
