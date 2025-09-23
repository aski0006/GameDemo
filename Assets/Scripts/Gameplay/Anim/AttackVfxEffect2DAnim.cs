using Animancer;
using AsakiFramework;
using System;
using UnityEngine;

namespace Gameplay.Anim
{
    public class AttackVfxEffect2DAnim : AsakiMono
    {
        [NotNullComponent, Header("精灵渲染器"), SerializeField] private SpriteRenderer spriteRenderer;
        [Header("特效动画片段"), SerializeField] private AnimationClip animationClip;
        [Header("Animancer组件"), SerializeField] private AnimancerComponent animancerComponent;

        private AnimancerState state;
        private void Awake()
        {
            animancerComponent ??= GetOrAddComponent<AnimancerComponent>();
            HasNotNullComponent(spriteRenderer);
        }

        public void PlayEffectAnim(Action OnFinished = null)
        {
            gameObject.SetActive(true);
            if (state != null) state.Stop();
            state = animancerComponent.Play(animationClip);
            state.OwnedEvents.OnEnd = () =>
            {
                OnFinished?.Invoke();
                forceStop();
            };
        }

        public void forceStop()
        {
            gameObject.SetActive(false);
            animancerComponent.Stop();
        }
    }
}
