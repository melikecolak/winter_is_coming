using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.Linq;

public class SetupCombatLayer
{
    public static void Execute()
    {
        string controllerPath = "Assets/models/ego_character/ego_character_controller.controller";
        var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
        if (controller == null) { Debug.LogError("Controller bulunamadi"); return; }

        // --- Parametre ekle ---
        bool hasAttack = controller.parameters.Any(p => p.name == "attack");
        if (!hasAttack)
        {
            controller.AddParameter("attack", AnimatorControllerParameterType.Trigger);
            Debug.Log("'attack' trigger eklendi");
        }

        // --- Combat Layer ekle (yoksa) ---
        bool hasLayer = controller.layers.Any(l => l.name == "Combat Layer");
        if (!hasLayer)
        {
            controller.AddLayer("Combat Layer");
            Debug.Log("Combat Layer eklendi");
        }

        // Layer weight ve blending ayarla
        var layers = controller.layers;
        int layerIdx = System.Array.FindIndex(layers, l => l.name == "Combat Layer");
        layers[layerIdx].defaultWeight = 1f;
        layers[layerIdx].blendingMode  = AnimatorLayerBlendingMode.Override;
        controller.layers = layers;

        // --- Animasyon kliplerini yükle ---
        string slash1Path = "Assets/Animations/Pro Sword and Shield Pack/sword and shield slash.fbx";
        string slash3Path = "Assets/Animations/Pro Sword and Shield Pack/sword and shield slash (3).fbx";

        AnimationClip clip1 = null, clip2 = null;
        foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(slash1Path))
            if (asset is AnimationClip c && c.name == "combat_attack") { clip1 = c; break; }
        foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(slash3Path))
            if (asset is AnimationClip c && c.name == "combat_attack2") { clip2 = c; break; }

        if (clip1 == null || clip2 == null)
        {
            Debug.LogError($"Klipler bulunamadi: clip1={clip1?.name}, clip2={clip2?.name}");
            return;
        }
        Debug.Log($"Klipler bulundu: {clip1.name}, {clip2.name}");

        // --- State machine'i al ---
        var sm = controller.layers[layerIdx].stateMachine;

        // Varsa eski state'leri temizle
        foreach (var s in sm.states.ToArray())
            sm.RemoveState(s.state);

        // --- State'leri oluştur ---
        var defaultState = sm.AddState("Default State");
        defaultState.motion = null;
        sm.defaultState = defaultState;

        var attackState1 = sm.AddState("Combat Attack");
        attackState1.motion = clip1;
        attackState1.speed  = 1.3f;

        var attackState2 = sm.AddState("Combat Attack 2");
        attackState2.motion = clip2;
        attackState2.speed  = 1.3f;

        // --- Transition'ları kur ---
        var attackParam = new AnimatorCondition { mode = AnimatorConditionMode.If, parameter = "attack", threshold = 0 };
        var moveParam   = new AnimatorCondition { mode = AnimatorConditionMode.If, parameter = "move",   threshold = 0 };

        // Default → Combat Attack 1
        var t1 = defaultState.AddTransition(attackState1);
        t1.hasExitTime = false; t1.duration = 0.1f;
        t1.AddCondition(AnimatorConditionMode.If, 0, "attack");

        // Combat Attack 1 → Combat Attack 2 (kombo)
        var t2 = attackState1.AddTransition(attackState2);
        t2.hasExitTime = false; t2.duration = 0.05f;
        t2.AddCondition(AnimatorConditionMode.If, 0, "attack");

        // Combat Attack 2 → Combat Attack 1 (kombo döngüsü)
        var t3 = attackState2.AddTransition(attackState1);
        t3.hasExitTime = false; t3.duration = 0.05f;
        t3.AddCondition(AnimatorConditionMode.If, 0, "attack");

        // Combat Attack 1 → Default (animasyon bitince, smooth blend)
        var t4 = attackState1.AddTransition(defaultState);
        t4.hasExitTime = true; t4.exitTime = 1f; t4.duration = 0.25f;
        t4.AddCondition(AnimatorConditionMode.If, 0, "move");

        // Combat Attack 2 → Default (animasyon bitince, smooth blend)
        var t5 = attackState2.AddTransition(defaultState);
        t5.hasExitTime = true; t5.exitTime = 1f; t5.duration = 0.25f;
        t5.AddCondition(AnimatorConditionMode.If, 0, "move");

        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
        Debug.Log("Combat Layer kurulumu tamamlandi!");
    }
}
