using AsakiFramework;
using Data;
using Gameplay.Controller;
using Gameplay.View;
using Model;
using System;
using UnityEngine;

namespace Gameplay.System
{
    public class TestSystem : AsakiMono
    {
        [SerializeField] private HeroCharacterData heroData;
        [SerializeField] private HeroCharacterView heroView;
        
        private HeroCharacterController heroController;

        private void Awake()
        {
            if (heroData == null || heroView == null)
            {
                LogError("HeroCharacterData or HeroCharacterView is not assigned in the inspector.");
                return;
            }
            heroController = new HeroCharacterController(new CombatantModel(heroData), heroView);
        }
        
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                heroController.TakeDamage(10);
                var model = heroController.GetModel<CombatantModel>();
                if (model == null)
                {
                    LogError("Failed to get CombatantModel from HeroCharacterController.");
                }
                else
                {
                    LogInfo($"Current HP: {model.CurrentHp}, Max HP: {model.MaxHp}");
                }
                
            }
            if (Input.GetKeyDown(KeyCode.R))
            {
                heroController.Heal(10);
                var model = heroController.GetModel<CombatantModel>();
                if (model == null)
                {
                    LogError("Failed to get CombatantModel from HeroCharacterController.");
                }
                else
                {
                    LogInfo($"Current HP: {model.CurrentHp}, Max HP: {model.MaxHp}");
                }
            }
            if (Input.GetKeyDown(KeyCode.H))
            {
                heroController.ShowView();
                LogInfo("HeroCharacterView shown.");
            }
            if (Input.GetKeyDown(KeyCode.J))
            {
                heroController.HideView();
                LogInfo("HeroCharacterView hidden.");
            }
        }
    }
}
