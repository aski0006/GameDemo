using AsakiFramework;
using Gameplay.Data;
using Gameplay.GA;
using Gameplay.MVC.Controller;
using Gameplay.MVC.Data;
using Gameplay.MVC.Model;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.System
{
    public class MatchSetupSystem : AsakiMono
    {
        [Header("英雄角色数据资产")][SerializeField] private List<HeroCharacterData> heroCharacterDataList = new List<HeroCharacterData>();
        [Header("敌人角色数据资产")][SerializeField] private List<EnemyCharacterData> enemyCharacterDataList = new List<EnemyCharacterData>();
        [Header("藏品数据资产")] [SerializeField] private List<CollectionData> collectionDataList = new List<CollectionData>();
        [Header("初始手牌数量")][SerializeField] private int initialHandSize = 8;
        private readonly List<CardData> cardDataList = new List<CardData>();
        private CardSystem cardSystem;
        private EnemySystem enemySystem;
        private HeroSystem heroSystem;
        private CollectionSystem collectionSystem;
        private void Awake()
        {
            cardSystem = FromScene<CardSystem>();
            heroSystem = FromScene<HeroSystem>();
            enemySystem = FromScene<EnemySystem>();
            collectionSystem = FromScene<CollectionSystem>();
            LogInfo("英雄数据数量: " + heroCharacterDataList.Count);
            LogInfo("敌人数据数量: " + enemyCharacterDataList.Count);
            LogInfo("藏品数据数量: " + collectionDataList.Count);
            heroSystem.LoadHeroCharacterModel(heroCharacterDataList, () =>
            {
                CollectHeroCardData();
                cardSystem.Setup(cardDataList);
            });
            enemySystem.LoadEnemyCharacterModel(enemyCharacterDataList);
            RunNextFrame(LoadCollections);
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
            LogInfo($"卡牌数量 :{cardDataList.Count}");
        }
        
        private void LoadCollections()
        {
            LogInfo("加载收藏品");
            foreach(var data in collectionDataList)
            {
                var model = new CollectionModel(data);
                collectionSystem.Add(model);
            }
        }

       
    }
}
