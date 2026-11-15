// Miguel Rodríguez Gallego
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using AICookbook.Shared;

namespace AICookbook.ReinforcementLearning
{
    /// <summary>
    ///     Visual demo of Q-Learning and SARSA on a grid. Everything is built and drawn at runtime:
    ///     <list type="bullet">
    ///         <item>each cell is coloured by the value the agent has learned for it (heat map of max Q)</item>
    ///         <item>arrows show the best action learned in each cell (the policy)</item>
    ///         <item>the cyan line is the path the agent would follow now without exploring</item>
    ///         <item>the panel on the right controls the algorithm, the parameters and the speed, and plots the learning curve</item>
    ///     </list>
    ///     The maze can be edited while it learns, to see how the agent adapts.
    /// </summary>
    public class ReinforcementLearningDemo : MonoBehaviour
    {
        enum Algorithm { QLearning, Sarsa }

        [SerializeField] int seed = 0;

        // Simulation
        GridMap map = GridMaps.Maze;
        int mapIndex;
        Algorithm algorithm = Algorithm.QLearning;
        GridWorld world;
        TabularLearner learner;
        bool running = true;
        float stepsPerSecond = 60f;
        float stepBudget;
        const int MaxStepsPerFrame = 20000;

        // Rendering
        Camera cam;
        Material lineMaterial;
        List<GridPos> greedyPath = new List<GridPos>();
        float minValue, maxValue;

        // UI
        Rect panelRect;
        Vector2 panelScroll;
        GUIStyle panelStyle, titleStyle, labelStyle, smallStyle, headerStyle, buttonStyle;
        Texture2D panelBackground, pixel;
        float stylesScale = -1f;
        static readonly Color Accent = new Color32(110, 205, 165, 255);

        static readonly string[] AlgorithmNames = { "Q-Learning", "SARSA" };

        void Start()
        {
            cam = Camera.main;
            if (cam != null)
            {
                cam.orthographic = true;
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = new Color32(17, 19, 27, 255);
            }
            LoadMap(0);
        }

        // ---------------------------------------------------------------- simulation

        void LoadMap(int index)
        {
            mapIndex = index;
            map = GridMaps.All[index];
            world = map.CreateWorld();
            learner = null; // a new map starts with its own default parameters
            CreateLearner();
        }

        void CreateLearner()
        {
            float alpha = map.Alpha, gamma = map.Gamma, epsilon = map.Epsilon;
            if (learner != null && learner.Episodes > 0)
            {
                // Keep the parameters the user was trying when switching algorithm
                alpha = learner.Alpha; gamma = learner.Gamma; epsilon = learner.Epsilon;
            }

            learner = algorithm == Algorithm.QLearning ? new QLearning(world, seed) : (TabularLearner)new Sarsa(world, seed);
            learner.Alpha = alpha;
            learner.Gamma = gamma;
            learner.Epsilon = epsilon;
            stepBudget = 0f;
            RefreshPolicy();
        }

        void ResetParameters()
        {
            learner.Alpha = map.Alpha;
            learner.Gamma = map.Gamma;
            learner.Epsilon = map.Epsilon;
        }

        void Update()
        {
            HandleMouse();

            if (!running) return;

            stepBudget += stepsPerSecond * Time.deltaTime;
            int steps = Mathf.Min((int)stepBudget, MaxStepsPerFrame);
            stepBudget -= steps;
            if (stepBudget > MaxStepsPerFrame) stepBudget = 0f; // don't accumulate when the frame rate drops

            for (int i = 0; i < steps; i++)
                learner.Step();

            if (steps > 0)
                RefreshPolicy();
        }

        void TrainEpisodes(int count)
        {
            for (int i = 0; i < count; i++)
                learner.RunEpisode();
            RefreshPolicy();
        }

        void RefreshPolicy()
        {
            greedyPath = learner.GreedyPath();

            minValue = float.PositiveInfinity;
            maxValue = float.NegativeInfinity;
            for (int row = 0; row < world.Rows; row++)
            {
                for (int column = 0; column < world.Columns; column++)
                {
                    var p = new GridPos(row, column);
                    if (world[p] != Cell.Free || p == world.Goal) continue;
                    float v = learner.MaxQ(p);
                    minValue = Mathf.Min(minValue, v);
                    maxValue = Mathf.Max(maxValue, v);
                }
            }
            if (minValue > maxValue) minValue = maxValue = 0f;
        }

        // ---------------------------------------------------------------- editing the grid with the mouse

        void HandleMouse()
        {
#if ENABLE_LEGACY_INPUT_MANAGER
            if (cam == null) return;
            bool left = Input.GetMouseButtonDown(0), right = Input.GetMouseButtonDown(1);
            if (!left && !right) return;

            Vector2 guiMouse = new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);
            if (panelRect.Contains(guiMouse)) return;

            Vector3 worldPoint = cam.ScreenToWorldPoint(Input.mousePosition);
            var cell = new GridPos(Mathf.RoundToInt(-worldPoint.y), Mathf.RoundToInt(worldPoint.x));
            if (!world.InBounds(cell)) return;

            bool shift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            if (left && shift) MoveStart(cell);
            else if (left) ToggleWall(cell);
            else MoveGoal(cell);

            RefreshPolicy();
#endif
        }

        void ToggleWall(GridPos cell)
        {
            if (cell == world.Start || cell == world.Goal) return;
            world[cell] = world[cell] == Cell.Wall ? Cell.Free : Cell.Wall;
            learner.ClearCell(cell);
            if (learner.AgentPosition == cell) learner.BeginEpisode();
        }

        void MoveGoal(GridPos cell)
        {
            if (world[cell] == Cell.Wall || cell == world.Start) return;
            world[cell] = Cell.Free;
            world.Goal = cell;
            learner.BeginEpisode();
        }

        void MoveStart(GridPos cell)
        {
            if (world[cell] == Cell.Wall || cell == world.Goal) return;
            world[cell] = Cell.Free;
            world.Start = cell;
            learner.BeginEpisode();
        }

        // ---------------------------------------------------------------- camera

        void LateUpdate()
        {
            if (cam == null || world == null) return;

            float scale = OverlayScale.Current;
            float panelWidth = Mathf.Min(PanelWidth(scale), Screen.width * 0.5f);
            float available = Mathf.Max(1f, Screen.width - panelWidth);
            float aspect = (float)Screen.width / Screen.height;
            const float margin = 1.2f;

            // Half height that fits the grid vertically and in the free width left of the panel
            float byHeight = (world.Rows + margin) * 0.5f;
            float byWidth = (world.Columns + margin) * 0.5f / (aspect * available / Screen.width);
            cam.orthographicSize = Mathf.Max(byHeight, byWidth);

            // Centre the grid in the area left of the panel
            float visibleWidth = cam.orthographicSize * 2f * aspect;
            float areaCentre = available * 0.5f / Screen.width; // 0..1 of the screen
            float centreX = (world.Columns - 1) * 0.5f;
            float centreY = -(world.Rows - 1) * 0.5f;
            cam.transform.position = new Vector3(centreX - (areaCentre - 0.5f) * visibleWidth, centreY, -10f);
            cam.transform.rotation = Quaternion.identity;
        }

        static float PanelWidth(float scale) => 400f * scale;

        // ---------------------------------------------------------------- drawing the grid (GL)

        void OnRenderObject()
        {
            if (world == null || learner == null || Camera.current != cam || !EnsureMaterial()) return;

            lineMaterial.SetPass(0);
            GL.PushMatrix();

            GL.Begin(GL.QUADS);
            for (int row = 0; row < world.Rows; row++)
                for (int column = 0; column < world.Columns; column++)
                    DrawCell(new GridPos(row, column));
            GL.Color(Color.white);
            Quad(Center(learner.AgentPosition), 0.28f); // agent
            GL.End();

            GL.Begin(GL.LINES);
            for (int row = 0; row < world.Rows; row++)
                for (int column = 0; column < world.Columns; column++)
                    DrawPolicyArrow(new GridPos(row, column));
            DrawGreedyPath();
            Outline(world.Start, new Color(0.35f, 0.9f, 0.45f), 0.46f);
            Outline(world.Goal, new Color(1f, 0.85f, 0.25f), 0.46f);
            GL.End();

            GL.PopMatrix();
        }

        void DrawCell(GridPos p)
        {
            Color color;
            if (world[p] == Cell.Wall) color = new Color(0.11f, 0.12f, 0.16f);
            else if (world[p] == Cell.Cliff) color = new Color(0.55f, 0.12f, 0.12f);
            else if (p == world.Goal) color = new Color(0.85f, 0.68f, 0.15f);
            else color = HeatColor(learner.MaxQ(p));

            GL.Color(color);
            Quad(Center(p), 0.47f);
        }

        /// <summary> Dark blue for the lowest learned value, through teal, to yellow for the highest </summary>
        Color HeatColor(float value)
        {
            if (maxValue - minValue < 0.0001f) return new Color(0.16f, 0.18f, 0.25f);
            float t = Mathf.InverseLerp(minValue, maxValue, value);
            Color low = new Color(0.13f, 0.16f, 0.32f), mid = new Color(0.12f, 0.5f, 0.52f), high = new Color(0.95f, 0.85f, 0.35f);
            return t < 0.5f ? Color.Lerp(low, mid, t * 2f) : Color.Lerp(mid, high, (t - 0.5f) * 2f);
        }

        void DrawPolicyArrow(GridPos p)
        {
            if (world[p] == Cell.Wall || p == world.Goal) return;
            int action = learner.PolicyAction(p);
            if (action < 0 || learner.Q(p, action) == 0f) return; // nothing learned here yet

            Vector3 centre = Center(p);
            Vector3 dir = Direction(action);
            Vector3 tip = centre + dir * 0.32f;
            Vector3 side = new Vector3(-dir.y, dir.x, 0f);

            GL.Color(new Color(1f, 1f, 1f, 0.55f));
            Line(centre - dir * 0.2f, tip);
            Line(tip, tip - dir * 0.14f + side * 0.1f);
            Line(tip, tip - dir * 0.14f - side * 0.1f);
        }

        void DrawGreedyPath()
        {
            GL.Color(new Color(0.3f, 0.95f, 1f, 0.95f));
            for (int i = 1; i < greedyPath.Count; i++)
            {
                Vector3 a = Center(greedyPath[i - 1]), b = Center(greedyPath[i]);
                if ((a - b).sqrMagnitude > 1.5f) continue; // fell off the cliff and teleported to the start
                Line(a, b);
                Line(a + new Vector3(0.03f, 0.03f), b + new Vector3(0.03f, 0.03f)); // thicker
            }
        }

        void Outline(GridPos p, Color color, float half)
        {
            GL.Color(color);
            Vector3 c = Center(p);
            Vector3 a = c + new Vector3(-half, -half), b = c + new Vector3(half, -half), d = c + new Vector3(half, half), e = c + new Vector3(-half, half);
            Line(a, b); Line(b, d); Line(d, e); Line(e, a);
        }

        static Vector3 Center(GridPos p) => new Vector3(p.Column, -p.Row, 0f);
        static Vector3 Direction(int action) => action switch { 0 => Vector3.up, 1 => Vector3.right, 2 => Vector3.down, _ => Vector3.left };

        static void Quad(Vector3 c, float half)
        {
            GL.Vertex(c + new Vector3(-half, -half));
            GL.Vertex(c + new Vector3(-half, half));
            GL.Vertex(c + new Vector3(half, half));
            GL.Vertex(c + new Vector3(half, -half));
        }

        static void Line(Vector3 a, Vector3 b)
        {
            GL.Vertex(a);
            GL.Vertex(b);
        }

        bool EnsureMaterial()
        {
            if (lineMaterial != null) return true;
            var shader = Shader.Find("Hidden/Internal-Colored");
            if (shader == null) return false;
            lineMaterial = new Material(shader) { hideFlags = HideFlags.HideAndDontSave };
            lineMaterial.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
            lineMaterial.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
            lineMaterial.SetInt("_Cull", (int)CullMode.Off);
            lineMaterial.SetInt("_ZWrite", 0);
            lineMaterial.SetInt("_ZTest", (int)CompareFunction.Always);
            return true;
        }

        // ---------------------------------------------------------------- control panel (IMGUI)

        void OnGUI()
        {
            if (learner == null) return;
            float scale = OverlayScale.Current;
            BuildStyles(scale);

            float margin = 12f * scale;
            float width = Mathf.Min(PanelWidth(scale), Screen.width * 0.5f) - margin;
            panelRect = new Rect(Screen.width - width - margin, margin, width, Screen.height - margin * 2f);

            GUILayout.BeginArea(panelRect);
            panelScroll = GUILayout.BeginScrollView(panelScroll, false, false, GUIStyle.none, GUI.skin.verticalScrollbar, GUIStyle.none);
            GUILayout.BeginVertical(panelStyle);

            GUILayout.Label("Reinforcement Learning", titleStyle);
            GUILayout.Label(map.Description, smallStyle);
            GUILayout.Space(8 * scale);

            GUILayout.Label("MAP", headerStyle);
            var mapNames = new string[GridMaps.All.Length];
            for (int i = 0; i < mapNames.Length; i++) mapNames[i] = GridMaps.All[i].Name;
            int newMap = GUILayout.SelectionGrid(mapIndex, mapNames, mapNames.Length, buttonStyle);
            if (newMap != mapIndex) LoadMap(newMap);

            GUILayout.Label("ALGORITHM", headerStyle);
            int newAlgorithm = GUILayout.Toolbar((int)algorithm, AlgorithmNames, buttonStyle);
            if (newAlgorithm != (int)algorithm)
            {
                algorithm = (Algorithm)newAlgorithm;
                CreateLearner();
            }

            GUILayout.Space(6 * scale);
            GUILayout.Label("PARAMETERS", headerStyle);
            learner.Alpha = Slider($"Learning rate α  {learner.Alpha:F2}", learner.Alpha, 0.01f, 1f);
            learner.Gamma = Slider($"Discount γ  {learner.Gamma:F2}", learner.Gamma, 0f, 1f);
            learner.Epsilon = Slider($"Exploration ε  {learner.Epsilon:F2}", learner.Epsilon, 0f, 1f);
            if (GUILayout.Button("Default parameters for this map", buttonStyle)) ResetParameters();

            GUILayout.Space(6 * scale);
            GUILayout.Label("SPEED", headerStyle);
            float logSpeed = Slider($"{stepsPerSecond:N0} steps / second", Mathf.Log10(stepsPerSecond), 0f, 4.5f);
            stepsPerSecond = Mathf.Round(Mathf.Pow(10f, logSpeed));

            GUILayout.BeginHorizontal();
            if (GUILayout.Button(running ? "Pause" : "Play", buttonStyle)) running = !running;
            if (GUILayout.Button("Step", buttonStyle)) { running = false; learner.Step(); RefreshPolicy(); }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("+100 episodes", buttonStyle)) TrainEpisodes(100);
            if (GUILayout.Button("Reset learning", buttonStyle)) CreateLearner();
            GUILayout.EndHorizontal();

            GUILayout.Space(6 * scale);
            GUILayout.Label("RESULTS", headerStyle);
            DrawStats();
            DrawLearningCurve(scale);

            GUILayout.Space(6 * scale);
            GUILayout.Label("EDIT THE GRID", headerStyle);
            GUILayout.Label("Left click: wall on/off   ·   Right click: goal   ·   Shift + click: start", smallStyle);
            GUILayout.Label("Colour = learned value of the cell, arrows = best action, cyan = current best path.", smallStyle);

            GUILayout.EndVertical();
            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        void DrawStats()
        {
            int optimal = world.ShortestPathLength();
            bool reaches = learner.GreedyPathReachesGoal(out int pathLength);
            int lastSteps = learner.StepsPerEpisode.Count > 0 ? learner.StepsPerEpisode[learner.StepsPerEpisode.Count - 1] : 0;

            GUILayout.Label($"{learner.Name}  ·  episode {learner.Episodes + 1}  ·  {learner.TotalSteps:N0} steps in total", labelStyle);
            GUILayout.Label($"Steps in the last episode: {(learner.Episodes > 0 ? lastSteps.ToString() : "-")}", labelStyle);
            GUILayout.Label(reaches
                ? $"Best path now: {pathLength} steps  (shortest possible: {(optimal >= 0 ? optimal.ToString() : "none")})"
                : "Best path now: doesn't reach the goal yet", labelStyle);

            if (map.Layout == GridMaps.Cliff.Layout && learner.FallsPerEpisode.Count > 0)
            {
                int recent = Mathf.Min(50, learner.FallsPerEpisode.Count);
                int falls = 0;
                for (int i = learner.FallsPerEpisode.Count - recent; i < learner.FallsPerEpisode.Count; i++) falls += learner.FallsPerEpisode[i];
                GUILayout.Label($"Falls in the last {recent} episodes: {falls}", labelStyle);
            }
        }

        /// <summary> Steps per episode of the last episodes: it drops as the agent learns </summary>
        void DrawLearningCurve(float scale)
        {
            GUILayout.Label("Steps per episode (last 100)", smallStyle);
            Rect rect = GUILayoutUtility.GetRect(10f, 90f * scale, GUILayout.ExpandWidth(true));
            GUI.color = new Color(0f, 0f, 0f, 0.35f);
            GUI.DrawTexture(rect, pixel);

            var steps = learner.StepsPerEpisode;
            int count = Mathf.Min(100, steps.Count);
            if (count > 0)
            {
                int start = steps.Count - count;
                int max = 1;
                for (int i = start; i < steps.Count; i++) max = Mathf.Max(max, steps[i]);

                float barWidth = rect.width / 100f;
                GUI.color = Accent;
                for (int i = 0; i < count; i++)
                {
                    float h = rect.height * steps[start + i] / max;
                    GUI.DrawTexture(new Rect(rect.x + i * barWidth, rect.yMax - h, Mathf.Max(1f, barWidth - 1f), h), pixel);
                }
                GUI.color = Color.white;
                GUI.Label(new Rect(rect.x + 4, rect.y + 2, rect.width, 20 * scale), $"max {max}", smallStyle);
            }
            GUI.color = Color.white;
        }

        float Slider(string label, float value, float min, float max)
        {
            GUILayout.Label(label, labelStyle);
            return GUILayout.HorizontalSlider(value, min, max);
        }

        void BuildStyles(float scale)
        {
            if (panelStyle != null && Mathf.Approximately(scale, stylesScale)) return;
            stylesScale = scale;

            if (panelBackground == null)
            {
                panelBackground = new Texture2D(1, 1) { hideFlags = HideFlags.HideAndDontSave };
                panelBackground.SetPixel(0, 0, new Color(0.07f, 0.075f, 0.105f, 0.92f));
                panelBackground.Apply();
                pixel = Texture2D.whiteTexture;
            }

            int pad = Mathf.RoundToInt(16 * scale);
            panelStyle = new GUIStyle(GUI.skin.box) { padding = new RectOffset(pad, pad, pad, pad), normal = { background = panelBackground } };
            titleStyle = Label(24, scale, FontStyle.Bold, Color.white);
            headerStyle = Label(13, scale, FontStyle.Bold, Accent);
            labelStyle = Label(15, scale, FontStyle.Normal, new Color32(215, 220, 230, 255));
            smallStyle = Label(13, scale, FontStyle.Italic, new Color32(170, 178, 195, 255));
            buttonStyle = new GUIStyle(GUI.skin.button) { fontSize = OverlayScale.Font(14, scale), fontStyle = FontStyle.Bold, wordWrap = false };
        }

        static GUIStyle Label(float size, float scale, FontStyle style, Color color) => new GUIStyle(GUI.skin.label)
        {
            fontSize = OverlayScale.Font(size, scale),
            fontStyle = style,
            wordWrap = true,
            normal = { textColor = color }
        };
    }
}
