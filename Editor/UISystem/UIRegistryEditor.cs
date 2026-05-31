using UnityEngine;
using UnityEditor;
using ProjectBase.UI.Core;
using ProjectBase.Editor;

namespace ProjectBase.UI.Editor
{
    [CustomEditor(typeof(UIRegistry))]
    public class UIRegistryEditor : UnityEditor.Editor
    {
        private SerializedProperty _entriesProperty;
        private string _searchFilter = "";
        private bool _showAddSection;

        private string _newId = "";
        private UIRegistry.UIEntryType _newType = UIRegistry.UIEntryType.Screen;
        private bool _newPreload;
        private Object _newPrefab;

        private static readonly Color ScreenColor = PBEditorStyles.AccentBlue;
        private static readonly Color PopupColor = PBEditorStyles.AccentOrange;
        private static readonly Color OverlayColor = PBEditorStyles.AccentPurple;

        private void OnEnable()
        {
            _entriesProperty = serializedObject.FindProperty("_entries");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            PBEditorStyles.DrawTitle("UI REGISTRY");

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"{_entriesProperty.arraySize} entries",
                EditorStyles.miniLabel);
            GUILayout.FlexibleSpace();
            DrawEntryCounts();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(4);
            DrawSearchBar();
            EditorGUILayout.Space(4);
            DrawEntryList();
            EditorGUILayout.Space(2);
            DrawAddEntry();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawEntryCounts()
        {
            int screens = 0, popups = 0, overlays = 0;
            for (var i = 0; i < _entriesProperty.arraySize; i++)
            {
                var typeProp = _entriesProperty.GetArrayElementAtIndex(i)
                    .FindPropertyRelative("type");
                switch (typeProp.enumValueIndex)
                {
                    case 0: screens++; break;
                    case 1: popups++; break;
                    case 2: overlays++; break;
                }
            }

            if (screens > 0)
                PBEditorStyles.DrawBadge($"{screens} SCR",
                    new Color(ScreenColor.r, ScreenColor.g, ScreenColor.b, 0.3f), ScreenColor);
            if (popups > 0)
                PBEditorStyles.DrawBadge($"{popups} POP",
                    new Color(PopupColor.r, PopupColor.g, PopupColor.b, 0.3f), PopupColor);
            if (overlays > 0)
                PBEditorStyles.DrawBadge($"{overlays} OVL",
                    new Color(OverlayColor.r, OverlayColor.g, OverlayColor.b, 0.3f), OverlayColor);
        }

        private void DrawSearchBar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            _searchFilter = EditorGUILayout.TextField(_searchFilter,
                EditorStyles.toolbarSearchField);
            if (GUILayout.Button("Clear", EditorStyles.toolbarButton, GUILayout.Width(48)))
                _searchFilter = "";
            EditorGUILayout.EndHorizontal();
        }

        private void DrawEntryList()
        {
            var accentColor = PBEditorStyles.AccentBlue;
            PBEditorStyles.BeginSection(
                $"ENTRIES ({_entriesProperty.arraySize})", accentColor);

            var removeIndex = -1;

            for (var i = 0; i < _entriesProperty.arraySize; i++)
            {
                var entryProp = _entriesProperty.GetArrayElementAtIndex(i);
                var idProp = entryProp.FindPropertyRelative("id");
                var assetRefProp = entryProp.FindPropertyRelative("assetReference");
                var typeProp = entryProp.FindPropertyRelative("type");
                var preloadProp = entryProp.FindPropertyRelative("preload");

                if (!string.IsNullOrEmpty(_searchFilter) &&
                    !idProp.stringValue.ToLower().Contains(_searchFilter.ToLower()))
                    continue;

                var entryType = (UIRegistry.UIEntryType)typeProp.enumValueIndex;
                GetTypeVisuals(entryType, out var badge, out var badgeBg, out var badgeText);

                var rowRect = EditorGUILayout.BeginHorizontal();
                EditorGUI.DrawRect(rowRect, PBEditorStyles.GetRowColor(i));
                EditorGUILayout.Space(4);

                PBEditorStyles.DrawBadge(badge, badgeBg, badgeText);
                EditorGUILayout.LabelField(idProp.stringValue, EditorStyles.boldLabel,
                    GUILayout.Width(120));

                EditorGUILayout.PropertyField(assetRefProp, GUIContent.none,
                    GUILayout.MinWidth(80));

                preloadProp.boolValue = EditorGUILayout.ToggleLeft("Preload",
                    preloadProp.boolValue, GUILayout.Width(60));

                if (PBEditorStyles.DangerButton("X", 18))
                    removeIndex = i;

                EditorGUILayout.Space(2);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space(1);
            }

            if (_entriesProperty.arraySize == 0)
                EditorGUILayout.LabelField("No entries registered.",
                    EditorStyles.centeredGreyMiniLabel);

            if (removeIndex >= 0)
                _entriesProperty.DeleteArrayElementAtIndex(removeIndex);

            PBEditorStyles.EndSection();
        }

        private void DrawAddEntry()
        {
            _showAddSection = EditorGUILayout.Foldout(_showAddSection,
                "Add New Entry", true, EditorStyles.foldoutHeader);
            if (!_showAddSection) return;

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.Space(2);

            _newId = EditorGUILayout.TextField("ID", _newId);
            _newPrefab = EditorGUILayout.ObjectField("Prefab", _newPrefab,
                typeof(GameObject), false);
            _newType = (UIRegistry.UIEntryType)EditorGUILayout.EnumPopup("Type", _newType);
            _newPreload = EditorGUILayout.Toggle("Preload", _newPreload);

            EditorGUILayout.Space(4);

            GUI.enabled = !string.IsNullOrWhiteSpace(_newId) && _newPrefab != null;
            if (PBEditorStyles.PrimaryButton("+ Add Entry", 28))
            {
                var registry = (UIRegistry)target;
                var prefabPath = AssetDatabase.GetAssetPath(_newPrefab);
                var guid = AssetDatabase.AssetPathToGUID(prefabPath);

                var entry = new UIRegistry.UIEntry
                {
                    id = _newId,
                    assetReference = new UnityEngine.AddressableAssets.AssetReference(guid),
                    type = _newType,
                    preload = _newPreload
                };
                registry.AddEntry(entry);
                AssetDatabase.SaveAssets();
                _newId = "";
                _newPrefab = null;
            }
            GUI.enabled = true;

            EditorGUILayout.Space(2);
            EditorGUILayout.EndVertical();
        }

        private static void GetTypeVisuals(UIRegistry.UIEntryType type,
            out string badge, out Color bg, out Color text)
        {
            switch (type)
            {
                case UIRegistry.UIEntryType.Popup:
                    badge = "POPUP";
                    text = PopupColor;
                    bg = new Color(PopupColor.r, PopupColor.g, PopupColor.b, 0.25f);
                    break;
                case UIRegistry.UIEntryType.Overlay:
                    badge = "OVERLAY";
                    text = OverlayColor;
                    bg = new Color(OverlayColor.r, OverlayColor.g, OverlayColor.b, 0.25f);
                    break;
                default:
                    badge = "SCREEN";
                    text = ScreenColor;
                    bg = new Color(ScreenColor.r, ScreenColor.g, ScreenColor.b, 0.25f);
                    break;
            }
        }
    }
}
