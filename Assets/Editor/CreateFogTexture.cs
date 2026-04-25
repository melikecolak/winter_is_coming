using UnityEngine;
using UnityEditor;

public class CreateFogTexture : EditorWindow
{
    [MenuItem("Tools/Create Fog Texture")]
    static void Create()
    {
        int size = 256;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Vector2 center = new Vector2(size / 2f, size / 2f);
        float maxDist = size / 2f;

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center);
                float alpha = Mathf.Clamp01(1f - (dist / maxDist));
                alpha = Mathf.Pow(alpha, 1.8f);
                tex.SetPixel(x, y, new Color(1, 1, 1, alpha));
            }
        }

        tex.Apply();
        byte[] bytes = tex.EncodeToPNG();
        string folder = Application.dataPath + "/Textures";
        if (!System.IO.Directory.Exists(folder))
            System.IO.Directory.CreateDirectory(folder);
        System.IO.File.WriteAllBytes(folder + "/T_SoftFog.png", bytes);
        AssetDatabase.Refresh();
        Debug.Log("Fog texture olusturuldu: Assets/Textures/T_SoftFog.png");
    }
}