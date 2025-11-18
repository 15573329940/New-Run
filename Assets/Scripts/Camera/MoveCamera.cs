using UnityEngine;

public class GenshinLikeCamera : MonoBehaviour
{
    [Header("目标设置")]
    public Transform target; // 跟随的角色
    public Transform cameraPivot; // 相机旋转支点（通常在角色头顶上方）

    [Header("视角参数")]
    public float defaultDistance = 6f; // 默认距离角色的距离
    public float minDistance = 1f; // 最小距离（碰撞时拉近的极限）
    public float maxDistance = 8f; // 最大距离
    public float heightOffset = 1.5f; // 支点高度（角色头顶偏移）

    [Header("旋转控制")]
    public float horizontalSensitivity = 2f; // 水平旋转灵敏度
    public float verticalSensitivity = 2f; // 垂直旋转灵敏度
    public float minVerticalAngle = -30f; // 最小俯视角度（负角度向下）
    public float maxVerticalAngle = 60f; // 最大仰视角度（正角度向上）

    private float currentHorizontalAngle; // 当前水平旋转角度（绕Y轴）
    private float currentVerticalAngle; // 当前垂直旋转角度（绕X轴）
    private float currentDistance; // 当前相机距离（可能因碰撞缩短）

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // 初始化支点位置（跟随角色头顶）
        cameraPivot.position = target.position + Vector3.up * heightOffset;
        // 初始角度：默认在角色后方（180度水平角，0度垂直角）
        currentHorizontalAngle = 180f;
        currentVerticalAngle = 0f;
        currentDistance = defaultDistance;
    }

    void LateUpdate()
    {
        if (target == null) return;

        // 1. 更新支点位置（始终在角色头顶）
        cameraPivot.position = target.position + Vector3.up * heightOffset;

        // 2. 处理玩家输入（鼠标旋转视角）
        HandleInput();

        // 3. 计算相机目标位置（基于当前角度和距离）
        Vector3 targetCameraPos = CalculateTargetPosition();

        // 4. 碰撞检测：若有障碍物，拉近相机
        targetCameraPos = CheckCollision(targetCameraPos);

        // 5. 直接设置相机位置（无平滑）
        transform.position = targetCameraPos;

        // 6. 直接设置相机旋转（看向支点，无平滑）
        transform.LookAt(cameraPivot.position);
    }

    // 处理鼠标输入，更新旋转角度
    void HandleInput()
    {
        // 鼠标X轴控制水平旋转（绕Y轴）
        currentHorizontalAngle += Input.GetAxis("Mouse X") * horizontalSensitivity;
        // 水平角度无限循环（0-360度）
        currentHorizontalAngle %= 360f;

        // 鼠标Y轴控制垂直旋转（绕X轴），并限制角度范围
        currentVerticalAngle -= Input.GetAxis("Mouse Y") * verticalSensitivity;
        currentVerticalAngle = Mathf.Clamp(currentVerticalAngle, minVerticalAngle, maxVerticalAngle);
    }

    // 计算相机基于角度和距离的目标位置
    Vector3 CalculateTargetPosition()
    {
        // 水平旋转：绕Y轴
        Quaternion horizontalRot = Quaternion.Euler(0f, currentHorizontalAngle, 0f);
        // 垂直旋转：绕X轴（基于水平旋转后的局部X轴）
        Quaternion verticalRot = Quaternion.Euler(currentVerticalAngle, 0f, 0f);

        // 相机方向：从支点向后（初始方向为Vector3.back，即(0,0,-1)）
        Vector3 cameraDir = horizontalRot * verticalRot * Vector3.back;
        // 目标位置 = 支点位置 + 方向 × 距离
        return cameraPivot.position + cameraDir * currentDistance;
    }

    // 碰撞检测：射线检测相机到支点的路径，有障碍物则缩短距离
    Vector3 CheckCollision(Vector3 desiredPos)
    {
        // 射线起点：支点，方向：相机到支点的反方向（从支点看向相机）
        Vector3 dirToCamera = desiredPos - cameraPivot.position;
        Ray ray = new Ray(cameraPivot.position, dirToCamera.normalized);
        RaycastHit hit;

        // 检测是否有障碍物（忽略角色自身，需给角色设置Layer）
        int ignoreLayer = 1 << LayerMask.NameToLayer("Player"); // 假设角色在"Player"层
        int detectLayers = ~ignoreLayer; // 检测除了角色之外的层

        if (Physics.Raycast(ray, out hit, dirToCamera.magnitude, detectLayers))
        {
            // 有障碍物，相机位置调整到障碍物前方一点
            currentDistance = Mathf.Clamp(hit.distance - 0.1f, minDistance, defaultDistance);
            return cameraPivot.position + dirToCamera.normalized * currentDistance;
        }
        else
        {
            // 无障碍物，直接恢复到默认距离（无平滑过渡）
            currentDistance = defaultDistance;
            return desiredPos;
        }
    }

    void OnDestroy()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}