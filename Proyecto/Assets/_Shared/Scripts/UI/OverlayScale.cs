// Miguel Rodríguez Gallego
using UnityEngine;

namespace AICookbook.Shared
{
    /// <summary>
    ///     Scale for the IMGUI overlays (help panel, buttons, AI debug labels) so they follow the window size
    ///     the same way a uGUI CanvasScaler set to "Scale With Screen Size" (1920x1080, match 0.5) does.
    ///     Clamped so text never gets unreadable in small windows nor huge on 4K screens.
    /// </summary>
    public static class OverlayScale
    {
        public const float ReferenceWidth = 1920f;
        public const float ReferenceHeight = 1080f;
        public const float Min = 0.75f;
        public const float Max = 2.5f;

        public static float Current
        {
            get
            {
                float widthLog = Mathf.Log(Mathf.Max(1, Screen.width) / ReferenceWidth, 2f);
                float heightLog = Mathf.Log(Mathf.Max(1, Screen.height) / ReferenceHeight, 2f);
                return Mathf.Clamp(Mathf.Pow(2f, (widthLog + heightLog) * 0.5f), Min, Max);
            }
        }

        /// <summary>
        ///     <paramref name="size"/> in reference pixels, scaled and rounded for a GUIStyle
        /// </summary>
        public static int Font(float size, float scale) => Mathf.Max(8, Mathf.RoundToInt(size * scale));
    }
}
