using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour, IDamageable
{
    [Header("Can Ayarları")]
    public float maxHealth = 100f;
    [SerializeField] private float currentHealth;

    [Header("Olaylar")]
    public UnityEvent onDeath;
    public UnityEvent<float, float> onHealthChanged; // current, max

    private bool isDead = false;
    public bool IsDead => isDead;
    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;

        currentHealth = Mathf.Clamp(currentHealth - amount, 0f, maxHealth);
        onHealthChanged?.Invoke(currentHealth, maxHealth);

        Debug.Log($"[Health] {gameObject.name} → {currentHealth}/{maxHealth}");

        if (currentHealth <= 0f)
            Die();
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;
        onDeath?.Invoke();
    }
}