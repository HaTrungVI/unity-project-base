# Changelog

## [1.0.0] - 2026-05-31

### Added
- UI System: BaseView/BaseScreen/BasePopup, UIManager with 3 layers (Screen, Popup, Overlay)
- UI Animation: ViewAnimationConfig, ViewAnimator, Screen/Popup/Overlay animation builders
- UI MVP: Auto Presenter creation via `[PresenterType]` attribute
- UI Popup Dimmer: Auto fade dimmer on popup stack
- Data System: DataModule<T> SO-based persistence, DataRegistry, DataAutoSaver
- Data System: JsonBinarySerializer (JSON > GZip > AES-256), optimistic update, server sync
- Time System: TrustedTimeProvider (anti-cheat), TimerService, ScheduleService
- Time System: SO Service Reference pattern (TimeServiceRef)
- Audio System: AudioManager with pooled AudioSources, Addressables, DOTween fade
- Asset System: AssetLoader, SceneLoader (Addressables wrappers)
- Common: MonoSingleton<T>, Singleton<T>, EventBus, ObjectPool<T>, PoolManager
- Common: StateMachine (async), CollectionExtensions, StringExtensions, MathUtils
- Common: RewardEntry/RewardBundle/IRewardGranter, RequirementEntry/IRequirementChecker
- SO Architecture: SOVariable<T>, SOReference<T>, SOEventChannel<T>, SOEventListener, SORuntimeSet<T>
- SO Architecture: 10 built-in types (int, float, bool, string, Vector2/3, Quaternion, Color, Sprite, GameObject)
- GameFlow: GameBootstrapper, BaseSplashController, IBootstrapTask pipeline
- GameFlow: PreloadAssetsTask, InitializeServicesTask, LoadSceneTask, UnloadSceneTask
- Notification: INotificationService, LocalNotificationService interface
- Editor: UI Creator Wizard, Data Module Creator Wizard
- Editor: UIRegistry Inspector, DataRegistry Inspector, DataModule Inspector
- Editor: ViewAnimationConfigDrawer (auto-detect UI type)
