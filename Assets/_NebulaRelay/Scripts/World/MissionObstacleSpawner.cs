using System.Collections.Generic;
using UnityEngine;
using NebulaRelay.Core;

namespace NebulaRelay.World
{
    /// <summary>
    /// Spawns and manages the deterministic nested obstacle layout for each mission and difficulty mode.
    /// Supports both pre-baked scene hierarchy tiers (Obstacles_Tier1_Easy .. Tier4_Expert)
    /// and runtime dynamic fallback instantiation.
    /// </summary>
    public class MissionObstacleSpawner : MonoBehaviour
    {
        [SerializeField] private GameObject asteroidPrefab;

        private GameObject tier1Easy;
        private GameObject tier2Normal;
        private GameObject tier3Hard;
        private GameObject tier4Expert;

        private void Awake()
        {
            FindSceneTiers();
            SettingsManager.OnDifficultyChanged += HandleDifficultyChanged;
        }

        private void OnDestroy()
        {
            SettingsManager.OnDifficultyChanged -= HandleDifficultyChanged;
        }

        private void Start()
        {
            ApplyDifficulty(SettingsManager.CurrentDifficulty);
        }

        private void HandleDifficultyChanged(GameDifficulty newDiff)
        {
            ApplyDifficulty(newDiff);
        }

        private void FindSceneTiers()
        {
            GameObject obsRoot = GameObject.Find("Obstacles");
            if (obsRoot != null)
            {
                tier1Easy = obsRoot.transform.Find("Obstacles_Tier1_Easy")?.gameObject;
                tier2Normal = obsRoot.transform.Find("Obstacles_Tier2_Normal")?.gameObject;
                tier3Hard = obsRoot.transform.Find("Obstacles_Tier3_Hard")?.gameObject;
                tier4Expert = obsRoot.transform.Find("Obstacles_Tier4_Expert")?.gameObject;
            }

            if (tier1Easy == null) tier1Easy = GameObject.Find("Obstacles_Tier1_Easy");
            if (tier2Normal == null) tier2Normal = GameObject.Find("Obstacles_Tier2_Normal");
            if (tier3Hard == null) tier3Hard = GameObject.Find("Obstacles_Tier3_Hard");
            if (tier4Expert == null) tier4Expert = GameObject.Find("Obstacles_Tier4_Expert");
        }

        public void ApplyDifficulty(GameDifficulty diff)
        {
            FindSceneTiers();

            if (tier1Easy != null || tier2Normal != null || tier3Hard != null || tier4Expert != null)
            {
                // Active based on difficulty nesting: Easy ⊂ Normal ⊂ Hard ⊂ Expert
                if (tier1Easy != null) tier1Easy.SetActive(true);
                if (tier2Normal != null) tier2Normal.SetActive(diff >= GameDifficulty.Medium);
                if (tier3Hard != null) tier3Hard.SetActive(diff >= GameDifficulty.Hard);
                if (tier4Expert != null) tier4Expert.SetActive(diff >= GameDifficulty.Expert);
            }
            else
            {
                // Fallback runtime spawn if scene doesn't have baked tiers
                Transform existing = transform.Find("SpawnedObstacles");
                if (existing != null)
                {
                    Destroy(existing.gameObject);
                }
                int levelNum = LevelManager.Instance != null ? LevelManager.Instance.LevelNumber : 1;
                SpawnObstacles(levelNum, diff, transform, asteroidPrefab);
            }
        }

        public static void SpawnObstacles(int missionIndex, GameDifficulty diff, Transform parent = null, GameObject prefab = null)
        {
            if (prefab == null)
            {
#if UNITY_EDITOR
                prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_NebulaRelay/Prefabs/AsteroidHazard.prefab");
#endif
            }

            if (prefab == null) return;

            List<ObstacleSpawnInfo> obstacles = MissionObstacleGenerator.GetObstaclesForMission(missionIndex, diff);

            GameObject container = new GameObject("SpawnedObstacles");
            if (parent != null) container.transform.SetParent(parent, false);

            foreach (var info in obstacles)
            {
                GameObject obj = Object.Instantiate(prefab, info.Position, Quaternion.identity, container.transform);
                AsteroidHazard hazard = obj.GetComponent<AsteroidHazard>();
                if (hazard != null)
                {
                    hazard.ScaleMultiplier = info.ScaleMultiplier;
                    ConfigureHazardProperties(hazard, info);
                }
            }
        }

#if UNITY_EDITOR
        public static GameObject BuildSceneObstacles(int missionIndex, GameObject prefab)
        {
            if (prefab == null) return null;

            MissionObstacleGenerator.GetTierCounts(missionIndex, out int easyTarget, out int normalTarget, out int hardTarget, out int expertTarget);
            List<ObstacleSpawnInfo> candidates = MissionObstacleGenerator.GenerateAllCandidatesForMission(missionIndex);

            GameObject obsRoot = new GameObject("Obstacles");

            GameObject tier1 = new GameObject("Obstacles_Tier1_Easy");
            tier1.transform.SetParent(obsRoot.transform, false);

            GameObject tier2 = new GameObject("Obstacles_Tier2_Normal");
            tier2.transform.SetParent(obsRoot.transform, false);

            GameObject tier3 = new GameObject("Obstacles_Tier3_Hard");
            tier3.transform.SetParent(obsRoot.transform, false);

            GameObject tier4 = new GameObject("Obstacles_Tier4_Expert");
            tier4.transform.SetParent(obsRoot.transform, false);

            for (int i = 0; i < candidates.Count; i++)
            {
                ObstacleSpawnInfo info = candidates[i];
                GameObject parentTier = (i < easyTarget) ? tier1
                                      : (i < normalTarget) ? tier2
                                      : (i < hardTarget) ? tier3
                                      : tier4;

                GameObject obj = (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(prefab, parentTier.transform);
                obj.transform.position = info.Position;
                obj.name = $"Asteroid_{(info.IsMoving ? "Moving" : "Static")}_{i + 1:D2}";

                AsteroidHazard hazard = obj.GetComponent<AsteroidHazard>();
                if (hazard != null)
                {
                    UnityEditor.SerializedObject so = new UnityEditor.SerializedObject(hazard);
                    so.FindProperty("scaleMultiplier").floatValue = info.ScaleMultiplier;
                    so.FindProperty("rotationSpeed").floatValue = info.RotationSpeed;
                    so.FindProperty("patrolOffset").vector2Value = info.PatrolOffset;
                    so.FindProperty("patrolSpeed").floatValue = info.PatrolSpeed;
                    so.ApplyModifiedProperties();
                }
            }

            // Normal is the default difficulty: show tier1 and tier2 in Editor, hide hard & expert
            tier1.SetActive(true);
            tier2.SetActive(true);
            tier3.SetActive(false);
            tier4.SetActive(false);

            return obsRoot;
        }
#endif

        private static void ConfigureHazardProperties(AsteroidHazard hazard, ObstacleSpawnInfo info)
        {
            var rs = hazard.GetType().GetField("rotationSpeed", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            rs?.SetValue(hazard, info.RotationSpeed);
            var po = hazard.GetType().GetField("patrolOffset", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            po?.SetValue(hazard, info.PatrolOffset);
            var ps = hazard.GetType().GetField("patrolSpeed", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            ps?.SetValue(hazard, info.PatrolSpeed);
        }
    }
}
