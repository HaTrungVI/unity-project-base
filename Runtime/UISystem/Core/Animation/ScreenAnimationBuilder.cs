using UnityEngine;
using DG.Tweening;

namespace ProjectBase.UI.Core
{
    public static class ScreenAnimationBuilder
    {
        public static void Build(Sequence seq, ViewAnimationConfig config,
            CanvasGroup cg, RectTransform rt, bool isShow)
        {
            var type = (ScreenAnimationType)config.AnimationType;
            switch (type)
            {
                case ScreenAnimationType.Fade:
                    BuildFade(seq, cg, config.Duration, config.AnimationEase, isShow);
                    break;
                case ScreenAnimationType.SlideLeft:
                    BuildSlide(seq, cg, rt, config.Duration, config.AnimationEase, isShow, Vector2.left);
                    break;
                case ScreenAnimationType.SlideRight:
                    BuildSlide(seq, cg, rt, config.Duration, config.AnimationEase, isShow, Vector2.right);
                    break;
                case ScreenAnimationType.SlideUp:
                    BuildSlide(seq, cg, rt, config.Duration, config.AnimationEase, isShow, Vector2.up);
                    break;
                case ScreenAnimationType.SlideDown:
                    BuildSlide(seq, cg, rt, config.Duration, config.AnimationEase, isShow, Vector2.down);
                    break;
                case ScreenAnimationType.Zoom:
                    BuildZoom(seq, cg, rt, config.Duration, config.AnimationEase, isShow);
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
            float duration, Ease ease, bool isShow, Vector2 direction)
        {
            var screenOffset = new Vector2(Screen.width, Screen.height);
            var offset = direction * screenOffset;
            var restPos = Vector2.zero;

            if (isShow)
            {
                rt.anchoredPosition = -offset;
                cg.alpha = 1f;
                seq.Append(
                    DOTween.To(() => rt.anchoredPosition, x => rt.anchoredPosition = x, restPos, duration)
                        .SetEase(ease));
            }
            else
            {
                cg.alpha = 1f;
                seq.Append(
                    DOTween.To(() => rt.anchoredPosition, x => rt.anchoredPosition = x, offset, duration)
                        .SetEase(ease));
            }
        }

        private static void BuildZoom(Sequence seq, CanvasGroup cg, RectTransform rt,
            float duration, Ease ease, bool isShow)
        {
            if (isShow)
            {
                rt.localScale = Vector3.one * 0.85f;
                cg.alpha = 0f;
                seq.Append(
                    DOTween.To(() => rt.localScale, x => rt.localScale = x, Vector3.one, duration)
                        .SetEase(ease));
                seq.Join(
                    DOTween.To(() => cg.alpha, x => cg.alpha = x, 1f, duration * 0.6f)
                        .SetEase(Ease.OutQuad));
            }
            else
            {
                seq.Append(
                    DOTween.To(() => rt.localScale, x => rt.localScale = x, Vector3.one * 0.85f, duration)
                        .SetEase(ease));
                seq.Join(
                    DOTween.To(() => cg.alpha, x => cg.alpha = x, 0f, duration * 0.6f)
                        .SetEase(Ease.InQuad));
            }
        }
    }
}
