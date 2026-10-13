using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using NebulaRelay.Core;
using NebulaRelay.Player;
using NebulaRelay.Delivery;
using NebulaRelay.World;
using NebulaRelay.UI;
using NebulaRelay.Effects;

namespace NebulaRelay.Editor
{
    [InitializeOnLoad]
    public static class NebulaSceneBuilder
    {
        static NebulaSceneBuilder()
        {
            EditorApplication.delayCall += OnEditorReady;
        }

        private static void OnEditorReady()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                EditorApplication.delayCall += OnEditorReady;
                return;
            }

            BuildAllScenes();
        }

        [MenuItem("Nebula Relay/Build All 3 Levels & Main Menu", priority = 1)]
        public static void BuildAllScenes()
        {
            UpgradePrefabs();
            BuildMainMenuScene();
            BuildLevel01Scene();
            BuildLevel02Scene();
            BuildLevel03Scene();
            ConfigureEditorBuildSettings();

            // Re-open Level 1 as the active working scene
            EditorSceneManager.OpenScene("Assets/_NebulaRelay/Scenes/Level_01.unity");
            Debug.Log("<color=#00FF66><b>[Nebula Relay] Cosmic Visual Polish Applied! All 3 Levels and Main Menu constructed and registered in Build Settings.</b></color>");
        }

        [MenuItem("Nebula Relay/Build Windows Standalone (.exe)", priority = 2)]
        public static void BuildWindowsExecutable()
        {
            BuildAllScenes();

            string buildDir = "Builds/Windows";
            if (!System.IO.Directory.Exists(buildDir))
            {
                System.IO.Directory.CreateDirectory(buildDir);
            }

            string exePath = $"{buildDir}/NebulaRelay.exe";
            string[] scenePaths = new string[]
            {
                "Assets/_NebulaRelay/Scenes/MainMenu.unity",
                "Assets/_NebulaRelay/Scenes/Level_01.unity",
                "Assets/_NebulaRelay/Scenes/Level_02.unity",
                "Assets/_NebulaRelay/Scenes/Level_03.unity"
            };

            BuildPlayerOptions opt = new BuildPlayerOptions
            {
                scenes = scenePaths,
                locationPathName = exePath,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            Debug.Log("[Nebula Relay] Starting Standalone Windows x64 build with Cosmic Visuals...");
            var report = BuildPipeline.BuildPlayer(opt);
            if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
            {
                Debug.Log($"<color=#00FF66><b>[Nebula Relay] Standalone Windows Build Succeeded! Executable: {exePath} ({report.summary.totalSize / 1024 / 1024} MB)</b></color>");
            }
            else
            {
                Debug.LogError($"[Nebula Relay] Build failed: {report.summary.result}");
            }
        }

        // ==========================================
        // PREFAB UPGRADES WITH HIGH-RES ART
        // ==========================================
        public static void UpgradePrefabs()
        {
            // 1. Player Drone Prefab
            string dronePrefabPath = "Assets/_NebulaRelay/Prefabs/PlayerDrone.prefab";
            GameObject dronePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(dronePrefabPath);
            if (dronePrefab != null)
            {
                Sprite droneSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_NebulaRelay/Art/Drone_Courier.png");
                SpriteRenderer sr = dronePrefab.GetComponent<SpriteRenderer>();
                if (sr == null) sr = dronePrefab.AddComponent<SpriteRenderer>();
                if (droneSprite != null) sr.sprite = droneSprite;
                sr.sortingOrder = 5;

                if (dronePrefab.GetComponent<DroneVisuals>() == null)
                {
                    dronePrefab.AddComponent<DroneVisuals>();
                }
                PrefabUtility.SavePrefabAsset(dronePrefab);
            }

            // 2. Relay Station Prefab
            string relayPrefabPath = "Assets/_NebulaRelay/Prefabs/RelayStation.prefab";
            GameObject relayPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(relayPrefabPath);
            if (relayPrefab != null)
            {
                Sprite relaySprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_NebulaRelay/Art/RelayStation_Tower.png");
                SpriteRenderer sr = relayPrefab.GetComponent<SpriteRenderer>();
                if (sr == null) sr = relayPrefab.AddComponent<SpriteRenderer>();
                if (relaySprite != null) sr.sprite = relaySprite;
                sr.sortingOrder = 2;

                if (relayPrefab.GetComponent<RelayStationVisuals>() == null)
                {
                    relayPrefab.AddComponent<RelayStationVisuals>();
                }
                PrefabUtility.SavePrefabAsset(relayPrefab);
            }

            // 3. Ring Gate Prefab
            string ringPrefabPath = "Assets/_NebulaRelay/Prefabs/RingGate.prefab";
            GameObject ringPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(ringPrefabPath);
            if (ringPrefab != null)
            {
                Sprite ringSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_NebulaRelay/Art/RingGate_Neon.png");
                SpriteRenderer sr = ringPrefab.GetComponent<SpriteRenderer>();
                if (sr == null) sr = ringPrefab.AddComponent<SpriteRenderer>();
                if (ringSprite != null) sr.sprite = ringSprite;
                sr.sortingOrder = 3;
                PrefabUtility.SavePrefabAsset(ringPrefab);
            }

            // 4. Asteroid Hazard Prefab
            string astPrefabPath = "Assets/_NebulaRelay/Prefabs/AsteroidHazard.prefab";
            GameObject astPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(astPrefabPath);
            if (astPrefab != null)
            {
                Sprite astSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_NebulaRelay/Art/Asteroid_Detailed.png");
                SpriteRenderer sr = astPrefab.GetComponent<SpriteRenderer>();
                if (sr == null) sr = astPrefab.AddComponent<SpriteRenderer>();
                if (astSprite != null) sr.sprite = astSprite;
                sr.sortingOrder = 4;
                PrefabUtility.SavePrefabAsset(astPrefab);
            }

            // 5. Energy Capsule Prefab
            string capPrefabPath = "Assets/_NebulaRelay/Prefabs/EnergyCapsule.prefab";
            GameObject capPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(capPrefabPath);
            if (capPrefab != null)
            {
                Sprite capSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_NebulaRelay/Art/EnergyCapsule_Core.png");
                SpriteRenderer sr = capPrefab.GetComponent<SpriteRenderer>();
                if (sr == null) sr = capPrefab.AddComponent<SpriteRenderer>();
                if (capSprite != null) sr.sprite = capSprite;
                sr.sortingOrder = 6;
                PrefabUtility.SavePrefabAsset(capPrefab);
            }

            AssetDatabase.SaveAssets();
        }

        private static GameObject CreateSceneCamera(Color backgroundColor, float orthoSize = 8f)
        {
            GameObject camObj = new GameObject("Main Camera");
            Camera cam = camObj.AddComponent<Camera>();
            camObj.AddComponent<AudioListener>();
            camObj.tag = "MainCamera";

            cam.orthographic = true;
            cam.orthographicSize = orthoSize;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = backgroundColor;
            camObj.transform.position = new Vector3(0, 0, -10f);

            // URP camera settings
            UniversalAdditionalCameraData urpCam = camObj.AddComponent<UniversalAdditionalCameraData>();
            urpCam.renderPostProcessing = true; // Enable post-processing for Bloom

            return camObj;
        }

        private static void CreateGlobalLight()
        {
            GameObject lightObj = new GameObject("Global Light 2D");
            Light2D light = lightObj.AddComponent<Light2D>();
            light.lightType = Light2D.LightType.Global;
            light.color = new Color(0.9f, 0.95f, 1f, 1f); // Soft cosmic tint
            light.intensity = 1.1f;
        }

        private static GameObject CreatePlayerDrone(Vector3 position)
        {
            string prefabPath = "Assets/_NebulaRelay/Prefabs/PlayerDrone.prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            GameObject drone;
            if (prefab != null)
            {
                drone = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                drone.transform.position = position;
            }
            else
            {
                drone = new GameObject("PlayerDrone");
                drone.transform.position = position;
                drone.tag = "Player";
                drone.AddComponent<DroneController>();
                drone.AddComponent<CapsuleLauncher>();
                drone.AddComponent<TrajectoryPreview>();
                drone.AddComponent<BatteryManager>();
                drone.AddComponent<DroneVisuals>();
            }

            drone.transform.localScale = new Vector3(0.85f, 0.85f, 1f);
            return drone;
        }

        private static GameObject CreateRing(int order, Vector3 position)
        {
            string prefabPath = "Assets/_NebulaRelay/Prefabs/RingGate.prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            GameObject ring;
            if (prefab != null)
            {
                ring = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                ring.transform.position = position;
            }
            else
            {
                ring = new GameObject($"RingGate_{order}");
                ring.transform.position = position;
                ring.AddComponent<RingGate>();
            }

            ring.transform.localScale = new Vector3(0.75f, 0.75f, 1f);
            RingGate gate = ring.GetComponent<RingGate>();
            if (gate != null)
            {
                SerializedObject so = new SerializedObject(gate);
                so.FindProperty("ringOrder").intValue = order;
                so.ApplyModifiedProperties();
            }

            return ring;
        }

        private static GameObject CreateRelayStation(int index, Vector3 position)
        {
            string prefabPath = "Assets/_NebulaRelay/Prefabs/RelayStation.prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            GameObject relay;
            if (prefab != null)
            {
                relay = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                relay.transform.position = position;
            }
            else
            {
                relay = new GameObject($"RelayStation_{index}");
                relay.transform.position = position;
                relay.AddComponent<RelayStation>();
            }

            relay.transform.localScale = new Vector3(0.95f, 0.95f, 1f);
            RelayStation station = relay.GetComponent<RelayStation>();
            if (station != null)
            {
                SerializedObject so = new SerializedObject(station);
                so.FindProperty("relayIndex").intValue = index;
                so.ApplyModifiedProperties();
            }

            if (relay.GetComponent<RelayStationVisuals>() == null)
            {
                relay.AddComponent<RelayStationVisuals>();
            }

            return relay;
        }

        private static GameObject CreateAsteroid(Vector3 position, float rotSpeed = 15f, Vector2 patrolOffset = default, float patrolSpeed = 1f)
        {
            string prefabPath = "Assets/_NebulaRelay/Prefabs/AsteroidHazard.prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            GameObject asteroid;
            if (prefab != null)
            {
                asteroid = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                asteroid.transform.position = position;
            }
            else
            {
                asteroid = new GameObject("AsteroidHazard");
                asteroid.transform.position = position;
                asteroid.AddComponent<AsteroidHazard>();
            }

            asteroid.transform.localScale = new Vector3(0.6f, 0.6f, 1f);
            AsteroidHazard hazard = asteroid.GetComponent<AsteroidHazard>();
            if (hazard != null)
            {
                SerializedObject so = new SerializedObject(hazard);
                so.FindProperty("rotationSpeed").floatValue = rotSpeed;
                so.FindProperty("patrolOffset").vector2Value = patrolOffset;
                so.FindProperty("patrolSpeed").floatValue = patrolSpeed;
                so.ApplyModifiedProperties();
            }

            return asteroid;
        }

        private static void CreateManagers(int levelNumber, string levelTitle)
        {
            // Level Coordinator
            GameObject levelMgrObj = new GameObject("LevelManager");
            LevelManager lm = levelMgrObj.AddComponent<LevelManager>();
            SerializedObject so = new SerializedObject(lm);
            so.FindProperty("levelNumber").intValue = levelNumber;
            so.FindProperty("levelTitle").stringValue = levelTitle;
            so.ApplyModifiedProperties();

            // HUD Controller
            levelMgrObj.AddComponent<HUDController>();

            // Persistent Core Managers
            GameObject coreObj = new GameObject("CoreManagers");
            coreObj.AddComponent<GameManager>();
            coreObj.AddComponent<AudioManager>();
        }

        private static void CreateCosmicEnvironment(int sector, Sprite nebulaSprite, Sprite planetSprite, Sprite asteroidSprite)
        {
            GameObject envObj = new GameObject("CosmicEnvironment");
            CosmicEnvironment env = envObj.AddComponent<CosmicEnvironment>();
            SerializedObject so = new SerializedObject(env);
            so.FindProperty("sectorIndex").intValue = sector;
            so.FindProperty("nebulaSprite").objectReferenceValue = nebulaSprite;
            so.FindProperty("planetSprite").objectReferenceValue = planetSprite;
            so.FindProperty("asteroidSprite").objectReferenceValue = asteroidSprite;
            so.ApplyModifiedProperties();

            env.SetupEnvironment();
        }

        // ==========================================
        // 1. MAIN MENU SCENE
        // ==========================================
        public static void BuildMainMenuScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            CreateSceneCamera(new Color(0.02f, 0.031f, 0.086f, 1f), 8f); // #050816 deep space
            CreateGlobalLight();

            // Panoramic cosmic title background
            Sprite menuBg = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_NebulaRelay/Art/MainMenu_CosmicBG.png");
            if (menuBg != null)
            {
                GameObject bgObj = new GameObject("MainMenu_Background");
                SpriteRenderer sr = bgObj.AddComponent<SpriteRenderer>();
                sr.sprite = menuBg;
                sr.sortingOrder = -100;
                bgObj.transform.position = new Vector3(0f, 0f, 0f);
                bgObj.transform.localScale = new Vector3(2.0f, 2.0f, 1f);
            }

            GameObject menuObj = new GameObject("MainMenuController");
            menuObj.AddComponent<MainMenuController>();
            menuObj.AddComponent<GameManager>();
            menuObj.AddComponent<AudioManager>();

            EditorSceneManager.SaveScene(scene, "Assets/_NebulaRelay/Scenes/MainMenu.unity");
        }

        // ==========================================
        // 2. LEVEL 1: FIRST SIGNAL
        // ==========================================
        public static void BuildLevel01Scene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            CreateSceneCamera(new Color(0.02f, 0.031f, 0.086f, 1f), 7f);
            CreateGlobalLight();

            Sprite nebula = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_NebulaRelay/Art/Nebula_FirstSignal.png");
            Sprite planet = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_NebulaRelay/Art/Planet_BlueOcean.png");
            Sprite asteroid = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_NebulaRelay/Art/Asteroid_Detailed.png");
            CreateCosmicEnvironment(1, nebula, planet, asteroid);

            CreatePlayerDrone(new Vector3(-6f, 0f, 0f));
            CreateRing(1, new Vector3(-2f, 1f, 0f));
            CreateRing(2, new Vector3(2.5f, 2f, 0f));
            CreateRelayStation(1, new Vector3(8f, 1.5f, 0f));

            GameObject ringMgrObj = new GameObject("RingManager");
            ringMgrObj.AddComponent<RingManager>();

            CreateManagers(1, "First Signal");

            EditorSceneManager.SaveScene(scene, "Assets/_NebulaRelay/Scenes/Level_01.unity");
        }

        // ==========================================
        // 3. LEVEL 2: THE DEBRIS FIELD
        // ==========================================
        public static void BuildLevel02Scene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            CreateSceneCamera(new Color(0.02f, 0.031f, 0.086f, 1f), 8.5f);
            CreateGlobalLight();

            Sprite nebula = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_NebulaRelay/Art/Nebula_DebrisField.png");
            Sprite asteroid = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_NebulaRelay/Art/Asteroid_Detailed.png");
            CreateCosmicEnvironment(2, nebula, null, asteroid);

            CreatePlayerDrone(new Vector3(-8f, 0f, 0f));

            CreateRing(1, new Vector3(-4f, -1.5f, 0f));
            CreateAsteroid(new Vector3(-1f, 1.5f, 0f), 20f, new Vector2(0f, 1f), 1.2f);

            CreateRing(2, new Vector3(1f, 2f, 0f));
            CreateAsteroid(new Vector3(4.5f, 0f, 0f), -15f, new Vector2(0f, 1.5f), 0.8f);

            CreateRing(3, new Vector3(6.5f, -2f, 0f));
            CreateRelayStation(2, new Vector3(11f, 0f, 0f));

            GameObject ringMgrObj = new GameObject("RingManager");
            ringMgrObj.AddComponent<RingManager>();

            CreateManagers(2, "The Debris Field");

            EditorSceneManager.SaveScene(scene, "Assets/_NebulaRelay/Scenes/Level_02.unity");
        }

        // ==========================================
        // 4. LEVEL 3: THE LAST TRANSMISSION
        // ==========================================
        public static void BuildLevel03Scene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            CreateSceneCamera(new Color(0.02f, 0.031f, 0.086f, 1f), 10f);
            CreateGlobalLight();

            Sprite nebula = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_NebulaRelay/Art/Nebula_LastTransmission.png");
            Sprite asteroid = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_NebulaRelay/Art/Asteroid_Detailed.png");
            CreateCosmicEnvironment(3, nebula, null, asteroid);

            CreatePlayerDrone(new Vector3(-10f, 0f, 0f));

            CreateRing(1, new Vector3(-6f, 2.5f, 0f));
            CreateAsteroid(new Vector3(-3f, 4f, 0f), 25f);
            CreateAsteroid(new Vector3(-3f, 0.5f, 0f), -20f);

            CreateRing(2, new Vector3(0f, 1f, 0f));
            CreateAsteroid(new Vector3(3.5f, 2.5f, 0f), 18f, new Vector2(0f, 2f), 1.5f);
            CreateAsteroid(new Vector3(3.5f, -2.5f, 0f), -18f, new Vector2(0f, -1.5f), 1.2f);

            CreateRing(3, new Vector3(7f, -1.5f, 0f));
            CreateRelayStation(3, new Vector3(13f, 1f, 0f));

            GameObject ringMgrObj = new GameObject("RingManager");
            ringMgrObj.AddComponent<RingManager>();

            CreateManagers(3, "The Last Transmission");

            EditorSceneManager.SaveScene(scene, "Assets/_NebulaRelay/Scenes/Level_03.unity");
        }

        private static void ConfigureEditorBuildSettings()
        {
            string[] scenePaths = new string[]
            {
                "Assets/_NebulaRelay/Scenes/MainMenu.unity",
                "Assets/_NebulaRelay/Scenes/Level_01.unity",
                "Assets/_NebulaRelay/Scenes/Level_02.unity",
                "Assets/_NebulaRelay/Scenes/Level_03.unity"
            };

            EditorBuildSettingsScene[] buildScenes = new EditorBuildSettingsScene[scenePaths.Length];
            for (int i = 0; i < scenePaths.Length; i++)
            {
                buildScenes[i] = new EditorBuildSettingsScene(scenePaths[i], true);
            }

            EditorBuildSettings.scenes = buildScenes;
        }
    }
}
