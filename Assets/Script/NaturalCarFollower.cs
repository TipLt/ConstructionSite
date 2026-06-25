using System.Collections.Generic;
using UnityEngine;

public class NaturalCarFollower : MonoBehaviour
{
    [Header("Path Settings")]
    public List<Transform> waypoints;
    public float waypointTolerance = 0.5f;
    public float lookAheadDistance = 3f;
    public bool isLooping = true;

    [Header("Movement Settings")]
    public float speed = 5f;
    public float minCurveSpeed = 2f;
    public float accelerationSmoothTime = 0.35f;
    public float turnSpeed = 7f;
    public float slowDownAngle = 45f;

    private int currentWaypointIndex = 0;
    private float currentSpeed;
    private float speedVelocity;

    private void OnValidate()
    {
        waypointTolerance = Mathf.Max(0.05f, waypointTolerance);
        lookAheadDistance = Mathf.Max(0.1f, lookAheadDistance);
        speed = Mathf.Max(0f, speed);
        minCurveSpeed = Mathf.Clamp(minCurveSpeed, 0f, speed);
        accelerationSmoothTime = Mathf.Max(0.01f, accelerationSmoothTime);
        turnSpeed = Mathf.Max(0.01f, turnSpeed);
        slowDownAngle = Mathf.Clamp(slowDownAngle, 1f, 180f);
    }

    private void Update()
    {
        if (waypoints == null || waypoints.Count == 0)
            return;

        MoveCar();
    }

    private void MoveCar()
    {
        AdvanceWaypointIfNeeded();

        Vector3 lookAheadTarget = GetLookAheadTarget();
        Vector3 direction = lookAheadTarget - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.001f)
            return;

        Vector3 desiredDirection = direction.normalized;
        float turnAngle = Vector3.Angle(transform.forward, desiredDirection);
        float curveSpeedFactor = Mathf.InverseLerp(slowDownAngle, 0f, turnAngle);
        float targetSpeed = Mathf.Lerp(minCurveSpeed, speed, curveSpeedFactor);

        currentSpeed = Mathf.SmoothDamp(
            currentSpeed,
            targetSpeed,
            ref speedVelocity,
            accelerationSmoothTime
        );

        Quaternion targetRotation = Quaternion.LookRotation(desiredDirection);
        float rotationLerp = 1f - Mathf.Exp(-turnSpeed * Time.deltaTime);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotationLerp
        );

        transform.position += transform.forward * currentSpeed * Time.deltaTime;
    }

    private void AdvanceWaypointIfNeeded()
    {
        if (currentWaypointIndex >= waypoints.Count)
            currentWaypointIndex = 0;

        Transform targetWaypoint = waypoints[currentWaypointIndex];

        if (targetWaypoint == null)
            return;

        Vector3 toWaypoint = targetWaypoint.position - transform.position;
        toWaypoint.y = 0f;

        bool isCloseEnough = toWaypoint.magnitude <= waypointTolerance;
        bool hasPassedWaypoint = HasPassedCurrentWaypoint(toWaypoint);

        if (!isCloseEnough && !hasPassedWaypoint)
            return;

        currentWaypointIndex++;

        if (currentWaypointIndex >= waypoints.Count)
        {
            if (isLooping)
                currentWaypointIndex = 0;
            else
                enabled = false;
        }
    }

    private bool HasPassedCurrentWaypoint(Vector3 toWaypoint)
    {
        int previousIndex = currentWaypointIndex - 1;

        if (previousIndex < 0)
        {
            if (!isLooping)
                return false;

            previousIndex = waypoints.Count - 1;
        }

        if (waypoints[previousIndex] == null)
            return false;

        Vector3 segmentDirection =
            waypoints[currentWaypointIndex].position - waypoints[previousIndex].position;

        segmentDirection.y = 0f;

        if (segmentDirection.sqrMagnitude <= 0.001f)
            return false;

        return Vector3.Dot(toWaypoint, segmentDirection.normalized) < 0f;
    }

    private Vector3 GetLookAheadTarget()
    {
        float remainingDistance = Mathf.Max(0.1f, lookAheadDistance);
        Vector3 segmentStart = transform.position;

        for (int step = 0; step < waypoints.Count; step++)
        {
            int waypointIndex = (currentWaypointIndex + step) % waypoints.Count;

            if (!isLooping && waypointIndex < currentWaypointIndex)
                break;

            Transform waypoint = waypoints[waypointIndex];

            if (waypoint == null)
                continue;

            Vector3 segmentEnd = waypoint.position;
            segmentEnd.y = segmentStart.y;

            float segmentLength = Vector3.Distance(segmentStart, segmentEnd);

            if (segmentLength >= remainingDistance)
            {
                float t = remainingDistance / segmentLength;
                return Vector3.Lerp(segmentStart, segmentEnd, t);
            }

            remainingDistance -= segmentLength;
            segmentStart = segmentEnd;
        }

        Transform finalWaypoint = waypoints[Mathf.Clamp(currentWaypointIndex, 0, waypoints.Count - 1)];
        return finalWaypoint != null ? finalWaypoint.position : transform.position;
    }

    private void OnDrawGizmos()
    {
        if (waypoints == null || waypoints.Count < 2)
            return;

        Gizmos.color = Color.cyan;

        for (int i = 0; i < waypoints.Count; i++)
        {
            if (waypoints[i] == null)
                continue;

            Gizmos.DrawWireSphere(
                waypoints[i].position,
                waypointTolerance
            );

            if (i < waypoints.Count - 1)
            {
                if (waypoints[i + 1] != null)
                    Gizmos.DrawLine(
                        waypoints[i].position,
                        waypoints[i + 1].position
                    );
            }
            else if (isLooping)
            {
                if (waypoints[0] != null)
                    Gizmos.DrawLine(
                        waypoints[i].position,
                        waypoints[0].position
                    );
            }
        }
    }
}
