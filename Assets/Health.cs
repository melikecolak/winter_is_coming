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

    [Header("Ses")]
    [SerializeField] AudioClip damage0Clip;
    [SerializeField] AudioClip damage1Clip;

    Animator     animator;
    AudioSource  audioSource;
    int          damageClipIndex;
    bool isDead;

    public bool IsDead        => isDead;
    public float CurrentHealth => currentHealth;
    public float MaxHealth     => maxHealth;

    void Awake()
    {
        currentHealth = maxHealth;
        animator      = GetComponentInChildren<Animator>();

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource              = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 0f;
            audioSource.playOnAwake  = false;
        }
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;
        currentHealth = Mathf.Clamp(currentHealth - amount, 0f, maxHealth);
        onHealthChanged?.Invoke(currentHealth, maxHealth);
        CameraShake.Instance?.ShakeCamera(2f, 0.2f);
        PlayDamageSound();
        if (currentHealth <= 0f) { Die(); return; }
        animator?.SetTrigger("damage");
    }

    void PlayDamageSound()
    {
        AudioClip clip = (damageClipIndex % 2 == 0) ? damage0Clip : damage1Clip;
        damageClipIndex++;
        if (clip != null) audioSource.PlayOneShot(clip);
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
