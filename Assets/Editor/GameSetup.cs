using System.Linq;
using UnityEditor;
using UnityEngine;

public class GameSetup
{
    const int PlayerLayer = 8;
    const int EnemyLayer  = 6; // projede Enemy zaten Layer 6'da

    const string AttackFBX =
        "Assets/Animations/Kevin Iglesias/Human Animations/Animations/Male/Combat/1H/HumanM@Attack1H01_R.fbx";

    // ── Menu ────────────────────────────────────────────────────────────

    [MenuItem("WinterIsComing/1 - Layer Ayarları")]
    static void SetupLayers()
    {
        EnsureLayerExists(PlayerLayer, "Player");
        EnsureLayerExists(EnemyLayer,  "Enemy");
        Debug.Log("[Setup] Layers OK: 8=Player, 9=Enemy");
    }

    [MenuItem("WinterIsComing/2 - Obje Layerları")]
    static void SetObjectLayers()
    {
        // Player → Layer 8
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            player.layer = PlayerLayer;
            EditorUtility.SetDirty(player);
            Debug.Log("[Setup] Player → Layer 8 (Player)");
        }
        else Debug.LogError("[Setup] 'Player' tag'ı olan obje bulunamadı!");

        // character_5 → Layer 9
        var enemy = GameObject.Find("character_5");
        if (enemy != null)
        {
            enemy.layer = EnemyLayer;
            EditorUtility.SetDirty(enemy);
            Debug.Log("[Setup] character_5 → Layer 9 (Enemy)");
        }
        else Debug.LogError("[Setup] 'character_5' sahnede bulunamadı!");
    }

    [MenuItem("WinterIsComing/3 - LayerMask Ayarları")]
    static void SetLayerMasks()
    {
        // DamageDealer (player silahı): enemyLayer = Layer 9
        int set1 = 0;
        foreach (var dd in Object.FindObjectsByType<DamageDealer>(FindObjectsSortMode.None))
        {
            var so = new SerializedObject(dd);
            so.FindProperty("enemyLayer").intValue = 1 << EnemyLayer;
            so.ApplyModifiedProperties();
            set1++;
        }
        Debug.Log($"[Setup] {set1} DamageDealer → enemyLayer = Layer 9 (Enemy)");

        // EnemyDamageDealer (düşman silahı): playerLayer = Layer 8
        int set2 = 0;
        foreach (var ed in Object.FindObjectsByType<EnemyDamageDealer>(FindObjectsSortMode.None))
        {
            var so = new SerializedObject(ed);
            so.FindProperty("playerLayer").intValue = 1 << PlayerLayer;
            so.ApplyModifiedProperties();
            set2++;
        }
        Debug.Log($"[Setup] {set2} EnemyDamageDealer → playerLayer = Layer 8 (Player)");
    }

    [MenuItem("WinterIsComing/4 - Attack Animation Events")]
    static void AddAnimationEvents()
    {
        var importer = AssetImporter.GetAtPath(AttackFBX) as ModelImporter;
        if (importer == null)
        {
            Debug.LogError($"[Setup] FBX bulunamadı: {AttackFBX}");
            return;
        }

        // Clip süresini öğren
        var clip = AssetDatabase.LoadAllAssetsAtPath(AttackFBX)
            .OfType<AnimationClip>()
            .FirstOrDefault(c => !c.name.StartsWith("__preview"));

        if (clip == null) { Debug.LogError("[Setup] Attack clip bulunamadı"); return; }

        float len = clip.length;
        Debug.Log($"[Setup] Attack clip süresi: {len:F3}s");

        // Mevcut import ayarları
        var clips = importer.clipAnimations.Length > 0
            ? importer.clipAnimations
            : importer.defaultClipAnimations;

        if (clips.Length == 0) { Debug.LogError("[Setup] clipAnimations boş"); return; }

        // Events: %25 → StartDealDamage, %70 → EndDealDamage
        clips[0].events = new[]
        {
            new AnimationEvent { functionName = "StartDealDamage", time = len * 0.25f },
            new AnimationEvent { functionName = "EndDealDamage",   time = len * 0.70f }
        };

        importer.clipAnimations = clips;
        importer.SaveAndReimport();

        Debug.Log($"[Setup] Events eklendi → StartDealDamage@{len*0.25f:F2}s, EndDealDamage@{len*0.70f:F2}s");
        Debug.Log("[Setup] İnce ayar için: Animation clip seç → Events sekmesi");
    }

    [MenuItem("WinterIsComing/0 - REVERT (Geri Al)")]
    static void Revert()
    {
        // Animation events'leri temizle
        var importer = AssetImporter.GetAtPath(AttackFBX) as ModelImporter;
        if (importer != null)
        {
            var clips = importer.clipAnimations.Length > 0
                ? importer.clipAnimations
                : importer.defaultClipAnimations;

            if (clips.Length > 0)
            {
                clips[0].events = new AnimationEvent[0];
                importer.clipAnimations = clips;
                importer.SaveAndReimport();
                Debug.Log("[Revert] Attack animation events temizlendi");
            }
        }

        // LayerMask'ları sıfırla
        foreach (var dd in Object.FindObjectsByType<DamageDealer>(FindObjectsSortMode.None))
        {
            var so = new SerializedObject(dd);
            so.FindProperty("enemyLayer").intValue = 0;
            so.ApplyModifiedProperties();
        }
        foreach (var ed in Object.FindObjectsByType<EnemyDamageDealer>(FindObjectsSortMode.None))
        {
            var so = new SerializedObject(ed);
            so.FindProperty("playerLayer").intValue = 0;
            so.ApplyModifiedProperties();
        }

        AssetDatabase.SaveAssets();
        Debug.Log("[Revert] Tüm değişiklikler geri alındı.");
    }

    [MenuItem("WinterIsComing/5 - Hepsini Çalıştır")]
    static void RunAll()
    {
        SetupLayers();
        SetObjectLayers();
        SetLayerMasks();
        AddAnimationEvents();
        AssetDatabase.SaveAssets();
        Debug.Log("=== TÜM KURULUM TAMAMLANDI ===");
    }

    // ── Yardımcı ────────────────────────────────────────────────────────

    static void EnsureLayerExists(int index, string name)
    {
        var tagManager = new SerializedObject(
            AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        var layers = tagManager.FindProperty("layers");
        var element = layers.GetArrayElementAtIndex(index);

        if (string.IsNullOrEmpty(element.stringValue))
        {
            element.stringValue = name;
            tagManager.ApplyModifiedProperties();
            Debug.Log($"[Setup] Layer {index} = '{name}' oluşturuldu");
        }
        else
        {
            Debug.Log($"[Setup] Layer {index} = '{element.stringValue}' (zaten var)");
        }
    }
}
