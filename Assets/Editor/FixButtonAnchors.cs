using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class FixButtonAnchors
{
    public static void Execute()
    {
        var containerPath = "Canvas/Panel_StartMenuUI/ButtonContainer";
        var container = GameObject.Find(containerPath);

        var vlg = container.GetComponent<VerticalLayoutGroup>();
        vlg.childForceExpandWidth  = false;
        vlg.childForceExpandHeight = false;
        vlg.childControlWidth      = false;
        vlg.childControlHeight     = false;
        vlg.spacing                = 10f;
        EditorUtility.SetDirty(vlg);

        var buttonNames = new[] { "PlayButton", "QuitButton", "AboutButton" };
        foreach (var name in buttonNames)
        {
            var go = GameObject.Find(containerPath + "/" + name);
            if (go == null) continue;

            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin        = new Vector2(0.5f, 0.5f);
            rt.anchorMax        = new Vector2(0.5f, 0.5f);
            rt.pivot            = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta        = new Vector2(300f, 80f);
            rt.localScale       = Vector3.one;
            EditorUtility.SetDirty(rt);
        }

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        Debug.Log("[FixButtonAnchors] Tamamlandı.");
    }
}
