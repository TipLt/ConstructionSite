using System;
using UnityEngine;

public class RunController : MonoBehaviour
{
    [Header("Route")]
    public Transform[] waypoints;
    public float moveSpeed = 5f;
    public float rotateSpeed = 5f;
    public float stoppingDistance = 0.2f;
    public bool autoStart = true;

    [Header("Special Stops")]
    public int busStationWaypointIndex = 0;
    public int dropOffWaypointIndex = 1;

    public event Action OnArrivedBusStation;
    public event Action OnArrivedDropOffPoint;

    public bool IsStopped => isStopped;
    public bool IsRouteFinished => isRouteFinished;

    int currentWaypointIndex;
    bool isRunning;
    bool isStopped;
    bool isRouteFinished;
    bool busStationHandled;
    bool dropOffHandled;

    void Start()
    {
        isRunning = autoStart;
    }

    void Update()
    {
        if (!isRunning || isStopped || isRouteFinished || waypoints == null || waypoints.Length == 0)
            return;

        MoveToCurrentWaypoint();
    }

    public void StartRoute()
    {
        if (isRouteFinished)
            return;

        isRunning = true;
        isStopped = false;
    }

    public void StopRoute()
    {
        isStopped = true;
    }

    public void ContinueRoute()
    {
        if (isRouteFinished)
            return;

        isRunning = true;
        isStopped = false;
        AdvanceWaypoint();
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
        direction.y = 0f;

        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotateSpeed * Time.deltaTime);
        }

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            moveSpeed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, targetPosition) <= stoppingDistance)
            ArriveAtWaypoint();
    }

    void ArriveAtWaypoint()
    {
        if (currentWaypointIndex == busStationWaypointIndex && !busStationHandled)
        {
            busStationHandled = true;
            isStopped = true;
            OnArrivedBusStation?.Invoke();
            return;
        }

        if (currentWaypointIndex == dropOffWaypointIndex && !dropOffHandled)
        {
            dropOffHandled = true;
            isStopped = true;
            OnArrivedDropOffPoint?.Invoke();
            return;
        }

        AdvanceWaypoint();
    }

    void AdvanceWaypoint()
    {
        currentWaypointIndex++;

        if (currentWaypointIndex >= waypoints.Length)
        {
            isRouteFinished = true;
            isRunning = false;
            isStopped = true;
        }
    }

    void OnDrawGizmos()
    {
        if (waypoints == null)
            return;

        Gizmos.color = Color.yellow;

        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] == null)
                continue;

            Gizmos.DrawSphere(waypoints[i].position, 0.25f);

            if (i + 1 < waypoints.Length && waypoints[i + 1] != null)
                Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
        }
    }
}
