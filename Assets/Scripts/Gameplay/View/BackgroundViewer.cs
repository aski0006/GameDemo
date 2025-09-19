using AsakiFramework;
using UnityEngine;

namespace Gameplay.View
{
    public class BackgroundViewer : AsakiMono
    {
        [Header("背景精灵渲染器")]
        [NotNullComponent, SerializeField] 
        private SpriteRenderer backgroundRenderer;

        private void Awake()
        {
            HasNotNullComponent<SpriteRenderer>();
            CoroutineUtility.DelayOneFrame(UpdateBackgroundSize);
        }

        private void UpdateBackgroundSize()
        {
            Camera cam = Camera.main;
            if (!cam)
            {
                LogError("主摄像机未找到！");
                return;
            }

            // 1. 确保精灵是“全屏”类型
            if (backgroundRenderer.drawMode != SpriteDrawMode.Tiled &&
                backgroundRenderer.drawMode != SpriteDrawMode.Simple)
            {
                backgroundRenderer.drawMode = SpriteDrawMode.Tiled;
            }

            // 2. 计算世界尺寸
            float height = cam.orthographicSize * 2f;
            float width  = height * cam.aspect;

            // 3. 获取精灵尺寸
            Sprite sprite = backgroundRenderer.sprite;
            if (!sprite)
            {
                LogError("背景精灵未设置！");
                return;
            }
            Vector2 spriteSize = sprite.bounds.size;
            
            // 4. 计算缩放比例
            Vector3 scale = backgroundRenderer.transform.localScale;
            scale.x = width / spriteSize.x;
            scale.y = height / spriteSize.y;
            backgroundRenderer.transform.localScale = scale;
            
            LogInfo($"背景已缩放到 {width:F2} × {height:F2}");
        }
    }
}
