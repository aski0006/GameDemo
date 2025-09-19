using System;
using UnityEngine;

namespace AsakiFramework.ObjectPool
{
    [Serializable]
    public class ObjectPoolConfig
    {
        [Header("对象池名称"), SerializeField] private string poolName = "DefaultPool";
        [Header("初始容量"), SerializeField, Min(0)] private int initialCapacity = 10;
        [Header("最大容量"), SerializeField, Min(1)] private int maxCapacity = 100;
        [Header("预设体（仅GameObject和Component池需要）"), SerializeField] private GameObject prefab;
        
        public string PoolName => poolName;
        public int InitialCapacity => initialCapacity;
        public int MaxCapacity => maxCapacity;
        public GameObject Prefab => prefab;
    }
    
}
