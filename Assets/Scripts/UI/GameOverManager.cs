using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class GameOverManager : MonoBehaviour
{
    // ── Singleton ────────────────────────────────────────────────────────────
    public static GameOverManager Instance { get; private set; }

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    // ── Inspector Fields ─────────────────────────────────────────────────────
    [Header("Canvas")]
    [SerializeField] GameObject gameOverCanvas;

    [Header("CanvasGroups")]
    [SerializeField] CanvasGroup fadePanel;
    [SerializeField] CanvasGroup youDiedText;
    [SerializeField] CanvasGroup buttonContainer;

    [Header("Timing")]
    [SerializeField] float fadeDuration       = 2f;
    [SerializeField] float textFadeDuration   = 1.5f;
    [SerializeField] float buttonFadeDuration = 1f;

    [Header("Audio")]
    [SerializeField] AudioClip wastedSound;
    AudioSource audioSource;

    // ── Public API ───────────────────────────────────────────────────────────
    public void ShowGameOver() => StartCoroutine(GameOverSequence());

    // ── Coroutine ────────────────────────────────────────────────────────────
    IEnumerator GameOverSequence()
    {
        // 1) Her şeyi sıfırla
        fadePanel.alpha            = 0f;
        fadePanel.blocksRaycasts   = true;  // FadePanel raycasts'i engeller — cascade sorunu çözülür
        fadePanel.interactable     = false;
        youDiedText.alpha          = 0f;
        buttonContainer.alpha      = 0f;
        buttonContainer.interactable   = false;
        buttonContainer.blocksRaycasts = false;

        // 2) Cursor'ı hemen aç — kullanıcı butonlara tıklayabilsin
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;

        // 3) Wasted sesini çal
        if (wastedSound != null)
            audioSource.PlayOneShot(wastedSound);

        // 3) Ekran siyaha kararsın
        yield return StartCoroutine(FadeCanvasGroup(fadePanel, 0f, 1f, fadeDuration));

        // 4) "You Died!" yazısı belirsin
        yield return StartCoroutine(FadeCanvasGroup(youDiedText, 0f, 1f, textFadeDuration));

        // 5) Butonlar belirsin
        yield return StartCoroutine(FadeCanvasGroup(buttonContainer, 0f, 1f, buttonFadeDuration));

        // 6) Butonları aktif et — fadePanel.interactable da true olmalı,
        //    aksi halde parent cascade ile buttonContainer'ı ezer.
        fadePanel.interactable         = true;
        buttonContainer.interactable   = true;
        buttonContainer.blocksRaycasts = true;
        Time.timeScale = 0f;
    }

    // Herhangi bir CanvasGroup'u verilen sürede lerp ile fade eden yardımcı coroutine.
    // Time.unscaledDeltaTime kullanır — timeScale=0 olsa bile çalışır.
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

    // ── Button Callbacks ─────────────────────────────────────────────────────
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
