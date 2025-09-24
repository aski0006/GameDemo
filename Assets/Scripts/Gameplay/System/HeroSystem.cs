using AsakiFramework;
using Gameplay.Data;
using Gameplay.MVC.Controller;
using Gameplay.Creator;
using Gameplay.MVC.View;
using Gameplay.MVC.Model;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Gameplay.System
{
    public class HeroSystem : AsakiMono
    {

        [Header("英雄视图创建器"), NotNullComponent]
        [SerializeField] private HeroCharacterCreator heroCharacterCreator;
        [Header("英雄区域"), SerializeField] private CombatantAreaView heroArea;

        private Dictionary<GUID, HeroCharacterController> heroIdToController = new();
        #region 创建英雄角色

        public void LoadHeroCharacterModel(List<HeroCharacterData> dataList, Action onComplete = null)
        {
            var handler = new HeroCreatorHandler(this);
            CreateOverFrames(
                source: dataList,
                handler: handler,
                perFrame: 3,
                maxMillisPerFrame: 8f,
                onProgress: (cur, total) => LogInfo($"Hero 创建进度 {cur}/{total}"),
                onComplete: results => onComplete?.Invoke());
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
                if (view == null) return null;
                if (_owner.heroArea.TryRegister(view) == false)
                {
                    // 英雄区域已满
                    _owner.heroCharacterCreator.ReturnHeroCharacterView(view);
                    return null;
                }
                var ctrl = new HeroCharacterController(model, view);
                _owner.heroIdToController.Add(ctrl.modelId, ctrl);
                return ctrl;
            }

            public void OnError(HeroCharacterData data, Exception e)
            {
                _owner.LogError($"创建英雄失败：{data.name}，错误：{e}");
            }
        }

        #endregion

        #region 英雄槽位管理

        public List<HeroCharacterController> GetAllHeroControllers() => new List<HeroCharacterController>(heroIdToController.Values);

        public HeroCharacterController GetHeroControllerById(GUID id)
        {
            if (heroIdToController.TryGetValue(id, out var ctrl))
            {
                return ctrl;
            }
            LogWarning($"尝试获取不存在的英雄角色 ID: {id}");
            return null;
        }
        
        public HeroCharacterController GetHeroControllerByView(HeroCharacterView view)
        {
            var boundModelId = view.BoundModelInstanceID;
            return GetHeroControllerById(boundModelId);
        }
        
        public void RemoveHeroByView(HeroCharacterView view)
        {
            var ctrl = GetHeroControllerByView(view);
            heroArea.Unregister(view);
            heroCharacterCreator.ReturnHeroCharacterView(view);
            heroIdToController.Remove(ctrl.modelId);
        }

        public void RemoveHeroById(GUID id)
        {
            if (heroIdToController.TryGetValue(id, out var ctrl))
            {
                var view = ctrl.GetView<HeroCharacterView>();
                heroArea.Unregister(view);
                heroCharacterCreator.ReturnHeroCharacterView(view);
                heroIdToController.Remove(id);
            }
        }

        #endregion

    }
}
