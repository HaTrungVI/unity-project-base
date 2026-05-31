using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using ProjectBase.Common.Patterns;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace ProjectBase.UI.Core
{
    public class UIManager : MonoSingleton<UIManager>, IUIManager
    {
        [SerializeField] private UIRegistry _registry;
        [SerializeField] private Transform _screenRoot;
        [SerializeField] private Transform _popupRoot;
        [SerializeField] private Transform _overlayRoot;

        [Header("Popup Dimmer")]
        [SerializeField] private CanvasGroup _popupDimmer;
        [SerializeField] private float _dimmerAlpha = 0.5f;
        [SerializeField] private float _dimmerFadeDuration = 0.25f;

        private readonly Dictionary<string, IScreen> _screenCache = new();
        private readonly Dictionary<string, IPopup> _popupCache = new();
        private readonly Dictionary<string, IView> _overlayCache = new();
        private readonly Dictionary<string, IPresenter> _presenterCache = new();
        private readonly Dictionary<string, AsyncOperationHandle<GameObject>> _loadedHandles = new();
        private readonly List<string> _popupStack = new();

        private static readonly Dictionary<Type, ConstructorInfo> _presenterCtorCache = new();

        private const int PopupBaseSortOrder = 100;
        private const int PopupSortOrderStep = 10;
        private const int OverlayBaseSortOrder = 200;

        public IScreen CurrentScreen { get; private set; }
        private string _currentScreenId;

        protected override void OnInitialized()
        {
            _registry.Initialize();
        }

        public async UniTask<IScreen> ShowScreen(string screenId, object parameter = null,
            CancellationToken ct = default)
        {
            if (_currentScreenId != null && _currentScreenId != screenId)
            {
                if (CurrentScreen != null)
                {
                    CurrentScreen.OnNavigatedFrom();
                    CurrentScreen.Hide();
                }
                DisposePresenter(_currentScreenId);
            }

            var screen = await GetOrCreateScreen(screenId, ct);
            CurrentScreen = screen;
            _currentScreenId = screenId;

            DisposePresenter(screenId);
            var presenter = CreatePresenter(screen as MonoBehaviour);
            if (presenter != null)
            {
                _presenterCache[screenId] = presenter;
                presenter.Initialize(parameter);
            }

            screen.OnNavigatedTo(parameter);
            screen.Show();
            return screen;
        }

        public async UniTask HideScreen(string screenId, CancellationToken ct = default)
        {
            if (_screenCache.TryGetValue(screenId, out var screen))
                screen.Hide();
            DisposePresenter(screenId);
            await UniTask.CompletedTask;
        }

        public void UnloadScreen(string screenId)
        {
            if (_screenCache.TryGetValue(screenId, out var screen))
            {
                DisposePresenter(screenId);
                var view = screen as MonoBehaviour;
                if (view != null)
                    Destroy(view.gameObject);
                _screenCache.Remove(screenId);
            }
            ReleaseHandle(screenId);
        }

        public async UniTask<IPopup> ShowPopup(string popupId, object data = null,
            CancellationToken ct = default)
        {
            var popup = await GetOrCreatePopup(popupId, ct);

            if (_popupStack.Count == 0)
                ShowDimmer();

            var sortOrder = PopupBaseSortOrder + _popupStack.Count * PopupSortOrderStep;
            SetCanvasSortOrder(popup as MonoBehaviour, sortOrder);

            DisposePresenter(popupId);
            var presenter = CreatePresenter(popup as MonoBehaviour);
            if (presenter != null)
            {
                _presenterCache[popupId] = presenter;
                presenter.Initialize(data);
            }

            popup.SetupData(data);
            popup.Show();
            _popupStack.Add(popupId);
            return popup;
        }

        public async UniTask ClosePopup(string popupId, CancellationToken ct = default)
        {
            if (_popupCache.TryGetValue(popupId, out var popup))
                popup.Hide();

            DisposePresenter(popupId);
            _popupStack.Remove(popupId);

            if (_popupStack.Count == 0)
                HideDimmer();

            await UniTask.CompletedTask;
        }

        public void UnloadPopup(string popupId)
        {
            if (_popupCache.TryGetValue(popupId, out var popup))
            {
                DisposePresenter(popupId);
                var view = popup as MonoBehaviour;
                if (view != null)
                    Destroy(view.gameObject);
                _popupCache.Remove(popupId);
            }
            _popupStack.Remove(popupId);
            ReleaseHandle(popupId);
        }

        public async UniTask CloseAllPopups(CancellationToken ct = default)
        {
            for (int i = _popupStack.Count - 1; i >= 0; i--)
            {
                var popupId = _popupStack[i];
                if (_popupCache.TryGetValue(popupId, out var popup))
                    popup.Hide();
                DisposePresenter(popupId);
            }
            _popupStack.Clear();
            HideDimmer();
            await UniTask.CompletedTask;
        }

        public async UniTask<IView> ShowOverlay(string overlayId, object data = null,
            CancellationToken ct = default)
        {
            var overlay = await GetOrCreateOverlay(overlayId, ct);

            SetCanvasSortOrder(overlay as MonoBehaviour, OverlayBaseSortOrder);

            DisposePresenter(overlayId);
            var presenter = CreatePresenter(overlay as MonoBehaviour);
            if (presenter != null)
            {
                _presenterCache[overlayId] = presenter;
                presenter.Initialize(data);
            }

            overlay.Show();
            return overlay;
        }

        public async UniTask HideOverlay(string overlayId, CancellationToken ct = default)
        {
            if (_overlayCache.TryGetValue(overlayId, out var overlay))
                overlay.Hide();
            DisposePresenter(overlayId);
            await UniTask.CompletedTask;
        }

        private async UniTask<IScreen> GetOrCreateScreen(string id, CancellationToken ct)
        {
            if (_screenCache.TryGetValue(id, out var cached))
                return cached;

            var view = await LoadView<BaseScreen>(id, _screenRoot, ct);
            _screenCache[id] = view;
            return view;
        }

        private async UniTask<IPopup> GetOrCreatePopup(string id, CancellationToken ct)
        {
            if (_popupCache.TryGetValue(id, out var cached))
                return cached;

            var view = await LoadView<BasePopup>(id, _popupRoot, ct);
            _popupCache[id] = view;
            return view;
        }

        private async UniTask<IView> GetOrCreateOverlay(string id, CancellationToken ct)
        {
            if (_overlayCache.TryGetValue(id, out var cached))
                return cached;

            var parent = _overlayRoot != null ? _overlayRoot : _screenRoot;
            var view = await LoadView<BaseView>(id, parent, ct);
            _overlayCache[id] = view;
            return view;
        }

        private async UniTask<T> LoadView<T>(string id, Transform parent, CancellationToken ct)
            where T : MonoBehaviour
        {
            if (!_registry.TryGetEntry(id, out var entry))
                throw new Exception($"[UIManager] No entry found for id: {id}");

            var handle = entry.assetReference.LoadAssetAsync<GameObject>();
            var prefab = await handle.ToUniTask(cancellationToken: ct);

            if (prefab == null)
                throw new Exception($"[UIManager] Failed to load asset for: {id}");

            _loadedHandles[id] = handle;

            var go = Instantiate(prefab, parent);
            var view = go.GetComponent<T>();
            if (view == null)
                throw new Exception($"[UIManager] Component {typeof(T).Name} not found on prefab");

            return view;
        }

        private static void SetCanvasSortOrder(MonoBehaviour view, int sortOrder)
        {
            if (view == null) return;
            var canvas = view.GetComponent<Canvas>();
            if (canvas == null) return;
            canvas.overrideSorting = true;
            canvas.sortingOrder = sortOrder;
        }

        private static IPresenter CreatePresenter(MonoBehaviour view)
        {
            if (view == null) return null;

            var viewType = view.GetType();

            if (!_presenterCtorCache.TryGetValue(viewType, out var ctor))
            {
                var attr = viewType.GetCustomAttribute<PresenterTypeAttribute>();
                if (attr != null)
                    ctor = attr.PresenterType.GetConstructor(new[] { viewType });
                _presenterCtorCache[viewType] = ctor;
            }

            if (ctor == null) return null;

            try
            {
                return (IPresenter)ctor.Invoke(new object[] { view });
            }
            catch (Exception e)
            {
                Debug.LogError($"[UIManager] Failed to create presenter: {e.Message}");
                return null;
            }
        }

        private void DisposePresenter(string id)
        {
            if (!_presenterCache.TryGetValue(id, out var presenter)) return;
            presenter.Dispose();
            _presenterCache.Remove(id);
        }

        private void ReleaseHandle(string id)
        {
            if (!_loadedHandles.TryGetValue(id, out var handle)) return;

            if (handle.IsValid())
                Addressables.Release(handle);
            _loadedHandles.Remove(id);
        }

        private Tween _dimmerTween;

        private void ShowDimmer()
        {
            if (_popupDimmer == null) return;
            _popupDimmer.gameObject.SetActive(true);
            _popupDimmer.blocksRaycasts = true;
            _dimmerTween?.Kill();
            _dimmerTween = DOTween.To(
                    () => _popupDimmer.alpha, x => _popupDimmer.alpha = x,
                    _dimmerAlpha, _dimmerFadeDuration)
                .SetUpdate(true);
        }

        private void HideDimmer()
        {
            if (_popupDimmer == null) return;
            _dimmerTween?.Kill();
            _dimmerTween = DOTween.To(
                    () => _popupDimmer.alpha, x => _popupDimmer.alpha = x,
                    0f, _dimmerFadeDuration)
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    _popupDimmer.blocksRaycasts = false;
                    _popupDimmer.gameObject.SetActive(false);
                });
        }

        protected override void OnDestroy()
        {
            _dimmerTween?.Kill();

            foreach (var presenter in _presenterCache.Values)
                presenter.Dispose();
            _presenterCache.Clear();

            foreach (var handle in _loadedHandles.Values)
            {
                if (handle.IsValid())
                    Addressables.Release(handle);
            }
            _loadedHandles.Clear();

            base.OnDestroy();
        }
    }
}
