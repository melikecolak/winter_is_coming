using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class VictoryManager : MonoBehaviour
{
    // ── Singleton ────────────────────────────────────────────────────────
    public static VictoryManager Instance { get; private set; }

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    void OnEnable()  => GameEvents.OnAllEnemiesDied += ShowVictory;
    void OnDisable() => GameEvents.OnAllEnemiesDied -= ShowVictory;

    // ── Inspector Fields ─────────────────────────────────────────────────
    [Header("CanvasGroups")]
    [SerializeField] CanvasGroup fadePanel;
    [SerializeField] CanvasGroup youWinText;
    [SerializeField] CanvasGroup buttonContainer;

    [Header("Timing")]
    [SerializeField] float fadeDuration       = 2f;
    [SerializeField] float textFadeDuration   = 1.5f;
    [SerializeField] float buttonFadeDuration = 1f;

    [Header("Audio")]
    [SerializeField] AudioClip victorySound;
    AudioSource audioSource;

    // ── Public API ───────────────────────────────────────────────────────
    public void ShowVictory() => StartCoroutine(VictorySequence());

    // ── Coroutine ────────────────────────────────────────────────────────
    IEnumerator VictorySequence()
    {
        fadePanel.alpha            = 0f;
        fadePanel.blocksRaycasts   = true;
        fadePanel.interactable     = false;
        youWinText.alpha           = 0f;
        buttonContainer.alpha      = 0f;
        buttonContainer.interactable   = false;
        buttonContainer.blocksRaycasts = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;

        if (victorySound != null)
            audioSource.PlayOneShot(victorySound);

        // Ekran beyaza kararsın
        yield return StartCoroutine(FadeCanvasGroup(fadePanel, 0f, 1f, fadeDuration));

        // "You Win!" yazısı belirsin
        yield return StartCoroutine(FadeCanvasGroup(youWinText, 0f, 1f, textFadeDuration));

        // Butonlar belirsin
        yield return StartCoroutine(FadeCanvasGroup(buttonContainer, 0f, 1f, buttonFadeDuration));

        fadePanel.interactable         = true;
        buttonContainer.interactable   = true;
        buttonContainer.blocksRaycasts = true;
        Time.timeScale = 0f;
    }

    IEnumerator FadeCanvasGroup(CanvasGroup cg, float from, float to, float duration)
    {
        float elapsed = 0f;
        cg.alpha = from;
        while (elapsed < duration)
        {
            elapsed  += Time.unscaledDeltaTime;
            cg.alpha  = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }
        cg.alpha = to;
    }

    // ── Button Callbacks ─────────────────────────────────────────────────
    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
