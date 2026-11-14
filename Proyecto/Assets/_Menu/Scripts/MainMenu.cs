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

        [Header("Layout (reference pixels, 1920x1080)")]
        [SerializeField] int maxColumns = 3;
        [SerializeField] float minCardWidth = 400;
        [SerializeField] float maxCardWidth = 560;
        [SerializeField] float cardHeight = 240;
        [SerializeField] float cardSpacing = 28;

        RectTransform canvasRect;
        VerticalLayoutGroup column;
        GridLayoutGroup grid;
        RectTransform gridRect;
        Vector2 lastCanvasSize;

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

        /// <summary>
        ///     Re-fits the layout whenever the window (and so the canvas) changes size
        /// </summary>
        void Update()
        {
            if (canvasRect == null) return;
            Vector2 size = canvasRect.rect.size;
            if (size == lastCanvasSize) return;
            lastCanvasSize = size;
            Relayout(size);
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
            canvasRect = (RectTransform)canvasGO.transform;

            var background = CreateImage("Background", canvasGO.transform, backgroundColor);
            Stretch(background.rectTransform);

            // Vertical column: header, scrollable demo grid, footer
            var content = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup));
            content.transform.SetParent(canvasGO.transform, false);
            Stretch((RectTransform)content.transform);
            column = content.GetComponent<VerticalLayoutGroup>();
            column.spacing = 12;
            column.childAlignment = TextAnchor.UpperCenter;
            column.childControlWidth = true;
            column.childControlHeight = true;
            column.childForceExpandWidth = true;
            column.childForceExpandHeight = false;

            var title = CreateText(content.transform, "AI Cookbook", 86, FontStyles.Bold, Color.white, TextAlignmentOptions.Center);
            title.enableAutoSizing = true;
            title.fontSizeMin = 40;
            title.fontSizeMax = 86;
            CreateText(content.transform, "Game AI techniques in Unity  ·  Miguel Rodríguez Gallego", 30, FontStyles.Normal, accentColor, TextAlignmentOptions.Center);
            CreateSpacer(content.transform, 24);

            // Scroll view: only scrolls when the cards don't fit in the window
            var scrollGO = new GameObject("DemosScroll", typeof(RectTransform), typeof(ScrollRect), typeof(LayoutElement));
            scrollGO.transform.SetParent(content.transform, false);
            scrollGO.GetComponent<LayoutElement>().flexibleHeight = 1;
            var scroll = scrollGO.GetComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 40;

            var viewport = new GameObject("Viewport", typeof(RectTransform), typeof(RectMask2D), typeof(Image));
            viewport.transform.SetParent(scrollGO.transform, false);
            Stretch((RectTransform)viewport.transform);
            viewport.GetComponent<Image>().color = Color.clear; // receives drag / wheel events
            scroll.viewport = (RectTransform)viewport.transform;

            var gridGO = new GameObject("Demos", typeof(RectTransform), typeof(GridLayoutGroup), typeof(ContentSizeFitter));
            gridGO.transform.SetParent(viewport.transform, false);
            gridRect = (RectTransform)gridGO.transform;
            gridRect.anchorMin = new Vector2(0, 1);
            gridRect.anchorMax = new Vector2(1, 1);
            gridRect.pivot = new Vector2(0.5f, 1);
            gridRect.offsetMin = gridRect.offsetMax = Vector2.zero;
            gridGO.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            scroll.content = gridRect;

            grid = gridGO.GetComponent<GridLayoutGroup>();
            grid.spacing = new Vector2(cardSpacing, cardSpacing);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.childAlignment = TextAnchor.UpperCenter;

            foreach (var demo in DemoCatalog.Demos)
                CreateCard(gridGO.transform, demo);

            CreateText(content.transform, "Press Esc or the Menu button inside any demo to come back here.", 22, FontStyles.Italic, secondaryTextColor, TextAlignmentOptions.Center);

            Relayout(canvasRect.rect.size);
        }

        /// <summary>
        ///     Margins, number of columns and card width from the current canvas size
        /// </summary>
        void Relayout(Vector2 canvasSize)
        {
            int side = Mathf.RoundToInt(Mathf.Clamp(canvasSize.x * 0.05f, 24, 100));
            int top = Mathf.RoundToInt(Mathf.Clamp(canvasSize.y * 0.06f, 20, 80));
            column.padding = new RectOffset(side, side, top, Mathf.RoundToInt(top * 0.6f));

            float available = canvasSize.x - side * 2;
            int columns = Mathf.Clamp(Mathf.FloorToInt((available + cardSpacing) / (minCardWidth + cardSpacing)), 1, Mathf.Max(1, maxColumns));
            float cardWidth = Mathf.Min(maxCardWidth, (available - cardSpacing * (columns - 1)) / columns);

            grid.constraintCount = columns;
            grid.cellSize = new Vector2(Mathf.Max(200, cardWidth), cardHeight);
            LayoutRebuilder.MarkLayoutForRebuild(gridRect);
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

            // Optional screenshot (saved with F12 inside the demo) as a darkened card background
            var thumbnail = Resources.Load<Texture2D>($"{DemoNavigator.ThumbnailsFolder}/{demo.Scene}");
            if (thumbnail != null)
            {
                card.AddComponent<RectMask2D>();
                var thumbGO = new GameObject("Thumbnail", typeof(RectTransform), typeof(RawImage), typeof(AspectRatioFitter), typeof(LayoutElement));
                thumbGO.transform.SetParent(card.transform, false);
                thumbGO.GetComponent<LayoutElement>().ignoreLayout = true;
                var raw = thumbGO.GetComponent<RawImage>();
                raw.texture = thumbnail;
                raw.raycastTarget = false;
                var fitter = thumbGO.GetComponent<AspectRatioFitter>();
                fitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
                fitter.aspectRatio = (float)thumbnail.width / thumbnail.height;

                // The screenshot is what the button tints: dark by default, brighter on hover
                image.color = cardColor;
                raw.color = Color.white;
                button.targetGraphic = raw;
                colors.normalColor = new Color(0.32f, 0.32f, 0.36f);
                colors.highlightedColor = new Color(0.55f, 0.55f, 0.6f);
                colors.selectedColor = colors.highlightedColor;
                colors.pressedColor = new Color(0.7f, 0.7f, 0.75f);
            }
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

            // Tag and title keep their height; only the description shrinks when the card is narrow
            var techniqueText = CreateText(card.transform, $"{demo.Number}  ·  {demo.Technique.ToUpperInvariant()}", 20, FontStyles.Bold, accentColor, TextAlignmentOptions.TopLeft);
            techniqueText.gameObject.AddComponent<LayoutElement>().minHeight = 26;
            var titleText = CreateText(card.transform, demo.Title, 38, FontStyles.Bold, Color.white, TextAlignmentOptions.TopLeft);
            titleText.gameObject.AddComponent<LayoutElement>().minHeight = 48;
            titleText.overflowMode = TextOverflowModes.Ellipsis;
            var description = CreateText(card.transform, demo.Description, 22, FontStyles.Normal, secondaryTextColor, TextAlignmentOptions.TopLeft);
            description.overflowMode = TextOverflowModes.Ellipsis; // narrow cards cut the text with "..." instead of overflowing
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
