using AsakiFramework;
using Data;
using Gameplay.GA;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.System
{
    public class MatchSetupSystem : AsakiMono
    {
        [Header("卡牌数据列表"), SerializeField] private List<CardData> cardDataList = new();
        [Header("初始手牌数量"), SerializeField] private int initialHandSize = 5;
        private CardSystem cardSystem;

        private void Awake()
        {
            cardSystem = GetOrAddComponent<CardSystem>(FindComponentMode.Scene);
            cardSystem.Setup(cardDataList);
        }

        private void Start()
        {
            ActionSystem.Instance.PerformGameAction(new DrawCardGA(initialHandSize));
        }
    }
}
