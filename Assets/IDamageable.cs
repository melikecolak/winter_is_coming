using UnityEngine;

public interface IDamageable
{
    void TakeDamage(float amount);
    void HitVFX(Vector3 hitPosition);
    bool IsDead { get; }
}
