using UnityEngine;

namespace SerwusStudio
{
    /// <summary>
    /// On-screen panel for the interactive demo: the prefab browser on top, the live
    /// shader sliders for whatever is currently on the turntable in the middle, and the
    /// rate / Discord links pinned to the bottom.
    ///
    /// The slider rows are whatever the director found on the current model's shader,
    /// so this file knows nothing about brick, plaster or cel shading.
    ///
    /// Plain IMGUI on purpose, same as the other Cursed Cozy demos: no canvas, no
    /// EventSystem, no TextMeshPro import step on first open, and nothing left to
    /// untangle when the buyer deletes the Demo folder.
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Serwus Studio/Cursed Cozy Village Free/Demo/CCVFree Demo HUD")]
    public class CCVFreeDemoHud : MonoBehaviour
    {
        [Tooltip("Director to drive. Empty = the one on this GameObject, then the one in the scene.")]
        public CCVFreeDemoDirector director;

        [Tooltip("Extra scaling on top of the automatic one. Raise it for 4K capture.")]
        [Range(0.5f, 3f)] public float uiScale = 1f;

        [Tooltip("Colour choices offered for every Color property the current shader has "
            + "(brick tint, plaster tint, prop tint). If they render brighter than they look "
            + "here, the project is in Linear colour space - retune these values until they match.")]
        public Color[] swatches =
        {
            new Color(1.00f, 1.00f, 1.00f),   // untinted
            new Color(0.78f, 0.43f, 0.30f),   // terracotta
            new Color(0.60f, 0.29f, 0.23f),   // dark brick
            new Color(0.85f, 0.79f, 0.66f),   // sand plaster
            new Color(0.91f, 0.90f, 0.86f),   // off white
            new Color(0.55f, 0.60f, 0.63f),   // cold grey
            new Color(0.43f, 0.50f, 0.37f),   // moss
            new Color(0.29f, 0.25f, 0.22f),   // dark brown
        };

        [Header("Call to action")]
        public string reviewUrl =
            "https://assetstore.unity.com/packages/3d/environments/cursed-cozy-village-free-pack-382598#reviews";
        public string discordUrl = "https://discord.gg/Cu2HF9xyVm";

        const float PanelWidth = 350f;
        const float Pad = 14f;

        static readonly Color PanelColor = new Color(0.04f, 0.05f, 0.05f, 0.86f);
        static readonly Color LeafColor = new Color(0.75f, 0.93f, 0.64f);   // wordmark green
        static readonly Color GreenColor = new Color(0f, 0.54f, 0.35f);     // logo green
        static readonly Color DimColor = new Color(0.82f, 0.82f, 0.82f);

        GUIStyle panel, eyebrow, title, body, leadLabel, hint, valueRight, swatch;
        Texture2D panelTexture;
        Vector2 scroll;

        void OnEnable()
        {
            if (director == null)
                director = GetComponent<CCVFreeDemoDirector>();
            if (director == null)
                director = FindFirstObjectByType<CCVFreeDemoDirector>();
        }

        void OnGUI()
        {
            if (director == null)
                return;

            BuildStyles();

            var previousMatrix = GUI.matrix;
            float scale = Mathf.Max(1f, Screen.height / 900f) * uiScale;
            GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1f));

            float height = Screen.height / scale - Pad * 2f;
            GUILayout.BeginArea(new Rect(Pad, Pad, PanelWidth, height), panel);

            DrawBrowser();
            DrawShaderPanel();
            GUILayout.FlexibleSpace();
            DrawCallToAction();

            GUILayout.EndArea();
            GUI.matrix = previousMatrix;
        }

        void DrawBrowser()
        {
            GUILayout.Label("/// CURSED COZY VILLAGE — FREE", eyebrow);
            GUILayout.Space(4f);
            GUILayout.Label(string.IsNullOrEmpty(director.CurrentName) ? "—" : director.CurrentName, title);
            GUILayout.Label(string.Format("{0:00} / {1:00}   ·   {2:n0} tris",
                director.Index + 1, director.Count, director.CurrentTriangles), hint);
            GUILayout.Space(8f);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("‹  Prev", GUILayout.Height(30f)))
                director.Prev();
            if (GUILayout.Button("Next  ›", GUILayout.Height(30f)))
                director.Next();
            GUILayout.EndHorizontal();
        }

        void DrawShaderPanel()
        {
            var controls = director.Controls;
            if (controls.Count == 0)
                return;

            GUILayout.Space(14f);
            GUILayout.Label("/// SHADER — " + director.ShaderName, eyebrow);
            GUILayout.Label("Everything here drives the model on the turntable live. "
                + "The material asset on disk is never touched.", hint);
            GUILayout.Space(6f);

            scroll = GUILayout.BeginScrollView(scroll, GUILayout.ExpandHeight(true));
            for (int i = 0; i < controls.Count; i++)
            {
                if (controls[i].isColor)
                    DrawColorRow(controls[i]);
                else
                    DrawSliderRow(controls[i], i == 0);   // first one is the headline knob
                GUILayout.Space(8f);
            }
            GUILayout.EndScrollView();

            if (GUILayout.Button("Reset shader", GUILayout.Height(24f)))
                director.ResetShader();
        }

        void DrawSliderRow(CCVFreeDemoDirector.ShaderControl control, bool lead)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(control.label, lead ? leadLabel : body);
            GUILayout.Label(control.value.ToString(control.max > 10f ? "n0" : "0.00"),
                valueRight, GUILayout.Width(48f));
            GUILayout.EndHorizontal();
            control.value = GUILayout.HorizontalSlider(control.value, control.min, control.max);
        }

        /// <summary>Swatches rather than an RGB picker: three sliders per colour would
        /// bury the damage slider, and a fixed palette keeps the demo looking like the
        /// pack instead of like whatever the visitor mixed.</summary>
        void DrawColorRow(CCVFreeDemoDirector.ShaderControl control)
        {
            GUILayout.Label(control.label, body);
            GUILayout.BeginHorizontal();
            var previousBackground = GUI.backgroundColor;
            foreach (var color in swatches)
            {
                GUI.backgroundColor = color;
                if (GUILayout.Button(GUIContent.none, swatch, GUILayout.Width(28f), GUILayout.Height(20f)))
                    control.color = color;
            }
            GUI.backgroundColor = previousBackground;
            GUILayout.EndHorizontal();
        }

        void DrawCallToAction()
        {
            GUILayout.Space(14f);
            GUILayout.Label("/// LIKE IT?", eyebrow);
            GUILayout.Label("This pack is free and still getting updates. A rating is the main "
                + "thing that puts it in front of other developers.", hint);
            GUILayout.Space(6f);

            var previousBackground = GUI.backgroundColor;
            GUI.backgroundColor = GreenColor;
            if (GUILayout.Button("★  Rate this pack", GUILayout.Height(30f)))
                Application.OpenURL(reviewUrl);
            GUI.backgroundColor = previousBackground;

            if (GUILayout.Button("→  Join the Discord", GUILayout.Height(26f)))
                Application.OpenURL(discordUrl);
        }

        void BuildStyles()
        {
            if (panel != null)
                return;

            panelTexture = new Texture2D(1, 1);
            panelTexture.SetPixel(0, 0, PanelColor);
            panelTexture.Apply();
            panelTexture.hideFlags = HideFlags.HideAndDontSave;

            panel = new GUIStyle(GUI.skin.box);
            panel.normal.background = panelTexture;
            panel.padding = new RectOffset(14, 14, 12, 12);

            eyebrow = new GUIStyle(GUI.skin.label) { fontSize = 10, fontStyle = FontStyle.Bold };
            eyebrow.normal.textColor = LeafColor;

            title = new GUIStyle(GUI.skin.label) { fontSize = 17, fontStyle = FontStyle.Bold };
            title.normal.textColor = Color.white;

            body = new GUIStyle(GUI.skin.label) { fontSize = 12 };
            body.normal.textColor = DimColor;

            leadLabel = new GUIStyle(body) { fontSize = 14, fontStyle = FontStyle.Bold };
            leadLabel.normal.textColor = Color.white;

            // Texture2D.whiteTexture so GUI.backgroundColor shows the swatch colour flat,
            // instead of tinting Unity's grey button gradient.
            swatch = new GUIStyle(GUI.skin.button) { margin = new RectOffset(0, 4, 0, 0) };
            swatch.normal.background = Texture2D.whiteTexture;
            swatch.hover.background = Texture2D.whiteTexture;
            swatch.active.background = Texture2D.whiteTexture;

            hint = new GUIStyle(body) { wordWrap = true, fontSize = 11 };
            hint.normal.textColor = new Color(DimColor.r, DimColor.g, DimColor.b, 0.55f);

            valueRight = new GUIStyle(body) { alignment = TextAnchor.MiddleRight };
            valueRight.normal.textColor = LeafColor;
        }

        void OnDisable()
        {
            if (panelTexture != null)
                DestroyImmediate(panelTexture);
            panel = null;
        }
    }
}
