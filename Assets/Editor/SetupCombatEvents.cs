using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class SetupCombatEvents
{
    public static void Execute()
    {
        // --- 1. Enemy layer ekle ---
        SerializedObject tagManager = new SerializedObject(
            AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty layers = tagManager.FindProperty("layers");

        bool enemyExists = false;
        for (int i = 0; i < layers.arraySize; i++)
        {
            if (layers.GetArrayElementAtIndex(i).stringValue == "Enemy")
            { enemyExists = true; break; }
        }

        if (!enemyExists)
        {
            // Layer 9'a Enemy yaz
            var layer9 = layers.GetArrayElementAtIndex(9);
            if (string.IsNullOrEmpty(layer9.stringValue))
            {
                layer9.stringValue = "Enemy";
                tagManager.ApplyModifiedProperties();
                Debug.Log("Enemy layer (9) eklendi");
            }
            else
                Debug.LogWarning($"Layer 9 dolu: {layer9.stringValue} — boş bir layer seç");
        }
        else
            Debug.Log("Enemy layer zaten mevcut");

        // --- 2. combat_attack animasyonuna event ekle ---
        AddEventsToClip(
            "Assets/Animations/Pro Sword and Shield Pack/sword and shield slash.fbx",
            "combat_attack", startTime: 0.2f, endTime: 0.65f);

        // --- 3. combat_attack2 animasyonuna event ekle ---
        AddEventsToClip(
            "Assets/Animations/Pro Sword and Shield Pack/sword and shield slash (3).fbx",
            "combat_attack2", startTime: 0.15f, endTime: 0.6f);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Animation event'ler eklendi!");
    }

    static void AddEventsToClip(string fbxPath, string clipName, float startTime, float endTime)
    {
        ModelImporter importer = AssetImporter.GetAtPath(fbxPath) as ModelImporter;
        if (importer == null) { Debug.LogError($"Importer bulunamadi: {fbxPath}"); return; }

        var clipAnimations = new List<ModelImporterClipAnimation>(importer.clipAnimations);
        for (int i = 0; i < clipAnimations.Count; i++)
        {
            if (clipAnimations[i].name != clipName) continue;

            var clip = clipAnimations[i];
            clip.events = new AnimationEvent[]
            {
                new AnimationEvent { time = startTime * (clip.lastFrame - clip.firstFrame) / 30f, functionName = "StartDealDamage" },
                new AnimationEvent { time = endTime   * (clip.lastFrame - clip.firstFrame) / 30f, functionName = "EndDealDamage"   }
            };
            clipAnimations[i] = clip;
            Debug.Log($"{clipName} event'leri eklendi");
            break;
        }
        importer.clipAnimations = clipAnimations.ToArray();
        importer.SaveAndReimport();
    }
}
