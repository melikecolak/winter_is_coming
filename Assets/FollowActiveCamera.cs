using UnityEngine;

public class FollowActiveCamera : MonoBehaviour
{
    public float heightOffset = 100f;

    void LateUpdate()
    {
        if (Camera.main != null)
        {
            Vector3 pos = Camera.main.transform.position;
            pos.y += heightOffset;
            transform.position = pos;
        }
    }
}