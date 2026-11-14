// Miguel Rodríguez Gallego
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AICookbook.Menu
{
    /// <summary>
    ///     Persistent helper created automatically on startup. Inside any demo it draws:
    ///     <list type="bullet">
    ///         <item>a help panel with the technique, controls and what to watch (H to toggle)</item>
    ///         <item>a "Menu" button (or Esc) to go back to the main menu</item>
    ///     </list>
    ///     so the original demo scenes don't need to be modified.
    ///     In the editor, F12 saves a screenshot of the current demo as its menu thumbnail.
    /// </summary>
    public class DemoNavigator : MonoBehaviour
    {
        public const string ThumbnailsFolder = "DemoThumbnails";

        static DemoNavigator instance;

        bool showHelp = true;
        bool hideOverlay;
        bool hasDemoInfo;
        DemoInfo currentDemo;

        GUIStyle panelStyle, titleStyle, accentStyle, textStyle, headerStyle, buttonStyle;
        Texture2D panelBackground;
        float stylesScale = -1f;

        static bool InMenu => SceneManager.GetActiveScene().name == DemoCatalog.MenuScene;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            if (instance != null) return;
            var go = new GameObject("[DemoNavigator]");
            DontDestroyOnLoad(go);
            instance = go.AddComponent<DemoNavigator>();
        }

        void Awake()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
        }

        void OnDestroy() => SceneManager.sceneLoaded -= OnSceneLoaded;

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            hasDemoInfo = DemoCatalog.TryGet(scene.name, out currentDemo);
            showHelp = true;
        }

        void Update()
        {
#if ENABLE_LEGACY_INPUT_MANAGER
            if (InMenu) return;

            if (Input.GetKeyDown(KeyCode.Escape))
                BackToMenu();

            if (Input.GetKeyDown(KeyCode.H))
                showHelp = !showHelp;

#if UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.F12))
                StartCoroutine(CaptureThumbnail());
#endif
#endif
        }

        void OnGUI()
        {
            if (InMenu || hideOverlay) return;

            float scale = Mathf.Max(1f, Screen.height / 1080f);
            BuildStyles(scale);
            GUI.depth = -1000;

            // Bottom-left buttons
            var menuRect = new Rect(12 * scale, Screen.height - 48 * scale, 160 * scale, 36 * scale);
            if (GUI.Button(menuRect, "< Menu (Esc)", buttonStyle))
                BackToMenu();

            if (!hasDemoInfo) return;

            var helpRect = new Rect(menuRect.xMax + 8 * scale, menuRect.y, 130 * scale, menuRect.height);
            if (GUI.Button(helpRect, showHelp ? "Hide help (H)" : "Help (H)", buttonStyle))
                showHelp = !showHelp;

            if (showHelp)
                DrawHelpPanel(scale);
        }

        void DrawHelpPanel(float scale)
        {
            float width = 420 * scale;
            var area = new Rect(12 * scale, 12 * scale, width, Screen.height - 80 * scale);

            GUILayout.BeginArea(area);
            GUILayout.BeginVertical(panelStyle);

            GUILayout.Label($"{currentDemo.Number}  ·  {currentDemo.Technique.ToUpperInvariant()}", accentStyle);
            GUILayout.Label(currentDemo.Title, titleStyle);
            GUILayout.Label(currentDemo.Description, textStyle);

            GUILayout.Space(10 * scale);
            GUILayout.Label("CONTROLS", headerStyle);
            foreach (string control in currentDemo.Controls)
                GUILayout.Label("•  " + control, textStyle);

            GUILayout.Space(10 * scale);
            GUILayout.Label("WHAT TO WATCH", headerStyle);
            GUILayout.Label(currentDemo.WhatToWatch, textStyle);

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        void BuildStyles(float scale)
        {
            if (Mathf.Approximately(scale, stylesScale) && panelStyle != null) return;
            stylesScale = scale;

            if (panelBackground == null)
            {
                panelBackground = new Texture2D(1, 1);
                panelBackground.SetPixel(0, 0, new Color(0.07f, 0.075f, 0.105f, 0.88f));
                panelBackground.Apply();
            }

            int padding = Mathf.RoundToInt(18 * scale);
            panelStyle = new GUIStyle(GUI.skin.box)
            {
                padding = new RectOffset(padding, padding, padding, padding),
                normal = { background = panelBackground }
            };
            var accent = new Color32(110, 205, 165, 255);
            var secondary = new Color32(205, 210, 222, 255);
            titleStyle = Label(26 * scale, FontStyle.Bold, Color.white);
            accentStyle = Label(13 * scale, FontStyle.Bold, accent);
            headerStyle = Label(13 * scale, FontStyle.Bold, accent);
            textStyle = Label(15 * scale, FontStyle.Normal, secondary);
            buttonStyle = new GUIStyle(GUI.skin.button) { fontStyle = FontStyle.Bold, fontSize = Mathf.RoundToInt(16 * scale) };
        }

        static GUIStyle Label(float size, FontStyle style, Color color) => new GUIStyle(GUI.skin.label)
        {
            fontSize = Mathf.RoundToInt(size),
            fontStyle = style,
            wordWrap = true,
            normal = { textColor = color }
        };

        public static void BackToMenu()
        {
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            SceneManager.LoadScene(DemoCatalog.MenuScene);
        }

#if UNITY_EDITOR
        /// <summary>
        ///     Saves the current frame (without this overlay) as the menu thumbnail of the demo
        /// </summary>
        IEnumerator CaptureThumbnail()
        {
            string scene = SceneManager.GetActiveScene().name;
            string folder = $"Assets/_Menu/Resources/{ThumbnailsFolder}";
            System.IO.Directory.CreateDirectory(folder);
            string path = $"{folder}/{scene}.png";

            hideOverlay = true;
            yield return new WaitForEndOfFrame();
            ScreenCapture.CaptureScreenshot(path);
            yield return null;
            yield return null;
            hideOverlay = false;

            UnityEditor.AssetDatabase.Refresh();
            Debug.Log($"[DemoNavigator] Thumbnail saved: {path}");
        }
#endif
    }
}
