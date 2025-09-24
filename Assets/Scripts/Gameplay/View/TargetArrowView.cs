using AsakiFramework;
using Gameplay.Utility;
using System;
using UnityEngine;

namespace Gameplay.MVC.View
{
    public class TargetArrowView : AsakiMono
    {
        
        [Header("默认启用")][SerializeField] private bool defaultActive = true;
        [NotNullComponent][Header("箭头"), SerializeField] private GameObject arrow;
        [NotNullComponent][Header("线条渲染器"), SerializeField] private LineRenderer lineRenderer;
        
        
        [Header("使用贝塞尔曲线绘制")] public bool useBezierCurve;

        [Header("贝塞尔曲线设置")]
        [SerializeField, Range(3, 128)] private int segmentCount = 24; // 曲线采样点数量
        [SerializeField, Tooltip("最大弯曲强度，单位为距离的倍数")] private float maxCurvature = 0.8f;

        [SerializeField, Range(0.001f, 0.05f), Tooltip("用于计算终点切线的参数偏移")] private float tangentSampleT = 0.01f;

        [Header("箭头渲染层优先（自动设置 SpriteRenderer.sortingOrder = LineRenderer.sortingOrder + 1 ）")]
        [SerializeField] private bool ensureArrowRenderedOnTop = true;

        private Vector3 startPosition;

        private void Awake()
        {
            gameObject.SetActive(defaultActive);
        }

        private void Update()
        {
            Vector3 mousePos = MouseUitility.GetMouseWorldPositionInWorldSpace();

            if (useBezierCurve)
            {
                // 在贝塞尔模式下：不要人为把线段末端回退（会造成箭头与线条位置不一致）
                // 把曲线的终点直接设为箭头尖（用户已把 Sprite 的 pivot 设置在箭尖）
                Vector3 end = mousePos;

                var positions = GetQuadraticBezierPositions(startPosition, end, segmentCount);

                lineRenderer.positionCount = positions.Length;
                lineRenderer.SetPositions(positions);

                // 末端采样点和倒数第二个点用于计算切线，保证箭头朝向与曲线末端切线一致
                if (positions.Length >= 2)
                {
                    Vector3 pEnd = positions[positions.Length - 1];
                    Vector3 pPrev = positions[positions.Length - 2];

                    Vector3 tangent = (pEnd - pPrev);
                    if (tangent.sqrMagnitude < 1e-8f)
                    {
                        // 如果采样点非常接近，则用解析的导数（退化情况）
                        // 二次贝塞尔在 t=1 的导数 B'(1) = 2 * (P2 - P1)
                        // 为简单起见，退化时仍使用起点到终点方向作为后备
                        tangent = (pEnd - startPosition).normalized;
                    }

                    // 把箭头尖放在曲线的最后一个采样点（因为 pivot 在箭尖）
                    arrow.transform.position = pEnd;
                    arrow.transform.right = tangent.normalized;
                }
                else
                {
                    // 极端情况回退到直接使用鼠标方向
                    arrow.transform.position = mousePos;
                    arrow.transform.right = (mousePos - startPosition).normalized;
                }

                // 确保箭头渲染在曲线之上（避免被线条覆盖）
                if (ensureArrowRenderedOnTop)
                    PromoteArrowSpriteSorting();
            }
            else
            {
                // 直线模式：仍然把线的末端设置到箭头尖（pivot 在尖部），这样二者不会分离
                Vector3 end = mousePos;
                Vector3 dir = (end - startPosition).normalized;

                lineRenderer.positionCount = 2;
                lineRenderer.SetPosition(0, startPosition);
                lineRenderer.SetPosition(1, end);

                arrow.transform.position = end;
                arrow.transform.right = dir;

                if (ensureArrowRenderedOnTop)
                    PromoteArrowSpriteSorting();
            }
        }

        /// <summary>
        /// 返回二次贝塞尔曲线的采样点数组（不直接设置 LineRenderer）
        /// 最后两个点保证用于切线计算：倒数第二为 t = 1 - tangentSampleT，最后一个为 t = 1（终点）
        /// </summary>
        private Vector3[] GetQuadraticBezierPositions(Vector3 start, Vector3 end, int segments)
        {
            if (segments < 3) segments = 3;

            Vector3 axis = Vector3.up;

            Vector3 toEnd = (end - start).normalized;
            float angleDeg = Vector3.Angle(toEnd, axis);
            float angleRad = angleDeg * Mathf.Deg2Rad;

            float distance = Vector3.Distance(start, end);
            float curvatureAmount = Mathf.Sin(angleRad) * maxCurvature * distance;

            Vector3 mid = (start + end) * 0.5f;
            Vector3 control = mid + axis.normalized * curvatureAmount;

            var positions = new Vector3[segments + 1];

            for (int i = 0; i <= segments; i++)
            {
                float t = (float)i / segments;
                positions[i] = EvaluateQuadraticBezier(start, control, end, t);
            }

            // 用靠近终点的小偏移参数确保倒数第二个点能用于更精确的切线计算
            if (tangentSampleT > 0f && tangentSampleT < 1f && positions.Length >= 2)
            {
                float smallT = Mathf.Clamp01(1f - tangentSampleT);
                Vector3 sampledBeforeEnd = EvaluateQuadraticBezier(start, control, end, smallT);
                positions[positions.Length - 2] = sampledBeforeEnd;
                positions[positions.Length - 1] = end;
            }
            else
            {
                // 保证末尾是精确的 end
                positions[positions.Length - 1] = end;
            }

            return positions;
        }

        private static Vector3 EvaluateQuadraticBezier(Vector3 p0, Vector3 p1, Vector3 p2, float t)
        {
            float omt = 1f - t;
            return omt * omt * p0 + 2f * omt * t * p1 + t * t * p2;
        }

        private void PromoteArrowSpriteSorting()
        {
            if (arrow == null || lineRenderer == null) return;

            var sr = arrow.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                // lineRenderer 有 sortingOrder 字段（用于 2D 渲染），把箭头的 sortingOrder 设置为比线高 1
                sr.sortingOrder = lineRenderer.sortingOrder + 1;
                // 也可以设置 sortingLayerID 一致，保持层级一致性
                sr.sortingLayerID = lineRenderer.sortingLayerID;
            }
            else
            {
                // 如果箭头不是 SpriteRenderer（例如 UI Image），可以微调 Z 以确保前置（谨慎使用）
                // arrow.transform.position = new Vector3(arrow.transform.position.x, arrow.transform.position.y, arrow.transform.position.z - 0.01f);
            }
        }

        public void SetupArrow(Vector3 startPosition)
        {
            this.startPosition = startPosition;
            if (useBezierCurve)
            {
                Vector3 end = MouseUitility.GetMouseWorldPositionInWorldSpace();
                var positions = GetQuadraticBezierPositions(startPosition, end, segmentCount);
                lineRenderer.positionCount = positions.Length;
                lineRenderer.SetPositions(positions);

                if (positions.Length >= 2)
                {
                    Vector3 pEnd = positions[positions.Length - 1];
                    Vector3 pPrev = positions[positions.Length - 2];
                    Vector3 tangent = (pEnd - pPrev);
                    if (tangent.sqrMagnitude < 1e-8f)
                        tangent = (pEnd - startPosition);

                    arrow.transform.position = pEnd;
                    arrow.transform.right = tangent.normalized;
                }
                else
                {
                    arrow.transform.position = end;
                    arrow.transform.right = (end - startPosition).normalized;
                }

                if (ensureArrowRenderedOnTop) PromoteArrowSpriteSorting();
            }
            else
            {
                lineRenderer.positionCount = 2;
                var end = MouseUitility.GetMouseWorldPositionInWorldSpace();
                lineRenderer.SetPosition(0, startPosition);
                lineRenderer.SetPosition(1, end);
                arrow.transform.position = end;
                arrow.transform.right = (end - startPosition).normalized;

                if (ensureArrowRenderedOnTop) PromoteArrowSpriteSorting();
            }
        }
    }
}