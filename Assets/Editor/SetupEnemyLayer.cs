using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class SetupEnemyLayer
{
    public static void Execute()
    {
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        if (enemyLayer == -1) { Debug.LogError("Enemy layer bulunamadi!"); return; }
        Debug.Log($"Enemy layer index: {enemyLayer}");

        // Tüm NPC karakterleri Enemy layer'a taşı
        string[] npcPaths = {
            "npc_characters/character_1",
            "npc_characters/character_2",
            "npc_characters/character_3",
            "npc_characters/character_4",
            "npc_characters/character_5",
            "npc_characters/character_6",
            "npc_characters/character_1 (1)"
        };

        int count = 0;
        foreach (var path in npcPaths)
        {
            var obj = GameObject.Find(path.Split('/')[1]);
            if (obj == null)
            {
                // Hiyerarşiden bul
                var parent = GameObject.Find("npc_characters");
                if (parent == null) continue;
                foreach (Transform child in parent.transform)
                {
                    if (child.name == path.Split('/')[1])
                    { SetLayerRecursive(child.gameObject, enemyLayer); count++; break; }
                }
            }
            else
            { SetLayerRecursive(obj, enemyLayer); count++; }
        }

        // npc_characters altındaki tümünü bul
        var npcRoot = GameObject.Find("npc_characters");
        if (npcRoot != null)
        {
            SetLayerRecursive(npcRoot, enemyLayer);
            Debug.Log("Tüm NPC'ler Enemy layer'a taşındı");
        }

        // DamageDealer layer mask ayarla
        var dd = Object.FindFirstObjectByType<DamageDealer>();
        if (dd != null)
        {
            var so = new SerializedObject(dd);
            var layerMaskProp = so.FindProperty("enemyLayer");
            layerMaskProp.intValue = 1 << enemyLayer;
            so.ApplyModifiedProperties();
            Debug.Log($"DamageDealer enemyLayer → {LayerMask.LayerToName(enemyLayer)} ({1 << enemyLayer})");
        }
        else
            Debug.LogWarning("DamageDealer bulunamadi — sahneyi Play modunda değil editor'da çalıştırın");

        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("Enemy layer kurulumu tamamlandi!");
    }

    static void SetLayerRecursive(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
            SetLayerRecursive(child.gameObject, layer);
    }
}
