using UnityEngine;

public class FogWindController : MonoBehaviour
{
    [Header("Particle Systems")]
    public ParticleSystem groundFogPS;

    [Header("Wind")]
    public WindZone windZone;

    void Update()
    {
        if (windZone == null || groundFogPS == null) return;

        Vector3 windDir = windZone.transform.forward;
        float windStrength = windZone.windMain;

        float spread = 0.6f;
        var vel = groundFogPS.velocityOverLifetime;

        vel.x = new ParticleSystem.MinMaxCurve(
            windDir.x * windStrength - spread,
            windDir.x * windStrength + spread);

        vel.z = new ParticleSystem.MinMaxCurve(
            windDir.z * windStrength - spread,
            windDir.z * windStrength + spread);
    }
}