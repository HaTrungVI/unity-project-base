using System;

namespace ProjectBase.UI.Core
{
    public interface IView
    {
        void Show();
        void Hide();
        bool IsVisible { get; }
        event Action OnShowCompleted;
        event Action OnHideCompleted;
    }
}