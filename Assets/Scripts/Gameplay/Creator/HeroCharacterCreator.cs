using AsakiFramework;
using AsakiFramework.ObjectPool;
using Gameplay.MVC.View;
using System;
using UnityEngine;

namespace Gameplay.Creator
{
    public class HeroCharacterCreator : AsakiMono
    {
        [Header("英雄角色对象池配置"), SerializeField] private ObjectPoolConfig objectPoolConfig;

        private void Awake()
        {
            ObjectPool.Create<HeroCharacterView>(
                prefab: objectPoolConfig.Prefab,
                initialSize: objectPoolConfig.InitialCapacity,
                maxSize: objectPoolConfig.MaxCapacity,
                poolName: objectPoolConfig.PoolName
            );
        }

        public HeroCharacterView CreateHeroCharacterView(Vector3 position, Quaternion rotation, Transform parent = null)
        {
            if (parent == null) parent = transform;
            GameObject heroCharacterViewObj =
                ObjectPool.Get(objectPoolConfig.Prefab, position, rotation, parent);
            if (heroCharacterViewObj == null)
            {
                LogError("从对象池获取对象失败");
                return null;
            }
            if (!heroCharacterViewObj.TryGetComponent<HeroCharacterView>(out var heroCharacterView))
            {
                LogError("从对象池获取的对象不包含 HeroCharacterView 组件");
                ObjectPool.Return(heroCharacterViewObj);
                return null;
            }
            return heroCharacterView;
        }
        
        public void ReturnHeroCharacterView(HeroCharacterView heroCharacterView) => ObjectPool.Return(heroCharacterView.gameObject);
    }
}
