using AsakiFramework;
using AsakiFramework.ObjectPool;
using DG.Tweening;
using Gameplay.UI;
using Gameplay.MVC.Model;
using System;
using UnityEngine;

namespace Gameplay.Creator
{
    public class CardViewCreator : AsakiMono
    {
        [Header("卡牌视图对象池配置"), SerializeField] private ObjectPoolConfig objectPoolConfig;

        private void Start()
        {
            ObjectPool.Create<CardViewer>(objectPoolConfig);
        }

        public CardViewer CreateCardView(CardModel cardModel, Vector3 position, Quaternion rotation, Transform parent = null)
        {
            if (!parent)
            {
                parent = objectPoolConfig.Parent == null ? transform : objectPoolConfig.Parent;
            }
            GameObject cardViewObj = ObjectPool.Get(objectPoolConfig.Prefab, position, rotation, parent);
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
            cardView.BindModel(cardModel);
            return cardView;
        }
        
        public void ReturnCardView(CardViewer cardView) => ObjectPool.Return(cardView.gameObject);
    }
}
