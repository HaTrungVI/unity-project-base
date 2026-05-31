using System.Threading;
using Cysharp.Threading.Tasks;

namespace ProjectBase.UI.Core
{
    public interface IUIManager
    {
        UniTask<IScreen> ShowScreen(string screenId, object parameter = null, CancellationToken ct = default);
        UniTask HideScreen(string screenId, CancellationToken ct = default);
        void UnloadScreen(string screenId);
        UniTask<IPopup> ShowPopup(string popupId, object data = null, CancellationToken ct = default);
        UniTask ClosePopup(string popupId, CancellationToken ct = default);
        void UnloadPopup(string popupId);
        UniTask CloseAllPopups(CancellationToken ct = default);
        UniTask<IView> ShowOverlay(string overlayId, object data = null, CancellationToken ct = default);
        UniTask HideOverlay(string overlayId, CancellationToken ct = default);
        IScreen CurrentScreen { get; }
    }
}
