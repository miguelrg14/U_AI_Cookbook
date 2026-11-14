// Miguel Rodríguez Gallego
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace AICookbook.Menu
{
    /// <summary>
    ///     Builds the demo selection menu at runtime from <see cref="DemoCatalog"/>.
    ///     Adding a demo only requires a new entry in the catalog and the scene in Build Profiles.
    /// </summary>
    public class MainMenu : MonoBehaviour
    {
        [Header("Colors")]
        [SerializeField] Color backgroundColor = new Color32(17, 19, 27, 255);
        [SerializeField] Color cardColor = new Color32(32, 36, 50, 255);
        [SerializeField] Color cardHoverColor = new Color32(48, 55, 78, 255);
        [SerializeField] Color accentColor = new Color32(110, 205, 165, 255);
        [SerializeField] Color secondaryTextColor = new Color32(196, 202, 216, 255);

        [Header("Layout")]
        [SerializeField] int columns = 3;
        [SerializeField] Vector2 cardSize = new Vector2(540, 240);

        void Awake()
        {
            // Demos can leave these changed (e.g. genetic algorithm speed-up, free camera)
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            EnsureEventSystem();
            BuildUI();
        }

        static void EnsureEventSystem()
        {
            if (FindAnyObjectByType<EventSystem>() != null) return;
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }

        void BuildUI()
        {
            var canvasGO = new GameObject("MenuCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGO.transform.SetParent(transform, false);
            canvasGO.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGO.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            var background = CreateImage("Background", canvasGO.transform, backgroundColor);
            Stretch(background.rectTransform);

            // Vertical column: header, demo grid, footer
            var content = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup));
            content.transform.SetParent(canvasGO.transform, false);
            Stretch((RectTransform)content.transform);
            var column = content.GetComponent<VerticalLayoutGroup>();
            column.padding = new RectOffset(100, 100, 80, 50);
            column.spacing = 12;
            column.childAlignment = TextAnchor.UpperCenter;
            column.childControlWidth = true;
            column.childControlHeight = true;
            column.childForceExpandWidth = true;
            column.childForceExpandHeight = false;

            CreateText(content.transform, "AI Cookbook", 86, FontStyles.Bold, Color.white, TextAlignmentOptions.Center);
            CreateText(content.transform, "Game AI techniques in Unity  ·  Miguel Rodríguez Gallego", 30, FontStyles.Normal, accentColor, TextAlignmentOptions.Center);
            CreateSpacer(content.transform, 40);

            var gridGO = new GameObject("Demos", typeof(RectTransform), typeof(GridLayoutGroup), typeof(LayoutElement));
            gridGO.transform.SetParent(content.transform, false);
            var grid = gridGO.GetComponent<GridLayoutGroup>();
            grid.cellSize = cardSize;
            grid.spacing = new Vector2(28, 28);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = Mathf.Max(1, columns);
            grid.childAlignment = TextAnchor.UpperCenter;
            gridGO.GetComponent<LayoutElement>().flexibleHeight = 1;

            foreach (var demo in DemoCatalog.Demos)
                CreateCard(gridGO.transform, demo);

            CreateText(content.transform, "Press Esc or the Menu button inside any demo to come back here.", 22, FontStyles.Italic, secondaryTextColor, TextAlignmentOptions.Center);
        }

        void CreateCard(Transform parent, DemoInfo demo)
        {
            var card = new GameObject(demo.Scene, typeof(RectTransform), typeof(Image), typeof(Button), typeof(VerticalLayoutGroup));
            card.transform.SetParent(parent, false);

            var image = card.GetComponent<Image>();
            image.color = Color.white; // tinted by the button color block

            var button = card.GetComponent<Button>();
            button.targetGraphic = image;
            var colors = button.colors;
            colors.normalColor = cardColor;
            colors.highlightedColor = cardHoverColor;
            colors.selectedColor = cardHoverColor;
            colors.pressedColor = Color.Lerp(cardHoverColor, accentColor, 0.35f);
            colors.colorMultiplier = 1f;
            colors.fadeDuration = 0.08f;
            button.colors = colors;
            string scene = demo.Scene;
            button.onClick.AddListener(() => LoadDemo(scene));

            var layout = card.GetComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(30, 30, 24, 24);
            layout.spacing = 8;
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            CreateText(card.transform, $"{demo.Number}  ·  {demo.Technique.ToUpperInvariant()}", 20, FontStyles.Bold, accentColor, TextAlignmentOptions.TopLeft);
            CreateText(card.transform, demo.Title, 38, FontStyles.Bold, Color.white, TextAlignmentOptions.TopLeft);
            CreateText(card.transform, demo.Description, 22, FontStyles.Normal, secondaryTextColor, TextAlignmentOptions.TopLeft);
        }

        static void LoadDemo(string scene)
        {
            if (!Application.CanStreamedLevelBeLoaded(scene))
            {
                Debug.LogError($"[MainMenu] Scene '{scene}' is not in Build Profiles > Scene List.");
                return;
            }
            SceneManager.LoadScene(scene);
        }

        // ---------------------------------------------------------------- UI helpers

        static Image CreateImage(string name, Transform parent, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var image = go.GetComponent<Image>();
            image.color = color;
            return image;
        }

        static TextMeshProUGUI CreateText(Transform parent, string text, float size, FontStyles style, Color color, TextAlignmentOptions alignment)
        {
            var go = new GameObject("Text", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = size;
            tmp.fontStyle = style;
            tmp.color = color;
            tmp.alignment = alignment;
            tmp.raycastTarget = false;
            return tmp;
        }

        static void CreateSpacer(Transform parent, float height)
        {
            var go = new GameObject("Spacer", typeof(RectTransform), typeof(LayoutElement));
            go.transform.SetParent(parent, false);
            go.GetComponent<LayoutElement>().minHeight = height;
        }

        static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }
}
