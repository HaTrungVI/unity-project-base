using System;
using UnityEngine;
using DG.Tweening;

namespace ProjectBase.UI.Core
{
    [Serializable]
    public class ViewAnimationConfig
    {
        [SerializeField] private int _animationType;
        [SerializeField] private float _duration = 0.3f;
        [SerializeField] private Ease _ease = Ease.Linear;
        [SerializeField] private float _delay;

        public int AnimationType => _animationType;
        public float Duration => _duration;
        public Ease AnimationEase => _ease;
        public float Delay => _delay;

        public void SetAnimationType(int type) => _animationType = type;
        public void SetDuration(float duration) => _duration = duration;
        public void SetEase(Ease ease) => _ease = ease;

        public static ViewAnimationConfig CreateDefault(UIRegistry.UIEntryType viewType, bool isShow)
        {
            return viewType switch
            {
                UIRegistry.UIEntryType.Screen => CreateScreenDefault(isShow),
                UIRegistry.UIEntryType.Popup => CreatePopupDefault(isShow),
                UIRegistry.UIEntryType.Overlay => CreateOverlayDefault(isShow),
                _ => new ViewAnimationConfig()
            };
        }

        private static ViewAnimationConfig CreateScreenDefault(bool isShow)
        {
            return new ViewAnimationConfig
            {
                _animationType = (int)ScreenAnimationType.Fade,
                _duration = 0.3f,
                _ease = Ease.Linear
            };
        }

        private static ViewAnimationConfig CreatePopupDefault(bool isShow)
        {
            return new ViewAnimationConfig
            {
                _animationType = (int)PopupAnimationType.ScaleBounce,
                _duration = isShow ? 0.35f : 0.25f,
                _ease = isShow ? Ease.OutBack : Ease.InBack
            };
        }

        private static ViewAnimationConfig CreateOverlayDefault(bool isShow)
        {
            return new ViewAnimationConfig
            {
                _animationType = (int)OverlayAnimationType.Fade,
                _duration = 0.4f,
                _ease = isShow ? Ease.InQuad : Ease.OutQuad
            };
        }
    }
}
