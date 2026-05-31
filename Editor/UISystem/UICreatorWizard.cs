using System.IO;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.Callbacks;
using ProjectBase.UI.Core;
using ProjectBase.Editor;

namespace ProjectBase.UI.Editor
{
    public class UICreatorWizard : EditorWindow
    {
        private string _name = "";
        private UIRegistry.UIEntryType _type = UIRegistry.UIEntryType.Screen;
        private bool _createPresenter = true;
        private UIRegistry _targetRegistry;
        private string _namespace = "ProjectBase.UI";
        private int _showAnimationType;
        private int _hideAnimationType;

        private const string PendingNameKey = "UICreator_PendingName";
        private const string PendingTypeKey = "UICreator_PendingType";
        private const string PendingRegistryGuidKey = "UICreator_PendingRegistryGuid";
        private const string PendingFolderKey = "UICreator_PendingFolder";
        private const string PendingShowAnimKey = "UICreator_PendingShowAnim";
        private const string PendingHideAnimKey = "UICreator_PendingHideAnim";

        [MenuItem("Tools/Project Base/UI Creator")]
        public static void ShowWindow()
        {
            var window = GetWindow<UICreatorWizard>("UI Creator");
            window.minSize = new Vector2(420, 380);
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(4);
            PBEditorStyles.DrawTitle("UI CREATOR");

            DrawConfiguration();
            EditorGUILayout.Space(4);
            DrawPreview();
            EditorGUILayout.Space(8);
            DrawCreateButton();
        }

        private void DrawConfiguration()
        {
            PBEditorStyles.BeginSection("CONFIGURATION", PBEditorStyles.AccentGray);

            _name = EditorGUILayout.TextField("Name", _name);

            var prevType = _type;
            _type = (UIRegistry.UIEntryType)EditorGUILayout.EnumPopup("Type", _type);
            if (_type != prevType)
                ApplyDefaultAnimations();

            _createPresenter = EditorGUILayout.Toggle("Create Presenter", _createPresenter);
            _namespace = EditorGUILayout.TextField("Namespace", _namespace);
            _targetRegistry = (UIRegistry)EditorGUILayout.ObjectField(
                "UI Registry", _targetRegistry, typeof(UIRegistry), false);

            EditorGUILayout.Space(4);
            DrawAnimationConfig();

            if (string.IsNullOrWhiteSpace(_name))
            {
                EditorGUILayout.Space(2);
                EditorGUILayout.HelpBox(
                    "Enter a name to get started.", MessageType.Info);
            }
            else if (_targetRegistry == null)
            {
                EditorGUILayout.Space(2);
                EditorGUILayout.HelpBox(
                    "Assign a UI Registry to enable creation.", MessageType.Warning);
            }

            PBEditorStyles.EndSection();
        }

        private void DrawAnimationConfig()
        {
            EditorGUILayout.LabelField("Animation", EditorStyles.boldLabel);
            switch (_type)
            {
                case UIRegistry.UIEntryType.Screen:
                    _showAnimationType = (int)(ScreenAnimationType)EditorGUILayout.EnumPopup(
                        "  Show", (ScreenAnimationType)_showAnimationType);
                    _hideAnimationType = (int)(ScreenAnimationType)EditorGUILayout.EnumPopup(
                        "  Hide", (ScreenAnimationType)_hideAnimationType);
                    break;
                case UIRegistry.UIEntryType.Popup:
                    _showAnimationType = (int)(PopupAnimationType)EditorGUILayout.EnumPopup(
                        "  Show", (PopupAnimationType)_showAnimationType);
                    _hideAnimationType = (int)(PopupAnimationType)EditorGUILayout.EnumPopup(
                        "  Hide", (PopupAnimationType)_hideAnimationType);
                    break;
                case UIRegistry.UIEntryType.Overlay:
                    _showAnimationType = (int)(OverlayAnimationType)EditorGUILayout.EnumPopup(
                        "  Show", (OverlayAnimationType)_showAnimationType);
                    _hideAnimationType = (int)(OverlayAnimationType)EditorGUILayout.EnumPopup(
                        "  Hide", (OverlayAnimationType)_hideAnimationType);
                    break;
            }
        }

        private void ApplyDefaultAnimations()
        {
            switch (_type)
            {
                case UIRegistry.UIEntryType.Screen:
                    _showAnimationType = (int)ScreenAnimationType.Fade;
                    _hideAnimationType = (int)ScreenAnimationType.Fade;
                    break;
                case UIRegistry.UIEntryType.Popup:
                    _showAnimationType = (int)PopupAnimationType.ScaleBounce;
                    _hideAnimationType = (int)PopupAnimationType.ScaleBounce;
                    break;
                case UIRegistry.UIEntryType.Overlay:
                    _showAnimationType = (int)OverlayAnimationType.Fade;
                    _hideAnimationType = (int)OverlayAnimationType.Fade;
                    break;
            }
        }

        private void DrawPreview()
        {
            if (string.IsNullOrWhiteSpace(_name)) return;

            GetTypeInfo(_type, out var typeSuffix, out var folder);
            var outputFolder = $"Assets/_Base/UI Elements/{folder}";
            var className = $"{_name}{typeSuffix}";

            PBEditorStyles.BeginSection("PREVIEW", PBEditorStyles.AccentBlue);

            EditorGUILayout.LabelField("Scripts:", EditorStyles.miniLabel);
            EditorGUILayout.LabelField(
                $"    {outputFolder}/{className}.cs", PBEditorStyles.PathLabel);

            if (_createPresenter)
                EditorGUILayout.LabelField(
                    $"    {outputFolder}/{className}Presenter.cs",
                    PBEditorStyles.PathLabel);

            EditorGUILayout.Space(2);
            EditorGUILayout.LabelField("Prefab:", EditorStyles.miniLabel);
            EditorGUILayout.LabelField(
                $"    {outputFolder}/{className}.prefab", PBEditorStyles.PathLabel);

            EditorGUILayout.Space(2);
            EditorGUILayout.LabelField("Addressable:", EditorStyles.miniLabel);
            EditorGUILayout.LabelField(
                $"    Group: UI_{folder}", PBEditorStyles.PathLabel);

            if (_targetRegistry != null)
            {
                EditorGUILayout.Space(2);
                EditorGUILayout.LabelField("Registry:", EditorStyles.miniLabel);
                EditorGUILayout.LabelField(
                    $"    {_targetRegistry.name}  Entry: {_name}",
                    PBEditorStyles.PathLabel);
            }

            EditorGUILayout.Space(2);
            EditorGUILayout.LabelField("Animation:", EditorStyles.miniLabel);
            var showName = GetAnimationName(_type, _showAnimationType);
            var hideName = GetAnimationName(_type, _hideAnimationType);
            EditorGUILayout.LabelField(
                $"    Show: {showName}  |  Hide: {hideName}",
                PBEditorStyles.PathLabel);

            PBEditorStyles.EndSection();
        }

        private void DrawCreateButton()
        {
            var isValid = !string.IsNullOrWhiteSpace(_name) && _targetRegistry != null;

            GUI.enabled = isValid;
            if (PBEditorStyles.PrimaryButton("Create UI Element", 32))
                CreateUI();
            GUI.enabled = true;
        }

        private void CreateUI()
        {
            GetTypeInfo(_type, out var typeSuffix, out var folder);
            var className = $"{_name}{typeSuffix}";
            var folderPath = $"Assets/_Base/UI Elements/{folder}";

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            var scriptContent = _type switch
            {
                UIRegistry.UIEntryType.Screen =>
                    UIScriptTemplates.GetScreenViewTemplate(className, _namespace),
                UIRegistry.UIEntryType.Popup =>
                    UIScriptTemplates.GetPopupViewTemplate(className, _namespace),
                UIRegistry.UIEntryType.Overlay =>
                    UIScriptTemplates.GetOverlayViewTemplate(className, _namespace),
                _ => UIScriptTemplates.GetScreenViewTemplate(className, _namespace)
            };

            File.WriteAllText($"{folderPath}/{className}.cs", scriptContent);

            if (_createPresenter)
            {
                var presenterName = $"{className}Presenter";
                var presenterContent = UIScriptTemplates.GetPresenterTemplate(
                    presenterName, className, _namespace);
                File.WriteAllText($"{folderPath}/{presenterName}.cs", presenterContent);
            }

            var registryGuid = AssetDatabase.AssetPathToGUID(
                AssetDatabase.GetAssetPath(_targetRegistry));

            SessionState.SetString(PendingNameKey, _name);
            SessionState.SetInt(PendingTypeKey, (int)_type);
            SessionState.SetString(PendingRegistryGuidKey, registryGuid);
            SessionState.SetString(PendingFolderKey, folderPath);
            SessionState.SetInt(PendingShowAnimKey, _showAnimationType);
            SessionState.SetInt(PendingHideAnimKey, _hideAnimationType);

            AssetDatabase.Refresh();
            Debug.Log($"[UICreator] Scripts created at {folderPath}. Waiting for compilation...");
        }

        [DidReloadScripts]
        private static void OnScriptsReloaded()
        {
            var pendingName = SessionState.GetString(PendingNameKey, "");
            if (string.IsNullOrEmpty(pendingName)) return;
            EditorApplication.delayCall += CreatePrefabAndRegister;
        }

        private static void CreatePrefabAndRegister()
        {
            var pendingName = SessionState.GetString(PendingNameKey, "");
            if (string.IsNullOrEmpty(pendingName)) return;

            var pendingType = (UIRegistry.UIEntryType)SessionState.GetInt(PendingTypeKey, 0);
            var registryGuid = SessionState.GetString(PendingRegistryGuidKey, "");
            var folderPath = SessionState.GetString(PendingFolderKey, "");
            var showAnimType = SessionState.GetInt(PendingShowAnimKey, 0);
            var hideAnimType = SessionState.GetInt(PendingHideAnimKey, 0);

            SessionState.EraseString(PendingNameKey);
            SessionState.EraseInt(PendingTypeKey);
            SessionState.EraseString(PendingRegistryGuidKey);
            SessionState.EraseString(PendingFolderKey);
            SessionState.EraseInt(PendingShowAnimKey);
            SessionState.EraseInt(PendingHideAnimKey);

            GetTypeInfo(pendingType, out var typeSuffix, out var folder);
            var className = $"{pendingName}{typeSuffix}";
            var prefabPath = $"{folderPath}/{className}.prefab";

            var go = new GameObject(className);

            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            if (pendingType == UIRegistry.UIEntryType.Overlay)
                canvas.sortingOrder = 200;

            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 1f;

            go.AddComponent<GraphicRaycaster>();
            go.AddComponent<CanvasGroup>();

            var content = new GameObject("Content");
            content.transform.SetParent(go.transform, false);
            var contentRT = content.AddComponent<RectTransform>();
            contentRT.anchorMin = Vector2.zero;
            contentRT.anchorMax = Vector2.one;
            contentRT.offsetMin = Vector2.zero;
            contentRT.offsetMax = Vector2.zero;

            var scriptType = FindType(className);
            if (scriptType != null)
            {
                var component = go.AddComponent(scriptType);

                var so = new SerializedObject(component);
                var idProp = so.FindProperty("_screenId")
                             ?? so.FindProperty("_popupId");
                if (idProp != null)
                {
                    idProp.stringValue = pendingName;
                }

                var animTargetProp = so.FindProperty("_animationTarget");
                if (animTargetProp != null)
                    animTargetProp.objectReferenceValue = contentRT;

                SetAnimationConfig(so, "_showAnimation", showAnimType, pendingType, true);
                SetAnimationConfig(so, "_hideAnimation", hideAnimType, pendingType, false);
                so.ApplyModifiedPropertiesWithoutUndo();
            }
            else
            {
                Debug.LogWarning(
                    $"[UICreator] Could not find type '{className}'.");
            }

            PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
            DestroyImmediate(go);

            RegisterAddressable(prefabPath, folder);

            if (!string.IsNullOrEmpty(registryGuid))
            {
                var registryPath = AssetDatabase.GUIDToAssetPath(registryGuid);
                var registry = AssetDatabase.LoadAssetAtPath<UIRegistry>(registryPath);
                RegisterInUIRegistry(prefabPath, pendingName, pendingType, registry);
            }

            Debug.Log($"[UICreator] Created: {prefabPath}");
        }

        private static void GetTypeInfo(UIRegistry.UIEntryType type,
            out string typeSuffix, out string folder)
        {
            switch (type)
            {
                case UIRegistry.UIEntryType.Popup:
                    typeSuffix = "Popup";
                    folder = "Popups";
                    break;
                case UIRegistry.UIEntryType.Overlay:
                    typeSuffix = "Overlay";
                    folder = "Overlays";
                    break;
                default:
                    typeSuffix = "Screen";
                    folder = "Screens";
                    break;
            }
        }

        private static void RegisterAddressable(string prefabPath, string folder)
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                Debug.LogWarning(
                    "[UICreator] Addressables not initialized.");
                return;
            }

            var groupName = $"UI_{folder}";
            var group = settings.FindGroup(groupName)
                        ?? settings.CreateGroup(groupName, false, false, true, null);

            var guid = AssetDatabase.AssetPathToGUID(prefabPath);
            settings.CreateOrMoveEntry(guid, group);
        }

        private static void RegisterInUIRegistry(string prefabPath, string name,
            UIRegistry.UIEntryType type, UIRegistry registry)
        {
            if (registry == null) return;

            var guid = AssetDatabase.AssetPathToGUID(prefabPath);
            var entry = new UIRegistry.UIEntry
            {
                id = name,
                assetReference = new UnityEngine.AddressableAssets.AssetReference(guid),
                type = type,
                preload = false
            };
            registry.AddEntry(entry);
            AssetDatabase.SaveAssets();
        }

        private static void SetAnimationConfig(SerializedObject so, string fieldName,
            int animType, UIRegistry.UIEntryType viewType, bool isShow)
        {
            var configProp = so.FindProperty(fieldName);
            if (configProp == null) return;

            configProp.FindPropertyRelative("_animationType").intValue = animType;

            var defaults = ViewAnimationConfig.CreateDefault(viewType, isShow);
            configProp.FindPropertyRelative("_duration").floatValue = defaults.Duration;
            configProp.FindPropertyRelative("_ease").intValue = (int)defaults.AnimationEase;
            configProp.FindPropertyRelative("_delay").floatValue = defaults.Delay;
        }

        private static string GetAnimationName(UIRegistry.UIEntryType type, int animIndex)
        {
            return type switch
            {
                UIRegistry.UIEntryType.Screen => ((ScreenAnimationType)animIndex).ToString(),
                UIRegistry.UIEntryType.Popup => ((PopupAnimationType)animIndex).ToString(),
                UIRegistry.UIEntryType.Overlay => ((OverlayAnimationType)animIndex).ToString(),
                _ => "Unknown"
            };
        }

        private static System.Type FindType(string className)
        {
            foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
            foreach (var t in assembly.GetTypes())
                if (t.Name == className)
                    return t;
            return null;
        }
    }
}
