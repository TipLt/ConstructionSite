using UnityEngine;

public class UAVController : MonoBehaviour
{
    [Header("UAV Speed Settings")]
    [SerializeField] private float minSpeed = 15f;
    [SerializeField] private float maxSpeed = 30f;
    [SerializeField] private float acceleration = 5f;

    [Header("Flight Control Responsiveness")]
    [Tooltip("W/S hoặc Up/Down — ngẩng/cúi mũi")]
    [SerializeField] private float pitchSpeed = 60f;

    [Tooltip("A/D hoặc Left/Right — quẹo trái/phải (yaw)")]
    [SerializeField] private float yawSpeed = 40f;

    [Tooltip("Q/E — nghiêng cánh (roll)")]
    [SerializeField] private float rollSpeed = 60f;

    [Header("Debug (Read Only)")]
    [SerializeField] private float currentSpeed;

    // ─────────────────────────────────────────────
    private void Start()
    {
        currentSpeed = minSpeed;
    }

    private void Update()
    {
        HandleThrustInput();
        ExecuteFlightMovement();
    }

    // Space = tăng tốc | Left Ctrl = giảm tốc
    private void HandleThrustInput()
    {
        if (Input.GetKey(KeyCode.Space))
            currentSpeed += acceleration * Time.deltaTime;
        else if (Input.GetKey(KeyCode.LeftControl))
            currentSpeed -= acceleration * Time.deltaTime;

        currentSpeed = Mathf.Clamp(currentSpeed, minSpeed, maxSpeed);
    }

    private void ExecuteFlightMovement()
    {
        // Luôn bay về phía trước theo local Z của UAV_Pivot
        transform.Translate(Vector3.forward * currentSpeed * Time.deltaTime, Space.Self);

        // W/S hoặc Up/Down arrow → Pitch (ngẩng/cúi mũi)
        float pitchInput = Input.GetAxis("Vertical");

        // A/D hoặc Left/Right arrow → Yaw (quẹo trái/phải)
        // Đổi từ Roll sang Yaw để giống game góc thứ ba
        float yawInput = Input.GetAxis("Horizontal");

        // Q → Roll trái | E → Roll phải (nghiêng cánh)
        float rollInput = 0f;
        if (Input.GetKey(KeyCode.E))      rollInput =  1f;
        else if (Input.GetKey(KeyCode.Q)) rollInput = -1f;

        transform.Rotate(Vector3.right * pitchInput * pitchSpeed * Time.deltaTime, Space.Self);
        transform.Rotate(Vector3.up    * yawInput   * yawSpeed   * Time.deltaTime, Space.Self);
        transform.Rotate(Vector3.back  * rollInput  * rollSpeed  * Time.deltaTime, Space.Self);
    }

    public float GetCurrentSpeed() => currentSpeed;
    public float GetMinSpeed()     => minSpeed;
    public float GetMaxSpeed()     => maxSpeed;
}
