using UnityEngine;

// ego_character objesine ekle — animation event'leri DamageDealer'a iletir
public class CombatAnimationEvents : MonoBehaviour
{
    private DamageDealer damageDealer;

    void Start()
    {
        damageDealer = GetComponentInChildren<DamageDealer>();
    }

    public void StartDealDamage() => damageDealer?.StartDealDamage();
    public void EndDealDamage()   => damageDealer?.EndDealDamage();
}
