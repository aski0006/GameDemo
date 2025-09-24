using UnityEditor;

namespace Gameplay.MVC.Interfaces
{
    public interface IModel
    {
        public GUID ModelInstanceID { get; }
    }

    public interface IView
    {
        public GUID BoundModelInstanceID { get; }
        public void UnbindModel();
    }
    
}
