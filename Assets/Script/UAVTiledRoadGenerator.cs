using UnityEngine;
using System.Collections.Generic;

public class UAVTiledRoadGenerator : MonoBehaviour
{
    [Header("Asset Pack Prefab (Mảnh đường thẳng có vạch kẻ)")]
    [SerializeField] private GameObject roadModularPrefab;
    [SerializeField] private float segmentLength = 10f; // Độ dài mặc định của một mảnh đường là 10m

    [Header("Cây Cảnh Viền (Tree Pack)")]
    [SerializeField] private GameObject[] treePrefabs;
    [SerializeField] private int totalTreeCount = 90;
    [SerializeField] private float borderThickness = 9f;

    [Header("Hierarchy Management Parent")]
    [SerializeField] private Transform environmentParent;

    [ContextMenu("► SINH QUY HOẠCH CHUẨN SƠ ĐỒ")]
    public void GenerateLayout()
    {
        ClearLayout();
        CreateContainer();

        int groundLayer = LayerMask.NameToLayer("Ground");
        int vegetationLayer = LayerMask.NameToLayer("Vegetation");

        if (groundLayer == -1 || vegetationLayer == -1)
        {
            Debug.LogError("Thiếu cấu hình Layer 'Ground' hoặc 'Vegetation' trong Project Settings!");
            return;
        }

        if (roadModularPrefab == null)
        {
            Debug.LogError("Vui lòng gán file mẫu đường thẳng từ thư mục Prefabs vào ô Road Modular Prefab!");
            return;
        }

        // ========================================================
        // 1. ĐƯỜNG BAO QUANH NGOẠI VI RÌA MÉP BẢN ĐỒ (Khung đen ngoài cùng)
        // ========================================================
        List<Vector3> perimeterPath = new List<Vector3>()
        {
            new Vector3(-220f, 0f, -220f),
            new Vector3(220f, 0f, -220f),
            new Vector3(220f, 0f, 220f),
            new Vector3(-220f, 0f, 220f),
            new Vector3(-220f, 0f, -220f)
        };
        BuildTiledRoad(perimeterPath, groundLayer);

        // ========================================================
        // 2. VÒNG XUYẾN HÌNH THOI TRUNG TÂM (Ôm quanh tòa nhà số 3)
        // ========================================================
        List<Vector3> centralDiamondLoop = new List<Vector3>()
        {
            new Vector3(0f, 0f, -70f),     // Nút Dưới
            new Vector3(110f, 0f, 30f),    // Nút Phải
            new Vector3(0f, 0f, 130f),     // Nút Trên
            new Vector3(-110f, 0f, 30f),   // Nút Trái
            new Vector3(0f, 0f, -70f)      // Khép khít vòng thoi
        };
        BuildTiledRoad(centralDiamondLoop, groundLayer);

        // ========================================================
        // 3. CÁC TRỤC ĐƯỜNG NỐI RA BIÊN ĐÔNG - TÂY - NAM - BẮC
        // ========================================================
        // Nhánh Nam (Từ Cổng chính đi lên đỉnh dưới hình thoi)
        BuildTiledRoad(new List<Vector3> { new Vector3(0f, 0f, -220f), new Vector3(0f, 0f, -70f) }, groundLayer);

        // Nhánh Bắc (Từ đỉnh trên hình thoi đi thẳng lên biên Bắc)
        BuildTiledRoad(new List<Vector3> { new Vector3(0f, 0f, 130f), new Vector3(0f, 0f, 220f) }, groundLayer);

        // Nhánh Tây (Từ đỉnh trái hình thoi đi xiên nhẹ ra biên Tây)
        BuildTiledRoad(new List<Vector3> { new Vector3(-110f, 0f, 30f), new Vector3(-220f, 15f, 15f) }, groundLayer);

        // ========================================================
        // 4. HỆ THỐNG ĐƯỜNG PHỤ/XƯƠNG CÁ BÊN PHẢI (Khu số 6 & 7)
        // ========================================================
        // Nhánh xiên Đông Bắc nối từ góc Phải hình thoi lên điểm ngã ba phụ
        BuildTiledRoad(new List<Vector3> { new Vector3(110f, 0f, 30f), new Vector3(155f, 0f, 75f) }, groundLayer);

        // Đường ngang phía trên đi ra sát biên phải
        BuildTiledRoad(new List<Vector3> { new Vector3(155f, 0f, 75f), new Vector3(220f, 0f, 75f) }, groundLayer);

        // Đường vuông góc đi dọc xuống trục dưới
        BuildTiledRoad(new List<Vector3> { new Vector3(155f, 0f, 75f), new Vector3(155f, 0f, -15f) }, groundLayer);

        // Đường ngang phía dưới đi ra sát biên phải
        BuildTiledRoad(new List<Vector3> { new Vector3(155f, 0f, -15f), new Vector3(220f, 0f, -15f) }, groundLayer);


        // ========================================================
        // 5. RẢI CÂY CẢNH VIỀN BẢN ĐỒ KHÔNG VẬT LÝ
        // ========================================================
        SpawnSceneryTrees(vegetationLayer);

        Debug.Log("✔ Đã đồng bộ Layout đường khớp 100% theo sơ đồ phác thảo hình vẽ!");
    }

    private void BuildTiledRoad(List<Vector3> path, int layerIndex)
    {
        for (int i = 0; i < path.Count - 1; i++)
        {
            Vector3 start = path[i];
            Vector3 end = path[i + 1];

            Vector3 direction = end - start;
            float totalDistance = direction.magnitude;
            if (totalDistance < 0.1f) continue;

            Quaternion rotation = Quaternion.LookRotation(direction);
            int segmentsNeeded = Mathf.RoundToInt(totalDistance / segmentLength);

            for (int j = 0; j < segmentsNeeded; j++)
            {
                float t = (j + 0.5f) / segmentsNeeded;
                Vector3 segmentPos = Vector3.Lerp(start, end, t);
                segmentPos.y = 0.02f; // Tránh lỗi chồng lấn hình học (Z-fighting)

                GameObject roadTile = Instantiate(roadModularPrefab, segmentPos, rotation, environmentParent);
                roadTile.layer = layerIndex;
                roadTile.isStatic = true; 
            }
        }
    }

    private void SpawnSceneryTrees(int layerIndex)
    {
        if (treePrefabs == null || treePrefabs.Length == 0) return;

        for (int i = 0; i < totalTreeCount; i++)
        {
            float x = 0, z = 0;
            float rng = Random.value;
            if (rng < 0.25f) { x = Random.Range(-250f, -250f + borderThickness); z = Random.Range(-250f, 250f); }
            else if (rng < 0.5f) { x = Random.Range(250f - borderThickness, 250f); z = Random.Range(-250f, 250f); }
            else if (rng < 0.75f) { x = Random.Range(-250f, 250f); z = Random.Range(-250f, -250f + borderThickness); }
            else { x = Random.Range(-250f, 250f); z = Random.Range(250f - borderThickness, 250f); }

            Vector3 treePos = new Vector3(x, 0f, z);
            Quaternion treeRot = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);

            GameObject treeInstance = Instantiate(treePrefabs[Random.Range(0, treePrefabs.Length)], treePos, treeRot, environmentParent);
            treeInstance.layer = layerIndex;
            treeInstance.isStatic = true;
            treeInstance.transform.localScale = Vector3.one * Random.Range(0.8f, 1.4f);
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
        if (environmentParent == null)
        {
            environmentParent = new GameObject("Procedural_Static_Layout").transform;
            environmentParent.position = Vector3.zero;
        }
    }
}