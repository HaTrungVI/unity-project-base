using System;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectBase.UI.Core
{
    public abstract class BasePopup : BaseView, IPopup
    {
        [SerializeField] private string _popupId;
        [SerializeField] private Button _closeButton;

        public string PopupId => _popupId;
        public Button CloseButton => _closeButton;
        public event Action OnClosed;
        protected override UIRegistry.UIEntryType ViewType => UIRegistry.UIEntryType.Popup;

        protected virtual void Start()
        {
            _closeButton?.onClick.AddListener(RequestClose);
        }

        public virtual void SetupData(object data = null) { }

        public void RequestClose()
        {
            OnClosed?.Invoke();
            _ = UIManager.Instance.ClosePopup(_popupId);
        }
    }
}
