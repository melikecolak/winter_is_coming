using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class InteractionSystem : MonoBehaviour
{
    public bool isTorchLit         { get; private set; }
    public int  torchUsesRemaining { get; private set; }

    private const int MaxTorchUses = 3;

    private enum InteractionType { None, PickupTorch, LightTorch, BurnCorpse }
    private InteractionType currentInteraction = InteractionType.None;

    private GameObject torchPickup;
    private GameObject torchInHand;
    private Enemy      currentCorpse;
    private TextMeshProUGUI promptText;
    private InputAction     interactAction;

    void Awake()
    {
        interactAction = new InputAction("Interact", InputActionType.Button);
        interactAction.AddBinding("<Keyboard>/e");
        interactAction.Enable();
    }

    void OnDestroy()
    {
        interactAction?.Disable();
        interactAction?.Dispose();
    }

    void Start()
    {
        torchPickup = GameObject.Find("TorchHighPoly");
        torchInHand = FindInChildrenIncludeInactive(transform, "TorchInHand");

        SetupPromptUI();

        if (torchPickup == null) Debug.LogWarning("[InteractionSystem] 'TorchHighPoly' sahnede bulunamadi.");
        if (torchInHand == null) Debug.LogWarning("[InteractionSystem] Player hiyerarşisinde 'TorchInHand' bulunamadi.");
    }

    void Update()
    {
        // Düşman dirildiyse veya başkası tarafından yakıldıysa promptu temizle
        if (currentInteraction == InteractionType.BurnCorpse &&
            (currentCorpse == null || !currentCorpse.IsDead || currentCorpse.IsBurned))
        {
            currentCorpse      = null;
            currentInteraction = InteractionType.None;
            HidePrompt();
        }

        if (currentInteraction != InteractionType.None && interactAction.WasPressedThisFrame())
            ExecuteInteraction();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (IsFromObject(other, "TorchHighPoly"))
        {
            currentInteraction = InteractionType.PickupTorch;
            ShowPrompt("[E] Pick Up");
            return;
        }

        if (IsFromObject(other, "Fireplace_01_stone"))
        {
            if (torchInHand != null && torchInHand.activeSelf)
            {
                currentInteraction = InteractionType.LightTorch;
                ShowPrompt("[E] Light Torch");
            }
            return;
        }

        // Ölü NPC cesedi mi? (Die() içinde CapsuleCollider isTrigger=true yapılıyor)
        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy != null && enemy.IsDead && !enemy.IsBurned && isTorchLit)
        {
            currentCorpse      = enemy;
            currentInteraction = InteractionType.BurnCorpse;
            ShowPrompt("[E] Burn");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (IsFromObject(other, "TorchHighPoly") && currentInteraction == InteractionType.PickupTorch)
        {
            currentInteraction = InteractionType.None;
            HidePrompt();
            return;
        }

        if (IsFromObject(other, "Fireplace_01_stone") && currentInteraction == InteractionType.LightTorch)
        {
            currentInteraction = InteractionType.None;
            HidePrompt();
            return;
        }

        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy != null && enemy == currentCorpse)
        {
            currentCorpse      = null;
            currentInteraction = InteractionType.None;
            HidePrompt();
        }
    }

    private void ExecuteInteraction()
    {
        switch (currentInteraction)
        {
            case InteractionType.PickupTorch: PickUpTorch(); break;
            case InteractionType.LightTorch:  LightTorch();  break;
            case InteractionType.BurnCorpse:  BurnCorpse();  break;
        }
    }

    // ── Meşale Alma ───────────────────────────────────────────────────────

    private void PickUpTorch()
    {
        if (torchPickup != null) torchPickup.SetActive(false);
        if (torchInHand != null) torchInHand.SetActive(true);

        currentInteraction = InteractionType.None;
        HidePrompt();
    }

    // ── Meşale Yakma ──────────────────────────────────────────────────────

    private void LightTorch()
    {
        if (torchInHand == null) return;

        GameObject vfx = FindInChildrenIncludeInactive(torchInHand.transform, "VFX_Fire_01_Small");
        if (vfx != null)
            vfx.SetActive(true);
        else
            Debug.LogWarning("[InteractionSystem] TorchInHand altında 'VFX_Fire_01_Small' bulunamadi.");

        isTorchLit         = true;
        torchUsesRemaining = MaxTorchUses; // her şömine etkileşiminde sayaç sıfırlanır

        currentInteraction = InteractionType.None;
        HidePrompt();
    }

    // ── Ceset Yakma ───────────────────────────────────────────────────────

    private void BurnCorpse()
    {
        if (currentCorpse == null) return;

        // NPC üzerindeki vfx_fire child objesini aktif et
        GameObject vfx = FindInChildrenIncludeInactive(currentCorpse.transform, "VFX_Fire_Floor_02_Simple");
        if (vfx != null)
        {
            vfx.transform.position = currentCorpse.transform.position + Vector3.up * 0.3f;
            vfx.SetActive(true);
        }
        else
            Debug.LogWarning("[InteractionSystem] Enemy üzerinde 'VFX_Fire_Floor_02_Simple' bulunamadi.");

        currentCorpse.Burn(); // 4 saniye sonra Destroy çağrılır

        torchUsesRemaining--;
        if (torchUsesRemaining <= 0)
            ExtinguishTorch();

        currentCorpse      = null;
        currentInteraction = InteractionType.None;
        HidePrompt();
    }

    private void ExtinguishTorch()
    {
        if (torchInHand != null)
        {
            GameObject vfx = FindInChildrenIncludeInactive(torchInHand.transform, "VFX_Fire_01_Small");
            if (vfx != null) vfx.SetActive(false);
        }
        isTorchLit = false;
    }

    // ── UI ────────────────────────────────────────────────────────────────

    private void ShowPrompt(string text)
    {
        if (promptText == null) return;
        promptText.text = text;
        promptText.gameObject.SetActive(true);
    }

    private void HidePrompt()
    {
        if (promptText == null) return;
        promptText.gameObject.SetActive(false);
    }

    private void SetupPromptUI()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogWarning("[InteractionSystem] Sahnede Canvas bulunamadi, UI prompt oluşturulamadi.");
            return;
        }

        Transform existing = canvas.transform.Find("InteractionPrompt");
        if (existing != null)
        {
            promptText = existing.GetComponent<TextMeshProUGUI>();
            promptText.gameObject.SetActive(false);
            return;
        }

        GameObject promptGO = new GameObject("InteractionPrompt");
        promptGO.transform.SetParent(canvas.transform, false);

        var rect = promptGO.AddComponent<RectTransform>();
        rect.anchorMin        = new Vector2(0.5f, 0f);
        rect.anchorMax        = new Vector2(0.5f, 0f);
        rect.pivot            = new Vector2(0.5f, 0f);
        rect.anchoredPosition = new Vector2(0f, 80f);
        rect.sizeDelta        = new Vector2(400f, 60f);

        promptText           = promptGO.AddComponent<TextMeshProUGUI>();
        promptText.text      = "[E] Interact";
        promptText.fontSize  = 30f;
        promptText.fontStyle = FontStyles.Bold;
        promptText.alignment = TextAlignmentOptions.Center;
        promptText.color     = new Color(1f, 0.95f, 0.7f);

        promptGO.SetActive(false);
    }

    // ── Yardımcılar ───────────────────────────────────────────────────────

    private static bool IsFromObject(Collider other, string objectName)
    {
        Transform t = other.transform;
        while (t != null)
        {
            if (t.name == objectName) return true;
            t = t.parent;
        }
        return false;
    }

    private static GameObject FindInChildrenIncludeInactive(Transform root, string childName)
    {
        foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
        {
            if (t.name == childName) return t.gameObject;
        }
        return null;
    }
}
