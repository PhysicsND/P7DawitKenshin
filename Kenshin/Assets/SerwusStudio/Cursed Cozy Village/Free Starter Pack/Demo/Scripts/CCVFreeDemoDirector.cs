using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace SerwusStudio
{
    /// <summary>
    /// Drives the interactive demo scene: one prefab at a time on a turntable, with
    /// live sliders for whatever shader that prefab happens to use.
    ///
    /// The sliders are not configured anywhere. Every time a model spawns, its shader
    /// is asked for its own Range properties (Shader.GetPropertyCount and friends,
    /// runtime API since 2021.1) and the panel is rebuilt from them, labels included.
    /// So a wall shows the plaster damage knobs, a barrel shows the prop knobs, and a
    /// shader added to the pack next month shows up with no code change.
    ///
    /// Values are written to Renderer.materials - per-instance copies that die with the
    /// spawned model. The .mat assets on disk are never touched, so playing with the
    /// sliders cannot leave modified files in the buyer's project.
    ///
    /// Demo-only. Deleting the Demo folder removes this and breaks nothing.
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Serwus Studio/Cursed Cozy Village Free/Demo/CCVFree Demo Director")]
    public class CCVFreeDemoDirector : MonoBehaviour
    {
        /// <summary>One row in the shader panel, built from the live shader. A class,
        /// not a struct, so the HUD can write <c>value</c> straight back.</summary>
        public class ShaderControl
        {
            public string label;
            public int id;
            public bool isColor;

            public float min;
            public float max;
            public float value;
            public float startValue;

            public Color color;
            public Color startColor;
        }

        [Tooltip("Everything the browser cycles through. Reorder here to change the order in the demo.")]
        public GameObject[] prefabs;

        [Tooltip("Empty transform the model is parented to and spun around. Leave empty to use this object.")]
        public Transform pivot;

        [Tooltip("Longest side of the spawned model in metres. Everything is scaled to this, so a mailbox and a wall both fill the frame.")]
        public float frameSize = 1.6f;

        [Tooltip("Turntable speed. 30 = a full turn every 12 seconds.")]
        public float degreesPerSecond = 30f;

        [Tooltip("Only shaders whose name starts with this get a slider panel. Keeps Unity's own materials out of it.")]
        public string shaderPrefix = "Serwus Studio/";

        [Tooltip("Shader properties to expose, in this order. Names the current shader does not have are skipped, "
            + "so one list covers every shader in the pack. Clear the array to show every Range and Color property instead.")]
        public string[] featured =
        {
            "_DamageAmount",      // the one people should touch first
            "_BrickColor",
            "_PlasterColor",
            "_DamageEdge",
            "_DamageNoiseScale",
            "_BrickRecess",
            "_CelStrength",
            "_BaseColor",         // props from here down
            "_BumpScale",
            "_AOStrength",
            "_EmissionStrength",
        };

        public int Index { get; private set; }
        public int Count { get { return prefabs == null ? 0 : prefabs.Length; } }
        public string CurrentName { get; private set; }
        public int CurrentTriangles { get; private set; }
        public string ShaderName { get; private set; }

        public List<ShaderControl> Controls { get { return sliders; } }

        readonly List<ShaderControl> sliders = new List<ShaderControl>();
        readonly List<Material> materials = new List<Material>();
        GameObject spawned;

        void Start()
        {
            if (pivot == null)
                pivot = transform;
            Show(0);
        }

        void Update()
        {
            pivot.Rotate(0f, degreesPerSecond * Time.deltaTime, 0f, Space.World);

            // Pushed to every material that has the property, not just the one the panel
            // was built from: a prefab with a wall and a wooden frame should shade as one
            // object when Cel Strength moves.
            for (int s = 0; s < sliders.Count; s++)
            {
                var control = sliders[s];
                for (int m = 0; m < materials.Count; m++)
                {
                    if (materials[m] == null || !materials[m].HasProperty(control.id))
                        continue;
                    if (control.isColor)
                        materials[m].SetColor(control.id, control.color);
                    else
                        materials[m].SetFloat(control.id, control.value);
                }
            }
        }

        public void Next() { Show(Index + 1); }
        public void Prev() { Show(Index - 1); }

        public void ResetShader()
        {
            foreach (var control in sliders)
            {
                control.value = control.startValue;
                control.color = control.startColor;
            }
        }

        public void Show(int index)
        {
            if (Count == 0)
            {
                CurrentName = "(no prefabs assigned)";
                CurrentTriangles = 0;
                sliders.Clear();
                materials.Clear();
                return;
            }

            Index = ((index % Count) + Count) % Count;

            if (spawned != null)
                Destroy(spawned);
            sliders.Clear();
            materials.Clear();
            ShaderName = null;

            var prefab = prefabs[Index];
            if (prefab == null)
            {
                CurrentName = "(missing prefab)";
                CurrentTriangles = 0;
                return;
            }

            pivot.localRotation = Quaternion.identity;   // every model starts facing the camera

            spawned = Instantiate(prefab, pivot);
            spawned.transform.localPosition = Vector3.zero;
            spawned.transform.localRotation = Quaternion.identity;
            spawned.transform.localScale = Vector3.one;
            Frame(spawned);

            CurrentName = prefab.name;
            CurrentTriangles = CountTriangles(spawned);
            CollectShaderControls(spawned);
        }

        /// <summary>Ask the spawned model's own shader what it can be tweaked with.</summary>
        void CollectShaderControls(GameObject go)
        {
            foreach (var renderer in go.GetComponentsInChildren<Renderer>())
                materials.AddRange(renderer.materials);

            // Not "the first pack material on the model" - wall.fbx lists its plank
            // material in slot 0 and the wall itself in slot 1, so first-wins showed the
            // plank knobs and no damage slider at all. Pick the material that can fill
            // the most of the featured list instead: the wall beats the trim, the
            // subject of the prefab wins over its frame.
            Material source = null;
            int best = -1;
            foreach (var material in materials)
            {
                if (material == null || material.shader == null)
                    continue;
                if (!material.shader.name.StartsWith(shaderPrefix))
                    continue;

                int score = FeaturedScore(material);
                if (score > best)
                {
                    best = score;
                    source = material;
                }
            }
            if (source == null)
                return;

            var shader = source.shader;
            ShaderName = shader.name.Substring(shader.name.LastIndexOf('/') + 1);

            int count = shader.GetPropertyCount();
            if (featured == null || featured.Length == 0)
            {
                for (int i = 0; i < count; i++)
                    AddControl(shader, source, i);
                return;
            }

            var byName = new Dictionary<string, int>(count);
            for (int i = 0; i < count; i++)
                byName[shader.GetPropertyName(i)] = i;

            foreach (var name in featured)
            {
                int i;
                if (byName.TryGetValue(name, out i))
                    AddControl(shader, source, i);
            }
        }

        /// <summary>How many of the featured properties this material can actually offer.
        /// With no featured list, fall back to raw property count so the richest shader
        /// still wins.</summary>
        int FeaturedScore(Material material)
        {
            if (featured == null || featured.Length == 0)
                return material.shader.GetPropertyCount();

            int score = 0;
            foreach (var name in featured)
                if (material.HasProperty(name))
                    score++;
            return score;
        }

        /// <summary>Range and Color only. A shader author writing Range(a, b) has already
        /// said "this one is a slider"; Float properties are toggles and enums as often as
        /// they are knobs, and guessing their limits gets it wrong.</summary>
        void AddControl(Shader shader, Material source, int i)
        {
            var type = shader.GetPropertyType(i);
            if (type != ShaderPropertyType.Range && type != ShaderPropertyType.Color)
                return;

            // "Cel Strength (0 soft - 1 hard)" is a good inspector label and a bad
            // panel label. The parenthetical is the shader author talking to himself.
            string label = shader.GetPropertyDescription(i);
            int paren = label.IndexOf(" (");
            if (paren > 0)
                label = label.Substring(0, paren);

            int id = shader.GetPropertyNameId(i);
            if (type == ShaderPropertyType.Color)
            {
                var current = source.GetColor(id);
                sliders.Add(new ShaderControl
                {
                    label = label, id = id, isColor = true,
                    color = current, startColor = current,
                });
                return;
            }

            var limits = shader.GetPropertyRangeLimits(i);
            float value = source.GetFloat(id);
            sliders.Add(new ShaderControl
            {
                label = label, id = id,
                min = limits.x, max = limits.y,
                value = value, startValue = value,
            });
        }

        /// <summary>Scale the model so its longest side is frameSize, then sit its
        /// centre on the pivot. Without this a wall dwarfs a mailbox and the browser
        /// looks broken.</summary>
        void Frame(GameObject go)
        {
            Bounds bounds;
            if (!TryGetBounds(go, out bounds))
                return;

            float longest = Mathf.Max(bounds.size.x, Mathf.Max(bounds.size.y, bounds.size.z));
            if (longest > 0.0001f)
                go.transform.localScale = Vector3.one * (frameSize / longest);

            if (TryGetBounds(go, out bounds))
                go.transform.position -= bounds.center - pivot.position;
        }

        static bool TryGetBounds(GameObject go, out Bounds bounds)
        {
            bounds = new Bounds();
            var renderers = go.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0)
                return false;

            bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
                bounds.Encapsulate(renderers[i].bounds);
            return true;
        }

        /// <summary>Index counts, not Mesh.triangles: index counts are metadata and work
        /// on meshes imported with Read/Write disabled, which is every FBX in this pack.</summary>
        static int CountTriangles(GameObject go)
        {
            int total = 0;
            foreach (var filter in go.GetComponentsInChildren<MeshFilter>())
            {
                var mesh = filter.sharedMesh;
                if (mesh == null)
                    continue;
                for (int sub = 0; sub < mesh.subMeshCount; sub++)
                    total += (int)(mesh.GetIndexCount(sub) / 3);
            }
            return total;
        }
    }
}
