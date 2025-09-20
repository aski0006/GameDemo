using AsakiFramework;
using Gameplay.GA;
using System;
using System.Collections;
using UnityEngine;

namespace Gameplay.System
{
    public class EnemySystem : AsakiMono
    {

        private void OnEnable()
        {
            ActionSystem.Instance.AttachPerformer<EnemyTurnGA>(EnemyTurnPerformer);
        }

        private void OnDisable()
        {
            ActionSystem.Instance?.DetachPerformer<EnemyTurnGA>();
        }

        private IEnumerator EnemyTurnPerformer(EnemyTurnGA enemyTurnGa)
        {
            LogInfo("Enemy Turn Started");
            yield return new WaitForSeconds(2f);
            LogInfo("Enemy Turn Ended");
        }
    }
}
