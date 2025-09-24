using AsakiFramework;
using Gameplay.MVC.View;
using System;
using UnityEngine;

namespace Gameplay.System
{
    public class ManualTargetSystem : AsakiMono
    {
        [Header("箭头视图")][SerializeField] private TargetArrowView arrowView;
        [Header("检测目标层级")][SerializeField] private LayerMask targetLayer;
        [Header("检测目标最大距离")][SerializeField] private float maxTargetDistance = 10f;

        [Header("调试设置")]
        [SerializeField, Tooltip("开启后会输出调试日志并在 Scene 中绘制射线/Gizmos")] private bool debugMode = false;
        [SerializeField, Tooltip("调试模式下 Gizmo 显示时长（秒）")] private float debugGizmoDisplayTime = 5f;

        // Gizmo绘制相关变量
        private bool showGizmoRay = false;
        private Vector3 gizmoStartPos;
        private Vector3 gizmoEndPos;
        private float gizmoTimer = 0f;

        private void Awake()
        {
            // 保持日志系统可用，但不再默认输出大量调试信息，只有在 debugMode 下才会记录诊断日志
            IsLogEnabled = true;
        }

        private void Update()
        {
            if (showGizmoRay)
            {
                gizmoTimer += Time.deltaTime;
                if (gizmoTimer >= debugGizmoDisplayTime)
                {
                    showGizmoRay = false;
                    gizmoTimer = 0f;
                }
            }
        }

        public void StartTargeting(Vector3 startPos)
        {
            arrowView.gameObject.SetActive(true);
            arrowView.SetupArrow(startPos);
        }

        public EnemyCharacterView EndTargeting(Vector3 endPos)
        {
            if (debugMode) LogInfo($"[Manual Target System] 请求结束目标选择：{endPos}");

            EnemyCharacterView view = null;

            // 隐藏箭头的视觉（下一帧执行）
            RunNextFrame(() => arrowView.gameObject.SetActive(false));

            // 构造基于摄像机的射线（比使用世界固定方向更鲁棒）
            Camera cam = Camera.main;
            Ray ray;
            if (cam != null)
            {
                Vector3 screenPoint = cam.WorldToScreenPoint(endPos);
                ray = cam.ScreenPointToRay(screenPoint);
            }
            else
            {
                if (debugMode) LogWarning("[Manual Target System] Camera.main is null - fallback to world forward ray.");
                ray = new Ray(endPos, Vector3.forward);
            }

            // 仅在调试模式下设置可视化与输出
            if (debugMode)
            {
                gizmoStartPos = ray.origin;
                gizmoEndPos = ray.origin + ray.direction * maxTargetDistance;
                showGizmoRay = true;
                gizmoTimer = 0f;
                DelayTime(debugGizmoDisplayTime, () => showGizmoRay = false);

                Debug.DrawRay(ray.origin, ray.direction * maxTargetDistance, Color.yellow, Mathf.Min(3f, debugGizmoDisplayTime));
                LogInfo($"[Manual Target System] Ray origin: {ray.origin}, dir: {ray.direction}, maxDist: {maxTargetDistance}, layerMask: {targetLayer.value}");
            }

            int mask = targetLayer.value;

            // 主检测：Raycast
            if (Physics.Raycast(ray, out RaycastHit hit, maxTargetDistance, mask))
            {
                if (debugMode) LogInfo($"[Manual Target System] Physics.Raycast 命中：{hit.collider.name}，位置：{hit.point}");
                view = hit.collider.GetComponentInParent<EnemyCharacterView>();
                if (view != null)
                {
                    if (debugMode) LogInfo($"[Manual Target System] 选中目标：{view.name}");
                    return view;
                }
                else
                {
                    if (debugMode) LogInfo($"[Manual Target System] 命中物体 '{hit.collider.name}'，但未在父级找到 EnemyCharacterView。");
                }
            }
            else
            {
                if (debugMode) LogInfo("[Manual Target System] Raycast 未命中任何对象。");
            }

            // 仅在调试模式下执行额外诊断，以免影响运行时性能和日志噪声
            if (debugMode)
            {
                RaycastHit[] hits = Physics.RaycastAll(ray, maxTargetDistance, mask);
                if (hits.Length > 0)
                {
                    LogInfo($"[Manual Target System] RaycastAll 命中数量: {hits.Length}");
                    foreach (var h in hits)
                    {
                        LogInfo($" - hit: {h.collider.name}, layer={LayerMask.LayerToName(h.collider.gameObject.layer)}, bounds={h.collider.bounds}");
                        var hv = h.collider.GetComponentInParent<EnemyCharacterView>();
                        LogInfo($"   -> GetComponentInParent<EnemyCharacterView>() = {(hv != null ? hv.name : "null")}");
                    }
                }

                Collider[] nearby = Physics.OverlapSphere(endPos, 0.5f, mask);
                if (nearby.Length > 0)
                {
                    LogInfo($"[Manual Target System] OverlapSphere 在 endPos 附近发现 {nearby.Length} 个 collider：");
                    foreach (var c in nearby)
                    {
                        LogInfo($" - nearby collider: {c.name}, enabled={c.enabled}, isTrigger={c.isTrigger}, layer={LayerMask.LayerToName(c.gameObject.layer)}, bounds={c.bounds}");
                    }
                }
                else
                {
                    LogInfo("[Manual Target System] OverlapSphere 未在 endPos 附近发现任何 collider（说明 collider 位置/层 与期望不匹配）。");
                }
            }

            if (debugMode) LogInfo($"[Manual Target System] 未选中有效目标: {endPos} ,hit.collider is null: True, view is null : True");

            return null;
        }

        // Gizmo 绘制（仅在调试模式下显示）
        private void OnDrawGizmos()
        {
            if (!debugMode) return;

            if (showGizmoRay)
            {
                Color originalColor = Gizmos.color;
                Gizmos.color = Color.red;
                Gizmos.DrawLine(gizmoStartPos, gizmoEndPos);
                Gizmos.DrawWireSphere(gizmoStartPos, 0.1f);
                Gizmos.DrawWireCube(gizmoEndPos, Vector3.one * 0.2f);
                DrawArrow(gizmoStartPos, gizmoEndPos - gizmoStartPos, 0.5f);
                Gizmos.color = originalColor;
            }
        }

        private void DrawArrow(Vector3 pos, Vector3 direction, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
        {
            if (direction == Vector3.zero) return;
            Gizmos.DrawRay(pos, direction);
            Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * new Vector3(0, 0, 1);
            Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * new Vector3(0, 0, 1);
            Gizmos.DrawRay(pos + direction, right * arrowHeadLength);
            Gizmos.DrawRay(pos + direction, left * arrowHeadLength);
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (!debugMode) return;
            if (!showGizmoRay)
            {
                Gizmos.color = new Color(1, 0, 0, 0.1f);
                // 之前的 test 对象已移除，不再绘制相关 Gizmo
            }
        }
#endif
    }
}