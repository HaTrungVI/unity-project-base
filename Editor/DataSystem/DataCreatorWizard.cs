using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using ProjectBase.Data.Core;
using ProjectBase.Editor;

namespace ProjectBase.Data.Editor
{
    public class DataCreatorWizard : EditorWindow
    {
        private string _name = "";
        private string _namespace = "ProjectBase.Data";
        private string _baseFolder = "Assets/_Base/_Data";
        private DataRegistry _targetRegistry;

        private const string PendingNameKey = "DataCreator_PendingName";
        private const string PendingNamespaceKey = "DataCreator_PendingNamespace";
        private const string PendingRegistryGuidKey = "DataCreator_PendingRegistryGuid";
        private const string PendingAssetFolderKey = "DataCreator_PendingAssetFolder";

        [MenuItem("Tools/Project Base/Data Module Creator")]
        public static void ShowWindow()
        {
            var window = GetWindow<DataCreatorWizard>("Data Module Creator");
            window.minSize = new Vector2(420, 360);
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(4);
            PBEditorStyles.DrawTitle("DATA MODULE CREATOR");

            DrawConfiguration();
            EditorGUILayout.Space(4);
            DrawPreview();
            EditorGUILayout.Space(8);
            DrawCreateButton();
        }

        private void DrawConfiguration()
        {
            PBEditorStyles.BeginSection("CONFIGURATION",
                new Color(0.47f, 0.56f, 0.61f));

            _name = EditorGUILayout.TextField("Module Name", _name);
            _namespace = EditorGUILayout.TextField("Namespace", _namespace);
            _baseFolder = EditorGUILayout.TextField("Output Folder", _baseFolder);
            _targetRegistry = (DataRegistry)EditorGUILayout.ObjectField(
                "Data Registry", _targetRegistry, typeof(DataRegistry), false);

            if (string.IsNullOrWhiteSpace(_name))
            {
                EditorGUILayout.Space(2);
                EditorGUILayout.HelpBox(
                    "Enter a module name to get started.", MessageType.Info);
            }

            PBEditorStyles.EndSection();
        }

        private void DrawPreview()
        {
            if (string.IsNullOrWhiteSpace(_name)) return;

            var codeFolder = $"{_baseFolder}/Code";
            var assetFolder = $"{_baseFolder}/SO";

            PBEditorStyles.BeginCurrentDataSection("PREVIEW");

            EditorGUILayout.LabelField("Scripts:", EditorStyles.miniLabel);
            EditorGUILayout.LabelField(
                $"    {codeFolder}/{_name}Data.cs", PBEditorStyles.PathLabel);
            EditorGUILayout.LabelField(
                $"    {codeFolder}/{_name}Module.cs", PBEditorStyles.PathLabel);

            EditorGUILayout.Space(2);
            EditorGUILayout.LabelField("Asset:", EditorStyles.miniLabel);
            EditorGUILayout.LabelField(
                $"    {assetFolder}/{_name}Module.asset", PBEditorStyles.PathLabel);

            if (_targetRegistry != null)
            {
                EditorGUILayout.Space(2);
                EditorGUILayout.LabelField("Registry:", EditorStyles.miniLabel);
                EditorGUILayout.LabelField(
                    $"    {_targetRegistry.name}", PBEditorStyles.PathLabel);
            }

            PBEditorStyles.EndSection();
        }

        private void DrawCreateButton()
        {
            var isValid = !string.IsNullOrWhiteSpace(_name);

            GUI.enabled = isValid;
            if (PBEditorStyles.PrimaryButton("Create Data Module", 32))
                CreateModule();
            GUI.enabled = true;
        }

        private void CreateModule()
        {
            var codeFolder = $"{_baseFolder}/Code";
            var dataClassName = $"{_name}Data";
            var moduleClassName = $"{_name}Module";
            var ns = string.IsNullOrWhiteSpace(_namespace) ? "" : _namespace;

            if (!Directory.Exists(codeFolder))
                Directory.CreateDirectory(codeFolder);

            File.WriteAllText($"{codeFolder}/{dataClassName}.cs",
                GenerateDataClassScript(dataClassName, ns));
            File.WriteAllText($"{codeFolder}/{moduleClassName}.cs",
                GenerateModuleScript(moduleClassName, dataClassName, ns));

            if (_targetRegistry != null)
            {
                var registryGuid = AssetDatabase.AssetPathToGUID(
                    AssetDatabase.GetAssetPath(_targetRegistry));
                SessionState.SetString(PendingRegistryGuidKey, registryGuid);
            }

            SessionState.SetString(PendingNameKey, _name);
            SessionState.SetString(PendingNamespaceKey, ns);
            SessionState.SetString(PendingAssetFolderKey, $"{_baseFolder}/SO");

            AssetDatabase.Refresh();
            Debug.Log($"[DataCreator] Scripts created. Waiting for compilation...");
        }

        [DidReloadScripts]
        private static void OnScriptsReloaded()
        {
            var pendingName = SessionState.GetString(PendingNameKey, "");
            if (string.IsNullOrEmpty(pendingName)) return;
            EditorApplication.delayCall += CreateAssetAndRegister;
        }

        private static void CreateAssetAndRegister()
        {
            var pendingName = SessionState.GetString(PendingNameKey, "");
            if (string.IsNullOrEmpty(pendingName)) return;

            var registryGuid = SessionState.GetString(PendingRegistryGuidKey, "");
            var assetFolder = SessionState.GetString(
                PendingAssetFolderKey, "Assets/_Base/_Data/SO");

            SessionState.EraseString(PendingNameKey);
            SessionState.EraseString(PendingNamespaceKey);
            SessionState.EraseString(PendingRegistryGuidKey);
            SessionState.EraseString(PendingAssetFolderKey);

            var moduleClassName = $"{pendingName}Module";

            if (!Directory.Exists(assetFolder))
                Directory.CreateDirectory(assetFolder);

            var assetPath = $"{assetFolder}/{moduleClassName}.asset";
            var moduleType = FindType(moduleClassName);

            if (moduleType == null)
            {
                Debug.LogWarning(
                    $"[DataCreator] Could not find type '{moduleClassName}'.");
                return;
            }

            var instance = CreateInstance(moduleType);
            var so = new SerializedObject(instance);
            var idProp = so.FindProperty("_moduleId");
            if (idProp != null)
            {
                idProp.stringValue = pendingName;
                so.ApplyModifiedPropertiesWithoutUndo();
            }

            AssetDatabase.CreateAsset(instance, assetPath);
            AssetDatabase.SaveAssets();

            if (!string.IsNullOrEmpty(registryGuid))
            {
                var registryPath = AssetDatabase.GUIDToAssetPath(registryGuid);
                var registry = AssetDatabase.LoadAssetAtPath<DataRegistry>(registryPath);
                if (registry != null)
                {
                    registry.AddModule(instance);
                    AssetDatabase.SaveAssets();
                }
            }

            Selection.activeObject = instance;
            Debug.Log($"[DataCreator] Created: {assetPath}");
        }

        private static string GenerateDataClassScript(string className, string ns)
        {
            var nsOpen = string.IsNullOrEmpty(ns) ? "" : $"namespace {ns}\n{{\n";
            var nsClose = string.IsNullOrEmpty(ns) ? "" : "}\n";
            var indent = string.IsNullOrEmpty(ns) ? "" : "    ";

            return $"using System;\n\n{nsOpen}" +
                   $"{indent}[Serializable]\n" +
                   $"{indent}public class {className}\n" +
                   $"{indent}{{\n" +
                   $"{indent}}}\n" +
                   $"{nsClose}";
        }

        private static string GenerateModuleScript(
            string moduleClass, string dataClass, string ns)
        {
            var nsOpen = string.IsNullOrEmpty(ns) ? "" : $"namespace {ns}\n{{\n";
            var nsClose = string.IsNullOrEmpty(ns) ? "" : "}\n";
            var indent = string.IsNullOrEmpty(ns) ? "" : "    ";

            return $"using UnityEngine;\nusing ProjectBase.Data.Core;\n\n{nsOpen}" +
                   $"{indent}[CreateAssetMenu(fileName = \"{moduleClass}\", " +
                   $"menuName = \"ProjectBase/Data/{moduleClass}\")]\n" +
                   $"{indent}public class {moduleClass} : DataModule<{dataClass}>\n" +
                   $"{indent}{{\n" +
                   $"{indent}}}\n" +
                   $"{nsClose}";
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
