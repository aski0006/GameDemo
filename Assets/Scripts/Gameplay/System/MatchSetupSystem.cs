using AsakiFramework;
using Gameplay.Data;
using Gameplay.GA;
using Gameplay.Model;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.System
{
    public class MatchSetupSystem : AsakiMono
    {
        [Header("英雄数据资产列表"), SerializeField] private List<HeroCharacterData> heroCharacterDataList = new();
        [Header("敌人角色数据列表"), SerializeField] private List<EnemyCharacterData> enemyCharacterDataList = new();
        [Header("初始手牌数量"), SerializeField] private int initialHandSize = 8;
        private CardSystem cardSystem;
        private HeroSystem heroSystem;
        private EnemySystem enemySystem;
        private List<CardData> cardDataList = new();
        private void Awake()
        {
            cardSystem = FromScene<CardSystem>();
            heroSystem = FromScene<HeroSystem>();
            enemySystem = FromScene<EnemySystem>();
            heroSystem.LoadHeroCharacterModel(heroCharacterDataList, () =>
            {
                CollectHeroCardData();
                cardSystem.Setup(cardDataList);
            });
            enemySystem.LoadEnemyCharacterModel(enemyCharacterDataList);
        }

        private void Start()
        {
            ActionSystem.Instance.PerformGameAction(new DrawCardGA(initialHandSize));
        }

        //收集每个加载的英雄持有的卡牌数据资产
        public void CollectHeroCardData()
        {
            LogInfo("收集英雄持有的卡牌数据");
            LogInfo($"英雄数量 :{heroSystem.GetAllHeroControllers().Count}");
            foreach (var hero in heroSystem.GetAllHeroControllers())
            {
                var model = hero.GetModel<HeroCharacter>();
                List<CardData> holdCardDataList = model.HoldCard;
                LogInfo($"收集到的数量 :{holdCardDataList.Capacity}");
                cardDataList.AddRange(holdCardDataList);
            }
        }
    }
}
