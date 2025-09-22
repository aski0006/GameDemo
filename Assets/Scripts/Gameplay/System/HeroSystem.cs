using AsakiFramework;
using Data;
using Gameplay.Controller;
using Gameplay.Creator;
using Gameplay.View;
using Model;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.System
{
    public class HeroSystem : AsakiMono
    {
        
        [Header("英雄视图创建器"), NotNullComponent]
        [SerializeField] private HeroCharacterCreator heroCharacterCreator;
        [Header("英雄区域"), SerializeField] private CombatantAreaView heroArea;
        
        private Dictionary<HeroCharacter, HeroCharacterController> heroCharacterControllerDict = new();
        
        #region 创建英雄角色
        public void LoadHeroCharacterModel(List<HeroCharacterData> dataList)
        {
            var handler = new HeroCreatorHandler(this);
            CreateOverFrames(
                source: dataList,
                handler: handler,
                perFrame: 3,
                maxMillisPerFrame: 8f,
                onProgress: (cur, total) => LogInfo($"Hero 创建进度 {cur}/{total}"),
                onComplete: results => LogInfo($"全部 Hero 创建完成，共 {results.Count} 个"));
        }

        /// <summary>
        /// 具体创建逻辑
        /// </summary>
        private sealed class HeroCreatorHandler : IFrameCreationHandler<HeroCharacterData, HeroCharacterController>
        {
            private readonly HeroSystem _owner;
            public HeroCreatorHandler(HeroSystem owner) => _owner = owner;

            public HeroCharacterController Create(HeroCharacterData data)
            {
                var model = new HeroCharacter(data);
                var view = _owner.heroCharacterCreator.CreateHeroCharacterView(
                    Vector3.zero, Quaternion.identity);
                if (_owner.heroArea.TryRegister(view) == false)
                {
                    // 英雄区域已满
                    _owner.heroCharacterCreator.ReturnHeroCharacterView(view);
                    return null;
                }
                var ctrl = new HeroCharacterController(model, view);
                _owner.heroCharacterControllerDict.Add(model, ctrl);
                return ctrl;
            }

            public void OnError(HeroCharacterData data, Exception e)
            {
                _owner.LogError($"创建英雄失败：{data.name}，错误：{e}");
            }
        }
        #endregion
        
        public List<HeroCharacterController> GetAllHeroControllers() => new List<HeroCharacterController>(heroCharacterControllerDict.Values);

        public void RemoveHero(HeroCharacter model)
        {
            if (heroCharacterControllerDict.TryGetValue(model, out var ctrl))
            {
                var view = ctrl.GetView<HeroCharacterView>();
                heroArea.Unregister(view);
                heroCharacterCreator.ReturnHeroCharacterView(view);
                heroCharacterControllerDict.Remove(model);
            }
        }

    }
}
