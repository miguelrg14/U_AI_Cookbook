// Miguel Rodríguez Gallego
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AICookbook.Menu
{
    /// <summary>
    ///     Persistent helper created automatically on startup. Inside any demo it draws a small
    ///     "Menu" button and listens to Esc to go back to the main menu, so the original demo
    ///     scenes don't need to be modified.
    /// </summary>
    public class DemoNavigator : MonoBehaviour
    {
        static DemoNavigator instance;
        GUIStyle buttonStyle;

        static bool InMenu => SceneManager.GetActiveScene().name == DemoCatalog.MenuScene;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            if (instance != null) return;
            var go = new GameObject("[DemoNavigator]");
            DontDestroyOnLoad(go);
            instance = go.AddComponent<DemoNavigator>();
        }

        void Update()
        {
#if ENABLE_LEGACY_INPUT_MANAGER
            if (!InMenu && Input.GetKeyDown(KeyCode.Escape))
                BackToMenu();
#endif
        }

        void OnGUI()
        {
            if (InMenu) return;

            float scale = Mathf.Max(1f, Screen.height / 1080f);
            buttonStyle ??= new GUIStyle(GUI.skin.button) { fontStyle = FontStyle.Bold };
            buttonStyle.fontSize = Mathf.RoundToInt(16 * scale);

            GUI.depth = -1000;
            var rect = new Rect(12 * scale, Screen.height - 48 * scale, 160 * scale, 36 * scale);
            if (GUI.Button(rect, "< Menu (Esc)", buttonStyle))
                BackToMenu();
        }

        public static void BackToMenu()
        {
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            SceneManager.LoadScene(DemoCatalog.MenuScene);
        }
    }
}
