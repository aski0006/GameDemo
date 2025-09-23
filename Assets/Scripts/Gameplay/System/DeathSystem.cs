using AsakiFramework;
using Gameplay.Model;
using Gameplay.View;
using System;
using System.Collections;
using UnityEditor;

namespace Gameplay.System
{
    public class DeathSystem : AsakiMono
    {

        private HeroSystem heroSystem;
        private EnemySystem enemySystem;
        private void Awake()
        {
            heroSystem = FromScene<HeroSystem>();
            enemySystem = FromScene<EnemySystem>();
        }
        private void OnEnable()
        {
            EventBus.Instance.Subscribe<CombatantModel.CombatantModelDeathEvent>(OnCombatantDeathHandler);
        }
        private void OnDisable()
        {
            EventBus.Instance?.Unsubscribe<CombatantModel.CombatantModelDeathEvent>(OnCombatantDeathHandler);
        }
        private void OnCombatantDeathHandler(CombatantModel.CombatantModelDeathEvent evt)
        {
            switch (evt.Type)
            {
                case CombatantType.Hero:
                    HandleHeroDeath(evt.InstanceID);
                    break;
                case CombatantType.Enemy:
                    HandleEnemyDeath(evt.InstanceID);
                    break;
            }
        }
        private void HandleHeroDeath(GUID instanceId)
        {
            var heroCtrl = heroSystem.GetHeroControllerById(instanceId);
            if (heroCtrl == null)
            {
                LogWarning($"找不到英雄控制器 ID: {instanceId}");
                return;
            }
            var heroView = heroCtrl.GetView<HeroCharacterView>();
            if (heroView == null)
            {
                LogWarning("找不到英雄视图");
                return;
            }
            heroView.PlayDeathAnimation(
                duration: 0.5f,
                onFinish: () =>
                {
                    heroSystem.RemoveHeroById(instanceId);
                }
            );
        }
        private void HandleEnemyDeath(GUID instanceId)
        {
            var enemyCtrl = enemySystem.GetEnemyControllerById(instanceId);
            if (enemyCtrl == null)
            {
                LogWarning($"找不到敌人控制器 ID: {instanceId}");
                return;
            }
            var enemyView = enemyCtrl.GetView<EnemyCharacterView>();
            if (enemyView == null)
            {
                LogWarning("找不到敌人视图");
                return;
            }
            enemyView.PlayDeathAnimation(
                duration: 0.5f,
                onFinish: () =>
                {
                    enemySystem.RemoveEnemyById(instanceId);
                }
            );
        }
    }
}
