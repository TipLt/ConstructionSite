using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkerManager : MonoBehaviour
{
    [Header("References")]
    public RunController runController;
    public Transform busRoot;
    public Transform busDoorPoint;
    public Transform dropOffPoint;
    public Transform targetPoint;
    public List<Transform> workers = new List<Transform>();

    [Header("Worker Movement")]
    public float workerMoveSpeed = 2f;
    public float boardDelay = 0.25f;
    public float dropOffDelay = 0.25f;
    public float targetSpacing = 1f;

    [Header("Inside Bus")]
    public Vector3 firstSeatLocalPosition = Vector3.zero;
    public Vector3 seatSpacingLocal = new Vector3(0.4f, 0f, 0f);
    public bool hideWorkersInsideBus = true;

    readonly List<Transform> workersInsideBus = new List<Transform>();

    void Awake()
    {
        if (runController == null)
            runController = GetComponent<RunController>();

        if (runController == null)
            runController = FindObjectOfType<RunController>();
    }

    void OnEnable()
    {
        if (runController == null)
            return;

        runController.OnArrivedBusStation += HandleBusStationArrival;
        runController.OnArrivedDropOffPoint += HandleDropOffArrival;
    }

    void OnDisable()
    {
        if (runController == null)
            return;

        runController.OnArrivedBusStation -= HandleBusStationArrival;
        runController.OnArrivedDropOffPoint -= HandleDropOffArrival;
    }

    void HandleBusStationArrival()
    {
        StartCoroutine(BoardWorkers());
    }

    void HandleDropOffArrival()
    {
        StartCoroutine(DropOffWorkers());
    }

    IEnumerator BoardWorkers()
    {
        foreach (Transform worker in workers)
        {
            if (worker == null)
                continue;

            worker.gameObject.SetActive(true);

            Vector3 doorPosition = GetPositionOrFallback(busDoorPoint, runController.transform.position);
            yield return MoveTransform(worker, doorPosition);

            worker.SetParent(busRoot != null ? busRoot : runController.transform);
            worker.localPosition = firstSeatLocalPosition + seatSpacingLocal * workersInsideBus.Count;
            workersInsideBus.Add(worker);

            if (hideWorkersInsideBus)
                worker.gameObject.SetActive(false);

            yield return new WaitForSeconds(boardDelay);
        }

        runController.ContinueRoute();
    }

    IEnumerator DropOffWorkers()
    {
        for (int i = 0; i < workersInsideBus.Count; i++)
        {
            Transform worker = workersInsideBus[i];
            if (worker == null)
                continue;

            Vector3 exitPosition = GetPositionOrFallback(dropOffPoint, runController.transform.position);
            worker.SetParent(null);
            worker.position = exitPosition;
            worker.gameObject.SetActive(true);

            StartCoroutine(WalkToTarget(worker, i));

            yield return new WaitForSeconds(dropOffDelay);
        }

        workersInsideBus.Clear();
        runController.ContinueRoute();
    }

    IEnumerator WalkToTarget(Transform worker, int workerIndex)
    {
        Vector3 finalPosition = GetTargetPosition(workerIndex);
        yield return MoveTransform(worker, finalPosition);
    }

    IEnumerator MoveTransform(Transform movingObject, Vector3 destination)
    {
        while (movingObject != null && Vector3.Distance(movingObject.position, destination) > 0.05f)
        {
            movingObject.position = Vector3.MoveTowards(
                movingObject.position,
                destination,
                workerMoveSpeed * Time.deltaTime
            );

            Vector3 direction = destination - movingObject.position;
            direction.y = 0f;

            if (direction.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                movingObject.rotation = Quaternion.Lerp(movingObject.rotation, targetRotation, 8f * Time.deltaTime);
            }

            yield return null;
        }
    }

    Vector3 GetTargetPosition(int workerIndex)
    {
        Vector3 center = GetPositionOrFallback(targetPoint, transform.position);

        if (targetSpacing <= 0f)
            return center;

        int row = workerIndex / 3;
        int column = workerIndex % 3;
        Vector3 offset = new Vector3((column - 1) * targetSpacing, 0f, row * targetSpacing);

        return center + offset;
    }

    Vector3 GetPositionOrFallback(Transform point, Vector3 fallback)
    {
        return point != null ? point.position : fallback;
    }
}
