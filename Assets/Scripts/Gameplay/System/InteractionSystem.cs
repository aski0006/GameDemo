using AsakiFramework;

namespace Gameplay.System
{
    public class InteractionSystem : AsakiMono
    {
        public bool PLayerIsDragging { get; set; } = false;
        public bool PlayerCanInteract()
        {
            if (ActionSystem.Instance.IsRunning == false) return true;
            else return false;
        }
        public bool PlayerCanHover()
        {
            if (PLayerIsDragging) return false;
            else return true;
        }
    }
}
