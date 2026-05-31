using System;
using UnityEngine;
using DG.Tweening;

namespace ProjectBase.UI.Core
{
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class BaseView : MonoBehaviour, IView
    {
        [Header("Animation")]
        [SerializeField] private ViewAnimationConfig _showAnimation = new();
        [SerializeField] private ViewAnimationConfig _hideAnimation = new();
        [SerializeField] private RectTransform _animationTarget;

        private CanvasGroup _canvasGroup;
        private RectTransform _animationRT;
        private Sequence _currentTween;

        public bool IsVisible { get; private set; }
        public event Action OnShowCompleted;
        public event Action OnHideCompleted;

        protected CanvasGroup ViewCanvasGroup => _canvasGroup;
        protected RectTransform AnimationTarget => _animationRT;
        protected virtual UIRegistry.UIEntryType ViewType => UIRegistry.UIEntryType.Overlay;

        protected virtual void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _animationRT = ResolveAnimationTarget();
        }

        public virtual void Show()
        {
            gameObject.SetActive(true);
            _currentTween?.Kill();
            _currentTween = ViewAnimator.Play(
                _showAnimation, _canvasGroup, _animationRT, ViewType, true);
            _currentTween.OnComplete(() =>
            {
                IsVisible = true;
                OnShowCompleted?.Invoke();
                OnAfterShow();
            });
        }

        public virtual void Hide()
        {
            _currentTween?.Kill();
            _currentTween = ViewAnimator.Play(
                _hideAnimation, _canvasGroup, _animationRT, ViewType, false);
            _currentTween.OnComplete(() =>
            {
                gameObject.SetActive(false);
                IsVisible = false;
                OnHideCompleted?.Invoke();
                OnAfterHide();
            });
        }

        protected virtual void OnAfterShow() { }
        protected virtual void OnAfterHide() { }

        protected virtual void OnDestroy()
        {
            _currentTween?.Kill();
        }

        private RectTransform ResolveAnimationTarget()
        {
            if (_animationTarget != null) return _animationTarget;
            var content = transform.Find("Content");
            if (content != null) return content as RectTransform;
            return GetComponent<RectTransform>();
        }
    }
}
