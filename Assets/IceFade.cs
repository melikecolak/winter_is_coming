using UnityEngine;

public class IceFade : MonoBehaviour
{
    public float fadeTime = 1.5f;
    private float timer = 0f;
    private Renderer rend;
    private Material mat;

    private void Start()
    {
        rend = GetComponent<Renderer>();
        // Materyalin kopyasını al, orijinali bozma
        mat = new Material(rend.material);
        rend.material = mat;
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer >= fadeTime)
        {
            float alpha = Mathf.Lerp(1f, 0f, (timer - fadeTime) / 2f);
            Color c = mat.color;
            c.a = alpha;
            mat.color = c;

            if (alpha <= 0f)
                Destroy(gameObject);
        }
    }
}
