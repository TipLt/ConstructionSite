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

    public float propellerSpeed = 3000f;

    float forwardInput;
    float verticalInput;
    float yawInput;

    Vector3 currentVelocity;
    Animator anim;

    bool rotorReady;
    float propellerRPM;
    float rpmTarget = 5000f;

    void Start()
    {
        anim = GetComponent<Animator>();
    }
    void Update()
    {

        bool hasInput = forwardInput != 0 || verticalInput != 0 || yawInput != 0;
        bool isGrounded = Physics.Raycast(transform.position + Vector3.up * 0.2f, Vector3.down, 0.5f);
        bool isFlying = hasInput && !isGrounded;
        bool wantsToFly = Keyboard.current.fKey.isPressed || forwardInput != 0 || yawInput != 0 || Keyboard.current.wKey.isPressed;

        forwardInput = 0f;
        verticalInput = 0f;
        yawInput = 0f;

        if (Keyboard.current.wKey.isPressed)
            forwardInput = 1f;

        if (Keyboard.current.sKey.isPressed)
            forwardInput = -1f;

        if (Keyboard.current.fKey.isPressed)
        {
            propellerRPM = Mathf.Lerp(propellerRPM, rpmTarget, Time.deltaTime * 2f);
        }
        else
        {
            propellerRPM = Mathf.Lerp(propellerRPM, 0f, Time.deltaTime * 1.5f);
        }

        rotorReady = propellerRPM >= rpmTarget * 0.85f;

        if (Keyboard.current.xKey.isPressed)
        {
            verticalInput = -1f;
        }
        else if (rotorReady && wantsToFly)
        {
            verticalInput = 1f;
        }
        else
        {
            verticalInput = 0f;
        }

        if (Keyboard.current.aKey.isPressed)
            yawInput = -1f;

        if (Keyboard.current.dKey.isPressed)
            yawInput = 1f;

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


        anim.SetBool("IsFlying", isFlying);
        anim.SetBool("IsGround", isGrounded);
        anim.SetBool("WantToFly", wantsToFly);
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
