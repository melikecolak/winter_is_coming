using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class ResizeMenuButtons
{
    public static void Execute()
    {
        var buttonNames = new[] { "PlayButton", "QuitButton", "AboutButton" };
        var containerPath = "Canvas/Panel_StartMenuUI/ButtonContainer";

        // Spacing küçült
        var container = GameObject.Find(containerPath);
        if (container != null)
        {
            var vlg = container.GetComponent<VerticalLayoutGroup>();
            if (vlg != null)
            {
                vlg.spacing = 10f;
                EditorUtility.SetDirty(vlg);
            }
        }

        foreach (var btnName in buttonNames)
        {
            var path = containerPath + "/" + btnName;
            var btnGO = GameObject.Find(path);
            if (btnGO == null) { Debug.LogWarning($"Bulunamadı: {path}"); continue; }

            // Buton scale sıfırla, SizeDelta büyüt
            var rt = btnGO.GetComponent<RectTransform>();
            rt.localScale  = Vector3.one;
            rt.sizeDelta   = new Vector2(300f, 80f);
            EditorUtility.SetDirty(rt);

            // Text child — TMP
            var tmp = btnGO.GetComponentInChildren<TextMeshProUGUI>(true);
            if (tmp != null)
            {
                tmp.fontSize = 72f;
                var trt = tmp.GetComponent<RectTransform>();
                trt.localScale        = Vector3.one;
                trt.anchoredPosition  = Vector2.zero;
                EditorUtility.SetDirty(tmp);
                EditorUtility.SetDirty(trt);
            }
        }

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        Debug.Log("[ResizeMenuButtons] Tamamlandı.");
    }
}
