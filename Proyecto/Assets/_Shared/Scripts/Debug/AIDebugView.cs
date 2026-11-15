// Miguel Rodríguez Gallego
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace AICookbook.Shared
{
    /// <summary>
    ///     In-game debug view of the agents' AI, toggled with G. It works in builds (not only in the
    ///     Scene view like Gizmos), so anyone playing the demo can see what the agents perceive and decide:
    ///     <list type="bullet">
    ///         <item>detection ranges of every <see cref="TargetDetectorBase"/> and lines to their targets</item>
    ///         <item>a label above each agent with its current decision or state (<see cref="IAIDebugLabel"/>)</item>
    ///     </list>
    /// </summary>
    public class AIDebugView : MonoBehaviour
    {
        public static bool Enabled { get; set; }

        static readonly List<TargetDetectorBase> detectors = new List<TargetDetectorBase>();
        static readonly List<IAIDebugLabel> labels = new List<IAIDebugLabel>();
        static AIDebugView instance;

        public static readonly Color SeeRangeColor = new Color(1f, 0.85f, 0.2f, 0.45f);
        public static readonly Color CloseRangeColor = new Color(1f, 0.35f, 0.25f, 0.75f);
        public static readonly Color CloseRangeExitColor = new Color(1f, 0.35f, 0.25f, 0.25f);

        const int CircleSegments = 48;

        Material lineMaterial;
        GUIStyle labelStyle, statusStyle;

        public static void Register(TargetDetectorBase detector) { if (!detectors.Contains(detector)) detectors.Add(detector); }
        public static void Unregister(TargetDetectorBase detector) => detectors.Remove(detector);
        public static void Register(IAIDebugLabel label) { if (!labels.Contains(label)) labels.Add(label); }
        public static void Unregister(IAIDebugLabel label) => labels.Remove(label);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            if (instance != null) return;
            var go = new GameObject("[AIDebugView]");
            DontDestroyOnLoad(go);
            instance = go.AddComponent<AIDebugView>();
        }

        /// <summary> Nothing to show in scenes without agents (menu, Minimax, genetics...) </summary>
        static bool HasContent => detectors.Count > 0 || labels.Count > 0;

        void Update()
        {
#if ENABLE_LEGACY_INPUT_MANAGER
            if (HasContent && Input.GetKeyDown(KeyCode.G))
                Enabled = !Enabled;
#endif
        }

        // ---------------------------------------------------------------- ranges and target lines

        void OnRenderObject()
        {
            if (!Enabled || detectors.Count == 0 || !EnsureMaterial()) return;

            lineMaterial.SetPass(0);
            GL.PushMatrix();
            GL.Begin(GL.LINES);

            foreach (var detector in detectors)
            {
                if (detector == null || !detector.isActiveAndEnabled) continue;
                Vector3 position = detector.transform.position;

                DrawCircle(position, detector.SeeTargetRange, SeeRangeColor);
                if (detector.target != null)
                    DrawLine(position, detector.target.transform.position, SeeRangeColor);

                if (detector is TwoRangeTargetDetector twoRange)
                {
                    DrawCircle(position, twoRange.CloseTargetRange, CloseRangeColor);
                    if (twoRange.CloseRangeExitDistance > twoRange.CloseTargetRange)
                        DrawCircle(position, twoRange.CloseRangeExitDistance, CloseRangeExitColor);
                    if (twoRange.CloseTarget != null)
                        DrawLine(position, twoRange.CloseTarget.transform.position, CloseRangeColor);
                }
            }

            GL.End();
            GL.PopMatrix();
        }

        bool EnsureMaterial()
        {
            if (lineMaterial == null) lineMaterial = GLMaterial.Get();
            return lineMaterial != null;
        }

        static void DrawLine(Vector3 from, Vector3 to, Color color)
        {
            GL.Color(color);
            GL.Vertex(from);
            GL.Vertex(to);
        }

        static void DrawCircle(Vector3 center, float radius, Color color)
        {
            GL.Color(color);
            Vector3 previous = center + new Vector3(radius, 0f, 0f);
            for (int i = 1; i <= CircleSegments; i++)
            {
                float angle = i * Mathf.PI * 2f / CircleSegments;
                Vector3 next = center + new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
                GL.Vertex(previous);
                GL.Vertex(next);
                previous = next;
            }
        }

        // ---------------------------------------------------------------- labels

        void OnGUI()
        {
            if (!HasContent) return;

            float scale = OverlayScale.Current;
            BuildStyles(scale);

            var status = new GUIContent(Enabled ? "AI debug: ON  (G)" : "AI debug: OFF  (G)");
            Vector2 statusSize = statusStyle.CalcSize(status);
            float margin = 12 * scale;
            GUI.Label(new Rect(Screen.width - statusSize.x - margin, margin, statusSize.x, statusSize.y), status, statusStyle);

            if (!Enabled) return;

            Camera cam = Camera.main;
            if (cam == null) return;

            foreach (var label in labels)
            {
                if (label is Behaviour behaviour && (behaviour == null || !behaviour.isActiveAndEnabled)) continue;

                string text = label.DebugLabel;
                if (string.IsNullOrEmpty(text)) continue;

                Vector3 screen = cam.WorldToScreenPoint(label.LabelPosition);
                if (screen.z <= 0) continue; // behind the camera

                // Size from the text itself (padding included), so it is never cut
                var content = new GUIContent(text);
                Vector2 size = labelStyle.CalcSize(content);
                var rect = new Rect(screen.x - size.x / 2f, Screen.height - screen.y - size.y, size.x, size.y);
                GUI.Label(rect, content, labelStyle);
            }
        }

        Texture2D labelBackground;
        float stylesScale = -1f;

        void BuildStyles(float scale)
        {
            if (labelStyle != null && Mathf.Approximately(scale, stylesScale)) return;
            stylesScale = scale;

            if (labelBackground == null)
            {
                labelBackground = new Texture2D(1, 1) { hideFlags = HideFlags.HideAndDontSave };
                labelBackground.SetPixel(0, 0, new Color(0f, 0f, 0f, 0.7f));
                labelBackground.Apply();
            }

            int padX = Mathf.RoundToInt(8 * scale);
            int padY = Mathf.RoundToInt(4 * scale);
            labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = OverlayScale.Font(16, scale),
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                wordWrap = false,                 // one line: CalcSize gives the exact width
                clipping = TextClipping.Overflow,
                padding = new RectOffset(padX, padX, padY, padY),
                margin = new RectOffset(0, 0, 0, 0),
                normal = { textColor = Color.white, background = labelBackground }
            };
            statusStyle = new GUIStyle(labelStyle) { normal = { textColor = new Color32(110, 205, 165, 255), background = labelBackground } };
        }
    }
}
