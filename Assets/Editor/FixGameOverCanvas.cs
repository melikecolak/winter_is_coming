using UnityEngine;
using UnityEditor;

public class FixGameOverCanvas
{
    public static void Execute()
    {
        // GameOverCanvas inactive olduğu için Awake çalışmıyor, Instance null kalıyor.
        // Görünmezlik alpha=0 ile sağlanıyor, SetActive(false) şart değil.
        var all = Resources.FindObjectsOfTypeAll<GameObject>();
        foreach (var go in all)
        {
            if (go.name == "GameOverCanvas" && go.scene.IsValid())
            {
                go.SetActive(true);
                EditorUtility.SetDirty(go);
                Debug.Log("[FixGameOverCanvas] GameOverCanvas aktif edildi.");
                break;
            }
        }

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
    }
}
