using System;
using System.Collections.Generic;
using UnityEngine;

namespace AsakiFramework
{
    public class AsakiMonoServiceLocator : Singleton<AsakiMonoServiceLocator>
    {
        private Dictionary<Type, AsakiMono> asakiMonoServices = new();

        public bool TryRegisterService<T>(T service) where T : AsakiMono
        {
            var asakiType = typeof(T);
            if (asakiMonoServices.ContainsKey(asakiType))
            {
                Debug.LogError("重复注册 AsakiMono 服务: " + asakiType.Name);
                return false;
            }
            asakiMonoServices.Add(asakiType, service);
            Debug.Log("注册 AsakiMono 服务: " + asakiType.Name);
            return true;
        }

        public void RemoveService<T>() where T : AsakiMono
        {
            var asakiType = typeof(T);
            if (!asakiMonoServices.Remove(asakiType))
            {
                Debug.LogError("未注册 AsakiMono 服务: " + asakiType.Name);
                return;
            }
            Debug.Log("移除 AsakiMono 服务: " + asakiType.Name);
        }

        public T GetService<T>() where T : AsakiMono
        {
            var asakiType = typeof(T);
            if (!asakiMonoServices.TryGetValue(asakiType, out var service))
            {
                Debug.LogError("未注册 AsakiMono 服务: " + asakiType.Name);
                return null;
            }
            return (T)service;
        }

        protected override void OnDestroy()
        {
            asakiMonoServices.Clear(); // 销毁所有服务
            base.OnDestroy();
        }
    }
}
