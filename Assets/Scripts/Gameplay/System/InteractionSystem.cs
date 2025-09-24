using AsakiFramework;
using Gameplay.MVC.Model;

namespace Gameplay.System
{
    public class InteractionSystem : AsakiMono
    {

        // 正在进行手动目标选择的卡（如果为 null 则不在 targeting 模式）
        // 传统拖拽标记（用于普通拖拽）
        public bool PLayerIsDragging { get; private set; }
        public Card TargetingCard { get; private set; }

        // 是否处于手动目标选择模式（拖动的是一张需要手动选择目标的卡）
        public bool IsTargetingMode => TargetingCard != null;

        // 普通交互权限（和之前相同，action system 正在运行时不可交互）
        public bool PlayerCanInteract()
        {
            if (ActionSystem.Instance == null) return true;
            return ActionSystem.Instance.IsRunning == false;
        }

        // hover 权限：如果处于 Targeting 模式，则仍然允许 Hover（以便在拖动目标卡时可以 hover 敌人）
        public bool PlayerCanHover()
        {
            if (IsTargetingMode) return true;
            return !PLayerIsDragging;
        }

        // 启动普通拖拽（返回是否成功）
        public bool StartDragging(Card card)
        {
            // 如果正在进行 manual targeting，不允许启动普通拖拽
            if (IsTargetingMode) return false;

            PLayerIsDragging = true;
            return true;
        }

        public void StopDragging()
        {
            PLayerIsDragging = false;
        }

        // 启动手动目标选择模式（用于 manual target 卡）
        public void StartManualTargeting(Card card)
        {
            TargetingCard = card;
            // 保持 PLayerIsDragging = false（目标选择不是传统“拖拽”）
        }

        // 结束手动目标选择模式
        public void StopManualTargeting()
        {
            TargetingCard = null;
        }
    }
}
