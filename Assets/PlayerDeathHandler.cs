using System.Collections;
using UnityEngine;

/// <summary>
/// Player nesnesine ekle. Health.onDeath event'ine otomatik abone olur.
/// Inspector'da ragdollPrefab atanırsa ragdoll yolu, atanmazsa ölüm animasyonu + sahne yenileme yolu çalışır.
/// </summary>
public class PlayerDeathHandler : MonoBehaviour
{
    [Header("Ragdoll (opsiyonel — boş bırakılırsa animasyon yolu çalışır)")]
    [SerializeField] GameObject ragdollPrefab;

    [Header("Sahne Yenileme")]
    [SerializeField] float reloadDelay = 3f;

    Character   character;
    Animator    animator;
    bool        isDead;

    void Awake()
    {
        character = GetComponent<Character>();
        animator  = GetComponentInChildren<Animator>();

        // Health.onDeath event'ine kod üzerinden abone ol (Inspector bağlantısına gerek yok)
        var health = GetComponent<Health>();
        if (health != null)
            health.onDeath.AddListener(OnPlayerDeath);
    }

    public void OnPlayerDeath()
    {
        if (isDead) return;
        isDead = true;

        if (character != null) character.enabled = false;

        // Notify all NPCs so they stop targeting this player immediately.
        GameEvents.RaisePlayerDied();

        if (ragdollPrefab != null)
        {
            var r = Instantiate(ragdollPrefab, transform.position, transform.rotation);
            Destroy(r, reloadDelay);
            StartCoroutine(TriggerGameOver());
            gameObject.SetActive(false);
        }
        else
        {
            if (animator != null && HasParameter("death", animator))
                animator.SetTrigger("death");

            StartCoroutine(TriggerGameOver());
        }
    }

    // Ölüm animasyonu tamamlanana kadar bekler, sonra Game Over ekranını açar.
    IEnumerator TriggerGameOver()
    {
        yield return new WaitForSecondsRealtime(reloadDelay);
        GameOverManager.Instance?.ShowGameOver();
    }

    // Animator'da parametre var mı kontrol et (hata almamak için)
    static bool HasParameter(string paramName, Animator anim)
    {
        foreach (var p in anim.parameters)
            if (p.name == paramName) return true;
        return false;
    }
}
