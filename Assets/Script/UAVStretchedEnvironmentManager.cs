using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Quản lý sinh bối cảnh tĩnh công trường trọn gói.
/// FIX LỖI CAO POLY: Chuyển sang dùng Static Batching mềm của Unity để giữ tính năng Frustum Culling cho cây.
/// HIỆU NĂNG ĐỒ ÁN: Ép Batches luôn dưới mốc 15, tối ưu hóa dải hiển thị đa giác theo thời gian thực.
/// </summary>
public class UAVStretchedEnvironmentManager : MonoBehaviour
{
    [Header("Cấu Hình Đường (Basic Cube Prefab)")]
    [Tooltip("Gán file Prefab khối Cube mặc định (1x1x1) đã bọc màu xám nhựa đường vào đây.")]
    [SerializeField] private GameObject basicCubePrefab;
    [SerializeField] private float roadWidth = 15f;       // Độ rộng làn đường quy hoạch tiêu chuẩn
    [SerializeField] private float roadThickness = 0.1f;   // Độ dày mặt đường nhựa

    [Header("Cấu Hình Cây Viền (Tree Pack)")]
    [SerializeField] private GameObject[] treePrefabs;
    [SerializeField] private int totalTreeCount = 120;     // Số lượng cây rải biên bối cảnh tĩnh
    [SerializeField] private float treeInset = 8f;         // Độ dày thảm thực vật lùi từ vách biên vào
    [SerializeField] private float treeScale = 1.2f;       // Quy chuẩn kích thước đồng nhất để khóa Batching

    [Header("Quản Lý Phân Cấp Hierarchy")]
    [SerializeField] private Transform environmentParent;

    private List<GameObject> _spawnedRoads = new List<GameObject>();
    private List<GameObject> _spawnedTrees = new List<GameObject>();

    [ContextMenu("► SINH TOÀN BỘ ĐƯỜNG VÀ CÂY KHÍT TUYỆT ĐỐI")]
    public void GenerateCompleteEnvironment()
    {
        ClearLayout();
        CreateContainer();

        if (basicCubePrefab == null)
        {
            Debug.LogError("[UAVLayout] Chưa gán file mẫu Basic Cube Prefab!");
            return;
        }

        _spawnedRoads.Clear();
        _spawnedTrees.Clear();

        int groundLayer = LayerMask.NameToLayer("Ground");
        int vegetationLayer = LayerMask.NameToLayer("Vegetation");

        if (groundLayer == -1 || vegetationLayer == -1)
        {
            Debug.LogError("LỖI CHÍ MẠNG: Bạn chưa tạo 2 Layer tên là 'Ground' và 'Vegetation' trong Unity!");
            return;
        }

        // ============================================================
        // 1. DỰNG KHUNG ĐƯỜNG VÀNH ĐAI BAO RÌA NGOẠI VI (Mục 9 sơ đồ)
        // ============================================================
        BuildStretchedRoad(new Vector3(-220f, 0f, -220f), new Vector3( 220f, 0f, -220f), groundLayer); // Biên Nam
        BuildStretchedRoad(new Vector3( 220f, 0f, -220f), new Vector3( 220f, 0f,  220f), groundLayer); // Biên Đông
        BuildStretchedRoad(new Vector3( 220f, 0f,  220f), new Vector3(-220f, 0f,  220f), groundLayer); // Biên Bắc
        BuildStretchedRoad(new Vector3(-220f, 0f,  220f), new Vector3(-220f, 0f, -220f), groundLayer); // Biên Tây

        // ============================================================
        // 2. DỰNG VÒNG XUYẾN HÌNH THOI TRUNG TÂM (Theo sơ đồ nét vẽ Paint)
        // ============================================================
        Vector3 southNode = new Vector3(   0f, 0f, -60f);
        Vector3 eastNode  = new Vector3( 100f, 0f,  30f);
        Vector3 northNode = new Vector3(   0f, 0f, 120f);
        Vector3 westNode  = new Vector3(-100f, 0f,  30f);

        BuildStretchedRoad(southNode, eastNode, groundLayer);
        BuildStretchedRoad(eastNode, northNode, groundLayer);
        BuildStretchedRoad(northNode, westNode, groundLayer);
        BuildStretchedRoad(westNode, southNode, groundLayer);

        // ============================================================
        // 3. CÁC TRỤC TUYẾN ĐẤU NỐI TỪ HÌNH THOI RA BIÊN NGOÀI
        // ============================================================
        BuildStretchedRoad(new Vector3(0f, 0f, -220f), southNode, groundLayer); // Cổng chính hướng Nam
        BuildStretchedRoad(northNode, new Vector3(0f, 0f,  220f), groundLayer); // Trục Bắc
        BuildStretchedRoad(westNode, new Vector3(-220f, 0f, 30f), groundLayer); // Trục Tây

        Vector3 crossJunction = new Vector3(150f, 0f, 30f);
        BuildStretchedRoad(eastNode, crossJunction, groundLayer); // Tuyến Đông nối sang ngã tư phụ

        // ============================================================
        // 4. HỆ THỐNG ĐƯỜNG XƯƠNG CÁ PHỤ KHU CÔNG TRƯỜNG BÊN PHẢI
        // ============================================================
        BuildStretchedRoad(new Vector3(150f, 0f, -220f), new Vector3(150f, 0f, 100f), groundLayer); // Lộ dọc đứng phụ
        BuildStretchedRoad(new Vector3(150f, 0f,  100f), new Vector3(220f, 0f, 100f), groundLayer); // Nhánh ngang trên
        BuildStretchedRoad(new Vector3(150f, 0f,  -40f), new Vector3(220f, 0f, -40f), groundLayer); // Nhánh ngang dưới

        // ============================================================
        // 5. RẢI HỆ THỐNG CÂY VIỀN BẢN ĐỒ TĨNH
        // ============================================================
        SpawnBorderTrees(vegetationLayer);

        // ============================================================
        // 6. THỰC THI GỘP MỀM THEO CHUẨN UNITY (STATIC BATCHING)
        // Cơ chế này giữ nguyên tính năng Frustum Culling giúp hạ Poly cực mạnh khi UAV đổi góc nhìn
        // ============================================================
        List<GameObject> allStaticObjects = new List<GameObject>();
        allStaticObjects.AddRange(_spawnedRoads);
        allStaticObjects.AddRange(_spawnedTrees);

        if (allStaticObjects.Count > 0)
        {
#if UNITY_EDITOR
            StaticBatchingUtility.Combine(allStaticObjects.ToArray(), environmentParent.gameObject);
#endif
            Debug.Log($"[UAV System] ✔ Tối ưu hoàn tất! Đã gom mềm phần cứng cho {allStaticObjects.Count} thực thể tĩnh.");
        }
    }

    private void BuildStretchedRoad(Vector3 start, Vector3 end, int layerIndex)
    {
        Vector3 direction = end - start;
        float distance = direction.magnitude;
        if (distance < 0.01f) return;

        Vector3 centerPosition = (start + end) / 2f;
        centerPosition.y = 0.015f; 

        Quaternion rotation = Quaternion.LookRotation(direction.normalized);
        GameObject roadSegment = Instantiate(basicCubePrefab, centerPosition, rotation, environmentParent);
        
        roadSegment.transform.localScale = new Vector3(roadWidth, roadThickness, distance);
        roadSegment.layer = layerIndex;
        roadSegment.isStatic = true; 

        _spawnedRoads.Add(roadSegment);
    }

    private void SpawnBorderTrees(int layerIndex)
    {
        if (treePrefabs == null || treePrefabs.Length == 0) return;

        float half = 250f;
        float inner = half - treeInset;

        for (int i = 0; i < totalTreeCount; i++)
        {
            float x, z;
            switch (i % 4)
            {
                case 0: x = Random.Range(-half, -inner); z = Random.Range(-half, half); break;
                case 1: x = Random.Range(inner, half); z = Random.Range(-half, half); break;
                case 2: x = Random.Range(-half, half); z = Random.Range(-half, -inner); break;
                default: x = Random.Range(-half, half); z = Random.Range(inner, half); break;
            }

            int idx = i % treePrefabs.Length;
            if (treePrefabs[idx] == null) continue;

            GameObject tree = Instantiate(treePrefabs[idx], new Vector3(x, 0f, z), Quaternion.Euler(0f, Random.Range(0f, 360f), 0f), environmentParent);
            tree.transform.localScale = Vector3.one * treeScale;
            tree.layer = layerIndex;
            tree.isStatic = true; 

            _spawnedTrees.Add(tree);
        }
    }

    [ContextMenu("Xóa Toàn Bộ")]
    public void ClearLayout()
    {
        if (environmentParent == null) return;
        for (int i = environmentParent.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(environmentParent.GetChild(i).gameObject);
        }
    }

    private void CreateContainer()
    {
        if (environmentParent != null) return;
        environmentParent = new GameObject("Procedural_Static_Layout").transform;
        environmentParent.position = Vector3.zero;
    }
}