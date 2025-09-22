using AsakiFramework;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.View
{
    public class CombatantAreaView : AsakiMono
    {
        [Serializable]
        private sealed class Slot
        {
            [SerializeField] private Transform anchor;
            private CombatantViewBase occupant;
            public CombatantViewBase Occupant => occupant;
            public Transform Anchor
            {
                get => anchor;
                set => anchor = value;
            }

            public bool IsFree => occupant == null;
            public void Occupy(CombatantViewBase view) => occupant = view;
            public void Clear() => occupant = null;
            public void Apply(CombatantViewBase view)
            {
                view.transform.SetParent(anchor);
                view.transform.localPosition = Vector3.zero;
                view.transform.localRotation = Quaternion.identity;
                view.transform.localScale = Vector3.one;
                Debug.Log("应用插槽位置" + anchor.name);
            }
        }

        [Header("插槽锚点"), SerializeField] private List<Transform> slotAnchors = new List<Transform>();

        private readonly List<Slot> slots = new List<Slot>();
        private readonly Queue<int> freeIndexQueue = new Queue<int>();

        /* ---------------------------------------------------------- */
        private void Awake()
        {
            BuildSlots();
        }

        /* ---------------------------------------------------------- */
        public bool TryRegister(CombatantViewBase view)
        {
            if (view == null) return false;

            // 已注册则直接返回
            foreach (var s in slots)
                if (s.Occupant == view)
                    return true;

            if (freeIndexQueue.Count == 0) return false;

            int idx = freeIndexQueue.Dequeue(); // 按顺序取出
            var slot = slots[idx];
            slot.Occupy(view);
            slot.Apply(view);
            return true;
        }

        public void Unregister(CombatantViewBase view)
        {
            if (view == null) return;

            for (int i = 0; i < slots.Count; ++i)
            {
                if (slots[i].Occupant != view) continue;

                slots[i].Clear();
                freeIndexQueue.Enqueue(i); // 归还队列
                view.Hide();
                view.transform.SetParent(null);
                return;
            }
        }

        public int OccupiedCount => slots.Count - freeIndexQueue.Count;
        public int TotalCount => slots.Count;

        /* ---------------------------------------------------------- */
        private void BuildSlots()
        {
            slots.Clear();
            freeIndexQueue.Clear();

            for (int i = 0; i < slotAnchors.Count; ++i)
            {
                slots.Add(new Slot { Anchor = slotAnchors[i] });
                freeIndexQueue.Enqueue(i); // 按顺序入队
            }
        }

        /* ---------------------------------------------------------- */
        #region 编辑器工具

#if UNITY_EDITOR
        [ContextMenu("收集子节点作为锚点")]
        private void CollectChildrenAsAnchors()
        {
            slotAnchors.Clear();
            foreach (Transform t in transform)
                slotAnchors.Add(t);
            BuildSlots();
        }
#endif

        #endregion
        public bool HasAvailableSlot() => freeIndexQueue.Count > 0;
    }
}
