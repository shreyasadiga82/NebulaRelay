using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using NebulaRelay.Core;
using NebulaRelay.Effects;
using NebulaRelay.World;

namespace NebulaRelay.UI
{
    public enum MenuState
    {
        Title,
        DifficultySelect,
        MissionSelect,
        Settings
    }

    /// <summary>
    /// MainMenuController provides the title screen, difficulty selection screen,
    /// 10-mission selection grid, and system settings.
    /// Complies strictly with the 4 difficulty modes, 10-mission progression,
    /// and 8-px design system tokens.
    /// </summary>
    [SelectionBase]
    public class MainMenuController : MonoBehaviour
    {
        private MenuState currentState = MenuState.Title;
        private int selectedMissionIndex = 1; // 1 to 10
        private GameDifficulty selectedDifficulty = GameDifficulty.Medium;
        private float viewWidth = 1280f;
        private float viewHeight = 720f;

        // Dynamic background reference
        private Transform bgTransform;
        private SpriteRenderer bgRenderer;

        // Difficulty card animation timers (0.15-0.25s)
        private float[] difficultyCardAnim = new float[4] { 0f, 1f, 0f, 0f };

        private void Start()
        {
            UIDesignSystem.EnsureInitialized();
            selectedDifficulty = SettingsManager.CurrentDifficulty;
            UpdateBackgroundScale();
        }

        private void Update()
        {
            UpdateBackgroundScale();
            HandleKeyboardNavigation();

            // Smooth animated easing on difficulty cards
            for (int i = 0; i < 4; i++)
            {
                float target = ((int)selectedDifficulty == i) ? 1f : 0f;
                difficultyCardAnim[i] = Mathf.MoveTowards(difficultyCardAnim[i], target, Time.deltaTime * 5f);
            }
        }

        private void HandleKeyboardNavigation()
        {
            var kb = Keyboard.current;
            if (kb == null) return;

            if (kb.escapeKey.wasPressedThisFrame)
            {
                if (currentState == MenuState.MissionSelect)
                {
                    currentState = MenuState.DifficultySelect;
                    AudioManager.Instance?.PlayButtonClick();
                }
                else if (currentState == MenuState.DifficultySelect || currentState == MenuState.Settings)
                {
                    currentState = MenuState.Title;
                    AudioManager.Instance?.PlayButtonClick();
                }
            }

            if (currentState == MenuState.DifficultySelect)
            {
                if (kb.leftArrowKey.wasPressedThisFrame || kb.aKey.wasPressedThisFrame)
                {
                    int prev = ((int)selectedDifficulty - 1 + 4) % 4;
                    selectedDifficulty = (GameDifficulty)prev;
                    SettingsManager.SetDifficulty(selectedDifficulty);
                    AudioManager.Instance?.PlayButtonClick();
                }
                if (kb.rightArrowKey.wasPressedThisFrame || kb.dKey.wasPressedThisFrame)
                {
                    int next = ((int)selectedDifficulty + 1) % 4;
                    selectedDifficulty = (GameDifficulty)next;
                    SettingsManager.SetDifficulty(selectedDifficulty);
                    AudioManager.Instance?.PlayButtonClick();
                }
                if (kb.enterKey.wasPressedThisFrame || kb.numpadEnterKey.wasPressedThisFrame || kb.spaceKey.wasPressedThisFrame)
                {
                    currentState = MenuState.MissionSelect;
                    AudioManager.Instance?.PlayButtonClick();
                }
            }
        }

        private void UpdateBackgroundScale()
        {
            if (bgTransform == null)
            {
                GameObject bgObj = GameObject.Find("MainMenu_Background");
                if (bgObj != null)
                {
                    bgTransform = bgObj.transform;
                    bgRenderer = bgObj.GetComponent<SpriteRenderer>();
                }
            }

            if (bgTransform != null && bgRenderer != null && bgRenderer.sprite != null)
            {
                Camera cam = Camera.main;
                if (cam == null) cam = FindFirstObjectByType<Camera>();
                if (cam != null && cam.orthographic)
                {
                    float worldHeight = cam.orthographicSize * 2f;
                    float worldWidth = worldHeight * cam.aspect;

                    float spriteW = bgRenderer.sprite.rect.width / bgRenderer.sprite.pixelsPerUnit;
                    float spriteH = bgRenderer.sprite.rect.height / bgRenderer.sprite.pixelsPerUnit;

                    float scale = Mathf.Max(worldWidth / spriteW, worldHeight / spriteH) * 1.06f;
                    bgTransform.localScale = new Vector3(scale, scale, 1f);
                    bgTransform.position = new Vector3(cam.transform.position.x, cam.transform.position.y, 0f);
                }
            }
        }

        private void OnGUI()
        {
            UIDesignSystem.EnsureInitialized();

            Matrix4x4 origMatrix = UIScale.BeginMatrix();
            viewWidth = UIScale.GetViewWidth();
            viewHeight = UIScale.GetViewHeight();

            try
            {
                switch (currentState)
                {
                    case MenuState.Title:
                        DrawTitleScreen();
                        break;
                    case MenuState.DifficultySelect:
                        DrawDifficultySelectScreen();
                        break;
                    case MenuState.MissionSelect:
                        DrawMissionSelectScreen();
                        break;
                    case MenuState.Settings:
                        DrawSettingsScreen();
                        break;
                }
            }
            finally
            {
                UIScale.EndMatrix(origMatrix);
            }
        }

        // ==========================================
        // 1. TITLE SCREEN (Centered-Left sci-fi panel)
        // ==========================================
        private void DrawTitleScreen()
        {
            float totalW = 560f;
            float totalH = 500f;
            // Placed slightly to the left so station art remains visible
            float posX = Mathf.Max(UIDesignSystem.MarginSafe, (viewWidth - totalW) * 0.35f);
            float posY = (viewHeight - totalH) / 2f;

            Rect mainRect = new Rect(posX, posY, totalW, totalH);
            GUI.Box(mainRect, GUIContent.none, UIDesignSystem.StylePanelSelected);

            float innerX = posX + 36f;
            float innerW = totalW - 72f;

            // Title
            Rect rTitle = new Rect(innerX, posY + 32f, innerW, 58f);
            UIDesignSystem.DrawShadowedLabel(rTitle, "DRIFT CORE", UIDesignSystem.StyleMenuMainTitle, UIDesignSystem.ColorCyan);

            // Subtitle
            Rect rSub = new Rect(innerX, posY + 94f, innerW, 24f);
            UIDesignSystem.DrawShadowedLabel(rSub, "PRECISION COURIER  //  ORBITAL RECONSTRUCTION", UIDesignSystem.StyleMenuSubtitle, UIDesignSystem.ColorTextMuted);

            float btnY = posY + 150f;
            float btnH = 56f;
            float gap = 16f;

            if (GUI.Button(new Rect(innerX, btnY, innerW, btnH), "NEW TRANSMISSION  ›", UIDesignSystem.StyleButtonPrimary))
            {
                AudioManager.Instance?.PlayButtonClick();
                selectedDifficulty = SettingsManager.CurrentDifficulty;
                currentState = MenuState.DifficultySelect;
            }

            if (GUI.Button(new Rect(innerX, btnY + (btnH + gap), innerW, btnH), "SYSTEM SETTINGS", UIDesignSystem.StyleButtonSecondary))
            {
                AudioManager.Instance?.PlayButtonClick();
                currentState = MenuState.Settings;
            }

            if (GUI.Button(new Rect(innerX, btnY + (btnH + gap) * 2, innerW, btnH), "DISCONNECT TERMINAL", UIDesignSystem.StyleButtonSecondary))
            {
                AudioManager.Instance?.PlayButtonClick();
                Application.Quit();
            }

            // Footer status line
            Rect rFooter = new Rect(innerX, posY + totalH - 46f, innerW, 22f);
            UIDesignSystem.DrawShadowedLabel(rFooter, "DEEP SPACE LOGISTICS NETWORK  •  10 SECTORS ACTIVE", UIDesignSystem.StyleFooter, UIDesignSystem.ColorTextMuted);
        }

        // ==========================================
        // 2. DIFFICULTY SELECTION SCREEN
        // 4 Cards: Explorer, Pilot (Normal), Veteran, Voidwalker
        // ==========================================
        private void DrawDifficultySelectScreen()
        {
            float totalW = Mathf.Min(1200f, viewWidth - (UIDesignSystem.MarginSafe * 2));
            float totalH = 590f;
            float posX = (viewWidth - totalW) / 2f;
            float posY = (viewHeight - totalH) / 2f;

            Rect mainRect = new Rect(posX, posY, totalW, totalH);
            GUI.Box(mainRect, GUIContent.none, UIDesignSystem.StylePanelSelected);

            float innerX = posX + 28f;
            float innerW = totalW - 56f;

            // Header
            Rect rHead = new Rect(innerX, posY + 20f, innerW, 36f);
            UIDesignSystem.DrawShadowedLabel(rHead, "SELECT FLIGHT DIFFICULTY MODE", UIDesignSystem.StyleSectionHeader, UIDesignSystem.ColorCyan);

            Rect rSub = new Rect(innerX, posY + 56f, innerW, 20f);
            UIDesignSystem.DrawShadowedLabel(rSub, "Choose your flight parameters. Difficulty persists across missions and can be changed anytime.", UIDesignSystem.StyleLabelMuted, UIDesignSystem.ColorTextMuted);

            // 4 Cards in a horizontal row
            float cardSpacing = 16f;
            float cardW = (innerW - (cardSpacing * 3)) / 4f;
            float cardH = 390f;
            float cardY = posY + 90f;

            GameDifficulty[] diffs = new GameDifficulty[4]
            {
                GameDifficulty.Easy,
                GameDifficulty.Medium,
                GameDifficulty.Hard,
                GameDifficulty.Expert
            };

            for (int i = 0; i < 4; i++)
            {
                float cx = innerX + i * (cardW + cardSpacing);
                DrawDifficultyCard(cx, cardY, cardW, cardH, diffs[i], i);
            }

            // Bottom Navigation
            float bottomY = posY + totalH - 68f;

            if (GUI.Button(new Rect(innerX, bottomY, 220f, 48f), "‹  BACK TO MENU", UIDesignSystem.StyleButtonSecondary))
            {
                AudioManager.Instance?.PlayButtonClick();
                currentState = MenuState.Title;
            }

            if (GUI.Button(new Rect(posX + totalW - 28f - 260f, bottomY, 260f, 48f), "CONFIRM & PROCEED  ›", UIDesignSystem.StyleButtonPrimary))
            {
                AudioManager.Instance?.PlayButtonClick();
                SettingsManager.SetDifficulty(selectedDifficulty);
                currentState = MenuState.MissionSelect;
            }
        }

        private void DrawDifficultyCard(float x, float y, float w, float h, GameDifficulty diff, int index)
        {
            bool isSelected = (selectedDifficulty == diff);
            Rect cardRect = new Rect(x, y, w, h);

            GUIStyle cardBox = isSelected ? UIDesignSystem.StylePanelSelected : UIDesignSystem.StylePanel;
            if (GUI.Button(cardRect, GUIContent.none, cardBox))
            {
                selectedDifficulty = diff;
                SettingsManager.SetDifficulty(diff);
                AudioManager.Instance?.PlayButtonClick();
            }

            float pad = 16f;
            float ix = x + pad;
            float iw = w - (pad * 2);

            // 1. Procedural Icon (48x48)
            ProceduralIcon iconType = diff == GameDifficulty.Easy ? ProceduralIcon.Beacon
                                    : (diff == GameDifficulty.Medium ? ProceduralIcon.Reticle
                                    : (diff == GameDifficulty.Hard ? ProceduralIcon.WarningTriangle : ProceduralIcon.Singularity));
            Color iconColor = diff == GameDifficulty.Easy ? UIDesignSystem.ColorGreenSafe
                            : (diff == GameDifficulty.Medium ? UIDesignSystem.ColorCyan
                            : (diff == GameDifficulty.Hard ? UIDesignSystem.ColorAmberWarning : UIDesignSystem.ColorMagenta));

            Rect rIcon = new Rect(x + (w - 48f) / 2f, y + 20f, 48f, 48f);
            UIDesignSystem.DrawIcon(rIcon, iconType, iconColor);

            // 2. Title & Badge
            string title = diff == GameDifficulty.Easy ? "EXPLORER"
                         : (diff == GameDifficulty.Medium ? "PILOT"
                         : (diff == GameDifficulty.Hard ? "VETERAN" : "VOIDWALKER"));

            Rect rTitle = new Rect(ix, y + 78f, iw, 28f);
            UIDesignSystem.DrawShadowedLabel(rTitle, title, UIDesignSystem.StyleCardTitle, iconColor);

            // Subtitle
            string sub = diff == GameDifficulty.Easy ? "EASY MODE"
                       : (diff == GameDifficulty.Medium ? "NORMAL MODE"
                       : (diff == GameDifficulty.Hard ? "HARD MODE (1.5x)" : "EXPERT MODE (2.0x)"));
            Rect rSub = new Rect(ix, y + 106f, iw, 18f);
            UIDesignSystem.DrawShadowedLabel(rSub, sub, UIDesignSystem.StyleCardSubtitle, UIDesignSystem.ColorTextMuted);

            // 3. Difficulty Bar (1 to 4 Pips)
            Rect rBar = new Rect(ix + (iw - 100f) / 2f, y + 130f, 100f, 10f);
            UIDesignSystem.DrawSegmentedBar(rBar, (index + 1) / 4f, 4, iconColor, new Color(0.1f, 0.15f, 0.25f, 0.8f));

            // 4. Description
            string desc = SettingsManager.GetDifficultyDescription(diff);
            Rect rDesc = new Rect(ix, y + 150f, iw, 50f);
            UIDesignSystem.DrawShadowedLabel(rDesc, desc, UIDesignSystem.StyleCardDesc, UIDesignSystem.ColorTextBright);

            // 5. Parameters List Box
            float listY = y + 210f;
            float rowH = 24f;

            string timerStr = diff == GameDifficulty.Easy ? "360s (06:00)" : (diff == GameDifficulty.Medium ? "300s (05:00)" : (diff == GameDifficulty.Hard ? "270s (04:30)" : "240s (04:00)"));
            string capsStr = diff == GameDifficulty.Easy ? "4 Capsules" : "2 Capsules";
            string dockStr = $"≤ {SettingsManager.GetMaxSafeDockingSpeed(diff):F1} u/s";
            string drainStr = $"{SettingsManager.GetBatteryDrainMultiplier(diff):F2}x Drain";
            string scoreStr = $"{SettingsManager.GetScoreMultiplier(diff):F1}x Score";

            DrawCardStatRow("TIMER", timerStr, ix, listY + rowH * 0, iw);
            DrawCardStatRow("CAPSULES", capsStr, ix, listY + rowH * 1, iw);
            DrawCardStatRow("DOCK LIMIT", dockStr, ix, listY + rowH * 2, iw);
            DrawCardStatRow("BATTERY", drainStr, ix, listY + rowH * 3, iw);
            DrawCardStatRow("SCORE MULT", scoreStr, ix, listY + rowH * 4, iw);

            // 6. Select Button
            float btnH = 38f;
            Rect rBtn = new Rect(ix, y + h - btnH - 14f, iw, btnH);
            GUIStyle btnStyle = isSelected ? UIDesignSystem.StyleButtonPrimary : UIDesignSystem.StyleButtonSecondary;
            string btnText = isSelected ? "✓ SELECTED" : "SELECT";
            if (GUI.Button(rBtn, btnText, btnStyle))
            {
                selectedDifficulty = diff;
                SettingsManager.SetDifficulty(diff);
                AudioManager.Instance?.PlayButtonClick();
            }
        }

        private void DrawCardStatRow(string label, string val, float x, float y, float w)
        {
            Rect rL = new Rect(x, y, w * 0.45f, 22f);
            Rect rV = new Rect(x + w * 0.45f, y, w * 0.55f, 22f);
            UIDesignSystem.DrawShadowedLabel(rL, label, UIDesignSystem.StyleLabelMuted, UIDesignSystem.ColorTextMuted);
            UIDesignSystem.DrawShadowedLabel(rV, val, UIDesignSystem.StyleCardValue, UIDesignSystem.ColorTextBright);
        }

        // ==========================================
        // 3. 10-MISSION SELECT SCREEN
        // 2x5 Grid of Mission Cards + Detail Panel on the right
        // ==========================================
        private void DrawMissionSelectScreen()
        {
            float totalW = Mathf.Min(1240f, viewWidth - (UIDesignSystem.MarginSafe * 2));
            float totalH = 640f;
            float posX = (viewWidth - totalW) / 2f;
            float posY = (viewHeight - totalH) / 2f;

            Rect mainRect = new Rect(posX, posY, totalW, totalH);
            GUI.Box(mainRect, GUIContent.none, UIDesignSystem.StylePanelSelected);

            float innerX = posX + 24f;
            float innerW = totalW - 48f;

            // Top Header: Never clipped, proper spacing
            Rect rHead = new Rect(innerX, posY + 16f, 420f, 32f);
            UIDesignSystem.DrawShadowedLabel(rHead, "DRIFT CORE  /  SELECT MISSION", UIDesignSystem.StyleSectionHeader, UIDesignSystem.ColorCyan);

            // Active Difficulty Badge with [CHANGE] link
            float diffX = innerX + 440f;
            string diffBadgeText = $"DIFFICULTY: {SettingsManager.GetDifficultyShortBadge(selectedDifficulty)} (x{SettingsManager.GetScoreMultiplier(selectedDifficulty):F1})";
            Rect rDiffBadge = new Rect(diffX, posY + 18f, 240f, 28f);
            UIDesignSystem.DrawBadge(rDiffBadge, diffBadgeText, new Color(0.04f, 0.08f, 0.16f, 0.95f), UIDesignSystem.ColorCyan, 14);

            Rect rChangeBtn = new Rect(diffX + 248f, posY + 18f, 96f, 28f);
            if (GUI.Button(rChangeBtn, "CHANGE", UIDesignSystem.StyleButtonSecondary))
            {
                AudioManager.Instance?.PlayButtonClick();
                currentState = MenuState.DifficultySelect;
            }

            // Progression status
            int completedCount = GetCompletedMissionsCount(selectedDifficulty);
            Rect rProg = new Rect(innerX + innerW - 200f, posY + 18f, 200f, 28f);
            UIDesignSystem.DrawShadowedLabel(rProg, $"COMPLETED: {completedCount}/10", UIDesignSystem.StyleProgressText, UIDesignSystem.ColorGreenSafe);

            // Divider line
            GUI.color = UIDesignSystem.ColorBorderCyan;
            GUI.DrawTexture(new Rect(innerX, posY + 52f, innerW, 1f), UIDesignSystem.TexWhite);
            GUI.color = Color.white;

            // Layout split: Left side 2x5 Grid (65% width), Right side Overview Panel (35% width)
            float leftW = (innerW - 20f) * 0.63f;
            float rightW = (innerW - 20f) * 0.37f;
            float contentY = posY + 62f;
            float contentH = totalH - 130f;

            // --- LEFT COLUMN: 2x5 GRID OF 10 MISSIONS ---
            float gridColW = (leftW - 12f) / 2f;
            float gridRowH = (contentH - (8f * 4)) / 5f;

            for (int i = 1; i <= 10; i++)
            {
                int col = (i - 1) % 2;
                int row = (i - 1) / 2;
                float mx = innerX + col * (gridColW + 12f);
                float my = contentY + row * (gridRowH + 8f);

                DrawMissionGridCard(i, mx, my, gridColW, gridRowH);
            }

            // --- RIGHT COLUMN: MISSION DETAIL PANEL ---
            float rightX = innerX + leftW + 20f;
            DrawMissionDetailPanel(rightX, contentY, rightW, contentH);

            // Bottom Navigation Bar
            float bottomY = posY + totalH - 58f;
            if (GUI.Button(new Rect(innerX, bottomY, 200f, 46f), "‹  DIFFICULTY", UIDesignSystem.StyleButtonSecondary))
            {
                AudioManager.Instance?.PlayButtonClick();
                currentState = MenuState.DifficultySelect;
            }
        }

        private void DrawMissionGridCard(int index, float x, float y, float w, float h)
        {
            MissionDefinition m = MissionRegistry.GetMission(index);
            bool isSelected = (selectedMissionIndex == index);
            bool isUnlocked = IsMissionUnlocked(index, selectedDifficulty);
            bool isDone = IsMissionDone(index, selectedDifficulty);

            Rect cardRect = new Rect(x, y, w, h);
            GUIStyle cardStyle = isSelected ? UIDesignSystem.StyleCardSelected : UIDesignSystem.StyleCard;

            if (GUI.Button(cardRect, GUIContent.none, cardStyle))
            {
                selectedMissionIndex = index;
                AudioManager.Instance?.PlayButtonClick();
            }

            float pad = 10f;
            float ix = x + pad;
            float iw = w - (pad * 2);

            // Line 1: Number Badge + Title + Status Icon
            Rect rNum = new Rect(ix, y + 8f, 32f, 20f);
            UIDesignSystem.DrawBadge(rNum, $"{index:D2}", new Color(0.02f, 0.05f, 0.12f, 0.95f), UIDesignSystem.ColorCyan, 14);

            Rect rTitle = new Rect(ix + 38f, y + 8f, iw - 70f, 20f);
            Color tColor = isSelected ? Color.white : (isUnlocked ? UIDesignSystem.ColorTextBright : UIDesignSystem.ColorTextMuted);
            UIDesignSystem.DrawFittedTitle(rTitle, m.Title.ToUpper(), UIDesignSystem.StyleGridTitle, tColor, 15, 14);

            // Status Icon (Lock / Checkmark)
            Rect rStatus = new Rect(ix + iw - 22f, y + 8f, 20f, 20f);
            if (isDone)
            {
                UIDesignSystem.DrawIcon(rStatus, ProceduralIcon.Checkmark, UIDesignSystem.ColorGreenSafe);
            }
            else if (!isUnlocked)
            {
                UIDesignSystem.DrawIcon(rStatus, ProceduralIcon.Lock, UIDesignSystem.ColorRedCritical);
            }

            // Line 2: One-line briefing description
            Rect rDesc = new Rect(ix, y + 32f, iw, 18f);
            UIDesignSystem.DrawFittedTitle(rDesc, m.ShortDescription, UIDesignSystem.StyleGridDesc, UIDesignSystem.ColorTextMuted, 14, 14);

            // Line 3: Gates count, Estimated par time, Best score
            string bestKey = $"best_{index}_{(int)selectedDifficulty}";
            int bestScore = PlayerPrefs.GetInt(bestKey, 0);
            string bestStr = bestScore > 0 ? $"BEST: {bestScore:N0}" : "BEST: ---";

            float par = m.GetEstimatedParTime();
            int parMins = Mathf.FloorToInt(par) / 60;
            int parSecs = Mathf.FloorToInt(par) % 60;
            string metaStr = $"{m.GateCount} GATES  •  PAR {parMins}:{parSecs:D2}  •  {bestStr}";

            Rect rMeta = new Rect(ix, y + 54f, iw, 18f);
            UIDesignSystem.DrawShadowedLabel(rMeta, metaStr, UIDesignSystem.StyleGridMeta, isDone ? UIDesignSystem.ColorGreenSafe : UIDesignSystem.ColorTextMuted);
        }

        private void DrawMissionDetailPanel(float x, float y, float w, float h)
        {
            Rect rPanel = new Rect(x, y, w, h);
            GUI.Box(rPanel, GUIContent.none, UIDesignSystem.StylePanel);

            MissionDefinition m = MissionRegistry.GetMission(selectedMissionIndex);
            bool isUnlocked = IsMissionUnlocked(selectedMissionIndex, selectedDifficulty);

            float pad = 18f;
            float ix = x + pad;
            float iw = w - (pad * 2);

            // Header: Mission Number & Title
            Rect rHeader = new Rect(ix, y + 14f, iw, 28f);
            string titleFull = $"MISSION {selectedMissionIndex:D2}: {m.Title.ToUpper()}";
            UIDesignSystem.DrawFittedTitle(rHeader, titleFull, UIDesignSystem.StyleTitle, UIDesignSystem.ColorCyan, 22, 16);

            // Sector Theme Line
            Rect rSector = new Rect(ix, y + 44f, iw, 20f);
            UIDesignSystem.DrawShadowedLabel(rSector, $"ORBITAL SECTOR {selectedMissionIndex:D2}  //  NETWORK RECONSTRUCTION", UIDesignSystem.StyleLabelMuted, UIDesignSystem.ColorTextMuted);

            // Full Briefing
            Rect rBriefing = new Rect(ix, y + 74f, iw, 110f);
            UIDesignSystem.DrawShadowedLabel(rBriefing, m.FullBriefing, UIDesignSystem.StyleDetailBriefing, UIDesignSystem.ColorTextBright);

            // Key Stats Grid
            float statsY = y + 195f;
            float rowH = 28f;

            float timerDuration = m.GetTimer(selectedDifficulty);
            int tMin = Mathf.FloorToInt(timerDuration) / 60;
            int tSec = Mathf.FloorToInt(timerDuration) % 60;

            int caps = SettingsManager.GetCapsuleLimit(selectedDifficulty);
            float safeSpd = SettingsManager.GetMaxSafeDockingSpeed(selectedDifficulty);
            float scoreMult = SettingsManager.GetScoreMultiplier(selectedDifficulty);

            string bestKey = $"best_{selectedMissionIndex}_{(int)selectedDifficulty}";
            int bestScore = PlayerPrefs.GetInt(bestKey, 0);

            DrawDetailStatRow("COMPULSORY GATES", $"{m.GateCount} Gates", ix, statsY + rowH * 0, iw);
            DrawDetailStatRow("MISSION TIMER", $"{tMin:D2}:{tSec:D2} ({timerDuration:F0}s)", ix, statsY + rowH * 1, iw);
            DrawDetailStatRow("CAPSULE RESERVES", $"{caps} Max", ix, statsY + rowH * 2, iw);
            DrawDetailStatRow("SAFE DOCKING SPEED", $"≤ {safeSpd:F1} u/s", ix, statsY + rowH * 3, iw);
            DrawDetailStatRow("SCORE MULTIPLIER", $"x{scoreMult:F1}", ix, statsY + rowH * 4, iw);
            DrawDetailStatRow("PERSONAL BEST", bestScore > 0 ? $"{bestScore:N0} PTS" : "UNCOMPLETED", ix, statsY + rowH * 5, iw);

            // START MISSION button
            float btnH = 52f;
            Rect rStartBtn = new Rect(ix, y + h - btnH - 16f, iw, btnH);

            if (isUnlocked)
            {
                if (GUI.Button(rStartBtn, "START MISSION  ›", UIDesignSystem.StyleButtonPrimary))
                {
                    AudioManager.Instance?.PlayButtonClick();
                    SceneManager.LoadScene($"Level_{selectedMissionIndex:D2}");
                }
            }
            else
            {
                GUI.Button(rStartBtn, "🔒 LOCKED — COMPLETE PREVIOUS SECTOR", UIDesignSystem.StyleButtonSecondary);
            }
        }

        private void DrawDetailStatRow(string label, string val, float x, float y, float w)
        {
            Rect rL = new Rect(x, y, w * 0.50f, 24f);
            Rect rV = new Rect(x + w * 0.50f, y, w * 0.50f, 24f);
            UIDesignSystem.DrawShadowedLabel(rL, label, UIDesignSystem.StyleLabelMuted, UIDesignSystem.ColorTextMuted);
            UIDesignSystem.DrawShadowedLabel(rV, val, UIDesignSystem.StyleTableRowValue, UIDesignSystem.ColorTextBright);
        }

        private bool IsMissionUnlocked(int index, GameDifficulty diff)
        {
            if (index == 1) return true;
            return PlayerPrefs.GetInt($"done_{index - 1}_{(int)diff}", 0) == 1;
        }

        private bool IsMissionDone(int index, GameDifficulty diff)
        {
            return PlayerPrefs.GetInt($"done_{index}_{(int)diff}", 0) == 1;
        }

        private int GetCompletedMissionsCount(GameDifficulty diff)
        {
            int count = 0;
            for (int i = 1; i <= 10; i++)
            {
                if (IsMissionDone(i, diff)) count++;
            }
            return count;
        }

        // ==========================================
        // 4. SETTINGS SCREEN
        // ==========================================
        private void DrawSettingsScreen()
        {
            float totalW = 600f;
            float totalH = 540f;
            float posX = (viewWidth - totalW) / 2f;
            float posY = (viewHeight - totalH) / 2f;

            Rect mainRect = new Rect(posX, posY, totalW, totalH);
            GUI.Box(mainRect, GUIContent.none, UIDesignSystem.StylePanelSelected);

            float innerX = posX + 36f;
            float innerW = totalW - 72f;

            Rect rTitle = new Rect(innerX, posY + 24f, innerW, 36f);
            UIDesignSystem.DrawShadowedLabel(rTitle, "SYSTEM SETTINGS", UIDesignSystem.StyleModalTitle, UIDesignSystem.ColorCyan);

            float curY = posY + 70f;

            // Sliders
            UIDesignSystem.DrawShadowedLabel(new Rect(innerX, curY, innerW, 20f), $"MASTER VOLUME  //  {(int)(SettingsManager.MasterVolume * 100)}%", UIDesignSystem.StyleLabelMuted, UIDesignSystem.ColorTextMuted);
            curY += 24f;
            float newMaster = GUI.HorizontalSlider(new Rect(innerX, curY, innerW, 20f), SettingsManager.MasterVolume, 0f, 1f);
            if (Mathf.Abs(newMaster - SettingsManager.MasterVolume) > 0.01f) SettingsManager.SetMasterVolume(newMaster);
            curY += 28f;

            UIDesignSystem.DrawShadowedLabel(new Rect(innerX, curY, innerW, 20f), $"SFX VOLUME  //  {(int)(SettingsManager.SFXVolume * 100)}%", UIDesignSystem.StyleLabelMuted, UIDesignSystem.ColorTextMuted);
            curY += 24f;
            float newSfx = GUI.HorizontalSlider(new Rect(innerX, curY, innerW, 20f), SettingsManager.SFXVolume, 0f, 1f);
            if (Mathf.Abs(newSfx - SettingsManager.SFXVolume) > 0.01f) SettingsManager.SetSFXVolume(newSfx);
            curY += 28f;

            UIDesignSystem.DrawShadowedLabel(new Rect(innerX, curY, innerW, 20f), $"MUSIC VOLUME  //  {(int)(SettingsManager.MusicVolume * 100)}%", UIDesignSystem.StyleLabelMuted, UIDesignSystem.ColorTextMuted);
            curY += 24f;
            float newMusic = GUI.HorizontalSlider(new Rect(innerX, curY, innerW, 20f), SettingsManager.MusicVolume, 0f, 1f);
            if (Mathf.Abs(newMusic - SettingsManager.MusicVolume) > 0.01f) SettingsManager.SetMusicVolume(newMusic);
            curY += 32f;

            // Toggles
            bool mute = GUI.Toggle(new Rect(innerX, curY, 220f, 24f), SettingsManager.IsMuted, " MUTE ALL AUDIO");
            if (mute != SettingsManager.IsMuted) SettingsManager.SetMuted(mute);

            bool warn = GUI.Toggle(new Rect(innerX + 240f, curY, 240f, 24f), SettingsManager.CountdownAudioEnabled, " COUNTDOWN AUDIO");
            if (warn != SettingsManager.CountdownAudioEnabled) SettingsManager.SetCountdownAudio(warn);
            curY += 32f;

            bool shake = GUI.Toggle(new Rect(innerX, curY, 220f, 24f), SettingsManager.ScreenShakeEnabled, " SCREEN SHAKE");
            if (shake != SettingsManager.ScreenShakeEnabled) SettingsManager.SetScreenShake(shake);

            bool vfx = GUI.Toggle(new Rect(innerX + 240f, curY, 240f, 24f), SettingsManager.VisualEffectsEnabled, " VISUAL EFFECTS");
            if (vfx != SettingsManager.VisualEffectsEnabled) SettingsManager.SetVisualEffects(vfx);
            curY += 36f;

            // Fullscreen toggle
            if (GUI.Button(new Rect(innerX, curY, innerW, 42f), Screen.fullScreen ? "SWITCH TO WINDOWED" : "SWITCH TO FULLSCREEN", UIDesignSystem.StyleButtonSecondary))
            {
                SettingsManager.ToggleFullscreen();
            }
            curY += 56f;

            // Back button
            if (GUI.Button(new Rect(innerX, posY + totalH - 64f, innerW, 48f), "‹  BACK TO MENU", UIDesignSystem.StyleButtonPrimary))
            {
                currentState = MenuState.Title;
                AudioManager.Instance?.PlayButtonClick();
            }
        }
    }
}
