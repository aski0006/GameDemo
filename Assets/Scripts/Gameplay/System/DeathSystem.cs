using AsakiFramework;
using Gameplay.Model;
using System;

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
            EventBus.Instance.Unsubscribe<CombatantModel.CombatantModelDeathEvent>(OnCombatantDeathHandler);
        }
        private void OnCombatantDeathHandler(CombatantModel.CombatantModelDeathEvent evt)
        {
            switch (evt.Type)
            {
                case CombatantType.Hero:
                    heroSystem.RemoveHeroById(evt.InstanceID);
                    break;
                case CombatantType.Enemy:
                    enemySystem.RemoveEnemyById(evt.InstanceID);
                    break;
            }
        }
    }
}
