using System;

namespace ProjectBase.UI.Core
{
    public interface IPopup : IView
    {
        string PopupId { get; }
        void SetupData(object data = null);
        event Action OnClosed;
    }
}