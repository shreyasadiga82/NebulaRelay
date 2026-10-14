using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using NebulaRelay.Core;
using NebulaRelay.Player;
using NebulaRelay.Delivery;
using NebulaRelay.World;
using NebulaRelay.Effects;

namespace NebulaRelay.UI
{
    /// <summary>
    /// HUDController delivers the polished sci-fi HUD and modal system:
    /// - Top-Left: Mission Card (Line 1: single-line title; Line 2: score + difficulty badge; Line 3: gate progress bar).
    /// - Top-Centre: Objective Banner with smooth fade transitions.
    /// - Top-Right: Large mm:ss Timer (amber < 60s, red pulse < 30s) + Pause button.
    /// - Bottom Telemetry Dock: Segmented Battery (>=260x26), Velocity + u/s + dock limit tick, Capsules in magenta, Next Target.
    /// - Centred Modals: Fullscreen dimming overlay, HUD hidden behind modals, 2-column stats table, count-up score animation.
    /// Zero per-frame allocations in OnGUI.
    /// </summary>
    [SelectionBase]
    public class HUDController : MonoBehaviour
    {
        public static HUDController Instance { get; private set; }

        private bool isPaused = false;
        private bool showSettings = false;
        private float viewWidth = 1280f;
        private float viewHeight = 720f;

        // Banner text caching & fade transitions
        private string cachedObjective = "";
        private string lastObjective = "";
        private float objectiveAlpha = 1f;

        // Smooth animated telemetry values
        private float displayBattery = 100f;
        private float animatedResultScore = 0f;
        private float resultAnimationStartTime = -1f;

        private void Awake()
        {
            Instance = this;
            UIDesignSystem.EnsureInitialized();
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public static bool IsPointerOverUI(Vector2 mouseScreenPos)
        {
            if (Instance == null) return false;
            if (Instance.isPaused) return true;

            var lm = LevelManager.Instance;
            if (lm != null && (lm.State == LevelState.Completed || lm.State == LevelState.Failed))
            {
                return true;
            }

            float scale = UIScale.GetScale();
            if (scale < 0.01f) return false;

            float guiY = Screen.height - mouseScreenPos.y;
            float refY = guiY / scale;

            // Top HUD area: y in 0..140 reference units
            if (refY <= 140f) return true;

            // Bottom telemetry dock area: y in (viewHeight - 110)..viewHeight reference units
            float viewH = UIScale.GetViewHeight();
            if (refY >= viewH - 110f) return true;

            return false;
        }

        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                TogglePause();
            }

            // Smooth battery display animation
            LevelManager lm = LevelManager.Instance;
            if (lm != null && lm.Battery != null)
            {
                float target = lm.Battery.CurrentBattery;
                displayBattery = Mathf.MoveTowards(displayBattery, target, Time.deltaTime * 60f);
            }

            // Objective text transition fade
            if (lm != null)
            {
                string curr = lm.CurrentObjectiveText;
                if (curr != cachedObjective)
                {
                    lastObjective = cachedObjective;
                    cachedObjective = curr;
                    objectiveAlpha = 0f;
                }
                if (objectiveAlpha < 1f)
                {
                    objectiveAlpha = Mathf.MoveTowards(objectiveAlpha, 1f, Time.deltaTime * 4f);
                }
            }
        }

        public void TogglePause()
        {
            isPaused = !isPaused;
            showSettings = false;
            Time.timeScale = isPaused ? 0f : 1f;
            if (isPaused) AudioManager.Instance?.PlayPauseSound();
            else AudioManager.Instance?.PlayResumeSound();
        }

        private void OnGUI()
        {
            UIDesignSystem.EnsureInitialized();

            LevelManager lm = LevelManager.Instance;
            if (lm == null) return;

            Matrix4x4 origMatrix = UIScale.BeginMatrix();
            viewWidth = UIScale.GetViewWidth();
            viewHeight = UIScale.GetViewHeight();

            try
            {
                bool isModalActive = isPaused || lm.State == LevelState.Completed || lm.State == LevelState.Failed;

                // When modals are active, HUD is hidden behind the dimming overlay
                if (!isModalActive)
                {
                    DrawTopMissionCard(lm);
                    DrawTopObjectiveBanner(lm);
                    DrawTopRightTimer(lm);
                    DrawFloatingScores();
                    DrawBottomTelemetryDock(lm);
                    DrawAlertBanner(lm);
                }
                else
                {
                    // Full screen 70% dark navy dimming overlay
                    UIDesignSystem.DrawDimScreenOverlay(viewWidth, viewHeight);

                    if (isPaused)
                    {
                        if (showSettings) DrawSettingsModal();
                        else DrawPauseModal(lm);
                    }
                    else if (lm.State == LevelState.Completed)
                    {
                        DrawResultsModal(lm);
                    }
                    else if (lm.State == LevelState.Failed)
                    {
                        DrawFailureModal(lm);
                    }
                }
            }
            finally
            {
                UIScale.EndMatrix(origMatrix);
            }
        }

        // ==========================================
        // 1. TOP-LEFT MISSION CARD
        // Line 1: MISSION 03: THE ION STORM (fits single line, auto-shrink)
        // Line 2: SCORE 1,250 [PILOT x1.0]
        // Line 3: GATE 07 / 24  [=======       ]
        // ==========================================
        private void DrawTopMissionCard(LevelManager lm)
        {
            float cardW = 380f;
            float cardH = 96f;
            float cardX = UIDesignSystem.MarginSafe;
            float cardY = 16f;

            Rect cardRect = new Rect(cardX, cardY, cardW, cardH);
            GUI.Box(cardRect, GUIContent.none, UIDesignSystem.StylePanel);

            float innerX = cardX + 16f;
            float innerW = cardW - 32f;

            // Line 1: Mission title (single line, auto-shrink, no wrapping)
            string missionTitle = $"MISSION {lm.LevelNumber:D2}: {lm.LevelTitle.ToUpper()}";
            Rect rLine1 = new Rect(innerX, cardY + 12f, innerW, 24f);
            UIDesignSystem.DrawFittedTitle(rLine1, missionTitle, UIDesignSystem.StyleTitle, UIDesignSystem.ColorCyan, 20, 14);

            // Line 2: Score + Difficulty badge with multiplier
            ScoreManager sm = ScoreManager.Instance;
            int score = sm != null ? sm.DisplayScore : 0;
            float mult = SettingsManager.GetScoreMultiplier();
            string multTag = mult > 1.0f ? $" x{mult:F1}" : "";
            string badgeText = $"{SettingsManager.GetDifficultyShortBadge(lm.Difficulty)}{multTag}";

            Rect rScore = new Rect(innerX, cardY + 38f, 180f, 22f);
            UIDesignSystem.DrawShadowedLabel(rScore, $"SCORE  {score:N0}", UIDesignSystem.StyleHeader, UIDesignSystem.ColorAmberWarning);

            Color badgeCol = lm.Difficulty == GameDifficulty.Expert ? UIDesignSystem.ColorMagenta
                           : (lm.Difficulty == GameDifficulty.Hard ? UIDesignSystem.ColorRedCritical
                           : (lm.Difficulty == GameDifficulty.Easy ? UIDesignSystem.ColorGreenSafe : UIDesignSystem.ColorCyan));
            Rect rBadge = new Rect(innerX + 190f, cardY + 38f, 130f, 22f);
            UIDesignSystem.DrawBadge(rBadge, badgeText, new Color(0.04f, 0.08f, 0.16f, 0.95f), badgeCol, 14);

            // Line 3: Gate progress & slim progress bar
            int activeGates = lm.Rings != null ? lm.Rings.ActivatedRelaysCount : 0;
            int totalGates = lm.Rings != null ? lm.Rings.TotalRelays : 16;
            float progress = totalGates > 0 ? (float)activeGates / totalGates : 0f;

            Rect rGateText = new Rect(innerX, cardY + 66f, 110f, 18f);
            UIDesignSystem.DrawShadowedLabel(rGateText, $"GATE {activeGates:D2} / {totalGates:D2}", UIDesignSystem.StyleLabelMuted, UIDesignSystem.ColorTextMuted);

            Rect rBar = new Rect(innerX + 115f, cardY + 71f, innerW - 115f, 8f);
            UIDesignSystem.DrawProgressBar(rBar, progress, UIDesignSystem.ColorGreenSafe, new Color(0.1f, 0.2f, 0.35f, 0.8f));
        }

        // ==========================================
        // 2. TOP-CENTRE OBJECTIVE BANNER
        // Single clear objective line, fades smoothly on changes
        // ==========================================
        private void DrawTopObjectiveBanner(LevelManager lm)
        {
            float bannerW = Mathf.Min(480f, viewWidth - 840f);
            if (bannerW < 300f) bannerW = 300f;
            float bannerH = 46f;
            float bannerX = (viewWidth - bannerW) / 2f;
            float bannerY = 16f;

            Rect bannerRect = new Rect(bannerX, bannerY, bannerW, bannerH);
            GUI.Box(bannerRect, GUIContent.none, UIDesignSystem.StylePanel);

            string objText = lm.CurrentObjectiveText;
            Color col = Color.Lerp(new Color(1f, 1f, 1f, 0.2f), UIDesignSystem.ColorTextBright, objectiveAlpha);
            Rect textRect = new Rect(bannerX + 12f, bannerY + 11f, bannerW - 24f, 24f);
            UIDesignSystem.DrawShadowedLabel(textRect, objText, UIDesignSystem.StyleObjectiveBanner, col);
        }

        // ==========================================
        // 3. TOP-RIGHT TIMER & PAUSE
        // Large mm:ss (amber < 60s, red pulse < 30s) + [❚❚] button
        // ==========================================
        private void DrawTopRightTimer(LevelManager lm)
        {
            float boxW = 210f;
            float boxH = 58f;
            float boxX = viewWidth - UIDesignSystem.MarginSafe - boxW;
            float boxY = 16f;

            Rect timerPanel = new Rect(boxX, boxY, boxW, boxH);
            GUI.Box(timerPanel, GUIContent.none, UIDesignSystem.StylePanel);

            float timeSec = lm.RemainingTime;
            int mins = Mathf.FloorToInt(timeSec) / 60;
            int secs = Mathf.FloorToInt(timeSec) % 60;
            string timeStr = $"{mins:D2}:{secs:D2}";

            Color timerColor = UIDesignSystem.ColorCyan;
            if (timeSec <= 30f)
            {
                float pulse = 0.5f + 0.5f * Mathf.Sin(Time.time * 8f);
                timerColor = Color.Lerp(UIDesignSystem.ColorRedCritical, Color.white, pulse * 0.35f);
            }
            else if (timeSec <= 60f)
            {
                timerColor = UIDesignSystem.ColorAmberWarning;
            }

            Rect rTimerText = new Rect(boxX + 18f, boxY + 12f, 110f, 34f);
            UIDesignSystem.DrawShadowedLabel(rTimerText, timeStr, UIDesignSystem.StyleTimerValue, timerColor);

            // Pause Button
            Rect rPauseBtn = new Rect(boxX + 140f, boxY + 9f, 54f, 40f);
            if (GUI.Button(rPauseBtn, "❚❚", UIDesignSystem.StyleButtonSecondary))
            {
                TogglePause();
            }
        }

        // ==========================================
        // 4. FLOATING SCORES (+100 near relays)
        // ==========================================
        private void DrawFloatingScores()
        {
            ScoreManager sm = ScoreManager.Instance;
            if (sm == null || Camera.main == null) return;

            Camera cam = Camera.main;
            float scale = UIScale.GetScale();
            var list = sm.FloatingScores;

            for (int i = 0; i < list.Count; i++)
            {
                var fs = list[i];
                float age = Time.time - fs.SpawnTime;
                Vector3 worldPos = fs.WorldPosition + Vector3.up * (age * 0.8f);
                Vector3 screenPos = cam.WorldToScreenPoint(worldPos);

                if (screenPos.z > 0)
                {
                    float rx = screenPos.x / scale;
                    float ry = (Screen.height - screenPos.y) / scale;
                    float alpha = Mathf.Clamp01(1f - (age / fs.Duration));
                    Color c = new Color(UIDesignSystem.ColorAmberWarning.r, UIDesignSystem.ColorAmberWarning.g, UIDesignSystem.ColorAmberWarning.b, alpha);
                    UIDesignSystem.DrawShadowedLabel(new Rect(rx - 50, ry - 14, 100, 28), $"+{fs.Points}", UIDesignSystem.StyleFloatingScore, c);
                }
            }
        }

        // ==========================================
        // 5. BOTTOM TELEMETRY DOCK (Equal columns, vertical dividers)
        // Col 1: BATTERY segmented bar >= 260x26
        // Col 2: VELOCITY large + u/s + speed bar with dock tick
        // Col 3: CAPSULES pips + "n READY" magenta
        // Col 4: NEXT TARGET icon + target text
        // ==========================================
        private void DrawBottomTelemetryDock(LevelManager lm)
        {
            float dockW = Mathf.Min(1200f, viewWidth - (UIDesignSystem.MarginSafe * 2));
            float dockH = 82f;
            float dockX = (viewWidth - dockW) / 2f;
            float dockY = viewHeight - dockH - 16f;

            Rect dockRect = new Rect(dockX, dockY, dockW, dockH);
            GUI.Box(dockRect, GUIContent.none, UIDesignSystem.StylePanel);

            float colW = (dockW - 48f) / 4f;

            // --- COLUMN 1: BATTERY SHIELD ---
            float c1X = dockX + 16f;
            Rect rC1Label = new Rect(c1X, dockY + 10f, colW - 10f, 18f);
            UIDesignSystem.DrawShadowedLabel(rC1Label, "BATTERY SHIELD", UIDesignSystem.StyleLabelMuted, UIDesignSystem.ColorTextMuted);

            float battPct = lm.Battery != null ? lm.Battery.BatteryPercentage : (displayBattery / 100f);
            Color battColor = battPct > 0.5f ? UIDesignSystem.ColorGreenSafe
                            : (battPct > 0.2f ? UIDesignSystem.ColorAmberWarning : UIDesignSystem.ColorRedCritical);

            // Segmented battery bar (minimum 260x26)
            float barW = Mathf.Max(220f, colW - 65f);
            Rect rBattBar = new Rect(c1X, dockY + 34f, barW, 26f);
            UIDesignSystem.DrawSegmentedBar(rBattBar, battPct, 12, battColor, new Color(0.08f, 0.16f, 0.28f, 0.7f));

            Rect rBattVal = new Rect(c1X + barW + 8f, dockY + 33f, 55f, 26f);
            UIDesignSystem.DrawShadowedLabel(rBattVal, $"{(int)(battPct * 100)}%", UIDesignSystem.StyleValueBold, battColor);

            // Divider 1
            DrawVerticalDivider(c1X + colW, dockY + 12f, dockH - 24f);

            // --- COLUMN 2: VELOCITY ---
            float c2X = c1X + colW + 12f;
            Rect rC2Label = new Rect(c2X, dockY + 10f, colW - 10f, 18f);
            UIDesignSystem.DrawShadowedLabel(rC2Label, "VELOCITY", UIDesignSystem.StyleLabelMuted, UIDesignSystem.ColorTextMuted);

            float speed = lm.Drone != null ? lm.Drone.Speed : 0f;
            float dockLimit = lm.Relay != null ? lm.Relay.MaxSafeSpeed : SettingsManager.GetMaxSafeDockingSpeed();
            Color speedColor = speed <= dockLimit ? UIDesignSystem.ColorGreenSafe : UIDesignSystem.ColorRedCritical;

            Rect rSpeedVal = new Rect(c2X, dockY + 30f, 85f, 28f);
            UIDesignSystem.DrawShadowedLabel(rSpeedVal, $"{speed:F1}", UIDesignSystem.StyleValueBold, speedColor);

            Rect rSpeedUnit = new Rect(c2X + 88f, dockY + 34f, 40f, 22f);
            UIDesignSystem.DrawShadowedLabel(rSpeedUnit, "u/s", UIDesignSystem.StyleLabelMuted, UIDesignSystem.ColorTextMuted);

            // Speedometer bar with safe dock tick
            Rect rSpeedBar = new Rect(c2X + 130f, dockY + 41f, colW - 150f, 10f);
            UIDesignSystem.DrawSpeedometerBar(rSpeedBar, speed, 8.0f, dockLimit);

            // Divider 2
            DrawVerticalDivider(c2X + colW - 12f, dockY + 12f, dockH - 24f);

            // --- COLUMN 3: CAPSULES ---
            float c3X = c2X + colW;
            Rect rC3Label = new Rect(c3X, dockY + 10f, colW - 10f, 18f);
            UIDesignSystem.DrawShadowedLabel(rC3Label, "CAPSULES", UIDesignSystem.StyleLabelMuted, UIDesignSystem.ColorTextMuted);

            int remainingCaps = lm.Launcher != null ? lm.Launcher.RemainingCapsules : 2;
            int maxCaps = lm.Launcher != null ? lm.Launcher.MaxCapsules : 2;

            // Draw capsule icon pips
            float pipX = c3X;
            for (int p = 0; p < maxCaps; p++)
            {
                Color pipColor = (p < remainingCaps) ? UIDesignSystem.ColorMagenta : new Color(0.2f, 0.25f, 0.35f, 0.5f);
                UIDesignSystem.DrawIcon(new Rect(pipX + p * 24f, dockY + 36f, 20f, 20f), ProceduralIcon.Capsule, pipColor);
            }

            Rect rCapsText = new Rect(pipX + maxCaps * 24f + 8f, dockY + 34f, 110f, 24f);
            UIDesignSystem.DrawShadowedLabel(rCapsText, $"{remainingCaps} READY", UIDesignSystem.StyleValueBold, UIDesignSystem.ColorMagenta);

            // Divider 3
            DrawVerticalDivider(c3X + colW - 12f, dockY + 12f, dockH - 24f);

            // --- COLUMN 4: NEXT TARGET ---
            float c4X = c3X + colW;
            Rect rC4Label = new Rect(c4X, dockY + 10f, colW - 10f, 18f);
            UIDesignSystem.DrawShadowedLabel(rC4Label, "NEXT TARGET", UIDesignSystem.StyleLabelMuted, UIDesignSystem.ColorTextMuted);

            string targetName = "GATE 01";
            ProceduralIcon targetIcon = ProceduralIcon.Gate;
            Color targetCol = UIDesignSystem.ColorCyan;

            if (lm.Rings != null && !lm.Rings.AllRelaysSynchronized)
            {
                targetName = $"GATE {lm.Rings.CurrentExpectedOrder:D2}";
                targetIcon = ProceduralIcon.Gate;
                targetCol = UIDesignSystem.ColorCyan;
            }
            else if (lm.Relay != null && lm.Relay.State == RelayState.Inactive)
            {
                targetName = "ENERGY RECEIVER";
                targetIcon = ProceduralIcon.Capsule;
                targetCol = UIDesignSystem.ColorAmberWarning;
            }
            else
            {
                targetName = "STATION DOCK";
                targetIcon = ProceduralIcon.Dock;
                targetCol = UIDesignSystem.ColorGreenSafe;
            }

            UIDesignSystem.DrawIcon(new Rect(c4X, dockY + 35f, 24f, 24f), targetIcon, targetCol);
            Rect rTargetText = new Rect(c4X + 32f, dockY + 34f, colW - 40f, 24f);
            UIDesignSystem.DrawShadowedLabel(rTargetText, targetName, UIDesignSystem.StyleValueBold, targetCol);
        }

        private void DrawVerticalDivider(float x, float y, float h)
        {
            Color p = GUI.color;
            GUI.color = new Color(0f, 0.85f, 1f, 0.20f);
            GUI.DrawTexture(new Rect(x, y, 1f, h), UIDesignSystem.TexWhite);
            GUI.color = p;
        }

        private void DrawAlertBanner(LevelManager lm)
        {
            if (!lm.IsAlertActive()) return;

            float w = 620f;
            float h = 48f;
            float x = (viewWidth - w) / 2f;
            float y = 125f;

            Color borderColor = lm.IsCurrentAlertWarning ? UIDesignSystem.ColorAmberWarning : UIDesignSystem.ColorGreenSafe;
            Color bgColor = lm.IsCurrentAlertWarning ? new Color(0.2f, 0.1f, 0.02f, 0.95f) : new Color(0.02f, 0.15f, 0.08f, 0.95f);

            GUI.color = bgColor;
            GUI.DrawTexture(new Rect(x, y, w, h), UIDesignSystem.TexWhite);
            GUI.color = borderColor;
            GUI.DrawTexture(new Rect(x, y, w, 2f), UIDesignSystem.TexWhite);
            GUI.DrawTexture(new Rect(x, y + h - 2f, w, 2f), UIDesignSystem.TexWhite);
            GUI.color = Color.white;

            UIDesignSystem.DrawShadowedLabel(new Rect(x, y + 10f, w, 28f), lm.CurrentAlertMessage, UIDesignSystem.StyleAlertBanner, borderColor);
        }

        // ==========================================
        // 6. MODALS: RESULTS, FAILURE, PAUSE, SETTINGS
        // ==========================================
        private void DrawResultsModal(LevelManager lm)
        {
            float modalW = 560f;
            float modalH = 540f;
            float modalX = (viewWidth - modalW) / 2f;
            float modalY = (viewHeight - modalH) / 2f;

            Rect r = new Rect(modalX, modalY, modalW, modalH);
            GUI.Box(r, GUIContent.none, UIDesignSystem.StylePanelSelected);

            float padX = modalX + 36f;
            float padW = modalW - 72f;

            // Title
            Rect rTitle = new Rect(padX, modalY + 24f, padW, 36f);
            UIDesignSystem.DrawShadowedLabel(rTitle, "MISSION ACCOMPLISHED", UIDesignSystem.StyleModalTitle, UIDesignSystem.ColorGreenSafe);

            // Subtitle
            string sub = $"SECTOR {lm.LevelNumber:D2}: {lm.LevelTitle.ToUpper()} RESTORED";
            Rect rSub = new Rect(padX, modalY + 62f, padW, 22f);
            UIDesignSystem.DrawShadowedLabel(rSub, sub, UIDesignSystem.StyleModalSubtitle, UIDesignSystem.ColorCyan);

            ScoreManager sm = ScoreManager.Instance;
            int total = sm != null ? sm.TotalScore : 0;

            // Animated count-up for total score
            if (resultAnimationStartTime < 0) resultAnimationStartTime = Time.unscaledTime;
            float elapsedAnim = Time.unscaledTime - resultAnimationStartTime;
            animatedResultScore = Mathf.MoveTowards(animatedResultScore, total, Mathf.Max(60f, (total - animatedResultScore) * 5f) * Time.unscaledDeltaTime);

            // 2-Column Stats Table
            float tableY = modalY + 100f;
            float rowH = 28f;

            DrawTableRow("COMPULSORY GATES", $"{lm.Rings?.ActivatedRelaysCount ?? 0} / {lm.Rings?.TotalRelays ?? 0}", padX, tableY + rowH * 0, padW);
            DrawTableRow("TIME ELAPSED", $"{lm.ElapsedTime:F1}s", padX, tableY + rowH * 1, padW);
            DrawTableRow("TIME EFFICIENCY BONUS", $"+{sm?.TimeBonus ?? 0} PTS", padX, tableY + rowH * 2, padW);
            DrawTableRow("CAPSULE RESERVES REMAINING", $"+{sm?.CapsuleBonus ?? 0} PTS", padX, tableY + rowH * 3, padW);
            DrawTableRow("COMPLETION BONUS", $"+{sm?.CompletionBonus ?? 0} PTS", padX, tableY + rowH * 4, padW);
            string diffBadge = $"{SettingsManager.GetDifficultyShortBadge(lm.Difficulty)} (x{SettingsManager.GetScoreMultiplier():F1})";
            DrawTableRow("DIFFICULTY MULTIPLIER", diffBadge, padX, tableY + rowH * 5, padW);

            // Total Score Highlight Box
            float totalBoxY = tableY + rowH * 6 + 10f;
            Rect rTotalBox = new Rect(padX, totalBoxY, padW, 64f);
            GUI.Box(rTotalBox, GUIContent.none, UIDesignSystem.StylePanel);

            Rect rTotalLbl = new Rect(padX + 16f, totalBoxY + 18f, 160f, 28f);
            UIDesignSystem.DrawShadowedLabel(rTotalLbl, "TOTAL SCORE", UIDesignSystem.StyleHeader, UIDesignSystem.ColorAmberWarning);

            Rect rTotalVal = new Rect(padX + padW - 220f, totalBoxY + 14f, 204f, 36f);
            UIDesignSystem.DrawShadowedLabel(rTotalVal, $"{(int)animatedResultScore:N0}", UIDesignSystem.StyleTotalScoreValue, UIDesignSystem.ColorGreenSafe);

            // Best score line
            string bestKey = $"best_{lm.LevelNumber}_{(int)lm.Difficulty}";
            int bestScore = PlayerPrefs.GetInt(bestKey, total);
            Rect rBest = new Rect(padX, totalBoxY + 70f, padW, 20f);
            UIDesignSystem.DrawShadowedLabel(rBest, $"PERSONAL BEST ({SettingsManager.GetDifficultyShortBadge(lm.Difficulty)}): {bestScore:N0} PTS", UIDesignSystem.StyleBestScoreText, UIDesignSystem.ColorTextMuted);

            // Buttons (>= 48px height)
            float btnY = modalY + modalH - 74f;
            float btnW = (padW - 16f) / 2f;

            if (GUI.Button(new Rect(padX, btnY, btnW, 50f), "RETRY [R]", UIDesignSystem.StyleButtonSecondary))
            {
                lm.RestartLevel();
            }

            int maxSectors = GameManager.Instance != null ? GameManager.Instance.MaxRelays : 10;
            string nextLabel = lm.LevelNumber >= maxSectors ? "MAIN MENU  ›" : "NEXT MISSION  ›";
            if (GUI.Button(new Rect(padX + btnW + 16f, btnY, btnW, 50f), nextLabel, UIDesignSystem.StyleButtonPrimary))
            {
                lm.AdvanceToNextLevel();
            }
        }

        private void DrawFailureModal(LevelManager lm)
        {
            float modalW = 540f;
            float modalH = 460f;
            float modalX = (viewWidth - modalW) / 2f;
            float modalY = (viewHeight - modalH) / 2f;

            Rect r = new Rect(modalX, modalY, modalW, modalH);
            GUI.Box(r, GUIContent.none, UIDesignSystem.StylePanelSelected);

            float padX = modalX + 36f;
            float padW = modalW - 72f;

            Rect rTitle = new Rect(padX, modalY + 28f, padW, 36f);
            UIDesignSystem.DrawShadowedLabel(rTitle, "MISSION FAILED", UIDesignSystem.StyleModalTitle, UIDesignSystem.ColorRedCritical);

            Rect rReason = new Rect(padX, modalY + 74f, padW, 26f);
            UIDesignSystem.DrawShadowedLabel(rReason, lm.FailureReason, UIDesignSystem.StyleFailReason, UIDesignSystem.ColorAmberWarning);

            float tableY = modalY + 125f;
            float rowH = 32f;
            DrawTableRow("MISSION", $"SECTOR {lm.LevelNumber:D2}: {lm.LevelTitle.ToUpper()}", padX, tableY + rowH * 0, padW);
            DrawTableRow("DIFFICULTY", SettingsManager.GetDifficultyName(lm.Difficulty), padX, tableY + rowH * 1, padW);
            DrawTableRow("GATES SYNCHRONIZED", $"{lm.Rings?.ActivatedRelaysCount ?? 0} / {lm.Rings?.TotalRelays ?? 0}", padX, tableY + rowH * 2, padW);
            DrawTableRow("BATTERY SHIELD REMAINING", $"{lm.Battery?.BatteryPercentage * 100f ?? 0f:F0}%", padX, tableY + rowH * 3, padW);
            DrawTableRow("CAPSULES REMAINING", $"{lm.Launcher?.RemainingCapsules ?? 0}", padX, tableY + rowH * 4, padW);

            float btnY = modalY + modalH - 74f;
            float btnW = (padW - 16f) / 2f;

            if (GUI.Button(new Rect(padX, btnY, btnW, 50f), "RETRY SECTOR [R]", UIDesignSystem.StyleButtonPrimary))
            {
                lm.RestartLevel();
            }

            if (GUI.Button(new Rect(padX + btnW + 16f, btnY, btnW, 50f), "MAIN MENU", UIDesignSystem.StyleButtonSecondary))
            {
                Time.timeScale = 1f;
                SceneManager.LoadScene("MainMenu");
            }
        }

        private void DrawPauseModal(LevelManager lm)
        {
            float modalW = 480f;
            float modalH = 460f;
            float modalX = (viewWidth - modalW) / 2f;
            float modalY = (viewHeight - modalH) / 2f;

            Rect r = new Rect(modalX, modalY, modalW, modalH);
            GUI.Box(r, GUIContent.none, UIDesignSystem.StylePanelSelected);

            float padX = modalX + 36f;
            float padW = modalW - 72f;

            Rect rTitle = new Rect(padX, modalY + 28f, padW, 36f);
            UIDesignSystem.DrawShadowedLabel(rTitle, "SYSTEM PAUSED", UIDesignSystem.StyleModalTitle, UIDesignSystem.ColorCyan);

            Rect rSub = new Rect(padX, modalY + 68f, padW, 22f);
            UIDesignSystem.DrawShadowedLabel(rSub, $"MISSION {lm.LevelNumber:D2}: {lm.LevelTitle.ToUpper()}", UIDesignSystem.StyleModalSubtitle, UIDesignSystem.ColorTextBright);

            // Best score line
            string bestKey = $"best_{lm.LevelNumber}_{(int)lm.Difficulty}";
            int bestScore = PlayerPrefs.GetInt(bestKey, 0);
            Rect rBest = new Rect(padX, modalY + 96f, padW, 20f);
            UIDesignSystem.DrawShadowedLabel(rBest, $"DIFFICULTY: {SettingsManager.GetDifficultyShortBadge(lm.Difficulty)}  •  BEST: {bestScore:N0} PTS", UIDesignSystem.StyleBestScoreText, UIDesignSystem.ColorTextMuted);

            float btnY = modalY + 140f;
            float btnH = 50f;
            float gap = 14f;

            if (GUI.Button(new Rect(padX, btnY, padW, btnH), "RESUME FLIGHT  ›", UIDesignSystem.StyleButtonPrimary))
            {
                TogglePause();
            }

            if (GUI.Button(new Rect(padX, btnY + (btnH + gap) * 1, padW, btnH), "RESTART SECTOR [R]", UIDesignSystem.StyleButtonSecondary))
            {
                TogglePause();
                lm.RestartLevel();
            }

            if (GUI.Button(new Rect(padX, btnY + (btnH + gap) * 2, padW, btnH), "SYSTEM SETTINGS", UIDesignSystem.StyleButtonSecondary))
            {
                showSettings = true;
                AudioManager.Instance?.PlayButtonClick();
            }

            if (GUI.Button(new Rect(padX, btnY + (btnH + gap) * 3, padW, btnH), "ABORT TO MAIN MENU", UIDesignSystem.StyleButtonSecondary))
            {
                Time.timeScale = 1f;
                SceneManager.LoadScene("MainMenu");
            }
        }

        private void DrawSettingsModal()
        {
            float modalW = 540f;
            float modalH = 520f;
            float modalX = (viewWidth - modalW) / 2f;
            float modalY = (viewHeight - modalH) / 2f;

            Rect r = new Rect(modalX, modalY, modalW, modalH);
            GUI.Box(r, GUIContent.none, UIDesignSystem.StylePanelSelected);

            float padX = modalX + 36f;
            float padW = modalW - 72f;

            Rect rTitle = new Rect(padX, modalY + 24f, padW, 36f);
            UIDesignSystem.DrawShadowedLabel(rTitle, "SYSTEM SETTINGS", UIDesignSystem.StyleModalTitle, UIDesignSystem.ColorCyan);

            float curY = modalY + 70f;

            // Audio sliders
            UIDesignSystem.DrawShadowedLabel(new Rect(padX, curY, padW, 20f), $"MASTER VOLUME  //  {(int)(SettingsManager.MasterVolume * 100)}%", UIDesignSystem.StyleLabelMuted, UIDesignSystem.ColorTextMuted);
            curY += 24f;
            float newMaster = GUI.HorizontalSlider(new Rect(padX, curY, padW, 20f), SettingsManager.MasterVolume, 0f, 1f);
            if (Mathf.Abs(newMaster - SettingsManager.MasterVolume) > 0.01f) SettingsManager.SetMasterVolume(newMaster);
            curY += 28f;

            UIDesignSystem.DrawShadowedLabel(new Rect(padX, curY, padW, 20f), $"SFX VOLUME  //  {(int)(SettingsManager.SFXVolume * 100)}%", UIDesignSystem.StyleLabelMuted, UIDesignSystem.ColorTextMuted);
            curY += 24f;
            float newSfx = GUI.HorizontalSlider(new Rect(padX, curY, padW, 20f), SettingsManager.SFXVolume, 0f, 1f);
            if (Mathf.Abs(newSfx - SettingsManager.SFXVolume) > 0.01f) SettingsManager.SetSFXVolume(newSfx);
            curY += 28f;

            UIDesignSystem.DrawShadowedLabel(new Rect(padX, curY, padW, 20f), $"MUSIC VOLUME  //  {(int)(SettingsManager.MusicVolume * 100)}%", UIDesignSystem.StyleLabelMuted, UIDesignSystem.ColorTextMuted);
            curY += 24f;
            float newMusic = GUI.HorizontalSlider(new Rect(padX, curY, padW, 20f), SettingsManager.MusicVolume, 0f, 1f);
            if (Mathf.Abs(newMusic - SettingsManager.MusicVolume) > 0.01f) SettingsManager.SetMusicVolume(newMusic);
            curY += 32f;

            // Toggles
            bool mute = GUI.Toggle(new Rect(padX, curY, 200f, 24f), SettingsManager.IsMuted, " MUTE ALL AUDIO");
            if (mute != SettingsManager.IsMuted) SettingsManager.SetMuted(mute);

            bool warn = GUI.Toggle(new Rect(padX + 220f, curY, 220f, 24f), SettingsManager.CountdownAudioEnabled, " COUNTDOWN AUDIO");
            if (warn != SettingsManager.CountdownAudioEnabled) SettingsManager.SetCountdownAudio(warn);
            curY += 32f;

            bool shake = GUI.Toggle(new Rect(padX, curY, 200f, 24f), SettingsManager.ScreenShakeEnabled, " SCREEN SHAKE");
            if (shake != SettingsManager.ScreenShakeEnabled) SettingsManager.SetScreenShake(shake);

            bool vfx = GUI.Toggle(new Rect(padX + 220f, curY, 220f, 24f), SettingsManager.VisualEffectsEnabled, " VISUAL EFFECTS");
            if (vfx != SettingsManager.VisualEffectsEnabled) SettingsManager.SetVisualEffects(vfx);
            curY += 36f;

            // Fullscreen toggle
            if (GUI.Button(new Rect(padX, curY, padW, 40f), Screen.fullScreen ? "SWITCH TO WINDOWED" : "SWITCH TO FULLSCREEN", UIDesignSystem.StyleButtonSecondary))
            {
                SettingsManager.ToggleFullscreen();
            }
            curY += 52f;

            // Back button
            if (GUI.Button(new Rect(padX, modalY + modalH - 64f, padW, 48f), "‹  BACK TO PAUSE MENU", UIDesignSystem.StyleButtonPrimary))
            {
                showSettings = false;
                AudioManager.Instance?.PlayButtonClick();
            }
        }

        private void DrawTableRow(string label, string val, float x, float y, float w)
        {
            Rect rLabel = new Rect(x, y, w * 0.55f, 24f);
            Rect rVal = new Rect(x + w * 0.55f, y, w * 0.45f, 24f);
            UIDesignSystem.DrawShadowedLabel(rLabel, label, UIDesignSystem.StyleTableRowLabel, UIDesignSystem.ColorTextMuted);
            UIDesignSystem.DrawShadowedLabel(rVal, val, UIDesignSystem.StyleTableRowValue, UIDesignSystem.ColorTextBright);
        }
    }
}
