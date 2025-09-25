using AsakiFramework;
using Gameplay.MVC.Model;
using System;
using System.Collections.Generic;

namespace Gameplay.System
{
    public class CollectionSystem : AsakiMono
    {
        private List<CollectionModel> collectionModels = new();
        private void Awake()
        {
            AutoRegister<CollectionSystem>();
        }

        public void Add(CollectionModel model)
        {
            collectionModels.Add(model);
            model.OnAdd();
        }
        public void Remove(CollectionModel model)
        {
            collectionModels.Remove(model);
            model.OnRemove();
        }
    }
}
