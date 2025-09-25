using Gameplay.MVC.Data;
using Gameplay.MVC.Interfaces;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using Gameplay.Common.Target;
using Gameplay.Interfaces;
using Gameplay.MVC.Controller;
using AsakiFramework; // for ActionSystem
using Gameplay.GA;

namespace Gameplay.MVC.Model
{
    public class CollectionModel : IModel, ICollectionReadOnly
    {
        public GUID ModelInstanceID { get; } = GUID.Generate();
        private CollectionData data;

        // 实现 ICollectionReadOnly 接口属性
        public Sprite CollectionSprite => data ? data.CollectionSprite : null;
        public CollectionTriggerCondition TriggerCondition => data ? data.TriggerCondition : null;
        public AutoTargetEffect CollectionEffect => data ? data.CollectionEffect : null;
        public bool UseAutoTarget => data && data.UseAutoTarget;
        public bool UseActionCasterAsTarget => data && data.UseActionCasterAsTarget;

        public CollectionModel() { } // 允许创建空的模型，后续再绑定数据
        public CollectionModel(CollectionData data) { BindData(data); }
        public void BindData(CollectionData collectionData)
        {
            data = collectionData;
        }

        public void Reaction(GameAction gameAction)
        {
            if (data == null) return;
            if (TriggerCondition.SubConditionIsMet(gameAction) == false) return;

            // 链路限制：如果 action 的 OriginPath 中包含自己（说明是由自己产生的），则跳过，避免自触发。
            if (gameAction.OriginPath.Contains(this.ModelInstanceID)) return;

            // 链深度限制：如果 action 的 ChainDepth 已达到 CollectionData.MaxChainDepth，则不再传播/触发。
            int maxChain = data.MaxChainDepth;
            if (maxChain > 0 && gameAction.ChainDepth >= maxChain) return;

            List<CombatantBaseController> targets = new();
            CombatantBaseController caster = null;
            if (gameAction is IHasCaster hasCaster)
            {
                caster = hasCaster.Caster;
            }

            if (UseActionCasterAsTarget && caster != null)
            {
                targets.Add(caster);
            }
            if (UseAutoTarget && CollectionEffect != null && CollectionEffect.TargetMode != null)
            {
                targets.AddRange(CollectionEffect.TargetMode.GetTargets());
            }

            // 创建 PerformEffectGA，并把本藏品 id 作为 origin 填入（以便 EffectSystem 将它 propagate 到子 GA）
            var performGA = new PerformEffectGA(CollectionEffect?.Effect, targets, caster, ModelInstanceID);

            // 标记 performGA 为生成的动作（可选），并控制是否传播：如果当前藏品配置不允许传播，则在 TriggerCondition 里阻断
            performGA.IsGenerated = true;

            ActionSystem.Instance.PerformGameAction(performGA);
        }
        public void OnAdd()
        {
            var actionType = CollectionEffect?.Effect?.ActionType;
            TriggerCondition.SubGACondition(Reaction, actionType);
        }

        public void OnRemove()
        {
            var actionType = CollectionEffect?.Effect?.ActionType;
            TriggerCondition.UnSubGACondition(Reaction, actionType);
        }
    }
}