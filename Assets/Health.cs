using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour, IDamageable
{
    [Header("Can")]
    public float maxHealth = 100f;
    [SerializeField] float currentHealth;

    [Header("VFX")]
    [SerializeField] GameObject hitVFXPrefab;

    [Header("Events")]
    public UnityEvent onDeath;
    public UnityEvent<float, float> onHealthChanged;

    Animator animator;
    bool isDead;

    public bool IsDead        => isDead;
    public float CurrentHealth => currentHealth;
    public float MaxHealth     => maxHealth;

    void Awake()
    {
        currentHealth = maxHealth;
        animator = GetComponentInChildren<Animator>();
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;
        currentHealth = Mathf.Clamp(currentHealth - amount, 0f, maxHealth);
        onHealthChanged?.Invoke(currentHealth, maxHealth);
        animator?.SetTrigger("damage");                    // Sorun 4: player damage animasyonu
        CameraShake.Instance?.ShakeCamera(2f, 0.2f);      // Sorun 4: camera shake
        if (currentHealth <= 0f) Die();
    }

    public void HitVFX(Vector3 hitPosition)
    {
        if (hitVFXPrefab == null) return;
        var vfx = Instantiate(hitVFXPrefab, hitPosition, Quaternion.identity);
        Destroy(vfx, 3f);
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;
        onDeath?.Invoke();
    }
}
