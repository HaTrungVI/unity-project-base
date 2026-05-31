using UnityEngine;

namespace ProjectBase.UI.Core
{
    public abstract class BaseScreen : BaseView, IScreen
    {
        [SerializeField] private string _screenId;

        public string ScreenId => _screenId;
        protected override UIRegistry.UIEntryType ViewType => UIRegistry.UIEntryType.Screen;

        public virtual void OnNavigatedTo(object parameter = null) { }
        public virtual void OnNavigatedFrom() { }
    }
}
