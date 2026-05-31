using UnityEngine;
using DG.Tweening;

namespace ProjectBase.UI.Core
{
    public static class PopupAnimationBuilder
    {
        public static void Build(Sequence seq, ViewAnimationConfig config,
            CanvasGroup cg, RectTransform rt, bool isShow)
        {
            var type = (PopupAnimationType)config.AnimationType;
            switch (type)
            {
                case PopupAnimationType.Fade:
                    BuildFade(seq, cg, config.Duration, config.AnimationEase, isShow);
                    break;
                case PopupAnimationType.ScaleBounce:
                    BuildScaleBounce(seq, cg, rt, config.Duration, config.AnimationEase, isShow);
                    break;
                case PopupAnimationType.ScaleFade:
                    BuildScaleFade(seq, cg, rt, config.Duration, config.AnimationEase, isShow);
                    break;
                case PopupAnimationType.SlideFromBottom:
                    BuildSlide(seq, cg, rt, config.Duration, config.AnimationEase, isShow, Vector2.down);
                    break;
                case PopupAnimationType.SlideFromTop:
                    BuildSlide(seq, cg, rt, config.Duration, config.AnimationEase, isShow, Vector2.up);
                    break;
                case PopupAnimationType.ElasticPop:
                    BuildElasticPop(seq, cg, rt, config.Duration, config.AnimationEase, isShow);
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

        private static void BuildScaleBounce(Sequence seq, CanvasGroup cg, RectTransform rt,
            float duration, Ease ease, bool isShow)
        {
            if (isShow)
            {
                rt.localScale = Vector3.zero;
                cg.alpha = 1f;
                seq.Append(
                    DOTween.To(() => rt.localScale, x => rt.localScale = x, Vector3.one, duration)
                        .SetEase(ease));
            }
            else
            {
                seq.Append(
                    DOTween.To(() => rt.localScale, x => rt.localScale = x, Vector3.zero, duration)
                        .SetEase(ease));
                seq.Join(
                    DOTween.To(() => cg.alpha, x => cg.alpha = x, 0f, duration * 0.8f)
                        .SetEase(Ease.InQuad));
            }
        }

        private static void BuildScaleFade(Sequence seq, CanvasGroup cg, RectTransform rt,
            float duration, Ease ease, bool isShow)
        {
            if (isShow)
            {
                rt.localScale = Vector3.one * 0.8f;
                cg.alpha = 0f;
                seq.Append(
                    DOTween.To(() => rt.localScale, x => rt.localScale = x, Vector3.one, duration)
                        .SetEase(ease));
                seq.Join(
                    DOTween.To(() => cg.alpha, x => cg.alpha = x, 1f, duration)
                        .SetEase(ease));
            }
            else
            {
                seq.Append(
                    DOTween.To(() => rt.localScale, x => rt.localScale = x, Vector3.one * 0.8f, duration)
                        .SetEase(ease));
                seq.Join(
                    DOTween.To(() => cg.alpha, x => cg.alpha = x, 0f, duration)
                        .SetEase(ease));
            }
        }

        private static void BuildSlide(Sequence seq, CanvasGroup cg, RectTransform rt,
            float duration, Ease ease, bool isShow, Vector2 fromDirection)
        {
            var offset = fromDirection * Screen.height;
            var restPos = Vector2.zero;

            if (isShow)
            {
                rt.anchoredPosition = offset;
                cg.alpha = 0f;
                seq.Append(
                    DOTween.To(() => rt.anchoredPosition, x => rt.anchoredPosition = x, restPos, duration)
                        .SetEase(ease));
                seq.Join(
                    DOTween.To(() => cg.alpha, x => cg.alpha = x, 1f, duration * 0.5f)
                        .SetEase(Ease.OutQuad));
            }
            else
            {
                seq.Append(
                    DOTween.To(() => rt.anchoredPosition, x => rt.anchoredPosition = x, offset, duration)
                        .SetEase(ease));
                seq.Join(
                    DOTween.To(() => cg.alpha, x => cg.alpha = x, 0f, duration * 0.5f)
                        .SetDelay(duration * 0.5f)
                        .SetEase(Ease.InQuad));
            }
        }

        private static void BuildElasticPop(Sequence seq, CanvasGroup cg, RectTransform rt,
            float duration, Ease ease, bool isShow)
        {
            if (isShow)
            {
                rt.localScale = Vector3.zero;
                cg.alpha = 1f;
                seq.Append(
                    DOTween.To(() => rt.localScale, x => rt.localScale = x, Vector3.one, duration)
                        .SetEase(Ease.OutElastic));
            }
            else
            {
                seq.Append(
                    DOTween.To(() => rt.localScale, x => rt.localScale = x, Vector3.zero, duration)
                        .SetEase(Ease.InBack));
                seq.Join(
                    DOTween.To(() => cg.alpha, x => cg.alpha = x, 0f, duration * 0.8f)
                        .SetEase(Ease.InQuad));
            }
        }
    }
}
