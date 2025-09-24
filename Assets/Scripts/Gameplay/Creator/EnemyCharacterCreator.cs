using AsakiFramework;
using AsakiFramework.ObjectPool;
using Gameplay.MVC.View;
using UnityEngine;

namespace Gameplay.Creator
{
    public class EnemyCharacterCreator : AsakiMono
    {
        [Header("敌人角色对象池配置"), SerializeField] private ObjectPoolConfig objectPoolConfig;
        private void Awake()
        {
            ObjectPool.Create<EnemyCharacterView>(
                prefab: objectPoolConfig.Prefab,
                initialSize: objectPoolConfig.InitialCapacity,
                maxSize: objectPoolConfig.MaxCapacity,
                poolName: objectPoolConfig.PoolName
            );
        }

        public EnemyCharacterView CreateEnemyCharacterView(Vector3 position, Quaternion rotation, Transform parent = null)
        {
            if (!parent)
            {
                parent = objectPoolConfig.Parent == null ? transform : objectPoolConfig.Parent;
            }
            GameObject enemyCharacterViewObj =
                ObjectPool.Get(objectPoolConfig.Prefab, position, rotation, parent);
            if (enemyCharacterViewObj == null)
            {
                LogError("从对象池获取敌人对象失败");
                return null;
            }
            if (!enemyCharacterViewObj.TryGetComponent<EnemyCharacterView>(out var enemyCharacterView))
            {
                LogError("从对象池获取的对象不包含 EnemyCharacterView 组件");
                ObjectPool.Return(enemyCharacterViewObj);
                return null;
            }
            return enemyCharacterView;
        }

        public void ReturnEnemyCharacterView(EnemyCharacterView enemyCharacterView) => ObjectPool.Return(enemyCharacterView.gameObject);
    }
}
