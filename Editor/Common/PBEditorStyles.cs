using UnityEngine;
using UnityEditor;

namespace ProjectBase.Editor
{
    public static class PBEditorStyles
    {
        public static readonly Color AccentBlue = new(0.31f, 0.76f, 0.97f);
        public static readonly Color AccentGray = new(0.47f, 0.56f, 0.61f);
        public static readonly Color AccentGreen = new(0.40f, 0.73f, 0.42f);
        public static readonly Color AccentOrange = new(0.90f, 0.49f, 0.13f);
        public static readonly Color AccentPurple = new(0.61f, 0.35f, 0.85f);

        private static readonly Color SavedBg = new(0.11f, 0.37f, 0.13f, 0.6f);
        private static readonly Color SavedText = new(0.65f, 0.84f, 0.65f);
        private static readonly Color NoFileBg = new(0.55f, 0.27f, 0.07f, 0.6f);
        private static readonly Color NoFileText = new(1f, 0.72f, 0.30f);
        private static readonly Color SuccessBg = new(0.11f, 0.37f, 0.13f, 0.6f);
        private static readonly Color SuccessText = new(0.65f, 0.84f, 0.65f);

        private static readonly Color BtnPrimary = new(0.16f, 0.47f, 0.80f);
        private static readonly Color BtnWarning = new(0.76f, 0.55f, 0.10f);
        private static readonly Color BtnDanger = new(0.70f, 0.22f, 0.22f);

        private static readonly Color RowEven = new(0.22f, 0.22f, 0.22f, 0.3f);
        private static readonly Color RowOdd = new(0.26f, 0.26f, 0.26f, 0.3f);

        private static GUIStyle _titleStyle;
        private static GUIStyle _sectionHeader;
        private static GUIStyle _badgeStyle;
        private static GUIStyle _pathLabel;

        public static GUIStyle TitleStyle => _titleStyle ??= new GUIStyle(UnityEditor.EditorStyles.boldLabel)
        {
            fontSize = 14,
            alignment = TextAnchor.MiddleCenter,
            padding = new RectOffset(0, 0, 4, 4)
        };

        public static GUIStyle SectionHeader => _sectionHeader ??= new GUIStyle(UnityEditor.EditorStyles.boldLabel)
        {
            fontSize = 11,
            padding = new RectOffset(8, 0, 2, 2)
        };

        public static GUIStyle BadgeStyle => _badgeStyle ??= new GUIStyle(UnityEditor.EditorStyles.miniLabel)
        {
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold,
            fontSize = 9,
            padding = new RectOffset(6, 6, 2, 2)
        };

        public static GUIStyle PathLabel => _pathLabel ??= new GUIStyle(UnityEditor.EditorStyles.miniLabel)
        {
            font = UnityEditor.EditorStyles.miniFont,
            richText = true
        };

        public static Rect BeginSection(string title, Color accentColor)
        {
            var rect = EditorGUILayout.BeginVertical(UnityEditor.EditorStyles.helpBox);
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, 3, rect.height), accentColor);
            EditorGUILayout.Space(2);
            EditorGUILayout.LabelField(title, SectionHeader);
            return rect;
        }

        public static void EndSection()
        {
            EditorGUILayout.Space(2);
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(2);
        }

        public static Rect BeginCurrentDataSection(string title = "CURRENT DATA")
        {
            return BeginSection(title, AccentBlue);
        }

        public static Rect BeginDefaultDataSection(string title = "DEFAULT DATA (TEMPLATE)")
        {
            return BeginSection(title, AccentGray);
        }

        public static Rect BeginModulesSection(string title = "MODULES")
        {
            return BeginSection(title, AccentGreen);
        }

        public static void DrawTitle(string text)
        {
            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField(text, TitleStyle);
            var rect = GUILayoutUtility.GetRect(0, 1, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(rect, new Color(0.4f, 0.4f, 0.4f, 0.5f));
            EditorGUILayout.Space(4);
        }

        public static void DrawSavedBadge() => DrawBadge("SAVED", SavedBg, SavedText);
        public static void DrawNoFileBadge() => DrawBadge("NO FILE", NoFileBg, NoFileText);
        public static void DrawSuccessBadge(string text) => DrawBadge(text, SuccessBg, SuccessText);

        public static void DrawBadge(string text, Color bgColor, Color textColor)
        {
            var style = new GUIStyle(BadgeStyle) { normal = { textColor = textColor } };
            var content = new GUIContent(text);
            var size = style.CalcSize(content);
            var rect = GUILayoutUtility.GetRect(size.x + 4, 16, GUILayout.MaxWidth(size.x + 4));
            EditorGUI.DrawRect(rect, bgColor);
            GUI.Label(rect, content, style);
        }

        public static bool PrimaryButton(string text, float height = 24)
        {
            var old = GUI.backgroundColor;
            GUI.backgroundColor = BtnPrimary;
            var clicked = GUILayout.Button(text, GUILayout.Height(height));
            GUI.backgroundColor = old;
            return clicked;
        }

        public static bool WarningButton(string text, float height = 24)
        {
            var old = GUI.backgroundColor;
            GUI.backgroundColor = BtnWarning;
            var clicked = GUILayout.Button(text, GUILayout.Height(height));
            GUI.backgroundColor = old;
            return clicked;
        }

        public static bool DangerButton(string text, float height = 24)
        {
            var old = GUI.backgroundColor;
            GUI.backgroundColor = BtnDanger;
            var clicked = GUILayout.Button(text, GUILayout.Height(height));
            GUI.backgroundColor = old;
            return clicked;
        }

        public static Color GetRowColor(int index)
        {
            return index % 2 == 0 ? RowEven : RowOdd;
        }

        public static void DrawFileInfo(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !System.IO.File.Exists(filePath)) return;

            var info = new System.IO.FileInfo(filePath);
            var sizeText = FormatFileSize(info.Length);

            EditorGUILayout.LabelField(
                $"<color=#888>Path:</color>  .../{info.Name}", PathLabel);
            EditorGUILayout.LabelField(
                $"<color=#888>Size:</color>  {sizeText}    " +
                $"<color=#888>Modified:</color>  {info.LastWriteTime:yyyy-MM-dd HH:mm:ss}",
                PathLabel);
        }

        public static string FormatFileSize(long bytes)
        {
            if (bytes < 1024) return $"{bytes} B";
            return $"{bytes / 1024f:F1} KB";
        }
    }
}
