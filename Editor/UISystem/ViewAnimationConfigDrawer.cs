using UnityEngine;
using UnityEditor;
using DG.Tweening;
using ProjectBase.UI.Core;

namespace ProjectBase.UI.Editor
{
    [CustomPropertyDrawer(typeof(ViewAnimationConfig))]
    public class ViewAnimationConfigDrawer : PropertyDrawer
    {
        private const float LineHeight = 18f;
        private const float Spacing = 2f;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (!property.isExpanded) return LineHeight;
            return LineHeight + (LineHeight + Spacing) * 4;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var rect = new Rect(position.x, position.y, position.width, LineHeight);
            property.isExpanded = EditorGUI.Foldout(rect, property.isExpanded, label, true);

            if (property.isExpanded)
            {
                EditorGUI.indentLevel++;

                var typeProp = property.FindPropertyRelative("_animationType");
                var durationProp = property.FindPropertyRelative("_duration");
                var easeProp = property.FindPropertyRelative("_ease");
                var delayProp = property.FindPropertyRelative("_delay");

                var viewType = DetectViewType(property);

                rect.y += LineHeight + Spacing;
                DrawAnimationTypeField(rect, typeProp, viewType);

                rect.y += LineHeight + Spacing;
                EditorGUI.PropertyField(rect, durationProp, new GUIContent("Duration"));

                rect.y += LineHeight + Spacing;
                DrawEaseField(rect, easeProp);

                rect.y += LineHeight + Spacing;
                EditorGUI.PropertyField(rect, delayProp, new GUIContent("Delay"));

                EditorGUI.indentLevel--;
            }

            EditorGUI.EndProperty();
        }

        private static void DrawAnimationTypeField(Rect rect, SerializedProperty typeProp,
            UIRegistry.UIEntryType viewType)
        {
            int current = typeProp.intValue;

            int newValue = viewType switch
            {
                UIRegistry.UIEntryType.Screen =>
                    (int)(ScreenAnimationType)EditorGUI.EnumPopup(
                        rect, "Animation", (ScreenAnimationType)current),
                UIRegistry.UIEntryType.Popup =>
                    (int)(PopupAnimationType)EditorGUI.EnumPopup(
                        rect, "Animation", (PopupAnimationType)current),
                UIRegistry.UIEntryType.Overlay =>
                    (int)(OverlayAnimationType)EditorGUI.EnumPopup(
                        rect, "Animation", (OverlayAnimationType)current),
                _ =>
                    (int)(ScreenAnimationType)EditorGUI.EnumPopup(
                        rect, "Animation", (ScreenAnimationType)current)
            };

            if (newValue != current)
            {
                typeProp.intValue = newValue;
                ApplyDefaults(typeProp.serializedObject, typeProp, viewType, newValue);
            }
        }

        private static void DrawEaseField(Rect rect, SerializedProperty easeProp)
        {
            var currentEase = (Ease)easeProp.intValue;
            var newEase = (Ease)EditorGUI.EnumPopup(rect, "Ease", currentEase);
            if (newEase != currentEase)
                easeProp.intValue = (int)newEase;
        }

        private static void ApplyDefaults(SerializedObject so, SerializedProperty typeProp,
            UIRegistry.UIEntryType viewType, int animType)
        {
            var parentPath = typeProp.propertyPath.Replace("._animationType", "");
            var durationProp = so.FindProperty(parentPath + "._duration");
            var easeProp = so.FindProperty(parentPath + "._ease");

            bool isShow = parentPath.Contains("show") || parentPath.Contains("Show");

            switch (viewType)
            {
                case UIRegistry.UIEntryType.Screen:
                    ApplyScreenDefaults(durationProp, easeProp, (ScreenAnimationType)animType, isShow);
                    break;
                case UIRegistry.UIEntryType.Popup:
                    ApplyPopupDefaults(durationProp, easeProp, (PopupAnimationType)animType, isShow);
                    break;
                case UIRegistry.UIEntryType.Overlay:
                    ApplyOverlayDefaults(durationProp, easeProp, (OverlayAnimationType)animType, isShow);
                    break;
            }
        }

        private static void ApplyScreenDefaults(SerializedProperty duration,
            SerializedProperty ease, ScreenAnimationType type, bool isShow)
        {
            switch (type)
            {
                case ScreenAnimationType.Fade:
                    duration.floatValue = 0.3f;
                    ease.intValue = (int)Ease.Linear;
                    break;
                case ScreenAnimationType.SlideLeft:
                case ScreenAnimationType.SlideRight:
                case ScreenAnimationType.SlideUp:
                case ScreenAnimationType.SlideDown:
                    duration.floatValue = 0.35f;
                    ease.intValue = isShow ? (int)Ease.OutCubic : (int)Ease.InCubic;
                    break;
                case ScreenAnimationType.Zoom:
                    duration.floatValue = 0.4f;
                    ease.intValue = isShow ? (int)Ease.OutBack : (int)Ease.InBack;
                    break;
            }
        }

        private static void ApplyPopupDefaults(SerializedProperty duration,
            SerializedProperty ease, PopupAnimationType type, bool isShow)
        {
            switch (type)
            {
                case PopupAnimationType.Fade:
                    duration.floatValue = isShow ? 0.3f : 0.25f;
                    ease.intValue = (int)Ease.Linear;
                    break;
                case PopupAnimationType.ScaleBounce:
                    duration.floatValue = isShow ? 0.35f : 0.25f;
                    ease.intValue = isShow ? (int)Ease.OutBack : (int)Ease.InBack;
                    break;
                case PopupAnimationType.ScaleFade:
                    duration.floatValue = isShow ? 0.3f : 0.25f;
                    ease.intValue = isShow ? (int)Ease.OutCubic : (int)Ease.InCubic;
                    break;
                case PopupAnimationType.SlideFromBottom:
                case PopupAnimationType.SlideFromTop:
                    duration.floatValue = isShow ? 0.35f : 0.3f;
                    ease.intValue = isShow ? (int)Ease.OutCubic : (int)Ease.InCubic;
                    break;
                case PopupAnimationType.ElasticPop:
                    duration.floatValue = isShow ? 0.5f : 0.25f;
                    ease.intValue = isShow ? (int)Ease.OutElastic : (int)Ease.InBack;
                    break;
            }
        }

        private static void ApplyOverlayDefaults(SerializedProperty duration,
            SerializedProperty ease, OverlayAnimationType type, bool isShow)
        {
            switch (type)
            {
                case OverlayAnimationType.Fade:
                    duration.floatValue = 0.4f;
                    ease.intValue = isShow ? (int)Ease.InQuad : (int)Ease.OutQuad;
                    break;
                default:
                    duration.floatValue = 0.45f;
                    ease.intValue = (int)Ease.InOutCubic;
                    break;
            }
        }

        private static UIRegistry.UIEntryType DetectViewType(SerializedProperty property)
        {
            var target = property.serializedObject.targetObject;
            if (target is BasePopup) return UIRegistry.UIEntryType.Popup;
            if (target is BaseScreen) return UIRegistry.UIEntryType.Screen;
            return UIRegistry.UIEntryType.Overlay;
        }
    }
}
