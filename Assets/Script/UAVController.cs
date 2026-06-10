using UnityEngine;
using UnityEngine.InputSystem;

public class UAVController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 6f;
    public float verticalSpeed = 4f;
    public float acceleration = 4f;


    [Header("Rotation")]
    public float yawSpeed = 50f;

    [Header("Tilt")]
    public float tiltAngle = 12f;
    public float tiltSpeed = 5f;

    [Header("Collision")]
    public Vector3 boxCastHalfExtents = new Vector3(0.5f, 0.2f, 0.5f);

    [Header("Propellers")]
    public Transform prop1;
    public Transform prop2;
    public Transform prop3;
    public Transform prop4;

    public float propellerSpeed = 3000f;

    float forwardInput;
    float verticalInput;
    float yawInput;

    Vector3 currentVelocity;
    Animator anim;

    void Start()
    {
        anim = GetComponent<Animator>();
    }
    void Update()
    {
        forwardInput = 0f;
        verticalInput = 0f;
        yawInput = 0f;

        if (Keyboard.current.wKey.isPressed)
            forwardInput = 1f;

        if (Keyboard.current.sKey.isPressed)
            forwardInput = -1f;

        if (Keyboard.current.fKey.isPressed)
            verticalInput = 1f;

        if (Keyboard.current.xKey.isPressed)
            verticalInput = -1f;

        if (Keyboard.current.aKey.isPressed)
            yawInput = -1f;

        if (Keyboard.current.dKey.isPressed)
            yawInput = 1f;

        prop1.Rotate(0, propellerSpeed * Time.deltaTime, 0);
        prop2.Rotate(0, -propellerSpeed * Time.deltaTime, 0);
        prop3.Rotate(0, propellerSpeed * Time.deltaTime, 0);
        prop4.Rotate(0, -propellerSpeed * Time.deltaTime, 0);

        float pitch = -forwardInput * tiltAngle;

        Quaternion targetRotation =
            Quaternion.Euler(
                pitch,
                transform.eulerAngles.y,
                0f
            );

        transform.rotation = Quaternion.Lerp(
            transform.rotation,
            targetRotation,
            tiltSpeed * Time.deltaTime
        );

        transform.Rotate(
            0f,
            yawInput * yawSpeed * Time.deltaTime,
            0f,
            Space.World
        );

        bool isFlying =forwardInput != 0 ||verticalInput != 0 ||yawInput != 0;

        anim.SetBool("IsFlying", isFlying);
    }

    void FixedUpdate()
    {
        Vector3 targetVelocity =
            transform.forward * forwardInput * moveSpeed +
            Vector3.up * verticalInput * verticalSpeed;

        currentVelocity = Vector3.Lerp(
            currentVelocity,
            targetVelocity,
            acceleration * Time.fixedDeltaTime
        );

        Vector3 moveDelta =
            currentVelocity * Time.fixedDeltaTime;

        if (moveDelta.magnitude < 0.001f)
            return;

        bool blocked = Physics.BoxCast(
            transform.position,
            boxCastHalfExtents,
            moveDelta.normalized,
            out RaycastHit hit,
            transform.rotation,
            moveDelta.magnitude
        );

        if (!blocked)
        {
            transform.position += moveDelta;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;

        Gizmos.matrix =
            Matrix4x4.TRS(
                transform.position,
                transform.rotation,
                Vector3.one
            );

        Gizmos.DrawWireCube(
            Vector3.zero,
            boxCastHalfExtents * 2f
        );
    }

}
