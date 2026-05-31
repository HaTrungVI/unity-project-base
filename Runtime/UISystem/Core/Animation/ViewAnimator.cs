using UnityEngine;
using DG.Tweening;

namespace ProjectBase.UI.Core
{
    public static class ViewAnimator
    {
        public static Sequence Play(ViewAnimationConfig config, CanvasGroup cg,
            RectTransform rt, UIRegistry.UIEntryType viewType, bool isShow)
        {
            var seq = DOTween.Sequence().SetUpdate(true);

            if (config.Delay > 0f)
                seq.AppendInterval(config.Delay);

            switch (viewType)
            {
                case UIRegistry.UIEntryType.Screen:
                    ScreenAnimationBuilder.Build(seq, config, cg, rt, isShow);
                    break;
                case UIRegistry.UIEntryType.Popup:
                    PopupAnimationBuilder.Build(seq, config, cg, rt, isShow);
                    break;
                case UIRegistry.UIEntryType.Overlay:
                    OverlayAnimationBuilder.Build(seq, config, cg, rt, isShow);
                    break;
            }

            return seq;
        }
    }
}
