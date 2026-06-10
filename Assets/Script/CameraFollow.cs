using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;

    public Vector3 offset = new Vector3(0, 3, -6);

    void LateUpdate()
    {
        transform.position = target.TransformPoint(offset);

        transform.LookAt(target);
    }


}
