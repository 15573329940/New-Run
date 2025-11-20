using UnityEngine;

public class TPSCamera : MonoBehaviour
{
    [Header("目标设置")]
    public Transform target;           // 跟随的目标 (通常是玩家)
    public Vector3 targetOffset = new Vector3(0, 1.6f, 0); // 目标中心偏移 (通常设在头部/肩膀位置)

    [Header("相机参数")]
    public float distance = 5.0f;      // 默认距离
    public float minDistance = 0.5f;   // 最近距离
    public float maxDistance = 10.0f;  // 最远距离
    public float sensitivity = 2.0f;   // 鼠标灵敏度
    public float zoomSpeed = 2.0f;     // 滚轮缩放速度

    [Header("旋转限制")]
    public float yMinLimit = -40f;     // 俯视角度限制
    public float yMaxLimit = 80f;      // 仰视角度限制

    [Header("碰撞检测")]
    public LayerMask obstacleLayers;   // 障碍物图层 (关键设置)
    public float collisionBuffer = 0.2f; // 碰撞缓冲距离 (防止相机穿模看到墙体内部)

    // 内部变量
    private float currentX = 0.0f;
    private float currentY = 0.0f;
    private float currentDistance;

    void Start()
    {
        // 初始化距离和角度
        currentDistance = distance;
        Vector3 angles = transform.eulerAngles;
        currentX = angles.y;
        currentY = angles.x;

        // 锁定鼠标并隐藏 (可选)
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void LateUpdate()
    {
        if (!target) return;

        // 1. 处理鼠标输入
        HandleInput();

        // 2. 计算旋转
        Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);

        // 3. 计算理想的目标位置 (没有障碍物时的位置)
        Vector3 focalPoint = target.position + targetOffset; // 焦点位置
        Vector3 direction = rotation * Vector3.back; // 相机朝后的方向
        Vector3 targetPosition = focalPoint + direction * currentDistance;

        // 4. 处理障碍物碰撞 (核心逻辑)
        Vector3 finalPosition = CheckCollision(focalPoint, targetPosition);

        // 5. 应用位置和旋转
        transform.rotation = rotation;
        transform.position = finalPosition;
    }

    void HandleInput()
    {
        // 旋转
        currentX += Input.GetAxis("Mouse X") * sensitivity;
        currentY -= Input.GetAxis("Mouse Y") * sensitivity;
        currentY = Mathf.Clamp(currentY, yMinLimit, yMaxLimit);

        // 缩放
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        currentDistance = Mathf.Clamp(currentDistance - scroll * zoomSpeed, minDistance, maxDistance);
    }

    /// <summary>
    /// 核心碰撞检测逻辑：复刻了原脚本 UpdateObstacles 的原理
    /// </summary>
    Vector3 CheckCollision(Vector3 start, Vector3 end)
    {
        RaycastHit hit;
        
        // 发射线性检测：从角色头部(start) -> 相机理想位置(end)
        // 仅检测 obstacleLayers 中包含的图层
        if (Physics.Linecast(start, end, out hit, obstacleLayers))
        {
            // 如果碰到障碍物，将相机位置设置在碰撞点前方一点点 (Buffer)
            return hit.point + (start - end).normalized * collisionBuffer;
        }

        // 如果没有障碍物，返回理想位置
        return end;
    }
}