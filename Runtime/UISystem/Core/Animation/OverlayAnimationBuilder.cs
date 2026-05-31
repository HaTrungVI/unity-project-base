using UnityEngine;
using DG.Tweening;

namespace ProjectBase.UI.Core
{
    public static class OverlayAnimationBuilder
    {
        public static void Build(Sequence seq, ViewAnimationConfig config,
            CanvasGroup cg, RectTransform rt, bool isShow)
        {
            var type = (OverlayAnimationType)config.AnimationType;
            switch (type)
            {
                case OverlayAnimationType.Fade:
                    BuildFade(seq, cg, config.Duration, config.AnimationEase, isShow);
                    break;
                case OverlayAnimationType.SlideFromLeft:
                    BuildSlide(seq, cg, rt, config.Duration, config.AnimationEase, isShow, Vector2.left);
                    break;
                case OverlayAnimationType.SlideFromRight:
                    BuildSlide(seq, cg, rt, config.Duration, config.AnimationEase, isShow, Vector2.right);
                    break;
                case OverlayAnimationType.SlideFromBottom:
                    BuildSlide(seq, cg, rt, config.Duration, config.AnimationEase, isShow, Vector2.down);
                    break;
                case OverlayAnimationType.SlideFromTop:
                    BuildSlide(seq, cg, rt, config.Duration, config.AnimationEase, isShow, Vector2.up);
                    break;
            }
        }

        private static void BuildFade(Sequence seq, CanvasGroup cg,
            float duration, Ease ease, bool isShow)
        {
            float from = isShow ? 0f : 1f;
            float to = isShow ? 1f : 0f;
            cg.alpha = from;
            seq.Append(
                DOTween.To(() => cg.alpha, x => cg.alpha = x, to, duration)
                    .SetEase(ease));
        }

        private static void BuildSlide(Sequence seq, CanvasGroup cg, RectTransform rt,
            float duration, Ease ease, bool isShow, Vector2 fromDirection)
        {
            var screenSize = new Vector2(Screen.width, Screen.height);
            var offset = fromDirection * screenSize;
            var restPos = Vector2.zero;

            if (isShow)
            {
                rt.anchoredPosition = offset;
                cg.alpha = 1f;
                seq.Append(
                    DOTween.To(() => rt.anchoredPosition, x => rt.anchoredPosition = x, restPos, duration)
                        .SetEase(ease));
            }
            else
            {
                seq.Append(
                    DOTween.To(() => rt.anchoredPosition, x => rt.anchoredPosition = x, -offset, duration)
                        .SetEase(ease));
            }
        }
    }
}
