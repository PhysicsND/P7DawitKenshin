using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace SerwusStudio
{
    /// <summary>
    /// Builds (and rebuilds) Demo_Inter.unity: floor, camera, light, turntable, and the
    /// Director + HUD wired to every prefab in the pack.
    ///
    /// There is deliberately no wall in the scene. The shader sliders read the shader
    /// off whatever model is on the turntable, so the wall prefabs are just three more
    /// entries in the browser - a static backdrop would only be a second thing to keep
    /// in sync.
    ///
    /// Re-runnable: objects are looked up by name and only the missing ones are created,
    /// so anything you add by hand survives a rebuild.
    ///
    /// Editor-only, and authoring tooling rather than content: delete this Editor folder
    /// before exporting the .unitypackage if you would rather not ship it.
    /// Menu: Tools > Serwus Studio > Cursed Cozy Village Free > Build Demo_Inter Scene
    /// </summary>
    public static class CCVFreeDemoBuilder
    {
        const string Root = "Assets/SerwusStudio/Cursed Cozy Village/Free Starter Pack";
        const string ScenePath = Root + "/Demo/Demo_Inter.unity";
        const string FloorMaterial = Root + "/Materials/MI_ground.mat";
        const string FloorMaterialFallback = Root + "/Demo/floor_demo.mat";

        const float PivotHeight = 1.0f;

        [MenuItem("Tools/Serwus Studio/Cursed Cozy Village Free/Build Demo_Inter Scene")]
        public static void Build()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            var scene = EditorSceneManager.OpenScene(ScenePath);

            // Left over from the first pass, when the walls were a fixed backdrop.
            var backdrop = Find("Backdrop");
            if (backdrop != null)
                Object.DestroyImmediate(backdrop);

            var demoRoot = Find("CCVFree Demo") ?? new GameObject("CCVFree Demo");
            var pivot = Child(demoRoot, "Turntable", new Vector3(0f, PivotHeight, 0f));

            BuildFloor();
            SetUpCamera();
            SetUpLighting();

            var director = Get<CCVFreeDemoDirector>(demoRoot);
            director.pivot = pivot.transform;
            director.prefabs = LoadPrefabs();

            var hud = Get<CCVFreeDemoHud>(demoRoot);
            hud.director = director;

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            Debug.Log("Demo_Inter built: " + director.prefabs.Length + " prefabs in the browser.");
        }

        // ---- pieces ---------------------------------------------------------

        static void BuildFloor()
        {
            var floor = Find("Floor");
            if (floor == null)
            {
                floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
                floor.name = "Floor";
                Object.DestroyImmediate(floor.GetComponent<Collider>());
            }
            floor.transform.position = Vector3.zero;
            floor.transform.localScale = new Vector3(2f, 1f, 2f);

            var material = AssetDatabase.LoadAssetAtPath<Material>(FloorMaterial)
                ?? AssetDatabase.LoadAssetAtPath<Material>(FloorMaterialFallback);
            if (material != null)
                floor.GetComponent<Renderer>().sharedMaterial = material;
        }

        static void SetUpCamera()
        {
            var camera = Object.FindFirstObjectByType<Camera>();
            if (camera == null)
                camera = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener)).GetComponent<Camera>();
            camera.tag = "MainCamera";
            camera.transform.position = new Vector3(0f, PivotHeight + 0.25f, -3.2f);
            camera.transform.LookAt(new Vector3(0f, PivotHeight, 0f));
            camera.fieldOfView = 50f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.09f, 0.10f, 0.11f);
        }

        static void SetUpLighting()
        {
            var light = Object.FindObjectsByType<Light>(FindObjectsSortMode.None)
                .FirstOrDefault(l => l.type == LightType.Directional);
            if (light == null)
                light = new GameObject("Directional Light", typeof(Light)).GetComponent<Light>();
            light.type = LightType.Directional;
            light.transform.rotation = Quaternion.Euler(38f, -150f, 0f);
            light.color = new Color(1f, 0.96f, 0.90f);
            light.intensity = 1.6f;
            light.shadows = LightShadows.Soft;

            // Flat ambient instead of the skybox one: the scene has to look the same on
            // a fresh import, with no lightmap bake and no reflection probe step.
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.22f, 0.23f, 0.27f);
        }

        static GameObject[] LoadPrefabs()
        {
            return AssetDatabase.FindAssets("t:Prefab", new[] { Root + "/Prefabs" })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<GameObject>)
                .Where(p => p != null)
                .OrderBy(p => p.name, System.StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }

        // ---- helpers --------------------------------------------------------

        static GameObject Find(string name)
        {
            return Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .FirstOrDefault(g => g.name == name);
        }

        static GameObject Child(GameObject parent, string name, Vector3 localPosition)
        {
            var existing = parent.transform.Find(name);
            var go = existing != null ? existing.gameObject : new GameObject(name);
            go.transform.SetParent(parent.transform, false);
            go.transform.localPosition = localPosition;
            return go;
        }

        static T Get<T>(GameObject go) where T : Component
        {
            var component = go.GetComponent<T>();
            return component != null ? component : go.AddComponent<T>();
        }
    }
}
