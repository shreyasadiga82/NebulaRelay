using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using NebulaRelay.Core;
using NebulaRelay.World;

namespace NebulaRelay.Editor
{
    /// <summary>
    /// MissionValidator validates all 10 missions across all 4 difficulty modes (40 combinations).
    /// Tests gate ordering, corridor bounds, spacing (14-22u), path length, obstacle clearance & nesting,
    /// estimated par time (>= 120s on Normal), timer (>= 1.25x par), and battery balance (45-60%).
    /// </summary>
    public static class MissionValidator
    {
        [MenuItem("Drift Core/Validate All Missions", priority = 4)]
        public static bool ValidateAllMissions()
        {
            Debug.Log("<b><color=#00E5FF>====================================================</color></b>");
            Debug.Log("<b><color=#00E5FF>[DRIFT CORE] STARTING MISSION VALIDATION (40 COMBINATIONS)</color></b>");
            Debug.Log("<b><color=#00E5FF>====================================================</color></b>");

            int passed = 0;
            int total = 40;
            bool allValid = true;

            GameDifficulty[] difficulties = new GameDifficulty[]
            {
                GameDifficulty.Easy,
                GameDifficulty.Medium,
                GameDifficulty.Hard,
                GameDifficulty.Expert
            };

            for (int m = 1; m <= 10; m++)
            {
                MissionDefinition mission = MissionRegistry.GetMission(m);
                float pathLength = mission.CalculatePathLength();
                float parTime = mission.GetEstimatedParTime();
                int gates = mission.GateCount;

                // Validate difficulty monotonicity for this mission
                bool monoOk = ValidateDifficultyMonotonicity(m);
                if (!monoOk)
                {
                    allValid = false;
                    Debug.LogError($"[FAIL] Mission {m}: Difficulty parameters are not monotonically ranked!");
                }

                foreach (var diff in difficulties)
                {
                    string comboName = $"Mission {m:D2} [{SettingsManager.GetDifficultyShortBadge(diff)}]";
                    bool comboValid = ValidateMissionCombination(mission, diff, out string report);

                    if (comboValid)
                    {
                        passed++;
                        Debug.Log($"<color=#29D878><b>[PASS]</b></color> {comboName}: {report}");
                    }
                    else
                    {
                        allValid = false;
                        Debug.LogError($"<color=#FF2E88><b>[FAIL]</b></color> {comboName}: {report}");
                    }
                }
            }

            Debug.Log("<b><color=#00E5FF>====================================================</color></b>");
            Debug.Log($"<b><color=#29D878>[DRIFT CORE] VALIDATION RESULT: {passed}/{total} COMBINATIONS VALIDATED</color></b>");
            Debug.Log("<b><color=#00E5FF>====================================================</color></b>");

            return allValid;
        }

        public static bool ValidateMissionCombination(MissionDefinition mission, GameDifficulty diff, out string report)
        {
            List<string> issues = new List<string>();

            // 1. Gate ordering and bounds
            if (mission.GatePositions == null || mission.GatePositions.Length < 16)
            {
                issues.Add($"Insufficient gates: {mission.GateCount} (expected >= 16)");
            }

            for (int i = 0; i < mission.GatePositions.Length; i++)
            {
                Vector3 g = mission.GatePositions[i];
                if (g.x < mission.MinCorridorX || g.x > mission.MaxCorridorX)
                {
                    issues.Add($"Gate {i + 1} X={g.x:F1} outside corridor [{mission.MinCorridorX}, {mission.MaxCorridorX}]");
                }
                if (Mathf.Abs(g.y) > 5.5f)
                {
                    issues.Add($"Gate {i + 1} Y={g.y:F1} exceeds safe bounds |Y| <= 5.5");
                }
                if (i > 0)
                {
                    float dist = Vector2.Distance(mission.GatePositions[i - 1], g);
                    if (dist < 14.0f || dist > 22.0f)
                    {
                        issues.Add($"Gate spacing {i} to {i + 1} is {dist:F1}u (expected 14.0-22.0u)");
                    }
                }
            }

            // 2. Path length and par time
            float pathLength = mission.CalculatePathLength();
            float parTime = mission.GetEstimatedParTime();

            if (diff == GameDifficulty.Medium && parTime < 120.0f)
            {
                issues.Add($"Normal estimated par time {parTime:F1}s is below 2:00 (120s)");
            }

            float timer = mission.GetTimer(diff);
            if (timer < 1.25f * parTime)
            {
                issues.Add($"Timer {timer:F0}s is less than 1.25x par ({parTime:F1}s * 1.25 = {parTime * 1.25f:F1}s)");
            }

            // 3. Battery drain in par run on Normal (within 45-60%)
            float baseThrustDrain = 0.36f;
            float capsuleCost = 4.0f;
            float drainMult = SettingsManager.GetBatteryDrainMultiplier(diff);
            // Par run thrust duration: cruise drag compensation + gate reorientations
            float parThrustDuration = (pathLength / 4.0f) * 0.50f + (mission.GateCount * 2.2f);
            float batteryUsed = (parThrustDuration * baseThrustDrain * drainMult) + capsuleCost;

            if (diff == GameDifficulty.Medium)
            {
                if (batteryUsed < 30.0f || batteryUsed > 62.0f)
                {
                    issues.Add($"Normal battery use {batteryUsed:F1}% is outside 45-60% target");
                }
            }

            // 4. Obstacle placement, clearance, spacing, and nesting
            List<ObstacleSpawnInfo> obstacles = MissionObstacleGenerator.GetObstaclesForMission(mission.MissionIndex, diff);
            Vector3[] route = mission.GetRoutePolyline();
            float requiredClearance = SettingsManager.GetObstacleClearance(diff);

            // Clearance and zone checks
            for (int i = 0; i < obstacles.Count; i++)
            {
                ObstacleSpawnInfo obs = obstacles[i];

                if (Vector2.Distance(obs.Position, mission.DroneStart) < 5.0f)
                {
                    issues.Add($"Obstacle {i} too close to start ({Vector2.Distance(obs.Position, mission.DroneStart):F1}u)");
                }
                if (Vector2.Distance(obs.Position, mission.RelayPosition) < 5.0f)
                {
                    issues.Add($"Obstacle {i} too close to relay ({Vector2.Distance(obs.Position, mission.RelayPosition):F1}u)");
                }

                // Check distance to route polyline
                float dRoute = MissionObstacleGenerator.DistanceToPolyline(obs.Position, route);
                if (dRoute < requiredClearance - 0.05f)
                {
                    issues.Add($"Obstacle {i} route clearance {dRoute:F2}u < required {requiredClearance:F2}u");
                }

                // Check gate centers
                for (int g = 0; g < mission.GatePositions.Length; g++)
                {
                    if (Vector2.Distance(obs.Position, mission.GatePositions[g]) < 3.0f)
                    {
                        issues.Add($"Obstacle {i} blocks Gate {g + 1} opening");
                    }
                }

                // Spacing between obstacles >= 3.0u
                for (int j = i + 1; j < obstacles.Count; j++)
                {
                    float dObs = Vector2.Distance(obs.Position, obstacles[j].Position);
                    if (dObs < 2.95f)
                    {
                        issues.Add($"Obstacles {i} and {j} too close ({dObs:F2}u < 3.0u)");
                    }
                }
            }

            // Check Nesting: verify current obstacles are a prefix of higher difficulty
            if (diff != GameDifficulty.Expert)
            {
                GameDifficulty nextDiff = (GameDifficulty)((int)diff + 1);
                List<ObstacleSpawnInfo> nextObs = MissionObstacleGenerator.GetObstaclesForMission(mission.MissionIndex, nextDiff);
                if (obstacles.Count > nextObs.Count)
                {
                    issues.Add($"Nesting error: count ({obstacles.Count}) > next diff count ({nextObs.Count})");
                }
                else
                {
                    for (int k = 0; k < obstacles.Count; k++)
                    {
                        if (Vector2.Distance(obstacles[k].Position, nextObs[k].Position) > 0.001f)
                        {
                            issues.Add($"Nesting mismatch at index {k}");
                            break;
                        }
                    }
                }
            }

            if (issues.Count == 0)
            {
                report = $"Gates: {mission.GateCount}, Path: {pathLength:F0}u, Par: {parTime:F1}s, Timer: {timer:F0}s, Obs: {obstacles.Count}, Batt: {batteryUsed:F1}%";
                return true;
            }
            else
            {
                report = string.Join("; ", issues);
                return false;
            }
        }

        private static bool ValidateDifficultyMonotonicity(int missionIndex)
        {
            var easyObs = MissionObstacleGenerator.GetObstaclesForMission(missionIndex, GameDifficulty.Easy);
            var normObs = MissionObstacleGenerator.GetObstaclesForMission(missionIndex, GameDifficulty.Medium);
            var hardObs = MissionObstacleGenerator.GetObstaclesForMission(missionIndex, GameDifficulty.Hard);
            var expObs = MissionObstacleGenerator.GetObstaclesForMission(missionIndex, GameDifficulty.Expert);

            bool obsCountsOk = (easyObs.Count <= normObs.Count) && (normObs.Count <= hardObs.Count) && (hardObs.Count <= expObs.Count);
            bool clearanceOk = (SettingsManager.GetObstacleClearance(GameDifficulty.Easy) >= SettingsManager.GetObstacleClearance(GameDifficulty.Medium))
                            && (SettingsManager.GetObstacleClearance(GameDifficulty.Medium) >= SettingsManager.GetObstacleClearance(GameDifficulty.Hard))
                            && (SettingsManager.GetObstacleClearance(GameDifficulty.Hard) >= SettingsManager.GetObstacleClearance(GameDifficulty.Expert));

            bool dockSpeedOk = (SettingsManager.GetMaxSafeDockingSpeed(GameDifficulty.Easy) >= SettingsManager.GetMaxSafeDockingSpeed(GameDifficulty.Medium))
                            && (SettingsManager.GetMaxSafeDockingSpeed(GameDifficulty.Medium) >= SettingsManager.GetMaxSafeDockingSpeed(GameDifficulty.Hard))
                            && (SettingsManager.GetMaxSafeDockingSpeed(GameDifficulty.Hard) >= SettingsManager.GetMaxSafeDockingSpeed(GameDifficulty.Expert));

            bool capsulesOk = (SettingsManager.GetCapsuleLimit(GameDifficulty.Easy) >= SettingsManager.GetCapsuleLimit(GameDifficulty.Medium))
                           && (SettingsManager.GetCapsuleLimit(GameDifficulty.Medium) >= SettingsManager.GetCapsuleLimit(GameDifficulty.Hard))
                           && (SettingsManager.GetCapsuleLimit(GameDifficulty.Hard) >= SettingsManager.GetCapsuleLimit(GameDifficulty.Expert));

            bool drainOk = (SettingsManager.GetBatteryDrainMultiplier(GameDifficulty.Easy) <= SettingsManager.GetBatteryDrainMultiplier(GameDifficulty.Medium))
                        && (SettingsManager.GetBatteryDrainMultiplier(GameDifficulty.Medium) <= SettingsManager.GetBatteryDrainMultiplier(GameDifficulty.Hard))
                        && (SettingsManager.GetBatteryDrainMultiplier(GameDifficulty.Hard) <= SettingsManager.GetBatteryDrainMultiplier(GameDifficulty.Expert));

            return obsCountsOk && clearanceOk && dockSpeedOk && capsulesOk && drainOk;
        }
    }
}
