using UnityEngine;
using Unity.Cinemachine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance { get; private set; }

    CinemachineBasicMultiChannelPerlin perlin;
    float shakerTimer;
    float shakerTimerTotal;
    float startingIntensity;

    void Awake()
    {
        Instance = this;
        perlin = GetComponent<CinemachineBasicMultiChannelPerlin>();
        if (perlin != null) perlin.AmplitudeGain = 0f; // başlangıçta shake yok
    }

    public void ShakeCamera(float intensity, float time)
    {
        if (perlin == null) return;
        perlin.AmplitudeGain = intensity;
        startingIntensity    = intensity;
        shakerTimerTotal     = time;
        shakerTimer          = time;
    }

    void Update()
    {
        if (shakerTimer <= 0f) return;
        shakerTimer -= Time.deltaTime;
        if (perlin != null)
            perlin.AmplitudeGain = Mathf.Lerp(startingIntensity, 0f,
                1f - shakerTimer / shakerTimerTotal);
    }
}
