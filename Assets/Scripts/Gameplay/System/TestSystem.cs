using AsakiFramework;
using Data;
using Gameplay.Services;
using Gameplay.View;
using Model;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Gameplay.System
{
    public class TestSystem : AsakiMono
    {
        [SerializeField] private CardViewCreator cardViewCreator;
        [SerializeField] private CardData testCardData;
        private void Update()
        {
            if (Keyboard.current.spaceKey.wasPressedThisFrame &&
                cardViewCreator != null)
            {
                var cardView = cardViewCreator.CreateCardView(
                    new CardModel(testCardData),
                    transform.position, 
                    Quaternion.identity);
                if (cardView == null)
                {
                    LogError("创建卡牌视图失败");
                    return;
                }
                EventBus.Instance.Trigger(new HandViewer.AddCardViewToHandViewEvent{cardView = cardView});
            }
        }
    }
}
