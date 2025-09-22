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
        [Header("英雄数据资产列表"),SerializeField] private List<HeroCharacterData> heroCharacterDataList = new();
        [Header("敌人角色数据列表"), SerializeField] private List<EnemyCharacterData> enemyCharacterDataList = new();
        [Header("卡牌数据列表"), SerializeField] private List<CardData> cardDataList = new();
        [Header("初始手牌数量"), SerializeField] private int initialHandSize = 5;
        private CardSystem cardSystem;
        private HeroSystem heroSystem;
        private EnemySystem enemySystem;
        private void Awake()
        {
            cardSystem = GetOrAddComponent<CardSystem>(FindComponentMode.Scene);
            heroSystem = GetOrAddComponent<HeroSystem>(FindComponentMode.Scene);
            enemySystem = GetOrAddComponent<EnemySystem>(FindComponentMode.Scene);
            cardSystem.Setup(cardDataList);
            heroSystem.LoadHeroCharacterModel(heroCharacterDataList);
            enemySystem.LoadEnemyCharacterModel(enemyCharacterDataList);
        }

        private void Start()
        {
            ActionSystem.Instance.PerformGameAction(new DrawCardGA(initialHandSize));
        }
    }
}
