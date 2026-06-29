using System.Collections.Generic;
using UnityEngine;

public class WaypointMover : MonoBehaviour
{
    [Header("Waypoints")]
    public List<Transform> waypoints = new List<Transform>();
    public bool isLooping = true;

    [Header("Movement")]
    public float moveSpeed = 5f;
    public float stoppingDistance = 0.05f;
    public bool autoStart = true;

    [Header("Rotation")]
    public bool rotateToMoveDirection = true;
    public float rotateSpeed = 8f;

    int currentWaypointIndex;
    bool isMoving;

    void Start()
    {
        isMoving = autoStart;
    }

    void Update()
    {
        if (!isMoving || waypoints == null || waypoints.Count == 0)
            return;

        MoveToCurrentWaypoint();
    }

    public void StartMoving()
    {
        isMoving = true;
    }

    public void StopMoving()
    {
        isMoving = false;
    }

    void MoveToCurrentWaypoint()
    {
        Transform targetWaypoint = waypoints[currentWaypointIndex];
        if (targetWaypoint == null)
        {
            AdvanceWaypoint();
            return;
        }

        Vector3 targetPosition = targetWaypoint.position;
        Vector3 direction = targetPosition - transform.position;

        if (rotateToMoveDirection)
            RotateToDirection(direction);

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            moveSpeed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, targetPosition) <= stoppingDistance)
            AdvanceWaypoint();
    }

    void RotateToDirection(Vector3 direction)
    {
        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.001f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotateSpeed * Time.deltaTime);
    }

    void AdvanceWaypoint()
    {
        currentWaypointIndex++;

        if (currentWaypointIndex < waypoints.Count)
            return;

        if (isLooping)
        {
            currentWaypointIndex = 0;
            return;
        }

        currentWaypointIndex = waypoints.Count - 1;
        isMoving = false;
    }

    void OnDrawGizmos()
    {
        if (waypoints == null)
            return;

        Gizmos.color = Color.cyan;

        for (int i = 0; i < waypoints.Count; i++)
        {
            if (waypoints[i] == null)
                continue;

            Gizmos.DrawSphere(waypoints[i].position, 0.2f);

            int nextIndex = i + 1;
            if (nextIndex < waypoints.Count && waypoints[nextIndex] != null)
                Gizmos.DrawLine(waypoints[i].position, waypoints[nextIndex].position);
        }

        if (isLooping && waypoints.Count > 1 && waypoints[0] != null && waypoints[waypoints.Count - 1] != null)
            Gizmos.DrawLine(waypoints[waypoints.Count - 1].position, waypoints[0].position);
    }
}
