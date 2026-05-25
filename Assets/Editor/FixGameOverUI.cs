using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class FixGameOverUI
{
    public static void Execute()
    {
        // ── YouDiedText ───────────────────────────────────────────────────────
        var youDied = GameObject.Find("GameOverCanvas/FadePanel/YouDiedText");
        if (youDied != null)
        {
            var tmp       = youDied.GetComponent<TextMeshProUGUI>();
            tmp.fontSize  = 80f;
            tmp.fontStyle = FontStyles.Bold | FontStyles.Italic;
            EditorUtility.SetDirty(tmp);
        }

        // ── ButtonContainer: layout tamamen manuel yap ────────────────────────
        var container = GameObject.Find("GameOverCanvas/FadePanel/ButtonContainer");
        if (container != null)
        {
            var vlg = container.GetComponent<VerticalLayoutGroup>();
            vlg.childControlWidth      = false;
            vlg.childControlHeight     = false;
            vlg.childForceExpandWidth  = false;
            vlg.childForceExpandHeight = false;
            vlg.spacing                = 20f;
            vlg.childAlignment         = TextAnchor.MiddleCenter;
            EditorUtility.SetDirty(vlg);

            // Container boyutu: 2 buton (70) + spacing (20) = 160 → 200 ile güvenli
            var crt          = container.GetComponent<RectTransform>();
            crt.sizeDelta    = new Vector2(300f, 200f);
            EditorUtility.SetDirty(crt);

            foreach (Transform child in container.transform)
            {
                // LayoutElement ile preferred height'ı zorla
                var le = child.GetComponent<LayoutElement>();
                if (le == null) le = child.gameObject.AddComponent<LayoutElement>();
                le.preferredWidth  = 250f;
                le.preferredHeight = 70f;
                le.minHeight       = 70f;

                var rt        = child.GetComponent<RectTransform>();
                rt.sizeDelta  = new Vector2(250f, 70f);
                rt.anchorMin  = new Vector2(0.5f, 0.5f);
                rt.anchorMax  = new Vector2(0.5f, 0.5f);
                rt.pivot      = new Vector2(0.5f, 0.5f);
                rt.localScale = Vector3.one;
                EditorUtility.SetDirty(rt);
                EditorUtility.SetDirty(le);
            }
        }

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        Debug.Log("[FixGameOverUI] Tamamlandı.");
    }
}
