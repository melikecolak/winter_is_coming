using UnityEngine;
using UnityEngine.InputSystem;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class MainMenuController : MonoBehaviour
{
    [SerializeField] GameObject menuUI;
    [SerializeField] GameObject hudUI;
    [SerializeField] Camera     menuCamera;
    [SerializeField] Camera     mainCamera;

    void Start()
    {
        menuCamera.gameObject.SetActive(true);
        mainCamera.gameObject.SetActive(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;
    }

    public void PlayGame()
    {
        menuUI.SetActive(false);
        if (hudUI != null) hudUI.SetActive(true);

        menuCamera.gameObject.SetActive(false);
        mainCamera.gameObject.SetActive(true);

        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            var ch = player.GetComponent<Character>();
            if (ch) ch.enabled = true;
            var pi = player.GetComponent<PlayerInput>();
            if (pi) pi.enabled = true;
        }

        foreach (var enemy in FindObjectsByType<Enemy>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            enemy.enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void OpenAbout()
    {
        Debug.Log("About clicked");
    }
}
