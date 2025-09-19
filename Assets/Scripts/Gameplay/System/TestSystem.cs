using AsakiFramework;
using Gameplay.Services;
using Gameplay.View;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Gameplay.System
{
    public class TestSystem : AsakiMono
    {
        [SerializeField] private CardViewCreator cardViewCreator;
        [SerializeField] private HandViewer handViewer;
        private void Update()
        {
            if (Keyboard.current.spaceKey.wasPressedThisFrame &&
                cardViewCreator != null &&
                handViewer != null)
            {
                var cardView = cardViewCreator.CreateCardView(transform.position, Quaternion.identity);
                if (cardView == null)
                {
                    LogError("创建卡牌视图失败");
                    return;
                }
                StartCoroutine(handViewer.AddCardViewToHandView(cardView));
            }
        }
    }
}
