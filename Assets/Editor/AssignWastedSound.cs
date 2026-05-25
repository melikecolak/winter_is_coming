using UnityEngine;
using UnityEditor;

public class AssignWastedSound
{
    public static void Execute()
    {
        var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(
            "Assets/Sounds/GTA V Wasted_Busted - Sound Effect (HD)_320k.mp3");

        if (clip == null) { Debug.LogError("AudioClip bulunamadı!"); return; }

        var canvasGO = GameObject.Find("GameOverCanvas");
        if (canvasGO == null) { Debug.LogError("GameOverCanvas bulunamadı!"); return; }

        var manager = canvasGO.GetComponent<GameOverManager>();
        var so = new SerializedObject(manager);
        so.FindProperty("wastedSound").objectReferenceValue = clip;
        so.ApplyModifiedProperties();

        EditorUtility.SetDirty(manager);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        Debug.Log($"[AssignWastedSound] '{clip.name}' atandı.");
    }
}
