using System.IO;
using UnityEngine;
using UnityEditor;
using ProjectBase.Data.Core;
using ProjectBase.Editor;

namespace ProjectBase.Data.Editor
{
    [CustomEditor(typeof(DataRegistry))]
    public class DataRegistryEditor : UnityEditor.Editor
    {
        private SerializedProperty _storageConfigProp;
        private SerializedProperty _modulesProp;
        private ScriptableObject _newModule;
        private bool _showAddSection;

        private void OnEnable()
        {
            _storageConfigProp = serializedObject.FindProperty("_storageConfig");
            _modulesProp = serializedObject.FindProperty("_modules");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawHeader();
            EditorGUILayout.Space(4);
            DrawStorageConfig();
            EditorGUILayout.Space(4);
            DrawActions();
            EditorGUILayout.Space(4);
            DrawModuleList();
            EditorGUILayout.Space(2);
            DrawAddModule();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawHeader()
        {
            PBEditorStyles.DrawTitle("DATA REGISTRY");

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"{_modulesProp.arraySize} module(s)",
                EditorStyles.miniLabel);
            GUILayout.FlexibleSpace();

            var hasConfig = _storageConfigProp.objectReferenceValue != null;
            if (hasConfig)
                PBEditorStyles.DrawBadge("CONFIG OK",
                    new Color(0.11f, 0.37f, 0.13f, 0.6f),
                    new Color(0.65f, 0.84f, 0.65f));
            else
                PBEditorStyles.DrawBadge("NO CONFIG",
                    new Color(0.55f, 0.27f, 0.07f, 0.6f),
                    new Color(1f, 0.72f, 0.30f));

            EditorGUILayout.EndHorizontal();
        }

        private void DrawStorageConfig()
        {
            EditorGUILayout.PropertyField(_storageConfigProp, new GUIContent("Storage Config"));

            if (_storageConfigProp.objectReferenceValue == null)
            {
                EditorGUILayout.HelpBox(
                    "Assign a DataStorageConfig. Create via: Create > ProjectBase > Data Storage Config",
                    MessageType.Warning);
            }
        }

        private void DrawActions()
        {
            EditorGUILayout.BeginHorizontal();
            if (PBEditorStyles.PrimaryButton("Load All"))
                LoadAllFromDisk();
            if (PBEditorStyles.PrimaryButton("Save All"))
                SaveAllToDisk();
            if (PBEditorStyles.DangerButton("Delete All Saves"))
            {
                if (EditorUtility.DisplayDialog("Delete All Saves",
                    "Delete ALL save files? This cannot be undone.",
                    "Delete All", "Cancel"))
                    DeleteAllSaves();
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawModuleList()
        {
            PBEditorStyles.BeginModulesSection(
                $"MODULES ({_modulesProp.arraySize})");

            var removeIndex = -1;

            for (var i = 0; i < _modulesProp.arraySize; i++)
            {
                var elementProp = _modulesProp.GetArrayElementAtIndex(i);
                var moduleObj = elementProp.objectReferenceValue as ScriptableObject;

                var rowRect = EditorGUILayout.BeginHorizontal();
                EditorGUI.DrawRect(rowRect, PBEditorStyles.GetRowColor(i));
                EditorGUILayout.Space(4);

                if (moduleObj != null)
                {
                    var dataModule = moduleObj as IDataModule;
                    var moduleId = dataModule?.ModuleId ?? moduleObj.name;
                    var filePath = GetModuleFilePath(moduleId);
                    var fileExists = !string.IsNullOrEmpty(filePath) && File.Exists(filePath);

                    EditorGUILayout.LabelField(moduleId, EditorStyles.boldLabel,
                        GUILayout.Width(120));

                    if (fileExists)
                    {
                        PBEditorStyles.DrawSavedBadge();
                        var info = new FileInfo(filePath);
                        EditorGUILayout.LabelField(
                            PBEditorStyles.FormatFileSize(info.Length),
                            EditorStyles.miniLabel, GUILayout.Width(50));
                    }
                    else
                    {
                        PBEditorStyles.DrawNoFileBadge();
                        GUILayout.Space(50);
                    }

                    GUILayout.FlexibleSpace();

                    if (GUILayout.Button("Select", EditorStyles.miniButton,
                        GUILayout.Width(48)))
                        Selection.activeObject = moduleObj;
                }
                else
                {
                    EditorGUILayout.LabelField("(null)", EditorStyles.miniLabel);
                    GUILayout.FlexibleSpace();
                }

                if (PBEditorStyles.DangerButton("X", 18))
                    removeIndex = i;

                EditorGUILayout.Space(2);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space(1);
            }

            if (_modulesProp.arraySize == 0)
            {
                EditorGUILayout.LabelField("No modules registered.",
                    EditorStyles.centeredGreyMiniLabel);
            }

            if (removeIndex >= 0)
                _modulesProp.DeleteArrayElementAtIndex(removeIndex);

            PBEditorStyles.EndSection();
        }

        private void DrawAddModule()
        {
            _showAddSection = EditorGUILayout.Foldout(_showAddSection,
                "Add Module", true, EditorStyles.foldoutHeader);
            if (!_showAddSection) return;

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.Space(2);

            EditorGUILayout.BeginHorizontal();
            _newModule = (ScriptableObject)EditorGUILayout.ObjectField(
                _newModule, typeof(ScriptableObject), false);

            GUI.enabled = _newModule != null && _newModule is IDataModule;
            if (PBEditorStyles.PrimaryButton("+ Add", 20))
            {
                var registry = (DataRegistry)target;
                registry.AddModule(_newModule);
                AssetDatabase.SaveAssets();
                _newModule = null;
            }
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();

            if (_newModule != null && _newModule is not IDataModule)
            {
                EditorGUILayout.HelpBox(
                    "Selected object does not implement IDataModule.",
                    MessageType.Warning);
            }

            EditorGUILayout.Space(2);
            EditorGUILayout.EndVertical();
        }

        private DataStorageConfig GetConfig()
        {
            return _storageConfigProp.objectReferenceValue as DataStorageConfig;
        }

        private string GetModuleFilePath(string moduleId)
        {
            if (string.IsNullOrEmpty(moduleId)) return string.Empty;
            var config = GetConfig();
            var folder = config != null ? config.SaveFolder : "SaveData";
            return Path.Combine(Application.persistentDataPath, folder, moduleId + ".dat");
        }

        private void LoadAllFromDisk()
        {
            var config = GetConfig();
            if (config == null)
            {
                EditorUtility.DisplayDialog("Error", "No StorageConfig assigned.", "OK");
                return;
            }

            var serializer = config.CreateSerializer();
            var loaded = 0;

            for (var i = 0; i < _modulesProp.arraySize; i++)
            {
                var moduleObj = _modulesProp.GetArrayElementAtIndex(i)
                    .objectReferenceValue as ScriptableObject;
                if (moduleObj == null) continue;

                var dataModule = moduleObj as DataModuleBase;
                if (dataModule == null) continue;

                var filePath = GetModuleFilePath(dataModule.ModuleId);
                if (!File.Exists(filePath)) continue;

                try
                {
                    var dataType = GetDataType(dataModule);
                    if (dataType == null) continue;

                    var bytes = File.ReadAllBytes(filePath);
                    var method = typeof(IDataSerializer).GetMethod("Deserialize")
                        .MakeGenericMethod(dataType);
                    var data = method.Invoke(serializer, new object[] { bytes });

                    Undo.RecordObject(moduleObj, "Load Data Module");
                    var field = GetCurrentDataField(dataModule);
                    if (field != null)
                    {
                        field.SetValue(dataModule, data);
                        EditorUtility.SetDirty(moduleObj);
                        loaded++;
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError(
                        $"[DataRegistry] Failed to load '{dataModule.ModuleId}': {e.Message}");
                }
            }

            Debug.Log($"[DataRegistry] Loaded {loaded} module(s) from disk.");
        }

        private void SaveAllToDisk()
        {
            var config = GetConfig();
            if (config == null)
            {
                EditorUtility.DisplayDialog("Error", "No StorageConfig assigned.", "OK");
                return;
            }

            var serializer = config.CreateSerializer();
            var savePath = Path.Combine(Application.persistentDataPath, config.SaveFolder);
            if (!Directory.Exists(savePath))
                Directory.CreateDirectory(savePath);

            var saved = 0;

            for (var i = 0; i < _modulesProp.arraySize; i++)
            {
                var moduleObj = _modulesProp.GetArrayElementAtIndex(i)
                    .objectReferenceValue as ScriptableObject;
                if (moduleObj == null) continue;

                var dataModule = moduleObj as DataModuleBase;
                if (dataModule == null) continue;

                try
                {
                    var dataType = GetDataType(dataModule);
                    if (dataType == null) continue;

                    var field = GetCurrentDataField(dataModule);
                    if (field == null) continue;

                    var data = field.GetValue(dataModule);
                    var method = typeof(IDataSerializer).GetMethod("Serialize")
                        .MakeGenericMethod(dataType);
                    var bytes = (byte[])method.Invoke(serializer, new[] { data });

                    File.WriteAllBytes(
                        Path.Combine(savePath, dataModule.ModuleId + ".dat"), bytes);
                    saved++;
                }
                catch (System.Exception e)
                {
                    Debug.LogError(
                        $"[DataRegistry] Failed to save '{dataModule.ModuleId}': {e.Message}");
                }
            }

            Repaint();
            Debug.Log($"[DataRegistry] Saved {saved} module(s) to disk.");
        }

        private void DeleteAllSaves()
        {
            var config = GetConfig();
            var folder = config != null ? config.SaveFolder : "SaveData";
            var savePath = Path.Combine(Application.persistentDataPath, folder);

            if (!Directory.Exists(savePath)) return;

            var files = Directory.GetFiles(savePath, "*.dat");
            foreach (var file in files)
                File.Delete(file);
            Repaint();
            Debug.Log($"[DataRegistry] Deleted {files.Length} save files.");
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

        private static System.Reflection.FieldInfo GetCurrentDataField(DataModuleBase module)
        {
            var type = module.GetType();
            while (type != null)
            {
                if (type.IsGenericType &&
                    type.GetGenericTypeDefinition() == typeof(DataModule<>))
                {
                    return type.GetField("_currentData",
                        System.Reflection.BindingFlags.NonPublic |
                        System.Reflection.BindingFlags.Instance);
                }
                type = type.BaseType;
            }
            return null;
        }
    }
}
