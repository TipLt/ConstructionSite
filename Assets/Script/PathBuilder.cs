using UnityEngine;

public class PathBuilder : MonoBehaviour
{
    [Header("Tùy chỉnh màu cho dễ nhìn")]
    public Color lineColor = Color.cyan; // Màu đường kẻ (mặc định xanh lơ)
    public float pointSize = 0.3f; // Kích thước cục tròn

    void OnDrawGizmos()
    {
        Gizmos.color = lineColor;
        
        // Tự động quét tất cả các thằng con bên trong nó để vẽ đường
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform current = transform.GetChild(i);
            Gizmos.DrawSphere(current.position, pointSize);

            // Nối điểm hiện tại với điểm tiếp theo
            if (i + 1 < transform.childCount)
            {
                Transform next = transform.GetChild(i + 1);
                Gizmos.DrawLine(current.position, next.position);
            }
        }
    }
}