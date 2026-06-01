using UnityEngine;
using UnityEngine.InputSystem;

public class CarController : MonoBehaviour
{
    public WheelCollider frontLeft;
    public WheelCollider frontRight;
    public WheelCollider rearLeft;
    public WheelCollider rearRight;

    public float motorForce = 1500f;
    public float steeringAngle = 30f;

    float moveInput;
    float steerInput;

    void Update()
    {
        moveInput = 0f;
        steerInput = 0f;

        if (Keyboard.current.wKey.isPressed)
        {
            moveInput = 1f;
            //Debug.Log("W");
        }

        if (Keyboard.current.sKey.isPressed)
            moveInput = -1f;

        if (Keyboard.current.aKey.isPressed)
            steerInput = -1f;

        if (Keyboard.current.dKey.isPressed)
            steerInput = 1f;
    }

    void FixedUpdate()
    {
        rearLeft.motorTorque = moveInput * motorForce;
        rearRight.motorTorque = moveInput * motorForce;
        //Debug.Log(moveInput * motorForce);
        frontLeft.steerAngle = steerInput * steeringAngle;
        frontRight.steerAngle = steerInput * steeringAngle;
    }


}
