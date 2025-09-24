using AsakiFramework;
using AsakiFramework.ObjectPool;
using DG.Tweening;
using Gameplay.MVC.Interfaces;
using Gameplay.UI;
using Gameplay.MVC.Model;
using System;
using System.Collections;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace Gameplay.MVC.View
{
    public class CombatantViewBase : AsakiMono, IPoolable, IView
    {
        [NotNullComponent, Header("战斗单位包装器"), SerializeField] public GameObject combatantWrapper;
        [NotNullComponent, Header("战斗单位精灵渲染器"), SerializeField] public SpriteRenderer combatantRenderer;
        [NotNullComponent, Header("战斗单位名称"), SerializeField] public TMP_Text combatantName;
        [NotNullComponent, Header("战斗单位血条"), SerializeField] public HPUI hpUI;

        public Action OnViewShow;
        public Action OnViewHide;
        protected CombatantModel boundModel;
        public GUID BoundModelInstanceID => boundModel != null ? boundModel.ModelInstanceID : default;

        // 管理受击动画序列，便于安全停止/替换
        private Sequence hitSequence;

        // 缓存“池化时”的初始状态（Awake 记录），作为恢复目标
        private Vector3 initialWrapperLocalPosition;
        private Vector3 initialWrapperLocalScale;
        private Color initialSpriteColor;

        #region Inspector 可调参数（受击特效）
        [Header("受击动画设置")]
        [SerializeField, Min(0f), Tooltip("抖动强度（单位：世界/局部距离）")] private float inspectorShakeStrength = 0.25f;
        [SerializeField, Min(0f), Tooltip("抖动总时长（秒）")] private float inspectorShakeDuration = 0.35f;
        [SerializeField, Min(1), Tooltip("抖动频率/震动次数（vibrato）")] private int inspectorVibrato = 10;
        [SerializeField, Range(0f, 180f), Tooltip("抖动随机性（仅 DOShake 时有效）")] private float inspectorRandomness = 90f;
        [SerializeField, Tooltip("使用 DOPunchPosition（推荐）代替 DOShakePosition")] private bool usePunchInsteadOfShake = true;

        [SerializeField, Tooltip("受击闪烁颜色")] private Color inspectorFlashColor = new Color(1f, 0.5f, 0.5f);
        [SerializeField, Min(0f), Tooltip("闪烁单次时长（秒），实际会做两次 Yoyo）")] private float inspectorFlashDuration = 0.12f;
        #endregion

        public void UnbindModel()
        {
            boundModel = null;
        }

        private void Awake()
        {
            HasNotNullComponent(combatantWrapper);
            HasNotNullComponent(combatantRenderer);
            HasNotNullComponent(combatantName);
            HasNotNullComponent(hpUI);

            // 记录“权威”的初始位置/缩放/颜色（只在 Awake 时记录一次）
            initialWrapperLocalPosition = combatantWrapper.transform.localPosition;
            initialWrapperLocalScale = combatantWrapper.transform.localScale;
            initialSpriteColor = combatantRenderer != null ? combatantRenderer.color : Color.white;
        }

        public void Show()
        {
            combatantWrapper.SetActive(true);
            hpUI.Show();
            OnViewShow?.Invoke();
        }

        public void Hide()
        {
            combatantWrapper.SetActive(false);
            hpUI.Hide();
            OnViewHide?.Invoke();
        }

        public void BindModel(CombatantModel model)
        {
            if (model == null) return;
            boundModel = model;
            BaseCombatantSetup(boundModel.Sprite, boundModel.Name, boundModel.CurrentHp, boundModel.MaxHp);
        }

        public virtual void RefreshView()
        {
            if (boundModel == null) return;
            hpUI.UpdateHpUI(boundModel.CurrentHp, boundModel.MaxHp);
        }

        private void BaseCombatantSetup(Sprite CombatantSprite, string combatantNameText, float currentHp, float maxHp)
        {
            combatantRenderer.sprite = CombatantSprite;
            combatantName.text = combatantNameText;
        }

        public void PlayDeathAnimation(float duration = 0.5f, Action onFinish = null)
        {
            StartCoroutine(OnCombatantDeathAnimationCoroutine(duration, onFinish));
        }

        private IEnumerator OnCombatantDeathAnimationCoroutine(float duration = 0.5f, Action onFinish = null)
        {
            // 使用局部变量保存当前缩放（避免覆盖 initialWrapperLocalScale）
            var startScale = combatantWrapper.transform.localScale;

            // 播放缩放到 0 的动画（等待完成）
            Tween t = combatantWrapper.transform.DOScale(Vector3.zero, duration);
            yield return t.WaitForCompletion();

            // 动画完成后（在回调里通常会回收到池），但为了保险这里恢复到 startScale
            // 注意：真正的池化恢复由 OnReturnToPool 使用 initialWrapperLocalScale 强制恢复
            combatantWrapper.transform.localScale = startScale;

            onFinish?.Invoke();
        }

        /// <summary>
        /// 无参接口：使用 Inspector 面板参数播放受击动画（推荐由 Controller 调用）
        /// </summary>
        public void PlayHitAnimation()
        {
            PlayHitAnimation(
                inspectorShakeStrength,
                inspectorShakeDuration,
                inspectorVibrato,
                inspectorRandomness,
                inspectorFlashColor,
                inspectorFlashDuration,
                usePunchInsteadOfShake);
        }

        /// <summary>
        /// 可编程接口：手动传参播放受击动画
        /// </summary>
        public void PlayHitAnimation(
            float shakeStrength = 0.25f,
            float shakeDuration = 0.35f,
            int vibrato = 10,
            float randomness = 90f,
            Color? flashColor = null,
            float flashDuration = 0.12f,
            bool usePunch = true)
        {
            if (combatantWrapper == null || combatantRenderer == null) return;

            // Kill 之前的 hit 动画，避免叠加
            if (hitSequence != null && hitSequence.IsActive())
            {
                hitSequence.SafeKill();
                hitSequence = null;
            }
            // 停止 transform/sprite 上的残留 tween
            combatantWrapper.transform.DOKill(true);
            combatantRenderer.DOKill(true);

            // 使用局部变量保存本次动画开始时的状态（用于恢复）
            var startLocalPos = combatantWrapper.transform.localPosition;
            var startLocalScale = combatantWrapper.transform.localScale;
            var startSpriteColor = combatantRenderer.color;

            Color targetFlash = flashColor ?? new Color(1f, 0.5f, 0.5f); // 柔和红

            // 构建 Sequence：抖动 + 闪烁（并行）
            hitSequence = DOTween.Sequence();

            Tween shakeTween;
            if (usePunch)
            {
                Vector3 punch = new Vector3(shakeStrength * 0.8f, shakeStrength * 0.5f, 0f);
                shakeTween = combatantWrapper.transform.DOPunchPosition(punch, shakeDuration, vibrato, 0.5f);
            }
            else
            {
                shakeTween = combatantWrapper.transform.DOShakePosition(shakeDuration, shakeStrength, vibrato, randomness, true, true);
            }

            var flashTween = combatantRenderer.DOColor(targetFlash, flashDuration).SetLoops(2, LoopType.Yoyo);

            // 并行播放
            hitSequence.Append(shakeTween);
            hitSequence.Join(flashTween);

            hitSequence.OnComplete(() =>
            {
                try
                {
                    if (combatantRenderer != null) combatantRenderer.color = startSpriteColor;
                    if (combatantWrapper != null)
                    {
                        combatantWrapper.transform.localPosition = startLocalPos;
                        combatantWrapper.transform.localScale = startLocalScale;
                    }
                }
                catch (Exception ex)
                {
                    LogError($"PlayHitAnimation 完成回调错误: {ex}");
                }
            });

            hitSequence.Play();
        }

        public void OnGetFromPool()
        {
            // 池中取出时强制恢复为 Awake 时记录的初始状态，避免因上次动画残留出现 (0,0,0)
            combatantWrapper.transform.localScale = initialWrapperLocalScale;
            combatantWrapper.transform.localPosition = initialWrapperLocalPosition;
            if (combatantRenderer != null) combatantRenderer.color = initialSpriteColor;
            combatantWrapper.SetActive(true);
        }

        public void OnReturnToPool()
        {
            // 回收到池时务必清理动画与绑定，避免复用残留
            if (hitSequence != null && hitSequence.IsActive())
            {
                hitSequence.SafeKill();
                hitSequence = null;
            }
            combatantWrapper.transform.DOKill(true);
            combatantRenderer.DOKill(true);

            // 恢复到 Awake 时的“权威”初始值，确保下次取出状态正常
            combatantWrapper.transform.localPosition = initialWrapperLocalPosition;
            combatantWrapper.transform.localScale = initialWrapperLocalScale;
            if (combatantRenderer != null) combatantRenderer.color = initialSpriteColor;

            combatantWrapper.SetActive(false);
            UnbindModel();
        }

        public void OnDestroyFromPool()
        { }
    }
}