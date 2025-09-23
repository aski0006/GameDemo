using AsakiFramework;
using AsakiFramework.ObjectPool;
using Gameplay.Anim;
using System;
using UnityEngine;

namespace Gameplay.Creator
{
    public class AttackVfxEffect2DAnimCreator : AsakiMono
    {
        [Header("攻击特效2D动画对象池配置"), SerializeField] private ObjectPoolConfig objectPoolConfig;
        public GameObject prefab => objectPoolConfig.Prefab;
        private void Start()
        {
            ObjectPool.Create(objectPoolConfig);
        }

        public GameObject Get(GameObject vfxPrefab, Vector3 position, Quaternion rotation, Transform parent = null)
        {
            if (parent == null) parent = transform;
            var vfxGameObject = ObjectPool.Get(vfxPrefab, position, rotation, parent);
            if (vfxGameObject == null)
            {
                LogError("从对象池获取攻击特效2D动画失败");
                return null;
            }
            return vfxGameObject;
        }

        public void Return(GameObject vfxGameObject)
        {
            if (vfxGameObject == null)
            {
                LogError("尝试归还一个空的攻击特效2D动画");
                return;
            }
            ObjectPool.Return(vfxGameObject);
        }

    }
}
