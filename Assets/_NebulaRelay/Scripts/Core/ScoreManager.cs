using System;
using System.Collections.Generic;
using UnityEngine;
using NebulaRelay.Effects;

namespace NebulaRelay.Core
{
    public struct FloatingScore
    {
        public Vector3 WorldPosition;
        public int Points;
        public float SpawnTime;
        public float Duration;
    }

    /// <summary>
    /// ScoreManager handles mission scoring, floating +100 point popups, smooth score animation,
    /// and final mission score breakdowns (relays, mission completion bonus, normalized time bonus, capsule bonus).
    /// </summary>
    public class ScoreManager : MonoBehaviour
    {
        public static ScoreManager Instance { get; private set; }

        public const int PointsPerRelay = 100;
        public const int MissionCompletionBonus = 250;
        public const int PointsPerRemainingSecond = 5;
        public const int PointsPerRemainingCapsule = 150;
        public const float ReferenceOldMissionDuration = 120.0f; // Old reference duration for time bonus normalization

        [Header("State")]
        [SerializeField] private int currentScore = 0;
        private float animatedScore = 0f;
        private int baseRelayScore = 0;
        private int timeBonus = 0;
        private int capsuleBonus = 0;
        private int completionBonus = 0;
        private int difficultyBonus = 0;
        private float scoreMultiplier = 1.0f;

        private readonly List<FloatingScore> floatingScores = new List<FloatingScore>();
        private readonly HashSet<int> scoredRelayIds = new HashSet<int>();

        public int CurrentScore => currentScore;
        public int DisplayScore => Mathf.RoundToInt(animatedScore);
        public int BaseRelayScore => baseRelayScore;
        public int TimeBonus => timeBonus;
        public int CapsuleBonus => capsuleBonus;
        public int CompletionBonus => completionBonus;
        public int DifficultyBonus => difficultyBonus;
        public float ScoreMultiplier => scoreMultiplier;
        public int SubtotalScore => baseRelayScore + completionBonus + timeBonus + capsuleBonus;
        public int TotalScore => currentScore;
        public IReadOnlyList<FloatingScore> FloatingScores => floatingScores;

        public event Action<int> OnScoreChanged;

        private void Awake()
        {
            Instance = this;
            ResetScore();
        }

        private void Update()
        {
            // Smoothly lerp display score towards current target score
            if (Mathf.Abs(animatedScore - currentScore) > 0.05f)
            {
                animatedScore = Mathf.MoveTowards(animatedScore, currentScore, Time.deltaTime * Mathf.Max(60f, Mathf.Abs(currentScore - animatedScore) * 4f));
            }
            else
            {
                animatedScore = currentScore;
            }

            // Cleanup expired floating scores (1.6s duration)
            float now = Time.time;
            floatingScores.RemoveAll(f => (now - f.SpawnTime) >= f.Duration);
        }

        public void ResetScore()
        {
            currentScore = 0;
            animatedScore = 0f;
            baseRelayScore = 0;
            timeBonus = 0;
            capsuleBonus = 0;
            completionBonus = 0;
            difficultyBonus = 0;
            scoreMultiplier = 1.0f;
            floatingScores.Clear();
            scoredRelayIds.Clear();
            OnScoreChanged?.Invoke(currentScore);
        }

        /// <summary>
        /// Awards 100 points for a relay if not already scored.
        /// Spawns a floating score popup and plays score reward audio.
        /// </summary>
        public bool AwardRelayPoints(int relayInstanceId, Vector3 relayWorldPosition, int points = PointsPerRelay)
        {
            if (scoredRelayIds.Contains(relayInstanceId))
            {
                return false; // Prevent duplicate scoring
            }

            scoredRelayIds.Add(relayInstanceId);
            baseRelayScore += points;
            currentScore += points;

            floatingScores.Add(new FloatingScore
            {
                WorldPosition = relayWorldPosition + Vector3.up * 0.4f,
                Points = points,
                SpawnTime = Time.time,
                Duration = 1.6f
            });

            AudioManager.Instance?.PlayScoreReward();
            OnScoreChanged?.Invoke(currentScore);
            return true;
        }

        /// <summary>
        /// Calculates final mission completion score including normalized time and capsule efficiency bonuses
        /// plus difficulty multiplier bonuses (e.g. 1.5x on Hard, 2.0x on Expert).
        /// </summary>
        public void FinalizeMissionScore(float remainingSeconds, int remainingCapsules)
        {
            completionBonus = MissionCompletionBonus;

            // Normalized time bonus based on fraction of mission timer remaining
            float totalDuration = LevelManager.Instance != null 
                ? LevelManager.Instance.MissionDuration 
                : ReferenceOldMissionDuration;

            float fractionRemaining = Mathf.Clamp01(remainingSeconds / Mathf.Max(1.0f, totalDuration));
            float maxTimeBonus = ReferenceOldMissionDuration * PointsPerRemainingSecond; // 600 points
            timeBonus = Mathf.Max(0, Mathf.RoundToInt(fractionRemaining * maxTimeBonus));

            capsuleBonus = Mathf.Max(0, remainingCapsules * PointsPerRemainingCapsule);

            int subtotal = baseRelayScore + completionBonus + timeBonus + capsuleBonus;
            scoreMultiplier = SettingsManager.GetScoreMultiplier();
            difficultyBonus = Mathf.RoundToInt(subtotal * (scoreMultiplier - 1.0f));

            currentScore = subtotal + difficultyBonus;
            animatedScore = currentScore; // Immediately set for results screen
            OnScoreChanged?.Invoke(currentScore);
        }
    }
}
