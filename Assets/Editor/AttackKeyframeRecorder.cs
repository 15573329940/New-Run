// AttackKeyframeRecorder.cs
// ！！！必须放在 "Editor" 文件夹中 ！！！

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class AttackKeyframeRecorder : EditorWindow
{
    private GameObject giantRoot; // 巨人根节点
    private AnimationClip animationClip; // 要采样的动画
    private TitanAttackSO attackInfo; // 要写入的 ScriptableObject
    private Transform hitboxTransform; // 要采样的 Hitbox
    private float startTime; // 开始时间
    private float endTime; // 结束时间
    private int samplesPerSecond = 10; // 采样率 (每秒采样10次)

    private Vector2 scrollPos;

    // 添加一个菜单项，以便从 Unity 顶部菜单打开此窗口
    [MenuItem("Tools/Giant AI/Attack Keyframe Recorder")]
    public static void ShowWindow()
    {
        GetWindow<AttackKeyframeRecorder>("Keyframe Recorder");
    }

    void OnGUI()
    {
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        EditorGUILayout.LabelField("1. 拖入你的对象", EditorStyles.boldLabel);
        giantRoot = (GameObject)EditorGUILayout.ObjectField("Giant Root (Animator)", giantRoot, typeof(GameObject), true);
        animationClip = (AnimationClip)EditorGUILayout.ObjectField("Animation Clip", animationClip, typeof(AnimationClip), false);
        attackInfo = (TitanAttackSO)EditorGUILayout.ObjectField("Attack Info (SO to Write)", attackInfo, typeof(TitanAttackSO), false);
        hitboxTransform = (Transform)EditorGUILayout.ObjectField("Hitbox (to Sample)", hitboxTransform, typeof(Transform), true);
        
        EditorGUILayout.Space(10);

        EditorGUILayout.LabelField("2. 设置采样范围 (从动画窗口读取)", EditorStyles.boldLabel);
        startTime = EditorGUILayout.FloatField("Start Time (sec)", startTime);
        endTime = EditorGUILayout.FloatField("End Time (sec)", endTime);
        samplesPerSecond = EditorGUILayout.IntSlider("Samples Per Second", samplesPerSecond, 1, 60);
        
        EditorGUILayout.HelpBox("提示: 打开动画窗口，找到攻击判定的开始和结束时间(秒)，填入上方。", MessageType.Info);

        EditorGUILayout.Space(20);

        // --- 执行录制 ---
        if (GUILayout.Button("!!! RECORD KEYFRAMES !!!", GUILayout.Height(40)))
        {
            if (ValidateFields())
            {
                RecordKeyframes();
            }
        }
        
        EditorGUILayout.EndScrollView();
    }

    private bool ValidateFields()
    {
        if (giantRoot == null || animationClip == null || attackInfo == null || hitboxTransform == null)
        {
            EditorUtility.DisplayDialog("Error", "所有字段都必须填写！", "OK");
            return false;
        }
        if (endTime <= startTime)
        {
            EditorUtility.DisplayDialog("Error", "结束时间必须大于开始时间！", "OK");
            return false;
        }
        return true;
    }

    private void RecordKeyframes()
    {
        // 确保 Hitbox 上有 SphereCollider
        SphereCollider collider = hitboxTransform.GetComponent<SphereCollider>();
        if (collider == null)
        {
            EditorUtility.DisplayDialog("Error", "Hitbox 必须有一个 SphereCollider 才能读取半径！", "OK");
            return;
        }
        
        float radius = collider.radius; // 获取半径 (假设半径在动画中不变)

        // 1. 清空旧数据
        attackInfo.Keyframes.Clear();

        // 2. 循环并采样
        float increment = 1.0f / samplesPerSecond; // 采样时间间隔
        int frameCount = 0;

        for (float t = startTime; t <= endTime; t += increment)
        {
            // 2a. 在场景中“摆姿势”
            animationClip.SampleAnimation(giantRoot, t);

            // 2b. 计算坐标 (从 Hitbox 世界位置 -> 巨人根节点局部位置)
            Vector3 worldPos = hitboxTransform.position;
            Vector3 localPos = giantRoot.transform.InverseTransformPoint(worldPos);

            // 2c. 创建并填充新关键帧
           TitanAttackKeyFrame newKeyframe = new TitanAttackKeyFrame(); // 确保你使用的是 AttackKeyframe (而不是 TitanAttackKeyFrame)
            newKeyframe.Time = t;
            newKeyframe.LocalPosition = localPos;
            newKeyframe.Radius = radius;
            // 2d. 添加到列表
            attackInfo.Keyframes.Add(newKeyframe);
            frameCount++;
        }
        attackInfo.animationLength = animationClip.length; // 记录动画长度
        // 3. 保存 ScriptableObject
        EditorUtility.SetDirty(attackInfo);
        AssetDatabase.SaveAssets();

        // 4. 显示成功信息
        EditorUtility.DisplayDialog("Success", $"录制完毕！\n成功写入 {frameCount} 帧关键帧到 {attackInfo.name}。", "OK");
        
        // 5. 选中该资产，让你能立刻看到结果
        Selection.activeObject = attackInfo;
    }
}