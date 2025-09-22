using AsakiFramework;
using AsakiFramework.ObjectPool;
using DG.Tweening;
using Gameplay.View;
using Model;
using System;
using UnityEngine;

namespace Gameplay.Creator
{
    public class CardViewCreator : AsakiMono
    {
        [Header("卡牌视图对象池配置"), SerializeField] private ObjectPoolConfig poolConfig;

        private void Start()
        {
            ObjectPool.Create<CardViewer>(poolConfig.Prefab, poolConfig.InitialCapacity, poolConfig.MaxCapacity, poolConfig.PoolName);
        }

        public CardViewer CreateCardView(Card card, Vector3 position, Quaternion rotation, Transform parent = null)
        {
            if (parent == null) parent = transform;
            GameObject cardViewObj = ObjectPool.Get(poolConfig.Prefab, position, rotation, parent);
            if (cardViewObj == null)
            {
                LogError("从对象池获取对象失败");
                return null;
            }
            if (!cardViewObj.TryGetComponent<CardViewer>(out var cardView))
            {
                LogError("从对象池获取的对象不包含 CardViewer 组件");
                ObjectPool.Return(cardViewObj);
                return null;
            }
            cardViewObj.transform.localScale = Vector3.zero;
            cardViewObj.transform.DOScale(Vector3.one, 0.15f);
            cardView.Setup(card);
            return cardView;
        }
        
        public void ReturnCardView(CardViewer cardView) => ObjectPool.Return(cardView.gameObject);
    }
}
