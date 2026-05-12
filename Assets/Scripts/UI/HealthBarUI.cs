using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    [SerializeField] Slider healthSlider;

    Health playerHealth;

    void Start()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning("HealthBarUI: 'Player' tag'li GameObject bulunamadi.");
            return;
        }

        playerHealth = player.GetComponent<Health>();
        if (playerHealth == null)
        {
            Debug.LogWarning("HealthBarUI: Player üzerinde Health component bulunamadi.");
            return;
        }

        playerHealth.onHealthChanged.AddListener(OnHealthChanged);

        healthSlider.maxValue = playerHealth.MaxHealth;
        healthSlider.value = playerHealth.CurrentHealth;
    }

    void OnDestroy()
    {
        if (playerHealth != null)
            playerHealth.onHealthChanged.RemoveListener(OnHealthChanged);
    }

    void OnHealthChanged(float current, float max)
    {
        healthSlider.value = current;
    }
}
