using AsakiFramework;
using Data;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.System
{
    public class TestSystem : AsakiMono
    {
        [SerializeField] private List<CardData> testCardDatas;
        private CardSystem cardSystem;
        private void Awake()
        {
            cardSystem = GetOrAddComponent<CardSystem>(FindComponentMode.Scene);
            cardSystem.Setup(testCardDatas);
        }
    }
}
