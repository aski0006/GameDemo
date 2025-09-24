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
        private void Start()
        {
            ObjectPool.Create(objectPoolConfig);
        }

        public AttackVfxEffect2DAnim CreateAttackVfxEffect(Vector3 position, Quaternion rotation, Transform parent = null)
        {
            if (parent == null)
            {
                if (objectPoolConfig.Parent == null)
                {
                    parent = transform;
                }
                else
                {
                    parent = objectPoolConfig.Parent;
                }
            }
            var vfxGameObject = ObjectPool.Get(objectPoolConfig.Prefab, position, rotation, parent);
            if (vfxGameObject == null)
            {
                LogError("从对象池获取攻击特效2D动画失败");
                return null;
            }
            var vfx = vfxGameObject.GetComponent<AttackVfxEffect2DAnim>();
            if (vfx == null)
            {
                LogError("从对象池获取的攻击特效2D动画不包含 AttackVfxEffect2DAnim 组件");
                ObjectPool.Return(vfxGameObject);
                return null;
            }
            return vfx;
        }

        public void ReturnAttackVfxEffect(AttackVfxEffect2DAnim vfx)
        {
            ObjectPool.Return(vfx.gameObject);
        }

    }
}
