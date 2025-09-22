using AsakiFramework;
using Gameplay.View;
using UnityEngine;

namespace Gameplay.System
{
    public class TestSystem : AsakiMono
    {
        private HeroSystem heroSystem;
        private EnemySystem enemySystem;
        private void Awake()
        {
            heroSystem = GetOrAddComponent<HeroSystem>(FindComponentMode.Scene);
            enemySystem = GetOrAddComponent<EnemySystem>(FindComponentMode.Scene);
        }

        private void Start()
        {
            // 延迟一帧执行，确保所有系统已初始化
            RunNextFrame(RunTest);
        }

        private void RunTest()
        {
            LogInfo("开始测试角色进场和离场...");
            // 延迟一段时间后测试离场
            DelayTime(3, TestExit);
        }

        private void TestExit()
        {
            LogInfo("开始测试角色离场...");


            var heroList = heroSystem.GetAllHeroControllers();
            if (heroList.Count > 0)
            {
                var heroToRemove = heroList[0];
                heroSystem.RemoveHero(heroToRemove.GetView<HeroCharacterView>()); // 也需要在 HeroSystem 中实现 RemoveHero 方法
            }

            var enemyList = enemySystem.GetAllEnemyControllers();
            if (enemyList.Count > 0)
            {
                var enemyToRemove = enemyList[0];
                enemySystem.RemoveEnemy(enemyToRemove.GetView<EnemyCharacterView>()); // 也需要在 EnemySystem 中实现 RemoveEnemy 方法
            }

        }

#if UNITY_EDITOR
        [ContextMenu("手动触发测试")]
        private void ManualTest()
        {
            RunTest();
        }
#endif
    }
}
