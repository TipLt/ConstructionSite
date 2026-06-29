using UnityEngine;

public class UniversalRotator : MonoBehaviour
{
    public enum RotationAxis
    {
        X,
        Y,
        Z
    }

    public enum RotationMode
    {
        Continuous,
        PingPong
    }

    [Header("Rotation Settings")]
    public RotationAxis rotationAxis = RotationAxis.X;
    public RotationMode rotationMode = RotationMode.Continuous;
    public float rotationSpeed = 180f;

    [Header("Ping Pong Settings")]
    public float limitAngle = 90f;

    Quaternion startRotation;
    float currentAngle;
    float direction = 1f;

    void Start()
    {
        startRotation = transform.localRotation;
    }

    void Update()
    {
        if (rotationMode == RotationMode.Continuous)
            RotateContinuous();
        else
            RotatePingPong();
    }

    void RotateContinuous()
    {
        transform.Rotate(GetAxisVector(), rotationSpeed * Time.deltaTime, Space.Self);
    }

    void RotatePingPong()
    {
        currentAngle += direction * rotationSpeed * Time.deltaTime;

        if (currentAngle > limitAngle)
        {
            currentAngle = limitAngle;
            direction = -1f;
        }
        else if (currentAngle < -limitAngle)
        {
            currentAngle = -limitAngle;
            direction = 1f;
        }

        transform.localRotation = startRotation * Quaternion.AngleAxis(currentAngle, GetAxisVector());
    }

    Vector3 GetAxisVector()
    {
        switch (rotationAxis)
        {
            case RotationAxis.X:
                return Vector3.right;
            case RotationAxis.Y:
                return Vector3.up;
            case RotationAxis.Z:
                return Vector3.forward;
            default:
                return Vector3.right;
        }
    }
}
