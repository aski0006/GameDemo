using AsakiFramework;
using Gameplay.MVC.Controller;
using Gameplay.MVC.Interfaces;
using Gameplay.MVC.Model;
using Gameplay.MVC.View;
using UnityEditor;

namespace Gameplay.System
{
    public class DeathSystem : AsakiMono
    {
        private EnemySystem enemySystem;

        private HeroSystem heroSystem;
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
            HeroCharacterController heroCtrl = heroSystem.GetHeroControllerById(instanceId);
            if (heroCtrl == null)
            {
                LogWarning($"找不到英雄控制器 ID: {instanceId}");
                return;
            }
            HeroCharacterView heroView = heroCtrl.GetView<HeroCharacterView>();
            if (heroView == null)
            {
                LogWarning("找不到英雄视图");
                return;
            }
            heroView.PlayDeathAnimation(
                0.5f,
                () =>
                {
                    heroSystem.RemoveHeroById(instanceId);
                }
            );
        }
        private void HandleEnemyDeath(GUID instanceId)
        {
            EnemyCharacterController enemyCtrl = enemySystem.GetEnemyControllerById(instanceId);
            if (enemyCtrl == null)
            {
                LogWarning($"找不到敌人控制器 ID: {instanceId}");
                return;
            }
            EnemyCharacterView enemyView = enemyCtrl.GetView<EnemyCharacterView>();
            if (enemyView == null)
            {
                LogWarning("找不到敌人视图");
                return;
            }
            enemyView.PlayDeathAnimation(
                0.5f,
                () =>
                {
                    enemySystem.RemoveEnemyById(instanceId);
                }
            );
        }
    }
}
