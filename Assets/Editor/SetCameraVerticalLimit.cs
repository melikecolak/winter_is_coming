using UnityEngine;
using Unity.Cinemachine;

public class SetCameraVerticalLimit
{
    public static void Execute()
    {
        var camObj = GameObject.Find("PlayerCamera");
        if (camObj == null) { Debug.LogError("PlayerCamera bulunamadı"); return; }

        // Deoccluder'ı kaldır
        var deoccluder = camObj.GetComponent<CinemachineDeoccluder>();
        if (deoccluder != null)
        {
            UnityEngine.Object.DestroyImmediate(deoccluder);
            Debug.Log("CinemachineDeoccluder kaldırıldı");
        }

        // Dikey açıyı dengeli değere döndür
        var orbital = camObj.GetComponent<CinemachineOrbitalFollow>();
        if (orbital == null) { Debug.LogError("CinemachineOrbitalFollow bulunamadı"); return; }

        var axis   = orbital.VerticalAxis;
        axis.Range = new Vector2(-10f, 60f);
        orbital.VerticalAxis = axis;
        Debug.Log($"Yeni VerticalAxis Range: {orbital.VerticalAxis.Range}");
    }
}
