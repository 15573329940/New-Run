using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class CursorAimer : MonoBehaviour
{
    static CursorAimer instance;
    public Image image;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            Debug.LogError("Cursor is not unique");
        }
    }
    static public void SetCursorColor(Color color)
    {
        instance.image.color = color;
    }
    /// <summary>
    /// 获取准心圆形区域内的所有射线（中心 + 周围均匀采样）
    /// </summary>
    /// <param name="camera">主相机</param>
    /// <param name="screenRadius">圆形半径（相对于屏幕高度的比例，例如 0.02 = 高度的 2%）</param>
    /// <param name="sampleCount">周围采样点数量（不包含中心）</param>
    /// <param name="includeCenter">是否包含中心射线</param>
    /// <returns>射线列表</returns>
    public static List<Ray> GetCrosshairRays(
        Camera camera,
        float screenRadius = 0.02f,
        int sampleCount = 8,
        bool includeCenter = true)
    {
        if (camera == null)
        {
            Debug.LogWarning("Camera is null in GetCrosshairRays");
            return new List<Ray>();
        }

        List<Ray> rays = new List<Ray>();

        // 中心射线（屏幕正中心）
        if (includeCenter)
        {
            rays.Add(camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0)));
        }

        if (sampleCount <= 0) return rays;

        float aspect = camera.aspect; // width / height

        for (int i = 0; i < sampleCount; i++)
        {
            float angle = (i / (float)sampleCount) * 2 * Mathf.PI;
            
            // 关键：X 方向除以 aspect，保证视觉上是正圆
            float dx = Mathf.Cos(angle) * screenRadius / aspect;
            float dy = Mathf.Sin(angle) * screenRadius;

            Vector3 viewportPos = new Vector3(0.5f + dx, 0.5f + dy, 0);

            // 可选：跳过视口外的点（通常不会越界，除非 radius 很大）
            // if (viewportPos.x < 0 || viewportPos.x > 1 || viewportPos.y < 0 || viewportPos.y > 1)
            //     continue;

            rays.Add(camera.ViewportPointToRay(viewportPos));
        }

        return rays;
    }
}
