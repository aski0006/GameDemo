using UnityEngine;

namespace Gameplay.Utility
{
    public static class MouseUitility
    {
        private static Camera mainCamera = Camera.main;
        public static Vector3 GetMouseWorldPositionInWorldSpace(float zPlane = 0f)
        {
            Plane dragPlane = new Plane(
                mainCamera.transform.forward, 
                new Vector3(0, 0, zPlane));
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (dragPlane.Raycast(ray, out float distance))
            {
                return ray.GetPoint(distance);
            }
            return Vector3.zero;
        }
    }
}
