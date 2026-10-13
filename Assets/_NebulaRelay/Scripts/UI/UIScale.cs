using UnityEngine;

namespace NebulaRelay.UI
{
    /// <summary>
    /// Shared static UI scaling helper for Nebula Relay.
    /// Uses a 1280x720 reference resolution to ensure UI text, buttons, and telemetry
    /// are 1.5x larger and immediately readable on laptops (1366x768, 1536x864, etc.)
    /// as well as 1080p, 1440p, 4K, and ultra-wide displays.
    /// </summary>
    public static class UIScale
    {
        public const float RefWidth = 1280f;
        public const float RefHeight = 720f;
        public const float MinScaleClamp = 0.5f;
        public const float MaxScaleClamp = 3.0f;
        public const int MinFontSize = 14;

        // Standardized font sizes in 1280x720 reference space
        public const int FontSmallLabel = 14;
        public const int FontBodyDetail = 16;
        public const int FontCardHeader = 18;
        public const int FontObjective = 20;
        public const int FontMenuSubtitle = 20;
        public const int FontButton = 20;
        public const int FontValue = 22;
        public const int FontHUDTitle = 26;
        public const int FontModalTitle = 32;
        public const int FontMenuMainTitle = 64;

        /// <summary>
        /// Calculates the GUI scale factor for the current screen size.
        /// scale = Mathf.Min(Screen.width / 1280f, Screen.height / 720f), clamped to 0.5..3.0.
        /// </summary>
        public static float GetScale()
        {
            return GetScale(Screen.width, Screen.height);
        }

        /// <summary>
        /// Calculates the GUI scale factor for arbitrary screen dimensions.
        /// </summary>
        public static float GetScale(float screenWidth, float screenHeight)
        {
            float scale = Mathf.Min(screenWidth / RefWidth, screenHeight / RefHeight);
            return Mathf.Clamp(scale, MinScaleClamp, MaxScaleClamp);
        }

        /// <summary>
        /// Returns the effective viewport width in scaled reference units.
        /// On aspect ratios wider than 16:9 (e.g. 21:9), viewWidth > 1280.
        /// </summary>
        public static float GetViewWidth()
        {
            float scale = GetScale();
            return Screen.width / scale;
        }

        /// <summary>
        /// Returns the effective viewport height in scaled reference units.
        /// On aspect ratios taller than 16:9 (e.g. 16:10 or 4:3), viewHeight > 720.
        /// </summary>
        public static float GetViewHeight()
        {
            float scale = GetScale();
            return Screen.height / scale;
        }

        /// <summary>
        /// Applies the scaling matrix to GUI.matrix and returns the original matrix to restore.
        /// </summary>
        public static Matrix4x4 BeginMatrix()
        {
            float scale = GetScale();
            Matrix4x4 orig = GUI.matrix;
            GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(scale, scale, 1f));
            return orig;
        }

        /// <summary>
        /// Restores GUI.matrix to its original state.
        /// </summary>
        public static void EndMatrix(Matrix4x4 origMatrix)
        {
            GUI.matrix = origMatrix;
        }

        /// <summary>
        /// Draws text with a subtle dark drop shadow for crisp legibility over vibrant nebulae.
        /// </summary>
        public static void DrawShadowedLabel(Rect r, string text, GUIStyle style, Color textColor, Color shadowColor, float offset = 1.5f)
        {
            Color prev = style.normal.textColor;
            style.normal.textColor = shadowColor;
            GUI.Label(new Rect(r.x + offset, r.y + offset, r.width, r.height), text, style);
            style.normal.textColor = textColor;
            GUI.Label(r, text, style);
            style.normal.textColor = prev;
        }

        /// <summary>
        /// Draws GUIContent with a subtle dark drop shadow for crisp legibility over vibrant nebulae.
        /// </summary>
        public static void DrawShadowedLabel(Rect r, GUIContent content, GUIStyle style, Color textColor, Color shadowColor, float offset = 1.5f)
        {
            Color prev = style.normal.textColor;
            style.normal.textColor = shadowColor;
            GUI.Label(new Rect(r.x + offset, r.y + offset, r.width, r.height), content, style);
            style.normal.textColor = textColor;
            GUI.Label(r, content, style);
            style.normal.textColor = prev;
        }
    }
}
