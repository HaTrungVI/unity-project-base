using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using ProjectBase.Common.Patterns.SOArchitecture;

namespace ProjectBase.Editor
{
    [CustomEditor(typeof(SOEventChannel), true)]
    public class SOEventChannelEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField(target.name, EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Type: Void Event Channel", EditorStyles.miniLabel);

            EditorGUILayout.Space(6);
            DrawSubscriberList(
                ((SOEventChannel)target).SubscriberNames,
                ((SOEventChannel)target).ListenerCount);

            if (Application.isPlaying)
            {
                EditorGUILayout.Space(4);
                if (GUILayout.Button("Raise (Test)", GUILayout.Height(24)))
                    ((SOEventChannel)target).Raise();
            }

            serializedObject.ApplyModifiedProperties();
        }

        public static void DrawSubscriberList(IReadOnlyList<string> names, int count)
        {
            EditorGUILayout.LabelField("Subscribers", EditorStyles.boldLabel);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            if (!Application.isPlaying)
            {
                EditorGUILayout.LabelField("Enter Play Mode to see subscribers.",
                    EditorStyles.centeredGreyMiniLabel);
            }
            else if (count == 0)
            {
                EditorGUILayout.LabelField("No subscribers.",
                    EditorStyles.centeredGreyMiniLabel);
            }
            else
            {
                for (int i = 0; i < names.Count; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField($"  {i}", GUILayout.Width(24));
                    EditorGUILayout.LabelField(names[i], EditorStyles.label);
                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.Space(2);
                EditorGUILayout.LabelField($"Total: {count}",
                    EditorStyles.miniLabel);
            }

            EditorGUILayout.EndVertical();

            if (Application.isPlaying)
                HandleUtility.Repaint();
        }
    }

    public abstract class GenericSOEventChannelEditor<T> : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var channel = (SOEventChannel<T>)target;
            EditorGUILayout.LabelField(target.name, EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Type: {typeof(T).Name} Event Channel",
                EditorStyles.miniLabel);

            EditorGUILayout.Space(6);
            SOEventChannelEditor.DrawSubscriberList(
                channel.SubscriberNames, channel.ListenerCount);

            serializedObject.ApplyModifiedProperties();
        }
    }

    [CustomEditor(typeof(IntEventChannel))]
    public class IntEventChannelEditor : GenericSOEventChannelEditor<int> { }

    [CustomEditor(typeof(FloatEventChannel))]
    public class FloatEventChannelEditor : GenericSOEventChannelEditor<float> { }

    [CustomEditor(typeof(BoolEventChannel))]
    public class BoolEventChannelEditor : GenericSOEventChannelEditor<bool> { }

    [CustomEditor(typeof(StringEventChannel))]
    public class StringEventChannelEditor : GenericSOEventChannelEditor<string> { }

    [CustomEditor(typeof(Vector2EventChannel))]
    public class Vector2EventChannelEditor : GenericSOEventChannelEditor<Vector2> { }

    [CustomEditor(typeof(Vector3EventChannel))]
    public class Vector3EventChannelEditor : GenericSOEventChannelEditor<Vector3> { }

    [CustomEditor(typeof(QuaternionEventChannel))]
    public class QuaternionEventChannelEditor : GenericSOEventChannelEditor<Quaternion> { }

    [CustomEditor(typeof(ColorEventChannel))]
    public class ColorEventChannelEditor : GenericSOEventChannelEditor<Color> { }

    [CustomEditor(typeof(SpriteEventChannel))]
    public class SpriteEventChannelEditor : GenericSOEventChannelEditor<Sprite> { }

    [CustomEditor(typeof(GameObjectEventChannel))]
    public class GameObjectEventChannelEditor : GenericSOEventChannelEditor<GameObject> { }
}
