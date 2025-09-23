using AsakiFramework;
using Data;
using DG.Tweening;
using Gameplay.Controller;
using Gameplay.Creator;
using Gameplay.GA;
using Gameplay.View;
using Gameplay.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Gameplay.System
{
    public class EnemySystem : AsakiMono
    {
        [Header("敌人角色创建器"), SerializeField] private EnemyCharacterCreator enemyCharacterCreator;
        [Header("敌人插值区域视图"), SerializeField] private CombatantAreaView enemyAreaView;
        private Dictionary<GUID, EnemyCharacterController> enemyIdToController = new();
        private Queue<EnemyCharacterData> pendingEnemyCreationQueue = new Queue<EnemyCharacterData>();
        private bool isProcessingQueue = false;
        private HeroSystem heroSystem;

        private void Awake()
        {
            heroSystem = FromScene<HeroSystem>();
            AutoRegister<EnemySystem>();
        }
        private void OnEnable()
        {
            if (ActionSystem.Instance == null) LogInfo("ActionSystem.Instance is null, EnemySystem will not attach performers.");
            LogInfo("开始注册敌人系统的执行者");
            ActionSystem.Instance.AttachPerformer<EnemyTurnGA>(EnemyTurnPerformer);
            ActionSystem.Instance.AttachPerformer<AttackTargetHeroGA>(AttackTargetHeroPerformer);
        }

        private void OnDisable()
        {
            if (ActionSystem.Instance == null) return;
            ActionSystem.Instance.DetachPerformer<EnemyTurnGA>();
            ActionSystem.Instance.DetachPerformer<AttackTargetHeroGA>();
        }

        private IEnumerator EnemyTurnPerformer(EnemyTurnGA enemyTurnGa)
        {
            LogInfo("敌人数量: " + enemyIdToController.Count);
            foreach (var enemy in enemyIdToController.Values)
            {
                LogInfo($"{enemy.GetModel<EnemyCharacter>().Name}开始攻击英雄");
                AttackTargetHeroGA attackTargetHeroGa = new AttackTargetHeroGA(enemy);
                enemyTurnGa.AddPerformReaction(attackTargetHeroGa);
            }
            yield return null;
        }

        private IEnumerator AttackTargetHeroPerformer(AttackTargetHeroGA attackTargetHeroGa)
        {
            LogInfo("开始执行 AttackTargetHeroGA");
            var attacker = attackTargetHeroGa.Attacker;
            var attackerView = attacker.GetView<EnemyCharacterView>();
            Tween tween = attackerView.transform.DOMoveX(attackerView.transform.position.x - 1f, 0.15f);
            yield return tween.WaitForCompletion();
            attackerView.transform.DOMoveX(attackerView.transform.position.x + 1f, 0.25f);
            EnemyCharacter enemy = attacker.GetModel<EnemyCharacter>();
            var heroController = heroSystem.GetHeroControllerOrDefault(random: true);
            if(heroController == null) yield break;
            LogInfo("开始对英雄造成伤害");
            DealDamageGA dealDamageGa = new DealDamageGA(enemy.CurrentAtk, new List<CombatantBaseController> { heroController });
            attackTargetHeroGa.AddPerformReaction(dealDamageGa);
        }


        #region 创建敌人角色

        public void LoadEnemyCharacterModel(List<EnemyCharacterData> dataList)
        {
            var handler = new EnemyCreatorHandler(this);
            CreateOverFrames(
                source: dataList,
                handler: handler,
                perFrame: 3,
                maxMillisPerFrame: 8f,
                onProgress: (cur, total) => LogInfo($"Enemy 创建进度 {cur}/{total}"),
                onComplete: results => LogInfo($"全部 Enemy 创建完成，共 {results.Count} 个"));
        }

        private sealed class EnemyCreatorHandler : IFrameCreationHandler<EnemyCharacterData, EnemyCharacterController>
        {
            private EnemySystem _owner;
            public EnemyCreatorHandler(EnemySystem owner)
            {
                _owner = owner;
            }
            public EnemyCharacterController Create(EnemyCharacterData data)
            {
                var view = _owner.enemyCharacterCreator.CreateEnemyCharacterView(
                    Vector3.zero, Quaternion.identity);
                if (view == null) return null;
                if (_owner.enemyAreaView.TryRegister(view) == false)
                {
                    // 敌人区域已满，将数据加入缓存队列
                    _owner.LogWarning($"敌人区域已满，将敌人 {data.name} 加入等待队列");
                    _owner.enemyCharacterCreator.ReturnEnemyCharacterView(view);
                    _owner.pendingEnemyCreationQueue.Enqueue(data);

                    // 开始处理队列（如果还没开始）
                    if (!_owner.isProcessingQueue)
                    {
                        _owner.StartCoroutine(_owner.ProcessPendingEnemyCreationQueue());
                    }
                    return null;
                }

                var model = new EnemyCharacter(data);
                var ctrl = new EnemyCharacterController(model, view);
                _owner.enemyIdToController.Add(ctrl.modelId, ctrl);
                return ctrl;
            }
            public void OnError(EnemyCharacterData data, Exception e)
            {
                _owner.LogError($"创建敌人失败：{data.name}，错误：{e}");
            }
        }

        /// <summary>
        /// 处理等待队列中的敌人创建请求
        /// </summary>
        private IEnumerator ProcessPendingEnemyCreationQueue()
        {
            isProcessingQueue = true;

            while (pendingEnemyCreationQueue.Count > 0)
            {
                // 检查是否有空位
                if (enemyAreaView.HasAvailableSlot())
                {
                    var data = pendingEnemyCreationQueue.Dequeue();
                    LogInfo($"从队列中创建等待的敌人: {data.name}");

                    // 创建敌人
                    var view = enemyCharacterCreator.CreateEnemyCharacterView(Vector3.zero, Quaternion.identity);
                    if (enemyAreaView.TryRegister(view))
                    {
                        var model = new EnemyCharacter(data);
                        var ctrl = new EnemyCharacterController(model, view);
                        enemyIdToController.Add(ctrl.modelId, ctrl);
                    }
                    else
                    {
                        // 如果还是失败，重新放回队列并等待
                        LogWarning($"创建队列敌人时区域又满了，重新排队: {data.name}");
                        pendingEnemyCreationQueue.Enqueue(data);
                        enemyCharacterCreator.ReturnEnemyCharacterView(view);
                        yield return new WaitForSeconds(0.5f); // 等待一段时间再重试
                    }
                }
                else
                {
                    // 没有空位，等待一段时间再检查
                    yield return new WaitForSeconds(0.5f);
                }
            }

            isProcessingQueue = false;
            LogInfo("敌人创建队列处理完成");
        }

        #endregion

        #region 敌人槽位管理

        public List<EnemyCharacterController> GetAllEnemyControllers() => new List<EnemyCharacterController>(enemyIdToController.Values);
        
        public EnemyCharacterController GetEnemyControllerById(GUID enemyId)
        {
            var ctrl = enemyIdToController.GetValueOrDefault(enemyId);
            if (ctrl == null)
            {
                LogError("无法找到敌人，ID: " + enemyId);
            }
            return ctrl;
        }
        public void RemoveEnemyById(GUID enemyId)
        {
            var ctrl = enemyIdToController.GetValueOrDefault(enemyId);
            if (ctrl == null)
            {
                LogError("无法找到敌人，ID: " + enemyId);
                return;
            }
            var view = ctrl.GetView<EnemyCharacterView>();
            enemyAreaView.Unregister(view);
            enemyCharacterCreator.ReturnEnemyCharacterView(view);
            enemyIdToController.Remove(enemyId);
            if (pendingEnemyCreationQueue.Count > 0 && !isProcessingQueue)
            {
                StartCoroutine(ProcessPendingEnemyCreationQueue());
            }
        }
        public void RemoveEnemyByView(EnemyCharacterView view)
        {
            enemyAreaView.Unregister(view);
            enemyCharacterCreator.ReturnEnemyCharacterView(view);
            var ctrls = enemyIdToController.Values.ToList();
            var ctrl = ctrls.Find(c => c.GetView<EnemyCharacterView>() == view);
            enemyIdToController.Remove(ctrl.modelId);

            // 移除敌人后检查是否有等待创建的敌人
            if (pendingEnemyCreationQueue.Count > 0 && !isProcessingQueue)
            {
                StartCoroutine(ProcessPendingEnemyCreationQueue());
            }

        }

        /// <summary>
        /// 获取当前等待创建的敌人数量
        /// </summary>
        public int GetPendingEnemyCount() => pendingEnemyCreationQueue.Count;

        /// <summary>
        /// 清空等待队列
        /// </summary>
        public void ClearPendingQueue()
        {
            pendingEnemyCreationQueue.Clear();
            LogInfo("敌人等待队列已清空");
        }

        #endregion
    }

}
