using UnityEngine;
using UnityEditor;

public class FixPlayerCamera
{
    public static void Execute()
    {
        // PlayerCamera'yı aktif et
        var playerCam = GameObject.Find("PlayerCamera");
        if (playerCam == null)
        {
            // Inactive olabilir, tüm objelerde ara
            var all = Resources.FindObjectsOfTypeAll<GameObject>();
            foreach (var go in all)
            {
                if (go.name == "PlayerCamera" && go.scene.IsValid())
                {
                    playerCam = go;
                    break;
                }
            }
        }

        if (playerCam != null)
        {
            playerCam.SetActive(true);
            EditorUtility.SetDirty(playerCam);
            Debug.Log($"[FixPlayerCamera] PlayerCamera aktif edildi. IsActive: {playerCam.activeSelf}");
        }
        else
        {
            Debug.LogError("[FixPlayerCamera] PlayerCamera bulunamadı!");
        }

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
    }
}
