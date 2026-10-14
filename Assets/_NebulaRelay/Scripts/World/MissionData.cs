using System;
using System.Collections.Generic;
using UnityEngine;
using NebulaRelay.Core;

namespace NebulaRelay.World
{
    public struct ObstacleSpawnInfo
    {
        public Vector3 Position;
        public float ScaleMultiplier;
        public float RotationSpeed;
        public Vector2 PatrolOffset;
        public float PatrolSpeed;
        public bool IsMoving => PatrolOffset.sqrMagnitude > 0.01f;
    }

    /// <summary>
    /// MissionDefinition stores the geometric route, parameters, and metadata for each of the 10 missions.
    /// Provides deterministic generation of gates, flight corridors, and par time estimates.
    /// </summary>
    public class MissionDefinition
    {
        public int MissionIndex;
        public string Title;
        public string ShortDescription;
        public string FullBriefing;
        public int SectorThemeIndex; // 1 to 5 cycling nebula & planet themes
        public bool HasPlanet;
        public int PlanetThemeIndex; // 1 or 4
        public float MinCorridorX;
        public float MaxCorridorX;
        public float MarginX = 4.0f;
        public float CorridorMarginY = 6.5f;

        public Vector3 DroneStart;
        public Vector3[] GatePositions;
        public Vector3 RelayPosition;

        public int GateCount => GatePositions != null ? GatePositions.Length : 0;

        public Vector3[] GetRoutePolyline()
        {
            int total = 1 + GateCount + 1;
            Vector3[] route = new Vector3[total];
            route[0] = DroneStart;
            for (int i = 0; i < GateCount; i++)
            {
                route[i + 1] = GatePositions[i];
            }
            route[total - 1] = RelayPosition;
            return route;
        }

        public float CalculatePathLength()
        {
            Vector3[] route = GetRoutePolyline();
            float length = 0f;
            for (int i = 0; i < route.Length - 1; i++)
            {
                length += Vector2.Distance(route[i], route[i + 1]);
            }
            return length;
        }

        public float GetEstimatedParTime()
        {
            float pathLength = CalculatePathLength();
            return (pathLength / 4.0f) + 15.0f + (GateCount * 1.5f);
        }

        public float GetTimer(GameDifficulty diff)
        {
            return SettingsManager.GetMissionDuration(diff, MissionIndex);
        }
    }

    /// <summary>
    /// MissionRegistry holds the authoritative mission layouts for all 10 missions.
    /// </summary>
    public static class MissionRegistry
    {
        private static readonly MissionDefinition[] missions = new MissionDefinition[10];

        static MissionRegistry()
        {
            InitializeMissions();
        }

        public static MissionDefinition GetMission(int index)
        {
            int i = Mathf.Clamp(index, 1, 10) - 1;
            if (missions[i] == null) InitializeMissions();
            return missions[i];
        }

        public static IReadOnlyList<MissionDefinition> GetAllMissions()
        {
            if (missions[0] == null) InitializeMissions();
            return missions;
        }

        private static void InitializeMissions()
        {
            // 10 Missions with dynamic slaloms, chicanes, and undulating waves (NOT in a straight line)
            // Rings alternate and weave significantly between |Y| <= 4.4, forcing active rocket steering
            // Gate spacing strictly between 18.2u and 19.8u (within 14-22u rule)

            // Mission 1: First Signal (16 rings - Slalom Weave)
            missions[0] = CreateMission(
                1, "First Signal",
                "Orbital slalom corridor. 16 compulsory relays. 2 capsules.",
                "Sector 1 presents an alternating slalom corridor through the Outer Beacon Reach. Maneuver up and down across all 16 compulsory relays, fire an energy capsule into the station receiver, and dock at safe speed (≤ 1.5 u/s).",
                1, true, 1, 16, -154.7f, 154.7f
            );

            // Mission 2: The Debris Field (18 rings - Paired Chicanes)
            missions[1] = CreateMission(
                2, "The Debris Field",
                "Asteroid chicanes. Tight switchbacks. 18 compulsory relays.",
                "Sector 2 cuts through an active asteroid field via paired chicane switchbacks. Avoid collision impacts that drain battery shields, navigate 18 compulsory relays, deliver the power capsule, and dock safely.",
                2, false, 0, 18, -172.9f, 172.9f
            );

            // Mission 3: The Ion Storm (20 rings - High-Energy Undulation)
            missions[2] = CreateMission(
                3, "The Ion Storm",
                "Turbulent moving hazards. 20 compulsory relays.",
                "High-energy ion turbulence creates moving hazard oscillation along the flight path. Thread through all 20 undulating relays under dynamic cross-drift to activate the third relay station.",
                3, false, 0, 20, -191.1f, 191.1f
            );

            // Mission 4: The Gravity Well (22 rings - Gravitational Rollercoaster)
            missions[3] = CreateMission(
                4, "The Gravity Well",
                "Gravitational rollercoaster. 22 compulsory relays.",
                "Golden gravitational anomalies cluster space debris into sweeping rollercoaster paths. Precision thrust and momentum management are essential across 22 compulsory relays.",
                4, true, 4, 22, -209.3f, 209.3f
            );

            // Mission 5: The Omega Transmission (24 rings - Omega Slalom Nexus)
            missions[4] = CreateMission(
                5, "The Omega Transmission",
                "Omega slalom nexus. 24 compulsory relays.",
                "A core galactic communication nexus. Navigate 24 sharp slalom relays across dense asteroid gauntlets and deliver the primary relay capsule.",
                5, false, 0, 24, -227.5f, 227.5f
            );

            // Mission 6: The Silent Array (26 rings - Silent Array Chicanes)
            missions[5] = CreateMission(
                6, "The Silent Array",
                "Dormant sensor chicanes. 26 compulsory relays.",
                "Sector 6 traverses a vast decommissioned sensor grid with alternating elevation chicanes. Re-establish contact across 26 sequential relays while dodging drifting metallic debris.",
                1, true, 1, 26, -245.7f, 245.7f
            );

            // Mission 7: The Pulsar Reach (28 rings - Rapid Pulsar Slalom)
            missions[6] = CreateMission(
                7, "The Pulsar Reach",
                "Rapid pulsar slalom. 28 compulsory relays.",
                "A turbulent radiation corridor buffeted by distant pulsar emissions. Guide your drone through 28 synchronized slalom gates under tightening obstacle corridors.",
                2, false, 0, 28, -263.9f, 263.9f
            );

            // Mission 8: The Shattered Moon (30 rings - Shattered Moon Wave)
            missions[7] = CreateMission(
                8, "The Shattered Moon",
                "Lunar fragment wave gauntlet. 30 compulsory relays.",
                "Navigate the shattered orbital ring of a fractured moon. Thread 30 undulating wave relays past dense asteroid clusters and moving debris corridors.",
                3, false, 0, 30, -282.1f, 282.1f
            );

            // Mission 9: The Event Horizon (32 rings - Event Horizon Gauntlet)
            missions[8] = CreateMission(
                9, "The Event Horizon",
                "High gravitational drift gauntlet. 32 compulsory relays.",
                "Approach the outer ergosphere of a gravitational singularity. Extreme drift control is necessary across 32 compulsory relays through deep switchback curves.",
                4, true, 4, 32, -293.7f, 293.7f
            );

            // Mission 10: The Last Signal (34 rings - Grand Finale Slalom)
            missions[9] = CreateMission(
                10, "The Last Signal",
                "Final network nexus. 34 compulsory relays. Grand finale.",
                "The central core of the Galactic Relay Network. Synchronize all 34 compulsory relays through the ultimate gauntlet of precision slaloms to restore deep space communications across the entire galaxy.",
                5, false, 0, 34, -306.25f, 306.25f
            );
        }

        private static float ComputeGateY(int missionIndex, int ringIndex, int ringCount)
        {
            switch (missionIndex)
            {
                case 1:
                    // Mission 1: The Outer Beacon Slalom (alternating high/low slalom)
                    float s1 = (ringIndex % 2 == 0) ? 1f : -1f;
                    float m1 = 0.3f * Mathf.Sin(ringIndex * 0.8f);
                    return s1 * (3.1f + m1);

                case 2:
                    // Mission 2: Debris Chicanes (paired switchbacks: ++ -- ++ --)
                    float[] p2 = { 2.6f, 3.5f, -2.6f, -3.5f };
                    return p2[ringIndex % 4];

                case 3:
                    // Mission 3: Ion Undulation (fast undulating harmonic wave)
                    return 3.6f * Mathf.Sin((ringIndex + 1) * 1.25f);

                case 4:
                    // Mission 4: Gravity Rollercoaster (compound wave with peaks and troughs)
                    return 3.2f * Mathf.Sin((ringIndex + 1) * 0.95f) + 0.8f * Mathf.Cos((ringIndex + 1) * 1.9f);

                case 5:
                    // Mission 5: Omega Slalom (alternating sharp slalom with center dips)
                    float[] p5 = { 3.4f, -1.8f, -3.4f, 1.8f };
                    return p5[ringIndex % 4];

                case 6:
                    // Mission 6: Silent Array Chicanes
                    float[] p6 = { 3.5f, 1.5f, -3.5f, -1.5f };
                    return p6[ringIndex % 4];

                case 7:
                    // Mission 7: Pulsar Rapid Slalom
                    float s7 = (ringIndex % 2 == 0) ? 1f : -1f;
                    return s7 * (3.4f + 0.4f * Mathf.Cos(ringIndex * 0.7f));

                case 8:
                    // Mission 8: Shattered Moon Wave
                    return 3.8f * Mathf.Sin((ringIndex + 1) * 1.1f) + 0.4f * Mathf.Sin((ringIndex + 1) * 2.3f);

                case 9:
                    // Mission 9: Event Horizon Gauntlet
                    float[] p9 = { 3.6f, -3.2f, 2.4f, -3.6f, 3.2f, -2.4f };
                    return p9[ringIndex % 6];

                case 10:
                default:
                    // Mission 10: Grand Finale Slalom
                    float[] p10 = { 3.8f, -3.8f, 2.8f, -2.8f, 3.5f, -3.5f };
                    return p10[ringIndex % 6];
            }
        }

        private static MissionDefinition CreateMission(
            int index, string title, string shortDesc, string fullBriefing,
            int theme, bool hasPlanet, int planetTheme,
            int ringCount, float minX, float maxX)
        {
            MissionDefinition def = new MissionDefinition
            {
                MissionIndex = index,
                Title = title,
                ShortDescription = shortDesc,
                FullBriefing = fullBriefing,
                SectorThemeIndex = theme,
                HasPlanet = hasPlanet,
                PlanetThemeIndex = planetTheme,
                MinCorridorX = minX,
                MaxCorridorX = maxX,
                MarginX = 4.0f,
                CorridorMarginY = 6.5f,
                DroneStart = new Vector3(minX, 0f, 0f),
                RelayPosition = new Vector3(maxX, 0f, 0f),
                GatePositions = new Vector3[ringCount]
            };

            // Compute gate positions with pronounced non-linear weave
            float totalSpan = maxX - minX;
            float step = totalSpan / (ringCount + 1);

            for (int i = 0; i < ringCount; i++)
            {
                float x = minX + step * (i + 1);
                float y = Mathf.Clamp(ComputeGateY(index, i, ringCount), -4.4f, 4.4f);
                def.GatePositions[i] = new Vector3(x, y, 0f);
            }

            return def;
        }
    }

    /// <summary>
    /// Deterministic seeded obstacle generator that produces nested candidate lists per mission.
    /// Easy is a subset of Normal, which is a subset of Hard, which is a subset of Expert.
    /// Strictly respects clearance from route polyline, gate openings, start, receiver, and relay.
    /// </summary>
    public static class MissionObstacleGenerator
    {
        private const int BaseSeed = 71828;

        public static List<ObstacleSpawnInfo> GetObstaclesForMission(int missionIndex, GameDifficulty diff)
        {
            List<ObstacleSpawnInfo> allCandidates = GenerateAllCandidatesForMission(missionIndex);
            MissionDefinition mission = MissionRegistry.GetMission(missionIndex);
            float pathLength = mission.CalculatePathLength();

            int countPer100 = SettingsManager.GetObstaclesPer100u(diff);
            int targetCount = Mathf.Max(2, Mathf.RoundToInt((pathLength / 100.0f) * countPer100));
            int actualCount = Mathf.Min(targetCount, allCandidates.Count);

            return allCandidates.GetRange(0, actualCount);
        }

        public static void GetTierCounts(int missionIndex, out int easyCount, out int normalCount, out int hardCount, out int expertCount)
        {
            MissionDefinition mission = MissionRegistry.GetMission(missionIndex);
            float pathLength = mission.CalculatePathLength();

            easyCount = Mathf.Max(2, Mathf.RoundToInt((pathLength / 100.0f) * SettingsManager.GetObstaclesPer100u(GameDifficulty.Easy)));
            normalCount = Mathf.Max(3, Mathf.RoundToInt((pathLength / 100.0f) * SettingsManager.GetObstaclesPer100u(GameDifficulty.Medium)));
            hardCount = Mathf.Max(4, Mathf.RoundToInt((pathLength / 100.0f) * SettingsManager.GetObstaclesPer100u(GameDifficulty.Hard)));
            expertCount = Mathf.Max(5, Mathf.RoundToInt((pathLength / 100.0f) * SettingsManager.GetObstaclesPer100u(GameDifficulty.Expert)));
        }

        public static List<ObstacleSpawnInfo> GenerateAllCandidatesForMission(int missionIndex)
        {
            GetTierCounts(missionIndex, out int easyTarget, out int normalTarget, out int hardTarget, out int expertTarget);

            MissionDefinition mission = MissionRegistry.GetMission(missionIndex);
            Vector3[] route = mission.GetRoutePolyline();

            List<ObstacleSpawnInfo> candidates = new List<ObstacleSpawnInfo>(expertTarget);

            // Deterministic PRNG seeded by mission index
            int seed = BaseSeed + missionIndex * 1013;
            System.Random rng = new System.Random(seed);

            // Clearances required per tier:
            // Tier 1 (Easy): clearance >= 3.2u
            // Tier 2 (Normal): clearance >= 2.6u
            // Tier 3 (Hard): clearance >= 2.2u
            // Tier 4 (Expert): clearance >= 1.9u
            GenerateTier(candidates, easyTarget, 3.2f, mission, route, rng, missionIndex, 1);
            GenerateTier(candidates, normalTarget, 2.6f, mission, route, rng, missionIndex, 2);
            GenerateTier(candidates, hardTarget, 2.2f, mission, route, rng, missionIndex, 3);
            GenerateTier(candidates, expertTarget, 1.9f, mission, route, rng, missionIndex, 4);

            return candidates;
        }

        private static void GenerateTier(
            List<ObstacleSpawnInfo> list, int targetTotal, float minClearance,
            MissionDefinition mission, Vector3[] route, System.Random rng, int missionIndex, int tier)
        {
            float minX = mission.MinCorridorX + 10f;
            float maxX = mission.MaxCorridorX - 10f;
            int maxAttempts = 3000;
            int attempts = 0;
            float side = 1.0f; // Alternates between top flank (+1) and bottom flank (-1) to guarantee balanced obstacles

            while (list.Count < targetTotal && attempts < maxAttempts)
            {
                attempts++;
                float x = (float)(minX + rng.NextDouble() * (maxX - minX));

                // Balanced flank distribution: alternate top (Y in [1.5, 5.0]) and bottom (Y in [-5.0, -1.5])
                // Obstacles flank both sides of the alternating slalom corridor, blocking straight-line coasting
                float y = (side > 0f)
                    ? (float)(1.5 + rng.NextDouble() * 3.5)
                    : (float)(-5.0 + rng.NextDouble() * 3.5);
                side = -side;

                Vector3 pos = new Vector3(x, y, 0f);

                // 1. Spacing from start, receiver, and relay >= 6.0u
                if (Vector2.Distance(pos, mission.DroneStart) < 6.5f) continue;
                if (Vector2.Distance(pos, mission.RelayPosition) < 6.5f) continue;

                // 2. Clearance from all gate centers >= 3.5u (ring diameter is 2.4u; opening strictly open)
                bool nearGate = false;
                for (int g = 0; g < mission.GatePositions.Length; g++)
                {
                    if (Vector2.Distance(pos, mission.GatePositions[g]) < 3.5f)
                    {
                        nearGate = true;
                        break;
                    }
                }
                if (nearGate) continue;

                // 3. Distance from route polyline >= minClearance
                float distToPath = DistanceToPolyline(pos, route);
                if (distToPath < minClearance) continue;

                // 4. Mutual spacing between obstacles >= 3.0u
                bool tooCloseToOther = false;
                for (int o = 0; o < list.Count; o++)
                {
                    if (Vector2.Distance(pos, list[o].Position) < 3.0f)
                    {
                        tooCloseToOther = true;
                        break;
                    }
                }
                if (tooCloseToOther) continue;

                // 5. Moving hazards across all missions:
                // M1-M2: 28% moving hazards; M3-M10: 35% to 60% moving hazards
                float movingChance = (missionIndex <= 2) ? 0.28f : Mathf.Min(0.60f, 0.32f + missionIndex * 0.035f);
                bool isMoving = rng.NextDouble() < movingChance;
                Vector2 patrol = Vector2.zero;
                float patrolSpd = 1.0f;

                if (isMoving)
                {
                    float amp = 1.2f + (float)rng.NextDouble() * 1.0f; // 1.2 to 2.2 amplitude
                    patrolSpd = 0.8f + (float)rng.NextDouble() * 0.7f;

                    // 75% vertical patrol, 25% horizontal patrol
                    bool isVertical = rng.NextDouble() < 0.75;
                    Vector3 testOffset = isVertical ? new Vector3(0f, amp, 0f) : new Vector3(amp, 0f, 0f);

                    Vector3 p1 = pos + testOffset;
                    Vector3 p2 = pos - testOffset;

                    if (Mathf.Abs(p1.y) <= 5.2f && Mathf.Abs(p2.y) <= 5.2f &&
                        p1.x > (mission.MinCorridorX + 6f) && p2.x < (mission.MaxCorridorX - 6f) &&
                        DistanceToPolyline(p1, route) >= (minClearance - 0.25f) &&
                        DistanceToPolyline(p2, route) >= (minClearance - 0.25f))
                    {
                        patrol = isVertical ? new Vector2(0f, amp) : new Vector2(amp, 0f);
                    }
                }

                // Asteroid scale multiplier between 0.65x and 0.95x (~0.95u to 1.35u diameter)
                float scaleMult = 0.68f + (float)rng.NextDouble() * 0.24f;
                float rotSpeed = (float)(-25.0 + rng.NextDouble() * 50.0);
                if (Mathf.Abs(rotSpeed) < 8.0f) rotSpeed = 12.0f;

                list.Add(new ObstacleSpawnInfo
                {
                    Position = pos,
                    ScaleMultiplier = scaleMult,
                    RotationSpeed = rotSpeed,
                    PatrolOffset = patrol,
                    PatrolSpeed = patrolSpd
                });
            }
        }

        public static float DistanceToPolyline(Vector2 point, Vector3[] polyline)
        {
            float minSqDist = float.MaxValue;
            for (int i = 0; i < polyline.Length - 1; i++)
            {
                float d = DistanceToSegment(point, polyline[i], polyline[i + 1]);
                if (d < minSqDist) minSqDist = d;
            }
            return minSqDist;
        }

        private static float DistanceToSegment(Vector2 p, Vector2 a, Vector2 b)
        {
            Vector2 ab = b - a;
            float l2 = ab.sqrMagnitude;
            if (l2 < 0.0001f) return Vector2.Distance(p, a);

            float t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / l2);
            Vector2 projection = a + t * ab;
            return Vector2.Distance(p, projection);
        }
    }
}
