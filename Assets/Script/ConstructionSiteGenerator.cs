using UnityEngine;
using System.Collections.Generic;

public class ConstructionSiteGenerator : MonoBehaviour
{
    [Header("Cấu Hình Đường (Road Pack)")]
    [Tooltip("Gán Khối CUBE mặc định của Unity đã bọc Material đường sạch lỗi hồng")]
    [SerializeField] private GameObject roadPrefab;
    [SerializeField] private float roadWidth = 15f; // Chiều rộng lòng đường tiêu chuẩn 15m - 25m theo quy định

    [Header("Cấu Hình Cây Viền (Tree Pack)")]
    [SerializeField] private GameObject[] treePrefabs;
    [SerializeField] private int totalTreeCount = 80;
    [SerializeField] private float borderThickness = 8f; // Dải cây xanh 5-8m chạy bao quanh rìa mép bản đồ

    [Header("Quản Lý Phân Cấp Hierarchy")]
    [SerializeField] private Transform environmentParent;

    [ContextMenu("► SINH TOÀN BỘ MÔI TRƯỜNG TĨNH")]
    public void GenerateAll()
    {
        ClearAll();
        CreateContainer();

        // Lấy tên Layer từ chuỗi text quy định trong tài liệu
        int groundLayer = LayerMask.NameToLayer("Ground");
        int vegetationLayer = LayerMask.NameToLayer("Vegetation");

        if (groundLayer == -1 || vegetationLayer == -1)
        {
            Debug.LogError("LỖI CHÍ MẠNG: Bạn chưa tạo 2 Layer tên là 'Ground' và 'Vegetation' trong Project Settings!");
            return;
        }

        // ==========================================
        // 1. DỰNG MẠNG LƯỚI ĐƯỜNG QUY HOẠCH QUANH TÂM (0,0,0)
        // ==========================================
        if (roadPrefab != null)
        {
            // Trục đường bao quanh 4 góc nội khu (Mục 9 trên sơ đồ)
            List<Vector3> perimeterPath = new List<Vector3>()
            {
                new Vector3(-220f, 0f, -220f),
                new Vector3(220f, 0f, -220f),
                new Vector3(220f, 0f, 220f),
                new Vector3(-220f, 0f, 220f),
                new Vector3(-220f, 0f, -220f)
            };
            BuildRoadSegments(perimeterPath, groundLayer);

            // Trục lộ chính từ Cổng chính đi vào tâm bối cảnh
            List<Vector3> centerMainRoad = new List<Vector3>()
            {
                new Vector3(0f, 0f, -220f), // Cổng chính (Cạnh đáy)
                new Vector3(0f, 0f, -50f)   // Bùng binh ngã ba trung tâm
            };
            BuildRoadSegments(centerMainRoad, groundLayer);

            // Nhánh chữ Y bên trái ôm qua lõi xây dựng tòa nhà số 3
            List<Vector3> leftFork = new List<Vector3>()
            {
                new Vector3(0f, 0f, -50f),
                new Vector3(-100f, 0f, 60f),
                new Vector3(-100f, 0f, 220f)
            };
            BuildRoadSegments(leftFork, groundLayer);

            // Nhánh chữ Y bên phải đi qua khu vực máy móc đào đất số 7
            List<Vector3> rightFork = new List<Vector3>()
            {
                new Vector3(0f, 0f, -50f),
                new Vector3(120f, 0f, 60f),
                new Vector3(120f, 0f, 220f)
            };
            BuildRoadSegments(rightFork, groundLayer);
        }

        // ==========================================
        // 2. RẢI CÂY NỀN KHÔNG VẬT LÝ DỌC 4 BIÊN 8M NGOẠI VI
        // ==========================================
        if (treePrefabs != null && treePrefabs.Length > 0)
        {
            for (int i = 0; i < totalTreeCount; i++)
            {
                float randomX = 0f;
                float randomZ = 0f;

                // Chia ngẫu nhiên phân bổ cây đều ở 4 cạnh viền ngoài bản đồ (500x500 từ -250 đến 250)
                float rng = Random.value;
                if (rng < 0.25f) // Viền Trái
                {
                    randomX = Random.Range(-250f, -250f + borderThickness);
                    randomZ = Random.Range(-250f, 250f);
                }
                else if (rng < 0.5f) // Viền Phải
                {
                    randomX = Random.Range(250f - borderThickness, 250f);
                    randomZ = Random.Range(-250f, 250f);
                }
                else if (rng < 0.75f) // Viền Dưới
                {
                    randomX = Random.Range(-250f, 250f);
                    randomZ = Random.Range(-250f, -250f + borderThickness);
                }
                else // Viền Trên
                {
                    randomX = Random.Range(-250f, 250f);
                    randomZ = Random.Range(250f - borderThickness, 250f);
                }

                Vector3 treePos = new Vector3(randomX, 0f, randomZ);
                Quaternion treeRot = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);

                GameObject chosenTree = treePrefabs[Random.Range(0, treePrefabs.Length)];
                GameObject treeInstance = Instantiate(chosenTree, treePos, treeRot, environmentParent);
                
                // Cấu hình quy chuẩn bắt buộc của đồ án
                treeInstance.layer = vegetationLayer;
                treeInstance.isStatic = true; // Khóa tĩnh Static Flag tự động
                treeInstance.transform.localScale = Vector3.one * Random.Range(0.8f, 1.4f);
            }
        }

        Debug.Log("✔ SINH HOÀN TẤT! Toàn bộ hạ tầng tĩnh đã được ĐÓNG BĂNG STATIC và gán đúng LAYER. Nhấn Ctrl + S để lưu Scene.");
    }

    private void BuildRoadSegments(List<Vector3> path, int layerIndex)
    {
        for (int i = 0; i < path.Count - 1; i++)
        {
            Vector3 start = path[i];
            Vector3 end = path[i + 1];

            Vector3 centerPos = (start + end) / 2f;
            centerPos.y = 0.02f; // Khử hiện tượng nhấp nháy đè hình Z-fighting

            Vector3 direction = end - start;
            float distance = direction.magnitude;
            if (distance < 0.1f) continue;

            Quaternion rotation = Quaternion.LookRotation(direction);
            GameObject roadInstance = Instantiate(roadPrefab, centerPos, rotation, environmentParent);
            
            // X = Độ rộng làn, Y = Độ dày tấm, Z = Chiều dài tịnh tiến giữa các điểm node
            roadInstance.transform.localScale = new Vector3(roadWidth, 0.2f, distance);
            
            // Ép quy chuẩn kỹ thuật đồ án
            roadInstance.layer = layerIndex;
            roadInstance.isStatic = true; // Khóa tĩnh Static Flag tự động
        }
    }

    [ContextMenu("Xóa Toàn Bộ")]
    public void ClearAll()
    {
        if (environmentParent == null) return;
        for (int i = environmentParent.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(environmentParent.GetChild(i).gameObject);
        }
    }

    private void CreateContainer()
    {
        if (environmentParent == null)
        {
            environmentParent = new GameObject("Procedural_Static_Layout").transform;
            environmentParent.position = Vector3.zero;
        }
    }
}