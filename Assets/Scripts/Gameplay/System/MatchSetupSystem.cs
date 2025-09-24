using AsakiFramework;
using Gameplay.Data;
using Gameplay.GA;
using Gameplay.MVC.Controller;
using Gameplay.MVC.Model;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.System
{
    public class MatchSetupSystem : AsakiMono
    {
        [Header("英雄数据资产列表")][SerializeField] private List<HeroCharacterData> heroCharacterDataList = new List<HeroCharacterData>();
        [Header("敌人角色数据列表")][SerializeField] private List<EnemyCharacterData> enemyCharacterDataList = new List<EnemyCharacterData>();
        [Header("初始手牌数量")][SerializeField] private int initialHandSize = 8;
        private readonly List<CardData> cardDataList = new List<CardData>();
        private CardSystem cardSystem;
        private EnemySystem enemySystem;
        private HeroSystem heroSystem;
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
        private void CollectHeroCardData()
        {
            LogInfo("收集英雄持有的卡牌数据");
            LogInfo($"英雄数量 :{heroSystem.GetAllHeroControllers().Count}");
            foreach (HeroCharacterController hero in heroSystem.GetAllHeroControllers())
            {
                HeroCharacterModel model = hero.GetModel<HeroCharacterModel>();
                var holdCardDataList = model.HoldCard;
                LogInfo($"收集到的数量 :{holdCardDataList.Capacity}");
                cardDataList.AddRange(holdCardDataList);
            }
        }

       
    }
}
