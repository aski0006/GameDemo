using AsakiFramework;
using DG.Tweening;
using Gameplay.Creator;
using Gameplay.Data;
using Gameplay.GA;
using Gameplay.MVC.Controller;
using Gameplay.MVC.Model;
using Gameplay.MVC.View;
using Gameplay.View;
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
        [Header("敌人角色创建器")][SerializeField] private EnemyCharacterCreator enemyCharacterCreator;
        [Header("敌人插值区域视图")][SerializeField] private CombatantAreaView enemyAreaView;
        private readonly Dictionary<GUID, EnemyCharacterController> enemyIdToController = new Dictionary<GUID, EnemyCharacterController>();
        private HeroSystem heroSystem;
        private bool isProcessingQueue;
        private readonly Queue<EnemyCharacterData> pendingEnemyCreationQueue = new Queue<EnemyCharacterData>();

        private void Awake()
        {
            heroSystem = FromScene<HeroSystem>();
            AutoRegister<EnemySystem>();
        }
        private void OnEnable()
        {
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
            foreach (EnemyCharacterController enemy in enemyIdToController.Values)
            {
                LogInfo($"{enemy.GetModel<EnemyCharacterModel>().Name}开始攻击英雄");
                AttackTargetHeroGA attackTargetHeroGa = new AttackTargetHeroGA(enemy);
                enemyTurnGa.AddPerformReaction(attackTargetHeroGa);
            }
            yield return null;
        }

        private IEnumerator AttackTargetHeroPerformer(AttackTargetHeroGA attackTargetHeroGa)
        {
            EnemyCharacterController attacker = attackTargetHeroGa.Attacker;
            EnemyCharacterView attackerView = attacker.GetView<EnemyCharacterView>();
            Tween tween = attackerView.transform.DOMoveX(attackerView.transform.position.x - 1f, 0.15f);
            yield return tween.WaitForCompletion();
            attackerView.transform.DOMoveX(attackerView.transform.position.x + 1f, 0.25f);
            EnemyCharacterModel enemy = attacker.GetModel<EnemyCharacterModel>();
            var copyHerps = new List<CombatantBaseController>(heroSystem.GetAllHeroControllers());
            CombatantBaseController heroController = copyHerps.ElementAt(UnityEngine.Random.Range(0, copyHerps.Count));
            if (heroController == null) yield break;
            InjuryHasSourceGA injuryHasSourceGa = new InjuryHasSourceGA(enemy.CurrentAtk, attacker, new List<CombatantBaseController> { heroController });
            attackTargetHeroGa.AddPerformReaction(injuryHasSourceGa);
        }


        #region 创建敌人角色

        public void LoadEnemyCharacterModel(List<EnemyCharacterData> dataList, Action onComplete = null)
        {
            EnemyCreatorHandler handler = new EnemyCreatorHandler(this);
            CreateOverFrames(
                dataList,
                handler,
                3,
                8f,
                (cur, total) => LogInfo($"Enemy 创建进度 {cur}/{total}"),
                results => onComplete?.Invoke());
        }

        private sealed class EnemyCreatorHandler : IFrameCreationHandler<EnemyCharacterData, EnemyCharacterController>
        {
            private readonly EnemySystem _owner;
            public EnemyCreatorHandler(EnemySystem owner)
            {
                _owner = owner;
            }
            public EnemyCharacterController Create(EnemyCharacterData data)
            {
                EnemyCharacterView view = _owner.enemyCharacterCreator.CreateEnemyCharacterView(
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

                EnemyCharacterModel model = new EnemyCharacterModel(data);
                EnemyCharacterController ctrl = new EnemyCharacterController(model, view);
                _owner.enemyIdToController.TryAdd(ctrl.modelId, ctrl);
                _owner.LogInfo("创建敌人 ID" + ctrl.modelId);
                return ctrl;
            }
            public void OnError(EnemyCharacterData data, Exception e)
            {
                _owner.LogError($"创建敌人失败：{data.name}，错误：{e}");
            }
        }

        /// <summary>
        ///     处理等待队列中的敌人创建请求
        /// </summary>
        private IEnumerator ProcessPendingEnemyCreationQueue()
        {
            isProcessingQueue = true;

            while (pendingEnemyCreationQueue.Count > 0)
            {
                // 检查是否有空位
                if (enemyAreaView.HasAvailableSlot())
                {
                    EnemyCharacterData data = pendingEnemyCreationQueue.Dequeue();
                    LogInfo($"从队列中创建等待的敌人: {data.name}");

                    // 创建敌人
                    EnemyCharacterView view = enemyCharacterCreator.CreateEnemyCharacterView(Vector3.zero, Quaternion.identity);
                    if (enemyAreaView.TryRegister(view))
                    {
                        EnemyCharacterModel model = new EnemyCharacterModel(data);
                        EnemyCharacterController ctrl = new EnemyCharacterController(model, view);
                        enemyIdToController.TryAdd(ctrl.modelId, ctrl);
                    }
                    else
                    {
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

        public List<EnemyCharacterController> GetAllEnemyControllers()
        {
            return new List<EnemyCharacterController>(enemyIdToController.Values);
        }

        public EnemyCharacterController GetEnemyControllerById(GUID enemyId)
        {
            EnemyCharacterController ctrl = enemyIdToController.GetValueOrDefault(enemyId);
            if (ctrl == null)
            {
                // 仅在传入非默认 GUID 时记录错误，避免对 default(GUID) 的误报
                if (!enemyId.Equals(default(GUID)))
                    LogError("无法找到敌人，ID: " + enemyId);
            }
            return ctrl;
        }

        public EnemyCharacterController GetEnemyControllerByView(EnemyCharacterView view)
        {
            if (view == null) return null;
            GUID boundModelId = view.BoundModelInstanceID;
            return GetEnemyControllerById(boundModelId);
        }

        public void RemoveEnemyById(GUID enemyId)
        {
            EnemyCharacterController ctrl = enemyIdToController.GetValueOrDefault(enemyId);
            if (ctrl == null)
            {
                return;
            }
            EnemyCharacterView view = ctrl.GetView<EnemyCharacterView>();
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
            EnemyCharacterController ctrl = GetEnemyControllerByView(view);
            if (ctrl == null)
            {
                LogError("无法找到敌人，视图: " + view.name);
                return;
            }
            enemyAreaView.Unregister(view);
            enemyCharacterCreator.ReturnEnemyCharacterView(view);
            enemyIdToController.Remove(ctrl.modelId);

            // 移除敌人后检查是否有等待创建的敌人
            if (pendingEnemyCreationQueue.Count > 0 && !isProcessingQueue)
            {
                StartCoroutine(ProcessPendingEnemyCreationQueue());
            }

        }

        /// <summary>
        ///     获取当前等待创建的敌人数量
        /// </summary>
        public int GetPendingEnemyCount()
        {
            return pendingEnemyCreationQueue.Count;
        }

        /// <summary>
        ///     清空等待队列
        /// </summary>
        public void ClearPendingQueue()
        {
            pendingEnemyCreationQueue.Clear();
            LogInfo("敌人等待队列已清空");
        }

        #endregion
    }

}
