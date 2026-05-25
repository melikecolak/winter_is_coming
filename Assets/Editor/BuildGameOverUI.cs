using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class BuildGameOverUI
{
    public static void Execute()
    {
        // ── GameOverCanvas ────────────────────────────────────────────────────
        var canvasGO = new GameObject("GameOverCanvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode  = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        canvasGO.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasGO.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920, 1080);
        canvasGO.AddComponent<GraphicRaycaster>();
        canvasGO.SetActive(false); // başlangıçta kapalı

        // ── FadePanel (tam ekran siyah Image + CanvasGroup) ───────────────────
        var fadePanelGO = new GameObject("FadePanel");
        fadePanelGO.transform.SetParent(canvasGO.transform, false);

        var fadePanelRT = fadePanelGO.AddComponent<RectTransform>();
        fadePanelRT.anchorMin        = Vector2.zero;
        fadePanelRT.anchorMax        = Vector2.one;
        fadePanelRT.offsetMin        = Vector2.zero;
        fadePanelRT.offsetMax        = Vector2.zero;

        var fadePanelImg   = fadePanelGO.AddComponent<Image>();
        fadePanelImg.color = Color.black;
        fadePanelImg.raycastTarget = true;

        var fadePanelCG  = fadePanelGO.AddComponent<CanvasGroup>();
        fadePanelCG.alpha            = 0f;
        fadePanelCG.interactable     = false;
        fadePanelCG.blocksRaycasts   = false;

        // ── YouDiedText ───────────────────────────────────────────────────────
        var youDiedGO = new GameObject("YouDiedText");
        youDiedGO.transform.SetParent(fadePanelGO.transform, false);

        var youDiedRT = youDiedGO.AddComponent<RectTransform>();
        youDiedRT.anchorMin        = new Vector2(0.5f, 0.6f);
        youDiedRT.anchorMax        = new Vector2(0.5f, 0.6f);
        youDiedRT.sizeDelta        = new Vector2(800f, 200f);
        youDiedRT.anchoredPosition = Vector2.zero;

        var youDiedTMP         = youDiedGO.AddComponent<TextMeshProUGUI>();
        youDiedTMP.text        = "You Died!";
        youDiedTMP.fontSize    = 120f;
        youDiedTMP.color       = Color.white;
        youDiedTMP.alignment   = TextAlignmentOptions.Center;
        youDiedTMP.fontStyle   = FontStyles.Bold;

        var youDiedCG            = youDiedGO.AddComponent<CanvasGroup>();
        youDiedCG.alpha          = 0f;
        youDiedCG.interactable   = false;
        youDiedCG.blocksRaycasts = false;

        // ── ButtonContainer ───────────────────────────────────────────────────
        var btnContainerGO = new GameObject("ButtonContainer");
        btnContainerGO.transform.SetParent(fadePanelGO.transform, false);

        var btnContainerRT         = btnContainerGO.AddComponent<RectTransform>();
        btnContainerRT.anchorMin   = new Vector2(0.5f, 0.35f);
        btnContainerRT.anchorMax   = new Vector2(0.5f, 0.35f);
        btnContainerRT.sizeDelta   = new Vector2(300f, 180f);
        btnContainerRT.anchoredPosition = Vector2.zero;

        var vlg                    = btnContainerGO.AddComponent<VerticalLayoutGroup>();
        vlg.spacing                = 20f;
        vlg.childAlignment         = TextAnchor.MiddleCenter;
        vlg.childForceExpandWidth  = true;
        vlg.childForceExpandHeight = false;
        vlg.childControlWidth      = true;
        vlg.childControlHeight     = true;

        var btnContainerCG           = btnContainerGO.AddComponent<CanvasGroup>();
        btnContainerCG.alpha         = 0f;
        btnContainerCG.interactable  = false;
        btnContainerCG.blocksRaycasts = false;

        // ── Restart & Quit Buttons ─────────────────────────────────────────────
        CreateButton(btnContainerGO.transform, "RestartButton", "Restart");
        CreateButton(btnContainerGO.transform, "QuitButton",    "Quit");

        // ── GameOverManager component ──────────────────────────────────────────
        var manager = canvasGO.AddComponent<GameOverManager>();

        // Inspector alanlarını reflection ile set et (EditorUtility yerine SerializedObject kullan)
        var so = new SerializedObject(manager);
        so.FindProperty("gameOverCanvas").objectReferenceValue    = canvasGO;
        so.FindProperty("fadePanel").objectReferenceValue         = fadePanelCG;
        so.FindProperty("youDiedText").objectReferenceValue       = youDiedCG;
        so.FindProperty("buttonContainer").objectReferenceValue   = btnContainerCG;
        so.ApplyModifiedProperties();

        // ── Prefab olarak kaydet ───────────────────────────────────────────────
        PrefabUtility.SaveAsPrefabAssetAndConnect(
            canvasGO,
            "Assets/Prefabs/GameOverCanvas.prefab",
            InteractionMode.AutomatedAction);

        EditorUtility.SetDirty(canvasGO);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        Debug.Log("[BuildGameOverUI] GameOverCanvas oluşturuldu ve prefab kaydedildi.");
    }

    static GameObject CreateButton(Transform parent, string goName, string label)
    {
        var btnGO = new GameObject(goName);
        btnGO.transform.SetParent(parent, false);

        var rt        = btnGO.AddComponent<RectTransform>();
        rt.sizeDelta  = new Vector2(250f, 70f);

        var img       = btnGO.AddComponent<Image>();
        img.color     = new Color(0.15f, 0.15f, 0.15f, 0.85f);

        btnGO.AddComponent<Button>();

        // Text child
        var textGO    = new GameObject("Text");
        textGO.transform.SetParent(btnGO.transform, false);
        var textRT         = textGO.AddComponent<RectTransform>();
        textRT.anchorMin   = Vector2.zero;
        textRT.anchorMax   = Vector2.one;
        textRT.offsetMin   = Vector2.zero;
        textRT.offsetMax   = Vector2.zero;

        var tmp        = textGO.AddComponent<TextMeshProUGUI>();
        tmp.text       = label;
        tmp.fontSize   = 42f;
        tmp.color      = Color.white;
        tmp.alignment  = TextAlignmentOptions.Center;

        return btnGO;
    }
}
