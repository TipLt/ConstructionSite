using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TruckController : MonoBehaviour
{
    [Header("1. Hành Vi Di Chuyển (Movement)")]
    public Transform[] waypoints;
    public float moveSpeed = 5f;
    public float turnSpeed = 8f;
    public float stoppingDistance = 0.2f;

    [Header("2. Trạm Dừng (Zones by Waypoint Index)")]
    public int loadingWaypointIndex = 0;   // Điểm đến để bốc hàng
    public int unloadingWaypointIndex = 2; // Điểm đến để dỡ hàng

    [Header("3. Cài Đặt Hàng Hóa (Cargo Settings)")]
    public Transform cargoHold;            // Vị trí thùng xe tải để gắn đồ
    public List<Transform> itemsOnGround;  // Danh sách đồ vật đang nằm ở kho
    public int maxLoadPerTrip = 3;         // Bốc tối đa 3 món mỗi chuyến
    
    [Header("Cài Đặt Quỹ Đạo Bay (Parabola)")]
    public float jumpHeight = 2.5f;        // Độ cao của vòng cung
    public float jumpDuration = 0.5f;      // Thời gian bay của mỗi món đồ
    public float delayBetweenItems = 0.3f; // Khựng lại một chút giữa các món

    [Header("4. Cài Đặt Dỡ Hàng (Unload Settings)")]
    public Transform unloadStartPoint;     // Điểm rớt đồ đầu tiên ở công trường
    public Vector3 spacingOffset = new Vector3(1.5f, 0, 0); // Khoảng cách giãn đồ (xếp thành hàng ngang)

    // Các biến nội bộ để theo dõi trạng thái
    private List<Transform> loadedItems = new List<Transform>();
    private int currentWaypointIndex = 0;
    private int totalUnloadedCount = 0; // Đếm tổng số đồ đã dỡ để tránh rơi đè lên nhau
    private bool isWorking = false;     // Đang bốc/dỡ đồ thì xe không chạy

    void Update()
    {
        // Nếu xe đang bốc/dỡ hàng hoặc chưa có đường đi -> Không chạy
        if (isWorking || waypoints == null || waypoints.Length == 0) return;

        MoveToWaypoint();
    }

    void MoveToWaypoint()
    {
        Transform target = waypoints[currentWaypointIndex];
        if (target == null) return;

        // Xoay đầu xe mượt mà
        Vector3 direction = target.position - transform.position;
        direction.y = 0;
        if (direction.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
        }

        // Tịnh tiến xe
        transform.position = Vector3.MoveTowards(transform.position, target.position, moveSpeed * Time.deltaTime);

        // Kiểm tra đến nơi chưa
        if (Vector3.Distance(transform.position, target.position) <= stoppingDistance)
        {
            HandleArrival();
        }
    }

    void HandleArrival()
    {
        // Kiểm tra xem trạm này là bốc hay dỡ hàng
        if (currentWaypointIndex == loadingWaypointIndex && loadedItems.Count == 0 && itemsOnGround.Count > 0)
        {
            StartCoroutine(LoadCargoRoutine());
        }
        else if (currentWaypointIndex == unloadingWaypointIndex && loadedItems.Count > 0)
        {
            StartCoroutine(UnloadCargoRoutine());
        }
        else
        {
            // Trạm bình thường thì chạy qua luôn
            NextWaypoint();
        }
    }

    void NextWaypoint()
    {
        currentWaypointIndex++;
        // Vòng lặp tuần hoàn (Loop): Trở lại điểm 0 nếu hết đường
        if (currentWaypointIndex >= waypoints.Length)
        {
            currentWaypointIndex = 0;
        }
    }

    // COROUTINE: Quá trình bốc hàng
    IEnumerator LoadCargoRoutine()
    {
        isWorking = true;
        yield return new WaitForSeconds(1f); // Dừng phanh xe 1 giây rồi mới bốc

        // Xác định số lượng hàng bốc đợt này (Tối đa 3 hoặc những gì còn lại)
        int itemsToLoad = Mathf.Min(maxLoadPerTrip, itemsOnGround.Count);

        for (int i = 0; i < itemsToLoad; i++)
        {
            Transform item = itemsOnGround[0]; // Luôn lấy món đầu tiên trong danh sách
            if (item != null)
            {
                // Vị trí đích trên thùng xe (xếp chồng hoặc xếp ngang tùy ý, ở đây xếp gọn vào giữa thùng)
                Vector3 targetLocalPos = new Vector3(0, i * 0.5f, 0); 
                
                yield return StartCoroutine(ParabolaJump(item, item.position, cargoHold.position + targetLocalPos));

                // Lên xe thành công
                item.SetParent(cargoHold);
                loadedItems.Add(item);
                itemsOnGround.RemoveAt(0);
            }
            yield return new WaitForSeconds(delayBetweenItems);
        }

        yield return new WaitForSeconds(0.5f); // Nghỉ 0.5s rồi nổ máy đi tiếp
        isWorking = false;
        NextWaypoint();
    }

    // COROUTINE: Quá trình dỡ hàng
    IEnumerator UnloadCargoRoutine()
    {
        isWorking = true;
        yield return new WaitForSeconds(1f);

        // Chạy ngược List để thảy đồ xuống
        for (int i = loadedItems.Count - 1; i >= 0; i--)
        {
            Transform item = loadedItems[i];
            if (item != null)
            {
                item.SetParent(null); // Tách khỏi xe

                // Tính toán vị trí rớt xuống. Mỗi món rớt xuống sẽ nhích ra một chút nhờ totalUnloadedCount
                Vector3 dropPos = unloadStartPoint.position + (spacingOffset * totalUnloadedCount);

                yield return StartCoroutine(ParabolaJump(item, item.position, dropPos));

                loadedItems.RemoveAt(i);
                totalUnloadedCount++; // Tăng biến đếm để món sau không rớt đè lên món trước
            }
            yield return new WaitForSeconds(delayBetweenItems);
        }

        yield return new WaitForSeconds(0.5f);
        isWorking = false;
        NextWaypoint();
    }

    // Thuật toán cốt lõi: Tính toán quỹ đạo bay Parabol
    IEnumerator ParabolaJump(Transform objectToMove, Vector3 startPoint, Vector3 endPoint)
    {
        float timer = 0f;
        while (timer < jumpDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / jumpDuration; // Trạng thái chạy từ 0 đến 1

            // Nội suy tuyến tính trục X và Z
            Vector3 currentPos = Vector3.Lerp(startPoint, endPoint, progress);
            
            // Bơm thêm độ cao (Y) bằng hình sin để tạo vòng cung
            currentPos.y += jumpHeight * Mathf.Sin(progress * Mathf.PI);

            objectToMove.position = currentPos;
            yield return null;
        }

        objectToMove.position = endPoint; // Ép tọa độ chuẩn xác khi kết thúc
    }
}