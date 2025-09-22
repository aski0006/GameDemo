using AsakiFramework;
using AsakiFramework.ObjectPool;
using Data;
using Gameplay.Controller;
using Gameplay.Creator;
using Gameplay.GA;
using Gameplay.View;
using Gameplay.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Gameplay.System
{
    public class EnemySystem : AsakiMono
    {
        [Header("敌人角色创建器"), SerializeField] private EnemyCharacterCreator enemyCharacterCreator;
        [Header("敌人插值区域视图"), SerializeField] private CombatantAreaView enemyAreaView;
        private Dictionary<EnemyCharacterView, EnemyCharacterController> enemyViews = new Dictionary<EnemyCharacterView, EnemyCharacterController>();
        private Queue<EnemyCharacterData> pendingEnemyCreationQueue = new Queue<EnemyCharacterData>();
        private bool isProcessingQueue = false;

        private void OnEnable()
        {
            ActionSystem.Instance.AttachPerformer<EnemyTurnGA>(EnemyTurnPerformer);
        }

        private void OnDisable()
        {
            ActionSystem.Instance?.DetachPerformer<EnemyTurnGA>();
        }

        public void Setup(List<EnemyCharacterData> dataList)
        { }
        private IEnumerator EnemyTurnPerformer(EnemyTurnGA enemyTurnGa)
        {
            LogInfo("Enemy Turn Started");
            yield return new WaitForSeconds(2f);
            LogInfo("Enemy Turn Ended");
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
                _owner.enemyViews.Add(view, ctrl);
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
                        enemyViews.Add(view, ctrl);
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

        public List<EnemyCharacterController> GetAllEnemyControllers() => new List<EnemyCharacterController>(enemyViews.Values);

        public void RemoveEnemy(EnemyCharacterView view)
        {
            enemyAreaView.Unregister(view);
            enemyCharacterCreator.ReturnEnemyCharacterView(view);
            enemyViews.Remove(view, out var controller);

            LogInfo($"敌人已移除，空出槽位");

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
    }

}
