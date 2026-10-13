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
    public static class NebulaSceneBuilder
    {
        [MenuItem("Drift Core/Build All 5 Levels & Main Menu", priority = 1)]
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
            Debug.Log("<color=#00FF66><b>[Drift Core] Calibrated Scales & Safe Scenes Generated! All 5 Levels and Main Menu constructed and registered in Build Settings.</b></color>");
        }

        [MenuItem("Drift Core/Build Windows Standalone (.exe)", priority = 2)]
        public static void BuildWindowsExecutable()
        {
            BuildAllScenes();

            string buildDir = "Builds/Windows";
            if (!System.IO.Directory.Exists(buildDir))
            {
                System.IO.Directory.CreateDirectory(buildDir);
            }

            string exePath = $"{buildDir}/DriftCore.exe";
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

            Debug.Log("[Drift Core] Starting Standalone Windows x64 build with 5 Sectors and Live Cosmic Motion...");
            var report = BuildPipeline.BuildPlayer(opt);
            if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
            {
                Debug.Log($"<color=#00FF66><b>[Drift Core] Standalone Windows Build Succeeded! Executable: {exePath} ({report.summary.totalSize / 1024 / 1024} MB)</b></color>");
            }
            else
            {
                Debug.LogError($"[Drift Core] Build failed: {report.summary.result}");
            }
        }

        // ==========================================
        // PREFAB UPGRADES WITH EXACT CALIBRATED SCALES & COLLIDERS
        // All sizing calibrated in prefabs only, never on scene instances
        // Target sizes: Drone ~1.2u, Gate ~2.4u, Relay ~3.2u, Asteroid 1.0-1.8u, Capsule ~0.4u, Receiver ~1.5u
        // ==========================================
        public static void UpgradePrefabs()
        {
            // 1. Player Drone Prefab (Visible hull: 8.05u wide x 9.06u tall -> Scale 0.132 => 1.20u tall)
            string dronePrefabPath = "Assets/_NebulaRelay/Prefabs/PlayerDrone.prefab";
            GameObject dronePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(dronePrefabPath);
            if (dronePrefab != null)
            {
                Sprite droneSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_NebulaRelay/Art/Drone_Courier.png");
                SpriteRenderer sr = dronePrefab.GetComponent<SpriteRenderer>();
                if (sr == null) sr = dronePrefab.AddComponent<SpriteRenderer>();
                if (droneSprite != null) sr.sprite = droneSprite;
                sr.sortingOrder = 5;

                dronePrefab.transform.localScale = new Vector3(0.132f, 0.132f, 1f);

                CircleCollider2D col = dronePrefab.GetComponent<CircleCollider2D>();
                if (col != null)
                {
                    col.radius = 4.0f; // 4.0 * 0.132 = 0.528 world radius (~1.06u hull diameter)
                }

                if (dronePrefab.GetComponent<DroneVisuals>() == null)
                {
                    dronePrefab.AddComponent<DroneVisuals>();
                }
                PrefabUtility.SavePrefabAsset(dronePrefab);
            }

            // 2. Relay Station Prefab (Visible spire: 9.55u tall -> Scale 0.335 => 3.20u tall)
            string relayPrefabPath = "Assets/_NebulaRelay/Prefabs/RelayStation.prefab";
            GameObject relayPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(relayPrefabPath);
            if (relayPrefab != null)
            {
                Sprite relaySprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_NebulaRelay/Art/RelayStation_Tower.png");
                SpriteRenderer sr = relayPrefab.GetComponent<SpriteRenderer>();
                if (sr == null) sr = relayPrefab.AddComponent<SpriteRenderer>();
                if (relaySprite != null) sr.sprite = relaySprite;
                sr.sortingOrder = 2;

                relayPrefab.transform.localScale = new Vector3(0.335f, 0.335f, 1f);

                CircleCollider2D col = relayPrefab.GetComponent<CircleCollider2D>();
                if (col != null)
                {
                    col.radius = 6.5f; // 6.5 * 0.335 = 2.18 world radius
                }

                RelayStation station = relayPrefab.GetComponent<RelayStation>();
                if (station != null)
                {
                    SerializedObject so = new SerializedObject(station);
                    so.FindProperty("activationZoneRadius").floatValue = 2.2f;
                    so.ApplyModifiedProperties();
                }

                if (relayPrefab.GetComponent<RelayStationVisuals>() == null)
                {
                    relayPrefab.AddComponent<RelayStationVisuals>();
                }
                PrefabUtility.SavePrefabAsset(relayPrefab);
            }

            // 3. Ring Gate Prefab (Visible ring: 9.23u diameter -> Scale 0.26 => 2.40u diameter)
            string ringPrefabPath = "Assets/_NebulaRelay/Prefabs/RingGate.prefab";
            GameObject ringPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(ringPrefabPath);
            if (ringPrefab != null)
            {
                Sprite ringSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_NebulaRelay/Art/RingGate_Neon.png");
                SpriteRenderer sr = ringPrefab.GetComponent<SpriteRenderer>();
                if (sr == null) sr = ringPrefab.AddComponent<SpriteRenderer>();
                if (ringSprite != null) sr.sprite = ringSprite;
                sr.sortingOrder = 3;

                ringPrefab.transform.localScale = new Vector3(0.26f, 0.26f, 1f);

                CircleCollider2D col = ringPrefab.GetComponent<CircleCollider2D>();
                if (col != null)
                {
                    col.radius = 4.5f; // 4.5 * 0.26 = 1.17 world radius (2.34u trigger diameter)
                }
                PrefabUtility.SavePrefabAsset(ringPrefab);
            }

            // 4. Asteroid Hazard Prefab (Visible rock: 8.2u average diameter -> Base Scale 0.17 => 1.40u)
            string astPrefabPath = "Assets/_NebulaRelay/Prefabs/AsteroidHazard.prefab";
            GameObject astPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(astPrefabPath);
            if (astPrefab != null)
            {
                Sprite astSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_NebulaRelay/Art/Asteroid_Detailed.png");
                SpriteRenderer sr = astPrefab.GetComponent<SpriteRenderer>();
                if (sr == null) sr = astPrefab.AddComponent<SpriteRenderer>();
                if (astSprite != null) sr.sprite = astSprite;
                sr.sortingOrder = 4;

                astPrefab.transform.localScale = new Vector3(0.17f, 0.17f, 1f);

                CircleCollider2D col = astPrefab.GetComponent<CircleCollider2D>();
                if (col != null)
                {
                    col.radius = 4.0f; // 4.0 * 0.17 = 0.68 world radius (1.36u diameter)
                }
                PrefabUtility.SavePrefabAsset(astPrefab);
            }

            // 5. Energy Capsule Prefab (Visible core: 7.0u diameter -> Scale 0.057 => 0.40u)
            string capPrefabPath = "Assets/_NebulaRelay/Prefabs/EnergyCapsule.prefab";
            GameObject capPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(capPrefabPath);
            if (capPrefab != null)
            {
                Sprite capSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_NebulaRelay/Art/EnergyCapsule_Core.png");
                SpriteRenderer sr = capPrefab.GetComponent<SpriteRenderer>();
                if (sr == null) sr = capPrefab.AddComponent<SpriteRenderer>();
                if (capSprite != null) sr.sprite = capSprite;
                sr.sortingOrder = 6;

                capPrefab.transform.localScale = new Vector3(0.057f, 0.057f, 1f);

                CircleCollider2D col = capPrefab.GetComponent<CircleCollider2D>();
                if (col != null)
                {
                    col.radius = 3.5f; // 3.5 * 0.057 = 0.20 world radius (0.40u diameter)
                }
                PrefabUtility.SavePrefabAsset(capPrefab);
            }

            // 6. Energy Receiver Prefab (Visible dish: 1.05u at PPU 100 -> Scale 1.43 => 1.50u)
            string recPrefabPath = "Assets/_NebulaRelay/Prefabs/EnergyReceiver.prefab";
            GameObject recPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(recPrefabPath);
            if (recPrefab != null)
            {
                recPrefab.transform.localScale = new Vector3(1.43f, 1.43f, 1f);
                CircleCollider2D col = recPrefab.GetComponent<CircleCollider2D>();
                if (col != null)
                {
                    col.radius = 0.55f; // 0.55 * 1.43 = 0.786 world radius (1.57u diameter)
                }
                PrefabUtility.SavePrefabAsset(recPrefab);
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

            GameObject drone = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            drone.transform.position = position;
            // Preserves prefab scale (0.132) without creating scene overrides
            return drone;
        }

        private static GameObject CreateRing(int order, Vector3 position)
        {
            string prefabPath = "Assets/_NebulaRelay/Prefabs/RingGate.prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            GameObject ring = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            ring.transform.position = position;
            // Preserves prefab scale (0.26) without creating scene overrides
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

            GameObject relay = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            relay.transform.position = position;
            // Preserves prefab scale (0.335) without creating scene overrides
            RelayStation station = relay.GetComponent<RelayStation>();
            if (station != null)
            {
                SerializedObject so = new SerializedObject(station);
                so.FindProperty("relayIndex").intValue = index;
                so.FindProperty("activationZoneRadius").floatValue = 2.2f;
                so.ApplyModifiedProperties();
            }

            if (relay.GetComponent<RelayStationVisuals>() == null)
            {
                relay.AddComponent<RelayStationVisuals>();
            }

            return relay;
        }

        private static GameObject CreateAsteroid(Vector3 position, float scaleMultiplier = 1.0f, float rotSpeed = 15f, Vector2 patrolOffset = default, float patrolSpeed = 1f)
        {
            string prefabPath = "Assets/_NebulaRelay/Prefabs/AsteroidHazard.prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            GameObject asteroid = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            asteroid.transform.position = position;
            // Preserves prefab scale (0.17) without creating transform overrides; applies variation via component property
            AsteroidHazard hazard = asteroid.GetComponent<AsteroidHazard>();
            if (hazard != null)
            {
                SerializedObject so = new SerializedObject(hazard);
                so.FindProperty("scaleMultiplier").floatValue = Mathf.Clamp(scaleMultiplier, 0.8f, 1.3f);
                so.FindProperty("rotationSpeed").floatValue = rotSpeed;
                so.FindProperty("patrolOffset").vector2Value = patrolOffset;
                so.FindProperty("patrolSpeed").floatValue = patrolSpeed;
                so.ApplyModifiedProperties();
            }

            return asteroid;
        }

        private static void CreateManagers(int levelNumber, string levelTitle, float minCorridorX, float maxCorridorX)
        {
            // Level Coordinator
            GameObject levelMgrObj = new GameObject("LevelManager");
            LevelManager lm = levelMgrObj.AddComponent<LevelManager>();
            SerializedObject so = new SerializedObject(lm);
            so.FindProperty("levelNumber").intValue = levelNumber;
            so.FindProperty("levelTitle").stringValue = levelTitle;
            so.FindProperty("minCorridorX").floatValue = minCorridorX;
            so.FindProperty("maxCorridorX").floatValue = maxCorridorX;
            so.FindProperty("marginX").floatValue = 2.0f;
            so.FindProperty("corridorMarginY").floatValue = 6.5f;
            so.ApplyModifiedProperties();

            // HUD Controller
            levelMgrObj.AddComponent<HUDController>();

            // Distinct Singletons for clean lifecycle
            GameObject gmObj = new GameObject("GameManager");
            gmObj.AddComponent<GameManager>();

            GameObject audioObj = new GameObject("AudioManager");
            audioObj.AddComponent<AudioManager>();
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

            CreateSceneCamera(new Color(0.02f, 0.031f, 0.086f, 1f), 0f, 0f, 8.0f);
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

            GameObject gmObj = new GameObject("GameManager");
            gmObj.AddComponent<GameManager>();

            GameObject audioObj = new GameObject("AudioManager");
            audioObj.AddComponent<AudioManager>();

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

            CreateManagers(1, "First Signal", -18f, 18f);

            EditorSceneManager.SaveScene(scene, "Assets/_NebulaRelay/Scenes/Level_01.unity");
        }

        // ==========================================
        // 3. LEVEL 2: THE DEBRIS FIELD (Corridor -24 to +24, 5 Rings, Asteroid Belts)
        // ==========================================
        public static void BuildLevel02Scene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            CreateSceneCamera(new Color(0.02f, 0.031f, 0.086f, 1f), -24f, 24f, 7.5f);
            CreateGlobalLight();

            Sprite nebula = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_NebulaRelay/Art/Nebula_DebrisField.png");
            Sprite asteroid = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_NebulaRelay/Art/Asteroid_Detailed.png");
            CreateCosmicEnvironment(2, nebula, null, asteroid);

            CreatePlayerDrone(new Vector3(-24f, 0f, 0f));

            CreateRing(1, new Vector3(-17f, -1.8f, 0f));
            CreateAsteroid(new Vector3(-12f, 1.5f, 0f), 1.05f, 15f, new Vector2(0f, 1.2f), 1.0f);

            CreateRing(2, new Vector3(-7f, 2.5f, 0f));
            CreateAsteroid(new Vector3(-2f, -1.2f, 0f), 1.15f, -20f, new Vector2(0f, 1.5f), 1.2f);

            CreateRing(3, new Vector3(3f, -2.5f, 0f));
            CreateAsteroid(new Vector3(8f, 2.0f, 0f), 0.95f, 18f);

            CreateRing(4, new Vector3(13f, 1.8f, 0f));
            CreateAsteroid(new Vector3(18f, -1.5f, 0f), 1.0f, -15f);

            CreateRing(5, new Vector3(19f, -1.2f, 0f));
            CreateRelayStation(2, new Vector3(24f, 0f, 0f));

            GameObject ringMgrObj = new GameObject("RingManager");
            ringMgrObj.AddComponent<RingManager>();

            CreateManagers(2, "The Debris Field", -24f, 24f);

            EditorSceneManager.SaveScene(scene, "Assets/_NebulaRelay/Scenes/Level_02.unity");
        }

        // ==========================================
        // 4. LEVEL 3: THE ION STORM (Corridor -28 to +28, 5 Rings, Slalom Hazards)
        // ==========================================
        public static void BuildLevel03Scene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            CreateSceneCamera(new Color(0.02f, 0.031f, 0.086f, 1f), -28f, 28f, 7.5f);
            CreateGlobalLight();

            Sprite nebula = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_NebulaRelay/Art/Nebula_LastTransmission.png");
            Sprite asteroid = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_NebulaRelay/Art/Asteroid_Detailed.png");
            CreateCosmicEnvironment(3, nebula, null, asteroid);

            CreatePlayerDrone(new Vector3(-28f, 0f, 0f));

            CreateRing(1, new Vector3(-20f, 2.5f, 0f));
            CreateAsteroid(new Vector3(-15f, 3.8f, 0f), 1.0f, 25f);
            CreateAsteroid(new Vector3(-15f, -0.8f, 0f), 0.95f, -20f);

            CreateRing(2, new Vector3(-10f, -2.5f, 0f));
            CreateAsteroid(new Vector3(-5f, 0f, 0f), 1.15f, 18f, new Vector2(0f, 2.0f), 1.5f);

            CreateRing(3, new Vector3(0f, 3.0f, 0f));
            CreateAsteroid(new Vector3(5f, 3.8f, 0f), 1.0f, 20f);
            CreateAsteroid(new Vector3(5f, -2.0f, 0f), 1.05f, -18f);

            CreateRing(4, new Vector3(11f, -2.2f, 0f));
            CreateAsteroid(new Vector3(16f, 1.5f, 0f), 0.95f, 15f);

            CreateRing(5, new Vector3(20f, 1.8f, 0f));
            CreateAsteroid(new Vector3(24f, -2.0f, 0f), 1.0f, -16f);

            CreateRelayStation(3, new Vector3(28f, 0f, 0f));

            GameObject ringMgrObj = new GameObject("RingManager");
            ringMgrObj.AddComponent<RingManager>();

            CreateManagers(3, "The Ion Storm", -28f, 28f);

            EditorSceneManager.SaveScene(scene, "Assets/_NebulaRelay/Scenes/Level_03.unity");
        }

        // ==========================================
        // 5. LEVEL 4: THE GRAVITY WELL (Corridor -32 to +32, 5 Rings, Amber Vortex)
        // ==========================================
        public static void BuildLevel04Scene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            CreateSceneCamera(new Color(0.02f, 0.031f, 0.086f, 1f), -32f, 32f, 7.5f);
            CreateGlobalLight();

            Sprite nebula = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_NebulaRelay/Art/Nebula_GravityWell.png");
            Sprite asteroid = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_NebulaRelay/Art/Asteroid_Detailed.png");
            CreateCosmicEnvironment(4, nebula, null, asteroid);

            CreatePlayerDrone(new Vector3(-32f, 0f, 0f));

            CreateRing(1, new Vector3(-24f, -2.5f, 0f));
            CreateAsteroid(new Vector3(-18f, 2.2f, 0f), 1.05f, 22f);
            CreateAsteroid(new Vector3(-18f, -3.5f, 0f), 0.95f, -18f);

            CreateRing(2, new Vector3(-12f, 3.2f, 0f));
            CreateAsteroid(new Vector3(-6f, 0f, 0f), 1.15f, 16f, new Vector2(0f, 2.2f), 1.4f);

            CreateRing(3, new Vector3(0f, -3.0f, 0f));
            CreateAsteroid(new Vector3(6f, 1.5f, 0f), 1.0f, -20f);
            CreateAsteroid(new Vector3(6f, -4.5f, 0f), 1.0f, 20f);

            CreateRing(4, new Vector3(13f, 2.5f, 0f));
            CreateAsteroid(new Vector3(19f, -1.8f, 0f), 1.05f, -15f);

            CreateRing(5, new Vector3(24f, -2.0f, 0f));
            CreateRelayStation(4, new Vector3(32f, 0f, 0f));

            GameObject ringMgrObj = new GameObject("RingManager");
            ringMgrObj.AddComponent<RingManager>();

            CreateManagers(4, "The Gravity Well", -32f, 32f);

            EditorSceneManager.SaveScene(scene, "Assets/_NebulaRelay/Scenes/Level_04.unity");
        }

        // ==========================================
        // 6. LEVEL 5: THE OMEGA TRANSMISSION (Corridor -38 to +38, 6 Rings, Grand Nexus)
        // ==========================================
        public static void BuildLevel05Scene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            CreateSceneCamera(new Color(0.02f, 0.031f, 0.086f, 1f), -38f, 38f, 7.5f);
            CreateGlobalLight();

            Sprite nebula = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_NebulaRelay/Art/Nebula_OmegaCore.png");
            Sprite asteroid = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_NebulaRelay/Art/Asteroid_Detailed.png");
            CreateCosmicEnvironment(5, nebula, null, asteroid);

            CreatePlayerDrone(new Vector3(-38f, 0f, 0f));

            CreateRing(1, new Vector3(-30f, 3.0f, 0f));
            CreateAsteroid(new Vector3(-24f, 1.5f, 0f), 1.0f, 20f);
            CreateAsteroid(new Vector3(-24f, -3.5f, 0f), 1.1f, -18f);

            CreateRing(2, new Vector3(-18f, -3.0f, 0f));
            CreateAsteroid(new Vector3(-12f, 2.0f, 0f), 1.15f, 16f, new Vector2(0f, 2.5f), 1.6f);

            CreateRing(3, new Vector3(-6f, 3.5f, 0f));
            CreateAsteroid(new Vector3(0f, 0.5f, 0f), 1.2f, -24f);
            CreateAsteroid(new Vector3(0f, -3.8f, 0f), 0.95f, 22f);

            CreateRing(4, new Vector3(7f, -3.0f, 0f));
            CreateAsteroid(new Vector3(13f, 2.5f, 0f), 1.05f, -15f);

            CreateRing(5, new Vector3(18f, 2.2f, 0f));
            CreateAsteroid(new Vector3(25f, 3.0f, 0f), 1.15f, 18f);
            CreateAsteroid(new Vector3(25f, -3.0f, 0f), 1.05f, -20f);

            CreateRing(6, new Vector3(30f, -1.0f, 0f));
            CreateRelayStation(5, new Vector3(38f, 0f, 0f));

            GameObject ringMgrObj = new GameObject("RingManager");
            ringMgrObj.AddComponent<RingManager>();

            CreateManagers(5, "The Omega Transmission", -38f, 38f);

            EditorSceneManager.SaveScene(scene, "Assets/_NebulaRelay/Scenes/Level_05.unity");
        }

        public static void ConfigureEditorBuildSettings()
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

            EditorBuildSettingsScene[] scenes = new EditorBuildSettingsScene[scenePaths.Length];
            for (int i = 0; i < scenePaths.Length; i++)
            {
                scenes[i] = new EditorBuildSettingsScene(scenePaths[i], true);
            }
            EditorBuildSettings.scenes = scenes;
        }
    }
}
