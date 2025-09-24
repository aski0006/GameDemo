using AsakiFramework;
using AsakiFramework.ObjectPool;
using Gameplay.UI;
using System;
using UnityEngine;

namespace Gameplay.Creator
{
    public class DamageTextCreator : AsakiMono
    {
        [Header("伤害文本对象池配置")][SerializeField] private ObjectPoolConfig objectPoolConfig;

        private void Awake()
        {
            ObjectPool.Create(objectPoolConfig);
        }

        public DamageTextUI CreateDamageText(int damage, Vector3 position, Quaternion rotation, Transform parent = null)
        {
            if (!parent)
            {
                parent = objectPoolConfig.Parent == null ? transform : objectPoolConfig.Parent;
            }
            GameObject damageTextObj = ObjectPool.Get(objectPoolConfig.Prefab, position, rotation, parent);
            if (!damageTextObj)
            {
                LogError("无法创建伤害文本对象，获取的对象为 null！");
                return null;
            }
            var damageTextView = damageTextObj.GetComponent<DamageTextUI>();
            if (!damageTextView)
            {
                LogError("伤害文本对象缺少 DamageTextUI 组件！");
                ObjectPool.Return(damageTextObj);
                return null;
            }
            damageTextView.SetDamageText(damage);
            return damageTextView;
        }

        public void ReturnDamageText(DamageTextUI damageTextUI)
        {
            ObjectPool.Return(damageTextUI.gameObject);
        }
    }
}
