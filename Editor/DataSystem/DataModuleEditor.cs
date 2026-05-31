using System.IO;
using UnityEngine;
using UnityEditor;
using ProjectBase.Data.Core;
using ProjectBase.Editor;

namespace ProjectBase.Data.Editor
{
    [CustomEditor(typeof(DataModuleBase), true)]
    public class DataModuleEditor : UnityEditor.Editor
    {
        private SerializedProperty _moduleIdProp;
        private SerializedProperty _versionProp;
        private SerializedProperty _defaultDataProp;
        private SerializedProperty _currentDataProp;
        private bool _showDefaultData;
        private bool _showFileInfo;

        private void OnEnable()
        {
            _moduleIdProp = serializedObject.FindProperty("_moduleId");
            _versionProp = serializedObject.FindProperty("_version");
            _defaultDataProp = serializedObject.FindProperty("_defaultData");
            _currentDataProp = serializedObject.FindProperty("_currentData");
            TryLoadFromDisk();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawHeader();
            EditorGUILayout.Space(4);
            DrawCustomFields();
            EditorGUILayout.Space(4);
            DrawActions();
            EditorGUILayout.Space(4);
            DrawCurrentData();
            EditorGUILayout.Space(2);
            DrawDefaultData();
            EditorGUILayout.Space(2);
            DrawFileInfo();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawHeader()
        {
            var moduleName = target.name;
            if (string.IsNullOrEmpty(moduleName))
                moduleName = _moduleIdProp.stringValue;
            PBEditorStyles.DrawTitle(moduleName.ToUpper());

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("ID", GUILayout.Width(20));
            EditorGUILayout.PropertyField(_moduleIdProp, GUIContent.none, GUILayout.MinWidth(80));
            EditorGUILayout.LabelField("v", GUILayout.Width(12));
            EditorGUILayout.PropertyField(_versionProp, GUIContent.none, GUILayout.Width(40));

            GUILayout.FlexibleSpace();
            var filePath = GetSaveFilePath();
            var hasFile = !string.IsNullOrEmpty(filePath) && File.Exists(filePath);
            if (hasFile)
                PBEditorStyles.DrawSavedBadge();
            else
                PBEditorStyles.DrawNoFileBadge();

            EditorGUILayout.EndHorizontal();
        }

        private void DrawActions()
        {
            EditorGUILayout.BeginHorizontal();
            if (PBEditorStyles.PrimaryButton("Load from Disk"))
                TryLoadFromDisk();
            if (PBEditorStyles.PrimaryButton("Save to Disk"))
                SaveToDisk();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(2);

            EditorGUILayout.BeginHorizontal();
            if (PBEditorStyles.WarningButton("Reset to Defaults"))
            {
                if (EditorUtility.DisplayDialog("Reset to Defaults",
                    "Copy Default Data into Current Data?", "Reset", "Cancel"))
                    CopyDefaultToCurrent();
            }

            if (PBEditorStyles.DangerButton("Delete Save File"))
            {
                if (EditorUtility.DisplayDialog("Delete Save File",
                    "Delete the persistent save file?", "Delete", "Cancel"))
                    DeleteSaveFile();
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawCustomFields()
        {
            var iterator = serializedObject.GetIterator();
            iterator.NextVisible(true);

            var skipFields = new System.Collections.Generic.HashSet<string>
            {
                "m_Script", "_moduleId", "_version", "_defaultData", "_currentData"
            };

            while (iterator.NextVisible(false))
            {
                if (skipFields.Contains(iterator.name)) continue;
                EditorGUILayout.PropertyField(iterator, true);
            }
        }

        private void DrawCurrentData()
        {
            PBEditorStyles.BeginCurrentDataSection();
            if (_currentDataProp != null)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_currentDataProp, true);
                EditorGUI.indentLevel--;
            }
            PBEditorStyles.EndSection();
        }

        private void DrawDefaultData()
        {
            _showDefaultData = EditorGUILayout.Foldout(_showDefaultData,
                "Default Data (Template)", true, EditorStyles.foldoutHeader);
            if (!_showDefaultData) return;

            PBEditorStyles.BeginDefaultDataSection();
            if (_defaultDataProp != null)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_defaultDataProp, true);
                EditorGUI.indentLevel--;
            }
            PBEditorStyles.EndSection();
        }

        private void DrawFileInfo()
        {
            var filePath = GetSaveFilePath();
            var hasFile = !string.IsNullOrEmpty(filePath) && File.Exists(filePath);
            if (!hasFile) return;

            _showFileInfo = EditorGUILayout.Foldout(_showFileInfo,
                "Save File Info", true, EditorStyles.foldoutHeader);
            if (!_showFileInfo) return;

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.Space(2);
            PBEditorStyles.DrawFileInfo(filePath);
            EditorGUILayout.Space(2);
            EditorGUILayout.EndVertical();
        }

        private string GetSaveFilePath()
        {
            var moduleId = _moduleIdProp?.stringValue;
            if (string.IsNullOrEmpty(moduleId)) return null;

            var config = FindStorageConfig();
            var folder = config != null ? config.SaveFolder : "SaveData";
            return Path.Combine(Application.persistentDataPath, folder, moduleId + ".dat");
        }

        private void TryLoadFromDisk()
        {
            var filePath = GetSaveFilePath();
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath)) return;

            var config = FindStorageConfig();
            if (config == null) return;

            try
            {
                var module = (DataModuleBase)target;
                var dataType = GetDataType(module);
                if (dataType == null) return;

                var bytes = File.ReadAllBytes(filePath);
                var serializer = config.CreateSerializer();
                var method = typeof(IDataSerializer).GetMethod("Deserialize")
                    .MakeGenericMethod(dataType);
                var data = method.Invoke(serializer, new object[] { bytes });

                ApplyToField("_currentData", data);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[DataModule] Failed to load from disk: {e.Message}");
            }
        }

        private void SaveToDisk()
        {
            var moduleId = _moduleIdProp?.stringValue;
            if (string.IsNullOrEmpty(moduleId))
            {
                EditorUtility.DisplayDialog("Error", "Module ID is empty.", "OK");
                return;
            }

            var config = FindStorageConfig();
            if (config == null)
            {
                EditorUtility.DisplayDialog("Error",
                    "No DataStorageConfig found.", "OK");
                return;
            }

            var module = (DataModuleBase)target;
            var dataType = GetDataType(module);
            if (dataType == null) return;

            var field = GetField(module, "_currentData");
            if (field == null) return;

            var data = field.GetValue(module);
            var serializer = config.CreateSerializer();
            var method = typeof(IDataSerializer).GetMethod("Serialize")
                .MakeGenericMethod(dataType);
            var bytes = (byte[])method.Invoke(serializer, new[] { data });

            var savePath = Path.Combine(Application.persistentDataPath, config.SaveFolder);
            if (!Directory.Exists(savePath))
                Directory.CreateDirectory(savePath);

            File.WriteAllBytes(Path.Combine(savePath, moduleId + ".dat"), bytes);
            Repaint();
        }

        private void CopyDefaultToCurrent()
        {
            var module = (DataModuleBase)target;
            var defaultField = GetField(module, "_defaultData");
            if (defaultField == null) return;

            var defaultData = defaultField.GetValue(module);
            var json = JsonUtility.ToJson(defaultData);
            var dataType = GetDataType(module);
            var cloned = JsonUtility.FromJson(json, dataType);

            ApplyToField("_currentData", cloned);
        }

        private void ApplyToField(string fieldName, object data)
        {
            Undo.RecordObject(target, "Update Data Module");
            var module = (DataModuleBase)target;
            var field = GetField(module, fieldName);
            if (field == null) return;

            field.SetValue(module, data);
            EditorUtility.SetDirty(target);
            serializedObject.Update();
            Repaint();
        }

        private void DeleteSaveFile()
        {
            var filePath = GetSaveFilePath();
            if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
            {
                File.Delete(filePath);
                Repaint();
            }
        }

        private DataStorageConfig FindStorageConfig()
        {
            var guids = AssetDatabase.FindAssets("t:DataStorageConfig");
            if (guids.Length == 0) return null;
            var path = AssetDatabase.GUIDToAssetPath(guids[0]);
            return AssetDatabase.LoadAssetAtPath<DataStorageConfig>(path);
        }

        private static System.Type GetDataType(DataModuleBase module)
        {
            var type = module.GetType();
            while (type != null)
            {
                if (type.IsGenericType &&
                    type.GetGenericTypeDefinition() == typeof(DataModule<>))
                    return type.GetGenericArguments()[0];
                type = type.BaseType;
            }
            return null;
        }

        private static System.Reflection.FieldInfo GetField(DataModuleBase module, string name)
        {
            var type = module.GetType();
            while (type != null)
            {
                if (type.IsGenericType &&
                    type.GetGenericTypeDefinition() == typeof(DataModule<>))
                {
                    return type.GetField(name,
                        System.Reflection.BindingFlags.NonPublic |
                        System.Reflection.BindingFlags.Instance);
                }
                type = type.BaseType;
            }
            return null;
        }
    }
}
