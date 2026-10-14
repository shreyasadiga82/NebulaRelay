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
        [MenuItem("Drift Core/Build All 10 Levels & Main Menu", priority = 1)]
        public static void BuildAllScenes()
        {
            UpgradePrefabs();
            BuildMainMenuScene();
            for (int i = 1; i <= 10; i++)
            {
                BuildMissionScene(i);
            }
            ConfigureEditorBuildSettings();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            EditorSceneManager.SaveOpenScenes();

            // Re-open Level 1 as the active working scene
            EditorSceneManager.OpenScene("Assets/_NebulaRelay/Scenes/Level_01.unity");
            Debug.Log("<color=#00FF66><b>[Drift Core] Calibrated Scales & Safe Scenes Generated! All 10 Levels and Main Menu constructed and registered in Build Settings.</b></color>");
        }

        private const string BuildPathPrefKey = "DriftCore_StandaloneExePath";
        public const string DefaultBuildExePath = "C:/Games/DriftCore/DriftCore.exe";

        public static string GetConfiguredBuildPath()
        {
            return EditorPrefs.GetString(BuildPathPrefKey, DefaultBuildExePath);
        }

        [MenuItem("Drift Core/Set Build Output Path...", priority = 3)]
        public static void SetBuildOutputPath()
        {
            string currentPath = GetConfiguredBuildPath();
            string currentDir = System.IO.Path.GetDirectoryName(currentPath);
            string selectedDir = EditorUtility.SaveFolderPanel("Select Standalone Build Folder", currentDir, "DriftCore");
            if (!string.IsNullOrEmpty(selectedDir))
            {
                string newExePath = System.IO.Path.Combine(selectedDir, "DriftCore.exe").Replace('\\', '/');
                EditorPrefs.SetString(BuildPathPrefKey, newExePath);
                Debug.Log($"[Drift Core] Build output path set to: {newExePath}");
            }
        }

        [MenuItem("Drift Core/Build Windows Standalone (.exe)", priority = 2)]
        public static void BuildWindowsExecutable()
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            EditorSceneManager.SaveOpenScenes();

            EditorApplication.delayCall += ExecuteBuildProcess;
        }

        public static void ExecuteBuildProcess()
        {
            string exePath = GetConfiguredBuildPath();
            string buildDir = System.IO.Path.GetDirectoryName(exePath);

            if (System.IO.Directory.Exists(buildDir))
            {
                try
                {
                    System.IO.Directory.Delete(buildDir, true);
                    Debug.Log($"[Drift Core] Cleaned previous build directory: {buildDir}");
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"[Drift Core] Warning cleaning build directory: {ex.Message}");
                }
            }

            if (!System.IO.Directory.Exists(buildDir))
            {
                System.IO.Directory.CreateDirectory(buildDir);
            }

            string[] scenePaths = new string[11];
            scenePaths[0] = "Assets/_NebulaRelay/Scenes/MainMenu.unity";
            for (int i = 1; i <= 10; i++)
            {
                scenePaths[i] = $"Assets/_NebulaRelay/Scenes/Level_{i:D2}.unity";
            }

            BuildPlayerOptions opt = new BuildPlayerOptions
            {
                scenes = scenePaths,
                locationPathName = exePath,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            Debug.Log($"[Drift Core] Starting Standalone Windows x64 build to: {exePath}...");
            var report = BuildPipeline.BuildPlayer(opt);
            if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
            {
                Debug.Log($"<color=#00FF66><b>[Drift Core] Standalone Windows Build Succeeded! Executable: {exePath} ({report.summary.totalSize / 1024 / 1024} MB)</b></color>");
                VerifyBuildIntegrity(exePath, report);
            }
            else
            {
                Debug.LogError($"[Drift Core] Build failed with result: {report.summary.result} (Errors: {report.summary.totalErrors})");
            }
        }

        public static bool VerifyBuildIntegrity(string exePath, UnityEditor.Build.Reporting.BuildReport report = null)
        {
            string buildDir = System.IO.Path.GetDirectoryName(exePath);
            string dataDir = System.IO.Path.Combine(buildDir, "DriftCore_Data");
            if (!System.IO.Directory.Exists(dataDir))
            {
                Debug.LogError($"[Drift Core Integrity Error] Data directory missing: {dataDir}");
                return false;
            }

            string[] requiredFiles = new string[]
            {
                "globalgamemanagers",
                "level0",
                "level1",
                "level2",
                "level3",
                "level4",
                "level5",
                "level6",
                "level7",
                "level8",
                "level9",
                "level10"
            };

            bool allValid = true;
            foreach (string file in requiredFiles)
            {
                string fullPath = System.IO.Path.Combine(dataDir, file);
                if (!System.IO.File.Exists(fullPath))
                {
                    Debug.LogError($"[Drift Core Integrity Error] Required build file missing: {file} at {fullPath}");
                    allValid = false;
                }
                else
                {
                    System.IO.FileInfo fi = new System.IO.FileInfo(fullPath);
                    if (fi.Length <= 0)
                    {
                        Debug.LogError($"[Drift Core Integrity Error] File {file} is empty (0 bytes)!");
                        allValid = false;
                    }
                    else
                    {
                        Debug.Log($"[Drift Core Integrity] OK: {file} ({fi.Length} bytes)");
                    }
                }
            }

            if (allValid)
            {
                Debug.Log("<color=#00FF66><b>[Drift Core] Build integrity verified! All level0..10 and globalgamemanagers present and valid (> 0 bytes).</b></color>");
            }
            return allValid;
        }

        // ==========================================
        // PREFAB UPGRADES WITH EXACT CALIBRATED SCALES & COLLIDERS
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

            UniversalAdditionalCameraData urpCam = camObj.AddComponent<UniversalAdditionalCameraData>();
            urpCam.renderPostProcessing = true;

            CameraFollow follow = camObj.AddComponent<CameraFollow>();
            follow.SetTarget(null, new Vector2(minX, -6f), new Vector2(maxX, 6f));

            camObj.AddComponent<CosmicDustParticles>();

            return camObj;
        }

        private static void CreateGlobalLight()
        {
            GameObject lightObj = new GameObject("Global Light 2D");
            Light2D light = lightObj.AddComponent<Light2D>();
            light.lightType = Light2D.LightType.Global;
            light.color = new Color(0.9f, 0.95f, 1f, 1f);
            light.intensity = 1.1f;
        }

        private static GameObject CreatePlayerDrone(Vector3 position)
        {
            string prefabPath = "Assets/_NebulaRelay/Prefabs/PlayerDrone.prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            GameObject drone = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            drone.transform.position = position;
            return drone;
        }

        private static GameObject CreateRing(int order, Vector3 position)
        {
            string prefabPath = "Assets/_NebulaRelay/Prefabs/RingGate.prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            GameObject ring = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            ring.transform.position = position;
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

        private static void CreateManagers(int levelNumber, string levelTitle, float minCorridorX, float maxCorridorX)
        {
            GameObject levelMgrObj = new GameObject("LevelManager");
            LevelManager lm = levelMgrObj.AddComponent<LevelManager>();
            SerializedObject so = new SerializedObject(lm);
            so.FindProperty("levelNumber").intValue = levelNumber;
            so.FindProperty("levelTitle").stringValue = levelTitle;
            so.FindProperty("minCorridorX").floatValue = minCorridorX;
            so.FindProperty("maxCorridorX").floatValue = maxCorridorX;
            so.FindProperty("marginX").floatValue = 4.0f;
            so.FindProperty("corridorMarginY").floatValue = 6.5f;
            so.ApplyModifiedProperties();

            levelMgrObj.AddComponent<HUDController>();
            MissionObstacleSpawner spawner = levelMgrObj.AddComponent<MissionObstacleSpawner>();
            string astPrefabPath = "Assets/_NebulaRelay/Prefabs/AsteroidHazard.prefab";
            GameObject astPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(astPrefabPath);
            if (astPrefab != null)
            {
                SerializedObject soSpawner = new SerializedObject(spawner);
                soSpawner.FindProperty("asteroidPrefab").objectReferenceValue = astPrefab;
                soSpawner.ApplyModifiedProperties();
            }

            GameObject gmObj = new GameObject("GameManager");
            gmObj.AddComponent<GameManager>();

            GameObject audioObj = new GameObject("AudioManager");
            AudioManager am = audioObj.AddComponent<AudioManager>();
            AssignAudioClips(am);
        }

        private static void AssignAudioClips(AudioManager am)
        {
            if (am == null) return;
            SerializedObject so = new SerializedObject(am);
            so.FindProperty("capsuleLaunchClip").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/Capsule_Launch.wav");
            so.FindProperty("capsuleLaunchDeniedClip").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/Capsule_Launch_Denied.wav");
            so.FindProperty("capsuleHumClip").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/Capsule_Hum.wav");
            so.FindProperty("collisionLightClip").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/Collision_Light.wav");
            so.FindProperty("collisionHeavyClip").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/Collision_Heavy.wav");
            so.FindProperty("relayActivateClip").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/Relay_Activate.wav");
            so.FindProperty("relayScoreClip").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/Relay_Score.wav");
            so.FindProperty("exitUnlockedClip").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/Exit_Unlocked.wav");
            so.FindProperty("exitLockedDenialClip").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/Exit_Locked_Denial.wav");
            so.FindProperty("missionStartupClip").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/Mission_Startup.wav");
            so.FindProperty("missionCompleteClip").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/Mission_Complete.wav");
            so.FindProperty("missionFailedClip").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/Mission_Failed.wav");
            so.FindProperty("missionResetClip").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/Mission_Reset.wav");
            so.FindProperty("countdownWarningClip").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/Countdown_Warning.wav");
            so.FindProperty("countdownUrgentClip").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/Countdown_Urgent.wav");
            so.FindProperty("uiClickClip").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/UI_Click.wav");
            so.FindProperty("uiPauseClip").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/UI_Pause.wav");
            so.FindProperty("uiResumeClip").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/UI_Resume.wav");
            so.FindProperty("ambienceSpaceStationClip").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Ambience/Ambience_SpaceStation.wav");
            so.ApplyModifiedProperties();
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
        // MAIN MENU SCENE
        // ==========================================
        public static void BuildMainMenuScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            CreateSceneCamera(new Color(0.02f, 0.031f, 0.086f, 1f), 0f, 0f, 8.0f);
            CreateGlobalLight();

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
            AudioManager am = audioObj.AddComponent<AudioManager>();
            AssignAudioClips(am);

            EditorSceneManager.SaveScene(scene, "Assets/_NebulaRelay/Scenes/MainMenu.unity");
        }

        // ==========================================
        // DATA-DRIVEN BUILDER FOR MISSIONS 1 TO 10
        // ==========================================
        public static void BuildMissionScene(int missionIndex)
        {
            MissionDefinition def = MissionRegistry.GetMission(missionIndex);
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            CreateSceneCamera(new Color(0.02f, 0.031f, 0.086f, 1f), def.MinCorridorX, def.MaxCorridorX, 7.5f);
            CreateGlobalLight();

            // Cycle through existing 5 nebula background sprites
            string[] nebulaPaths = new string[]
            {
                "Assets/_NebulaRelay/Art/Nebula_FirstSignal.png",
                "Assets/_NebulaRelay/Art/Nebula_DebrisField.png",
                "Assets/_NebulaRelay/Art/Nebula_LastTransmission.png",
                "Assets/_NebulaRelay/Art/Nebula_GravityWell.png",
                "Assets/_NebulaRelay/Art/Nebula_OmegaCore.png"
            };

            int themeIndex = ((missionIndex - 1) % 5);
            Sprite nebula = AssetDatabase.LoadAssetAtPath<Sprite>(nebulaPaths[themeIndex]);
            Sprite asteroid = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_NebulaRelay/Art/Asteroid_Detailed.png");
            Sprite planet = null;

            if (def.HasPlanet)
            {
                string planetPath = (def.PlanetThemeIndex == 1)
                    ? "Assets/_NebulaRelay/Art/Planet_BlueOcean.png"
                    : "Assets/_NebulaRelay/Art/Planet_AmberMoon.png";
                planet = AssetDatabase.LoadAssetAtPath<Sprite>(planetPath);
            }

            CreateCosmicEnvironment(missionIndex, nebula, planet, asteroid);

            // Player Drone
            CreatePlayerDrone(def.DroneStart);

            // Ring Gates in sequential order
            for (int i = 0; i < def.GatePositions.Length; i++)
            {
                CreateRing(i + 1, def.GatePositions[i]);
            }

            // Relay Station
            CreateRelayStation(missionIndex, def.RelayPosition);

            // Obstacles: Pre-baked scene hierarchy with nested difficulty tiers (Easy, Normal, Hard, Expert)
            string astPrefabPath = "Assets/_NebulaRelay/Prefabs/AsteroidHazard.prefab";
            GameObject astPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(astPrefabPath);
            if (astPrefab != null)
            {
                MissionObstacleSpawner.BuildSceneObstacles(missionIndex, astPrefab);
            }

            // Ring Manager
            GameObject ringMgrObj = new GameObject("RingManager");
            ringMgrObj.AddComponent<RingManager>();

            // Level & Game Managers
            CreateManagers(missionIndex, def.Title, def.MinCorridorX, def.MaxCorridorX);

            string scenePath = $"Assets/_NebulaRelay/Scenes/Level_{missionIndex:D2}.unity";
            EditorSceneManager.SaveScene(scene, scenePath);
        }

        // Backward compatibility wrappers
        public static void BuildLevel01Scene() => BuildMissionScene(1);
        public static void BuildLevel02Scene() => BuildMissionScene(2);
        public static void BuildLevel03Scene() => BuildMissionScene(3);
        public static void BuildLevel04Scene() => BuildMissionScene(4);
        public static void BuildLevel05Scene() => BuildMissionScene(5);

        public static void ConfigureEditorBuildSettings()
        {
            string[] scenePaths = new string[11];
            scenePaths[0] = "Assets/_NebulaRelay/Scenes/MainMenu.unity";
            for (int i = 1; i <= 10; i++)
            {
                scenePaths[i] = $"Assets/_NebulaRelay/Scenes/Level_{i:D2}.unity";
            }

            EditorBuildSettingsScene[] scenes = new EditorBuildSettingsScene[scenePaths.Length];
            for (int i = 0; i < scenePaths.Length; i++)
            {
                scenes[i] = new EditorBuildSettingsScene(scenePaths[i], true);
            }
            EditorBuildSettings.scenes = scenes;
        }
    }
}
