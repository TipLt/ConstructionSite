using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("Kéo Transform của UAV_Pivot vào đây")]
    [SerializeField] private Transform uavTarget;

    [Header("Offset Settings")]
    [SerializeField] private float followDistance = 12f;
    [SerializeField] private float followHeight   = 4f;

    [Header("Smoothing")]
    [SerializeField] private float positionSmoothSpeed = 6f;
    [SerializeField] private float rotationSmoothSpeed = 8f;

    [Header("Look Target")]
    [SerializeField] private float lookAheadDistance = 8f;

    private void LateUpdate()
    {
        if (uavTarget == null) return;
        FollowUAV();
    }

    private void FollowUAV()
    {
        Vector3 desiredPosition = uavTarget.position
            - uavTarget.forward * followDistance
            + Vector3.up        * followHeight;

        transform.position = Vector3.Lerp(
            transform.position,
            desiredPosition,
            positionSmoothSpeed * Time.deltaTime
        );

        Vector3 lookAtPoint  = uavTarget.position + uavTarget.forward * lookAheadDistance;
        Vector3 lookDirection = lookAtPoint - transform.position;

        if (lookDirection != Vector3.zero)
        {
            Quaternion desiredRotation = Quaternion.LookRotation(lookDirection, Vector3.up);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                desiredRotation,
                rotationSmoothSpeed * Time.deltaTime
            );
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (uavTarget == null) return;
        Gizmos.color = Color.cyan;
        Vector3 camPos = uavTarget.position
            - uavTarget.forward * followDistance
            + Vector3.up        * followHeight;
        Gizmos.DrawSphere(camPos, 0.4f);
        Gizmos.DrawLine(camPos, uavTarget.position);
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(uavTarget.position + uavTarget.forward * lookAheadDistance, 0.3f);
    }
#endif
}
