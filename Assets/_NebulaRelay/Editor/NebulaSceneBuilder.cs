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
            if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlaying)
            {
                return;
            }

            if (!System.IO.File.Exists("Assets/_NebulaRelay/Scenes/Level_04.unity") ||
                !System.IO.File.Exists("Assets/_NebulaRelay/Scenes/Level_05.unity"))
            {
                BuildAllScenes();
            }
        }

        [MenuItem("Nebula Relay/Build All 5 Levels & Main Menu", priority = 1)]
        public static void BuildAllScenes()
        {
            UpgradePrefabs();
            BuildMainMenuScene();
            BuildLevel01Scene();
            BuildLevel02Scene();
            BuildLevel03Scene();
            BuildLevel04Scene();
            BuildLevel05Scene();
            ConfigureEditorBuildSettings();

            // Re-open Level 1 as the active working scene
            EditorSceneManager.OpenScene("Assets/_NebulaRelay/Scenes/Level_01.unity");
            Debug.Log("<color=#00FF66><b>[Nebula Relay] Cosmic Visual Polish & Calibrated Scales Applied! All 5 Levels and Main Menu constructed and registered in Build Settings.</b></color>");
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
                "Assets/_NebulaRelay/Scenes/Level_03.unity",
                "Assets/_NebulaRelay/Scenes/Level_04.unity",
                "Assets/_NebulaRelay/Scenes/Level_05.unity"
            };

            BuildPlayerOptions opt = new BuildPlayerOptions
            {
                scenes = scenePaths,
                locationPathName = exePath,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            Debug.Log("[Nebula Relay] Starting Standalone Windows x64 build with 5 Sectors and Live Cosmic Motion...");
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
        // PREFAB UPGRADES WITH HIGH-RES ART & CALIBRATED COLLIDERS
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

                CircleCollider2D col = dronePrefab.GetComponent<CircleCollider2D>();
                if (col != null)
                {
                    col.radius = 4.2f; // At 0.16 scale -> 0.67 world radius, tight delta hull
                }

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

                RelayStation station = relayPrefab.GetComponent<RelayStation>();
                if (station != null)
                {
                    SerializedObject so = new SerializedObject(station);
                    so.FindProperty("activationZoneRadius").floatValue = 2.5f;
                    so.ApplyModifiedProperties();
                }

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

                CircleCollider2D col = ringPrefab.GetComponent<CircleCollider2D>();
                if (col != null)
                {
                    col.radius = 4.5f; // At 0.28 scale -> 1.26 world radius, comfortable gate
                }
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

                CircleCollider2D col = astPrefab.GetComponent<CircleCollider2D>();
                if (col != null)
                {
                    col.radius = 4.2f; // At 0.25 scale -> 1.05 world radius
                }
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

                capPrefab.transform.localScale = new Vector3(0.10f, 0.10f, 1f);

                CircleCollider2D col = capPrefab.GetComponent<CircleCollider2D>();
                if (col != null)
                {
                    col.radius = 4.5f; // At 0.10 scale -> 0.45 world radius
                }
                PrefabUtility.SavePrefabAsset(capPrefab);
            }

            AssetDatabase.SaveAssets();
        }

        private static GameObject CreateSceneCamera(Color backgroundColor, float minX, float maxX, float orthoSize = 7.5f)
        {
            GameObject camObj = new GameObject("Main Camera");
            Camera cam = camObj.AddComponent<Camera>();
            camObj.AddComponent<AudioListener>();
            camObj.tag = "MainCamera";

            cam.orthographic = true;
            cam.orthographicSize = orthoSize;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = backgroundColor;
            camObj.transform.position = new Vector3(minX, 0f, -10f);

            // URP camera settings
            UniversalAdditionalCameraData urpCam = camObj.AddComponent<UniversalAdditionalCameraData>();
            urpCam.renderPostProcessing = true; // Enable post-processing for Bloom

            // Smooth camera tracking for flight corridors
            CameraFollow follow = camObj.AddComponent<CameraFollow>();
            follow.SetTarget(null, new Vector2(minX, -6f), new Vector2(maxX, 6f));

            // Continuous live ambient cosmic dust / star particle field
            camObj.AddComponent<CosmicDustParticles>();

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

            // Calibrated sleek scale (approx 1.6 units long)
            drone.transform.localScale = new Vector3(0.16f, 0.16f, 1f);
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

            // Calibrated gate scale (approx 2.8 units diameter)
            ring.transform.localScale = new Vector3(0.28f, 0.28f, 1f);
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

            // Calibrated relay spire scale (approx 3.9 units tall)
            relay.transform.localScale = new Vector3(0.38f, 0.38f, 1f);
            RelayStation station = relay.GetComponent<RelayStation>();
            if (station != null)
            {
                SerializedObject so = new SerializedObject(station);
                so.FindProperty("relayIndex").intValue = index;
                so.FindProperty("activationZoneRadius").floatValue = 2.5f;
                so.ApplyModifiedProperties();
            }

            if (relay.GetComponent<RelayStationVisuals>() == null)
            {
                relay.AddComponent<RelayStationVisuals>();
            }

            return relay;
        }

        private static GameObject CreateAsteroid(Vector3 position, float scale = 0.25f, float rotSpeed = 15f, Vector2 patrolOffset = default, float patrolSpeed = 1f)
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

            // Calibrated asteroid obstacle scale (approx 2.2 - 3.2 units)
            asteroid.transform.localScale = new Vector3(scale, scale, 1f);
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

            GameObject camObj = CreateSceneCamera(new Color(0.02f, 0.031f, 0.086f, 1f), 0f, 0f, 8f);

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
        // 2. LEVEL 1: FIRST SIGNAL (Corridor -18 to +18, 4 Rings)
        // ==========================================
        public static void BuildLevel01Scene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            CreateSceneCamera(new Color(0.02f, 0.031f, 0.086f, 1f), -18f, 18f, 7.5f);
            CreateGlobalLight();

            Sprite nebula = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_NebulaRelay/Art/Nebula_FirstSignal.png");
            Sprite planet = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_NebulaRelay/Art/Planet_BlueOcean.png");
            Sprite asteroid = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_NebulaRelay/Art/Asteroid_Detailed.png");
            CreateCosmicEnvironment(1, nebula, planet, asteroid);

            CreatePlayerDrone(new Vector3(-18f, 0f, 0f));

            CreateRing(1, new Vector3(-10f, 1.2f, 0f));
            CreateRing(2, new Vector3(-2f, -1.5f, 0f));
            CreateRing(3, new Vector3(6f, 2.0f, 0f));
            CreateRing(4, new Vector3(12f, 0f, 0f));

            CreateRelayStation(1, new Vector3(18f, 0.5f, 0f));

            GameObject ringMgrObj = new GameObject("RingManager");
            ringMgrObj.AddComponent<RingManager>();

            CreateManagers(1, "First Signal");

            EditorSceneManager.SaveScene(scene, "Assets/_NebulaRelay/Scenes/Level_01.unity");
        }

        // ==========================================
        // 3. LEVEL 2: THE DEBRIS FIELD (Corridor -24 to +24, 5 Rings, Asteroid Belts)
        // ==========================================
        public static void BuildLevel02Scene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            CreateSceneCamera(new Color(0.02f, 0.031f, 0.086f, 1f), -24f, 24f, 8f);
            CreateGlobalLight();

            Sprite nebula = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_NebulaRelay/Art/Nebula_DebrisField.png");
            Sprite asteroid = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_NebulaRelay/Art/Asteroid_Detailed.png");
            CreateCosmicEnvironment(2, nebula, null, asteroid);

            CreatePlayerDrone(new Vector3(-24f, 0f, 0f));

            CreateRing(1, new Vector3(-17f, -1.8f, 0f));
            CreateAsteroid(new Vector3(-12f, 1.5f, 0f), 0.26f, 15f, new Vector2(0f, 1.2f), 1.0f);

            CreateRing(2, new Vector3(-7f, 2.5f, 0f));
            CreateAsteroid(new Vector3(-2f, -1.2f, 0f), 0.28f, -20f, new Vector2(0f, 1.5f), 1.2f);

            CreateRing(3, new Vector3(3f, -2.5f, 0f));
            CreateAsteroid(new Vector3(8f, 2.0f, 0f), 0.24f, 18f);

            CreateRing(4, new Vector3(13f, 1.8f, 0f));
            CreateAsteroid(new Vector3(18f, -1.5f, 0f), 0.25f, -15f);

            CreateRing(5, new Vector3(19f, -1.2f, 0f));
            CreateRelayStation(2, new Vector3(24f, 0f, 0f));

            GameObject ringMgrObj = new GameObject("RingManager");
            ringMgrObj.AddComponent<RingManager>();

            CreateManagers(2, "The Debris Field");

            EditorSceneManager.SaveScene(scene, "Assets/_NebulaRelay/Scenes/Level_02.unity");
        }

        // ==========================================
        // 4. LEVEL 3: THE ION STORM (Corridor -28 to +28, 5 Rings, Slalom Hazards)
        // ==========================================
        public static void BuildLevel03Scene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            CreateSceneCamera(new Color(0.02f, 0.031f, 0.086f, 1f), -28f, 28f, 8.5f);
            CreateGlobalLight();

            Sprite nebula = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_NebulaRelay/Art/Nebula_LastTransmission.png");
            Sprite asteroid = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_NebulaRelay/Art/Asteroid_Detailed.png");
            CreateCosmicEnvironment(3, nebula, null, asteroid);

            CreatePlayerDrone(new Vector3(-28f, 0f, 0f));

            CreateRing(1, new Vector3(-20f, 2.5f, 0f));
            CreateAsteroid(new Vector3(-15f, 3.8f, 0f), 0.25f, 25f);
            CreateAsteroid(new Vector3(-15f, -0.8f, 0f), 0.24f, -20f);

            CreateRing(2, new Vector3(-10f, -2.5f, 0f));
            CreateAsteroid(new Vector3(-5f, 0f, 0f), 0.28f, 18f, new Vector2(0f, 2.0f), 1.5f);

            CreateRing(3, new Vector3(0f, 3.0f, 0f));
            CreateAsteroid(new Vector3(5f, 3.8f, 0f), 0.25f, 20f);
            CreateAsteroid(new Vector3(5f, -2.0f, 0f), 0.26f, -18f);

            CreateRing(4, new Vector3(11f, -2.2f, 0f));
            CreateAsteroid(new Vector3(16f, 1.5f, 0f), 0.24f, 15f);

            CreateRing(5, new Vector3(20f, 1.8f, 0f));
            CreateAsteroid(new Vector3(24f, -2.0f, 0f), 0.25f, -16f);

            CreateRelayStation(3, new Vector3(28f, 0f, 0f));

            GameObject ringMgrObj = new GameObject("RingManager");
            ringMgrObj.AddComponent<RingManager>();

            CreateManagers(3, "The Ion Storm");

            EditorSceneManager.SaveScene(scene, "Assets/_NebulaRelay/Scenes/Level_03.unity");
        }

        // ==========================================
        // 5. LEVEL 4: THE GRAVITY WELL (Corridor -32 to +32, 5 Rings, Amber Vortex)
        // ==========================================
        public static void BuildLevel04Scene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            CreateSceneCamera(new Color(0.02f, 0.031f, 0.086f, 1f), -32f, 32f, 8.5f);
            CreateGlobalLight();

            Sprite nebula = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_NebulaRelay/Art/Nebula_GravityWell.png");
            Sprite asteroid = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_NebulaRelay/Art/Asteroid_Detailed.png");
            CreateCosmicEnvironment(4, nebula, null, asteroid);

            CreatePlayerDrone(new Vector3(-32f, 0f, 0f));

            CreateRing(1, new Vector3(-24f, -2.5f, 0f));
            CreateAsteroid(new Vector3(-18f, 2.2f, 0f), 0.26f, 22f);
            CreateAsteroid(new Vector3(-18f, -3.5f, 0f), 0.24f, -18f);

            CreateRing(2, new Vector3(-12f, 3.2f, 0f));
            CreateAsteroid(new Vector3(-6f, 0f, 0f), 0.28f, 16f, new Vector2(0f, 2.2f), 1.4f);

            CreateRing(3, new Vector3(0f, -3.0f, 0f));
            CreateAsteroid(new Vector3(6f, 1.5f, 0f), 0.25f, -20f);
            CreateAsteroid(new Vector3(6f, -4.5f, 0f), 0.25f, 20f);

            CreateRing(4, new Vector3(13f, 2.5f, 0f));
            CreateAsteroid(new Vector3(19f, -1.8f, 0f), 0.26f, -15f);

            CreateRing(5, new Vector3(24f, -2.0f, 0f));
            CreateRelayStation(4, new Vector3(32f, 0f, 0f));

            GameObject ringMgrObj = new GameObject("RingManager");
            ringMgrObj.AddComponent<RingManager>();

            CreateManagers(4, "The Gravity Well");

            EditorSceneManager.SaveScene(scene, "Assets/_NebulaRelay/Scenes/Level_04.unity");
        }

        // ==========================================
        // 6. LEVEL 5: THE OMEGA TRANSMISSION (Corridor -38 to +38, 6 Rings, Grand Nexus)
        // ==========================================
        public static void BuildLevel05Scene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            CreateSceneCamera(new Color(0.02f, 0.031f, 0.086f, 1f), -38f, 38f, 9f);
            CreateGlobalLight();

            Sprite nebula = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_NebulaRelay/Art/Nebula_OmegaCore.png");
            Sprite asteroid = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_NebulaRelay/Art/Asteroid_Detailed.png");
            CreateCosmicEnvironment(5, nebula, null, asteroid);

            CreatePlayerDrone(new Vector3(-38f, 0f, 0f));

            CreateRing(1, new Vector3(-30f, 3.0f, 0f));
            CreateAsteroid(new Vector3(-24f, 1.5f, 0f), 0.25f, 20f);
            CreateAsteroid(new Vector3(-24f, -3.5f, 0f), 0.27f, -18f);

            CreateRing(2, new Vector3(-18f, -3.0f, 0f));
            CreateAsteroid(new Vector3(-12f, 2.0f, 0f), 0.28f, 16f, new Vector2(0f, 2.5f), 1.6f);

            CreateRing(3, new Vector3(-6f, 3.5f, 0f));
            CreateAsteroid(new Vector3(0f, 0.5f, 0f), 0.29f, -24f);
            CreateAsteroid(new Vector3(0f, -3.8f, 0f), 0.24f, 22f);

            CreateRing(4, new Vector3(7f, -3.0f, 0f));
            CreateAsteroid(new Vector3(13f, 2.5f, 0f), 0.26f, -15f);

            CreateRing(5, new Vector3(18f, 2.2f, 0f));
            CreateAsteroid(new Vector3(25f, 3.0f, 0f), 0.28f, 18f);
            CreateAsteroid(new Vector3(25f, -3.0f, 0f), 0.26f, -20f);

            CreateRing(6, new Vector3(30f, -1.0f, 0f));
            CreateRelayStation(5, new Vector3(38f, 0f, 0f));

            GameObject ringMgrObj = new GameObject("RingManager");
            ringMgrObj.AddComponent<RingManager>();

            CreateManagers(5, "The Omega Transmission");

            EditorSceneManager.SaveScene(scene, "Assets/_NebulaRelay/Scenes/Level_05.unity");
        }

        private static void ConfigureEditorBuildSettings()
        {
            string[] scenePaths = new string[]
            {
                "Assets/_NebulaRelay/Scenes/MainMenu.unity",
                "Assets/_NebulaRelay/Scenes/Level_01.unity",
                "Assets/_NebulaRelay/Scenes/Level_02.unity",
                "Assets/_NebulaRelay/Scenes/Level_03.unity",
                "Assets/_NebulaRelay/Scenes/Level_04.unity",
                "Assets/_NebulaRelay/Scenes/Level_05.unity"
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
