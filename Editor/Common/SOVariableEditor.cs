using UnityEditor;
using UnityEngine;
using ProjectBase.Common.Patterns.SOArchitecture;

namespace ProjectBase.Editor
{
    [CustomEditor(typeof(SOVariable<>), true)]
    public class SOVariableEditor : UnityEditor.Editor
    {
        private SerializedProperty _initialValueProp;

        private void OnEnable()
        {
            _initialValueProp = serializedObject.FindProperty("_initialValue");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField(target.name, EditorStyles.boldLabel);

            var typeName = target.GetType().BaseType?.GetGenericArguments();
            if (typeName != null && typeName.Length > 0)
                EditorGUILayout.LabelField($"Type: {typeName[0].Name} Variable",
                    EditorStyles.miniLabel);

            EditorGUILayout.Space(6);

            if (_initialValueProp != null)
                EditorGUILayout.PropertyField(_initialValueProp, new GUIContent("Initial Value"));

            if (Application.isPlaying)
            {
                EditorGUILayout.Space(4);
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField("Runtime", EditorStyles.boldLabel);

                var valueProperty = target.GetType()
                    .GetProperty("Value", System.Reflection.BindingFlags.Public
                                          | System.Reflection.BindingFlags.Instance);
                if (valueProperty != null)
                {
                    var value = valueProperty.GetValue(target);
                    EditorGUI.BeginDisabledGroup(true);
                    EditorGUILayout.LabelField("Current Value",
                        value?.ToString() ?? "null");
                    EditorGUI.EndDisabledGroup();
                }

                EditorGUILayout.EndVertical();
                HandleUtility.Repaint();
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
