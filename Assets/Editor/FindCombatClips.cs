using UnityEngine;
using UnityEditor;

public class FindCombatClips
{
    public static void Execute()
    {
        string[] guids = AssetDatabase.FindAssets("t:AnimationClip");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var clips = AssetDatabase.LoadAllAssetsAtPath(path);
            foreach (var asset in clips)
            {
                if (asset is AnimationClip clip)
                {
                    string name = clip.name.ToLower();
                    if (name.Contains("combat") || name.Contains("attack"))
                        Debug.Log($"CLIP: {clip.name} | PATH: {path}");
                }
            }
        }
    }
}
