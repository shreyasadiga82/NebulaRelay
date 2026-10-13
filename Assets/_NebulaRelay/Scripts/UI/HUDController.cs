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
    /// HUDController provides the cinematic sci-fi HUD matching all mission requirements:
    /// - Top Telemetry: Mission, Countdown Timer (02:00), Current Score, Capsules (X/2), Relays (X/Y), Pause Button.
    /// - Floating +100 point popups near activated relays.
    /// - Bottom Telemetry Dock: Battery Shield, Speedometer with safe docking limit (1.5 m/s), Capsule status, Next Gate.
    /// - Comprehensive Results Modal (Score breakdown, completion time, capsules used, Next Mission, Retry).
    /// - Comprehensive Failure Modal (Reason, Time/Capsule status, Retry, Main Menu).
    /// - In-Game Pause & Settings Menu with Master/SFX/Music sliders, Mute, VFX, Screen Shake, Countdown audio toggles.
    /// </summary>
    [SelectionBase]
    public class HUDController : MonoBehaviour
    {
        [Header("Sci-Fi Palette")]
        [SerializeField] private Color cyanNeon = new Color(0f, 0.92f, 1f, 1f);       // #00EAFF
        [SerializeField] private Color magentaNeon = new Color(1f, 0.18f, 0.65f, 1f);  // #FF2EA6
        [SerializeField] private Color amberNeon = new Color(1f, 0.75f, 0f, 1f);       // #FFBF00
        [SerializeField] private Color greenNeon = new Color(0.18f, 0.95f, 0.5f, 1f);   // #2EF280
        [SerializeField] private Color redDanger = new Color(1f, 0.22f, 0.32f, 1f);     // #FF3852
        [SerializeField] private Color panelDark = new Color(0.03f, 0.05f, 0.1f, 0.93f); // #080D1A

        // GUI Styles
        private GUIStyle panelStyle;
        private GUIStyle dockStyle;
        private GUIStyle titleHeaderStyle;
        private GUIStyle cardHeaderStyle;
        private GUIStyle cardBodyStyle;
        private GUIStyle objectiveTextStyle;
        private GUIStyle valueBoldStyle;
        private GUIStyle smallLabelStyle;
        private GUIStyle iconStyle;
        private GUIStyle buttonSciFiStyle;
        private GUIStyle pauseBtnStyle;
        private GUIStyle modalTitleStyle;
        private GUIStyle modalBodyStyle;
        private GUIStyle floatingScoreStyle;

        private Texture2D panelTex;
        private Texture2D dockTex;
        private Texture2D whiteTex;
        private bool isStylesInit = false;

        // Singleton & UI Input Guard
        public static HUDController Instance { get; private set; }

        // Pause & Settings state
        private bool isPaused = false;
        private bool showSettings = false;
        private float viewWidth = 1280f;
        private float viewHeight = 720f;

        private void Awake()
        {
            Instance = this;
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

            // Top HUD area (top bar + objective sub-bar): y in 0..165 reference units
            if (refY <= 165f) return true;

            // Bottom telemetry dock area: y in (viewHeight - 110)..viewHeight reference units
            float viewH = UIScale.GetViewHeight();
            if (refY >= viewH - 110f) return true;

            return false;
        }

        private void InitStyles()
        {
            if (isStylesInit) return;

            whiteTex = MakeTex(2, 2, Color.white);
            panelTex = MakeBorderedTex(64, 64, panelDark, new Color(0f, 0.65f, 0.9f, 0.5f), 1);
            dockTex = MakeBorderedTex(64, 64, new Color(0.02f, 0.04f, 0.08f, 0.94f), new Color(0f, 0.75f, 1f, 0.45f), 1);

            panelStyle = new GUIStyle(GUI.skin.box);
            panelStyle.normal.background = panelTex;
            panelStyle.border = new RectOffset(6, 6, 6, 6);

            dockStyle = new GUIStyle(GUI.skin.box);
            dockStyle.normal.background = dockTex;
            dockStyle.border = new RectOffset(8, 8, 8, 8);

            titleHeaderStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = UIScale.FontHUDTitle, // 26
                fontStyle = FontStyle.Bold,
                normal = { textColor = cyanNeon }
            };

            cardHeaderStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = UIScale.FontCardHeader, // 18
                fontStyle = FontStyle.Bold,
                normal = { textColor = amberNeon }
            };

            cardBodyStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = UIScale.FontBodyDetail, // 16
                fontStyle = FontStyle.Bold,
                wordWrap = true,
                normal = { textColor = new Color(0.85f, 0.92f, 1f, 0.95f) }
            };

            objectiveTextStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = UIScale.FontObjective, // 20
                fontStyle = FontStyle.Bold,
                wordWrap = true,
                normal = { textColor = Color.white }
            };

            smallLabelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = UIScale.FontSmallLabel, // 14
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.7f, 0.82f, 0.95f, 0.85f) }
            };

            valueBoldStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = UIScale.FontValue, // 22
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white }
            };

            iconStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = UIScale.FontValue, // 22
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = cyanNeon }
            };

            buttonSciFiStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = UIScale.FontButton, // 20
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = cyanNeon, background = MakeBorderedTex(32, 32, new Color(0.04f, 0.09f, 0.18f, 0.95f), cyanNeon, 2) },
                hover = { textColor = Color.white, background = MakeBorderedTex(32, 32, new Color(0f, 0.45f, 0.75f, 0.95f), Color.white, 2) }
            };

            pauseBtnStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = UIScale.FontButton, // 20
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = cyanNeon, background = MakeBorderedTex(32, 32, new Color(0.03f, 0.06f, 0.12f, 0.92f), cyanNeon, 2) },
                hover = { textColor = Color.white, background = MakeBorderedTex(32, 32, new Color(0f, 0.5f, 0.8f, 0.95f), Color.white, 2) }
            };

            modalTitleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = UIScale.FontModalTitle, // 32
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = cyanNeon }
            };

            modalBodyStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = UIScale.FontCardHeader, // 18
                alignment = TextAnchor.MiddleCenter,
                wordWrap = true,
                normal = { textColor = Color.white }
            };

            floatingScoreStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 20,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = amberNeon }
            };

            isStylesInit = true;
        }

        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                TogglePause();
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
            InitStyles();

            LevelManager lm = LevelManager.Instance;
            if (lm == null) return;

            Matrix4x4 origMatrix = UIScale.BeginMatrix();
            viewWidth = UIScale.GetViewWidth();
            viewHeight = UIScale.GetViewHeight();

            try
            {
                DrawTopMissionHUD(lm);
                DrawFloatingScores();
                DrawBottomTelemetryDock(lm);
                DrawContextualPopups(lm);

                if (isPaused)
                {
                    if (showSettings) DrawSettingsModal();
                    else DrawPauseModal();
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
            finally
            {
                UIScale.EndMatrix(origMatrix);
            }
        }

        // ==========================================
        // 1. TOP SCI-FI TELEMETRY HUD BAR
        // Layout:
        // MISSION 01       TIME 01:42
        // SCORE 300        CAPSULES 1/2
        // RELAYS 3/5       [❚❚] PAUSE
        // ==========================================
        private void DrawTopMissionHUD(LevelManager lm)
        {
            float barW = Mathf.Min(1180f, viewWidth - 48f);
            float barH = 100f;
            float barX = (viewWidth - barW) / 2f;
            float barY = 18f;

            Rect barRect = new Rect(barX, barY, barW, barH);
            GUI.Box(barRect, GUIContent.none, panelStyle);

            GUILayout.BeginArea(new Rect(barRect.x + 20, barRect.y + 10, barRect.width - 40, barRect.height - 20));
            GUILayout.BeginHorizontal();

            // LEFT COLUMN: Mission ID, Score, Relay Progress
            GUILayout.BeginVertical(GUILayout.Width(barW * 0.42f));
            string diffTag = lm.Difficulty == GameDifficulty.Hard ? " <color=#FF3852>[HARD 1.5x]</color>" : (lm.Difficulty == GameDifficulty.Easy ? " <color=#2EF280>[EASY]</color>" : " <color=#00EAFF>[MED]</color>");
            GUILayout.Label($"MISSION {lm.LevelNumber:D2} : {lm.LevelTitle.ToUpper()}{diffTag}", titleHeaderStyle);

            ScoreManager sm = ScoreManager.Instance;
            int score = sm != null ? sm.DisplayScore : 0;
            int activeRelays = lm.Rings != null ? lm.Rings.ActivatedRelaysCount : 0;
            int totalRelays = lm.Rings != null ? lm.Rings.TotalRelays : 5;

            GUILayout.BeginHorizontal();
            GUIStyle scoreStyle = new GUIStyle(cardHeaderStyle) { normal = { textColor = amberNeon }, fontSize = 18 };
            GUILayout.Label($"SCORE: {score:D4}", scoreStyle, GUILayout.Width(160));

            GUIStyle relayProgStyle = new GUIStyle(cardHeaderStyle)
            {
                fontSize = 18,
                normal = { textColor = (activeRelays >= totalRelays && totalRelays > 0) ? greenNeon : cyanNeon }
            };
            GUILayout.Label($"RELAYS: {activeRelays}/{totalRelays}", relayProgStyle);
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();

            GUILayout.FlexibleSpace();

            // CENTER-RIGHT COLUMN: Countdown Timer & Capsule Allowance
            GUILayout.BeginVertical(GUILayout.Width(barW * 0.36f));
            int secondsLeft = Mathf.Max(0, Mathf.CeilToInt(lm.RemainingTime));
            int mins = secondsLeft / 60;
            int secs = secondsLeft % 60;

            Color timerColor = secondsLeft <= 10 ? redDanger : (secondsLeft <= 30 ? amberNeon : Color.white);
            GUIStyle timerStyle = new GUIStyle(titleHeaderStyle)
            {
                fontSize = 24,
                normal = { textColor = timerColor },
                alignment = TextAnchor.MiddleRight
            };
            GUILayout.Label($"TIME {mins:D2}:{secs:D2}", timerStyle);

            CapsuleLauncher launcher = lm.Launcher;
            int remainingCaps = launcher != null ? launcher.RemainingCapsules : 2;
            int maxCaps = CapsuleLauncher.MaxCapsulesPerMission;

            Color capCol = remainingCaps > 0 ? magentaNeon : redDanger;
            GUIStyle capStyle = new GUIStyle(cardHeaderStyle)
            {
                fontSize = 18,
                normal = { textColor = capCol },
                alignment = TextAnchor.MiddleRight
            };
            GUILayout.Label($"CAPSULES: {remainingCaps}/{maxCaps}", capStyle);

            GUILayout.EndVertical();

            GUILayout.Space(16);

            // FAR-RIGHT: Pause Button [ ❚❚ ]
            GUILayout.BeginVertical(GUILayout.Width(52));
            if (GUILayout.Button("❚❚", pauseBtnStyle, GUILayout.Width(50), GUILayout.Height(50)))
            {
                TogglePause();
            }
            GUILayout.EndVertical();

            GUILayout.EndHorizontal();
            GUILayout.EndArea();

            // SUB-BAR: Current Objective Banner
            float objW = barW * 0.75f;
            float objH = 34f;
            float objX = (viewWidth - objW) / 2f;
            float objY = barY + barH + 6f;
            Rect objRect = new Rect(objX, objY, objW, objH);
            GUI.Box(objRect, GUIContent.none, dockStyle);

            GUIStyle subObjStyle = new GUIStyle(objectiveTextStyle)
            {
                fontSize = 15,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = lm.AreAllRelaysSynchronized() ? greenNeon : Color.white }
            };
            GUI.Label(objRect, lm.CurrentObjectiveText, subObjStyle);
        }

        // ==========================================
        // 2. FLOATING +100 POINT INDICATORS
        // ==========================================
        private void DrawFloatingScores()
        {
            ScoreManager sm = ScoreManager.Instance;
            if (sm == null || sm.FloatingScores == null) return;

            Camera cam = Camera.main;
            if (cam == null) return;

            float currentScale = UIScale.CurrentScale;
            float now = Time.time;

            foreach (var fs in sm.FloatingScores)
            {
                float age = now - fs.SpawnTime;
                if (age >= fs.Duration) continue;

                Vector3 screenPos = cam.WorldToScreenPoint(fs.WorldPosition);
                if (screenPos.z < 0) continue; // Behind camera

                float screenY = Screen.height - screenPos.y; // Invert for GUI
                float refX = screenPos.x / currentScale;
                float refY = screenY / currentScale - (age * 40f); // Drift upward

                float alpha = Mathf.Clamp01(1f - (age / fs.Duration));
                Color col = amberNeon;
                col.a = alpha;

                GUIStyle style = new GUIStyle(floatingScoreStyle);
                style.normal.textColor = col;

                Rect r = new Rect(refX - 60f, refY - 15f, 120f, 30f);
                GUI.Label(r, $"+{fs.Points}", style);
            }
        }

        // ==========================================
        // 3. BOTTOM TELEMETRY DOCK
        // ==========================================
        private void DrawBottomTelemetryDock(LevelManager lm)
        {
            float dockW = Mathf.Min(1060f, viewWidth - 48f);
            float dockH = 90f;
            float dockX = (viewWidth - dockW) / 2f;
            float dockY = viewHeight - dockH - 20f;
            Rect dockRect = new Rect(dockX, dockY, dockW, dockH);

            GUI.Box(dockRect, GUIContent.none, dockStyle);

            float totalInnerW = dockW - 32f;
            float batW = 310f;
            float remainingW = totalInnerW - batW;
            float subW = remainingW / 3f;

            // Section A: Battery Shield Bar
            DrawDockBattery(new Rect(dockX + 16f, dockY + 12f, batW, dockH - 24f), lm);

            // Section B: Speed Dial (turns red > 1.5 m/s)
            DrawDockSpeed(new Rect(dockX + 16f + batW, dockY + 12f, subW, dockH - 24f), lm);

            // Section C: Capsule Status
            DrawDockCapsule(new Rect(dockX + 16f + batW + subW, dockY + 12f, subW, dockH - 24f), lm);

            // Section D: Next Relay / Ring
            DrawDockRing(new Rect(dockX + 16f + batW + subW * 2f, dockY + 12f, subW, dockH - 24f), lm);
        }

        private void DrawDockBattery(Rect r, LevelManager lm)
        {
            float pct = lm.Battery != null ? lm.Battery.BatteryPercentage : 1f;
            Color batCol = pct > 0.4f ? cyanNeon : (pct > 0.2f ? amberNeon : redDanger);

            GUIStyle pctStyle = new GUIStyle(valueBoldStyle)
            {
                fontSize = UIScale.FontValue,
                normal = { textColor = batCol }
            };

            GUI.Label(new Rect(r.x, r.y + 16f, 62f, 32f), $"{(int)(pct * 100)}%", pctStyle);

            GUI.Label(new Rect(r.x + 68f, r.y, r.width - 68f, 20f), "BATTERY SHIELD", smallLabelStyle);

            Rect barBg = new Rect(r.x + 68f, r.y + 24f, 224f, 24f);
            DrawRect(barBg, new Color(0.08f, 0.12f, 0.2f, 0.9f));
            Rect barFill = new Rect(barBg.x + 2f, barBg.y + 2f, (barBg.width - 4f) * Mathf.Clamp01(pct), barBg.height - 4f);
            DrawRect(barFill, batCol);
        }

        private void DrawDockSpeed(Rect r, LevelManager lm)
        {
            float speed = 0f;
            if (lm.Drone != null && lm.Drone.GetComponent<Rigidbody2D>() != null)
            {
                speed = lm.Drone.GetComponent<Rigidbody2D>().linearVelocity.magnitude;
            }

            GUI.Label(new Rect(r.x + 36f, r.y, r.width - 36f, 20f), "VELOCITY", smallLabelStyle);

            Color spdCol = (speed > 1.5f) ? redDanger : greenNeon;

            GUIStyle spdVal = new GUIStyle(valueBoldStyle)
            {
                fontSize = UIScale.FontValue,
                normal = { textColor = spdCol }
            };
            GUI.Label(new Rect(r.x + 36f, r.y + 22f, r.width - 36f, 28f), $"{speed:F1} m/s", spdVal);

            GUIStyle icon = new GUIStyle(iconStyle) { normal = { textColor = spdCol }, fontSize = 24 };
            GUI.Label(new Rect(r.x, r.y + 12f, 32f, 36f), "◷", icon);
        }

        private void DrawDockCapsule(Rect r, LevelManager lm)
        {
            CapsuleLauncher launcher = lm.Launcher;
            int remaining = launcher != null ? launcher.RemainingCapsules : 2;
            bool canFire = launcher != null && launcher.CanFire;

            GUI.Label(new Rect(r.x + 36f, r.y, r.width - 36f, 20f), "CAPSULES", smallLabelStyle);

            string statusText = remaining > 0 ? (canFire ? $"{remaining} READY" : "CHARGING") : "EMPTY (0/2)";
            Color capCol = remaining > 0 ? magentaNeon : redDanger;

            GUIStyle capVal = new GUIStyle(valueBoldStyle)
            {
                fontSize = 18,
                normal = { textColor = capCol }
            };
            GUI.Label(new Rect(r.x + 36f, r.y + 22f, r.width - 36f, 28f), statusText, capVal);

            GUIStyle icon = new GUIStyle(iconStyle) { normal = { textColor = capCol }, fontSize = 22 };
            GUI.Label(new Rect(r.x, r.y + 12f, 32f, 36f), "⬢", icon);
        }

        private void DrawDockRing(Rect r, LevelManager lm)
        {
            int nextRing = lm.Rings != null ? lm.Rings.CurrentExpectedOrder : 1;
            bool allDone = lm.Rings != null && lm.Rings.AllRelaysSynchronized;

            GUI.Label(new Rect(r.x + 36f, r.y, r.width - 36f, 20f), "RELAY GATE", smallLabelStyle);

            GUIStyle ringVal = new GUIStyle(valueBoldStyle)
            {
                fontSize = UIScale.FontValue,
                normal = { textColor = allDone ? greenNeon : amberNeon }
            };
            string text = allDone ? "SYNCHRONIZED" : $"GATE 0{nextRing}";
            GUI.Label(new Rect(r.x + 36f, r.y + 22f, r.width - 36f, 28f), text, ringVal);

            GUIStyle icon = new GUIStyle(iconStyle) { normal = { textColor = amberNeon }, fontSize = 22 };
            GUI.Label(new Rect(r.x, r.y + 12f, 32f, 36f), "◎", icon);
        }

        // ==========================================
        // 4. CONTEXTUAL POPUP NOTIFICATIONS
        // ==========================================
        private void DrawContextualPopups(LevelManager lm)
        {
            float popW = 380f;
            float popH = 76f;
            float popX = viewWidth - popW - 24f;
            float popY = 130f;

            // Global HUD Alert from LevelManager
            if (lm.IsAlertActive())
            {
                Color c = lm.IsCurrentAlertWarning ? redDanger : cyanNeon;
                DrawAlertCard(new Rect(popX, popY, popW, popH), c, lm.IsCurrentAlertWarning ? "⚠ ATTENTION" : "ℹ MISSION UPDATE", lm.CurrentAlertMessage);
                popY += popH + 10f;
            }

            // Low Battery Warning
            if (lm.Battery != null && lm.Battery.IsLowBattery && lm.Battery.CurrentBattery > 0f)
            {
                DrawAlertCard(new Rect(popX, popY, popW, popH), magentaNeon, "⚡ LOW BATTERY", $"{(int)lm.Battery.CurrentBattery}% power reserves remaining");
                popY += popH + 10f;
            }
        }

        private void DrawAlertCard(Rect r, Color accent, string title, string detail)
        {
            GUIStyle card = new GUIStyle(panelStyle);
            card.normal.background = MakeBorderedTex(32, 32, new Color(0.05f, 0.08f, 0.14f, 0.95f), accent, 2);
            GUI.Box(r, GUIContent.none, card);

            GUIStyle hStyle = new GUIStyle(cardHeaderStyle) { normal = { textColor = accent } };
            GUI.Label(new Rect(r.x + 16f, r.y + 10f, r.width - 32f, 24f), title, hStyle);
            GUI.Label(new Rect(r.x + 16f, r.y + 36f, r.width - 32f, 36f), detail, cardBodyStyle);
        }

        // ==========================================
        // 5. PAUSE MENU & SETTINGS MODALS
        // ==========================================
        private void DrawPauseModal()
        {
            DrawModalBackdrop();

            float w = 440f;
            float h = 380f;
            Rect modalRect = new Rect((viewWidth - w) / 2f, (viewHeight - h) / 2f, w, h);

            GUI.Box(modalRect, GUIContent.none, panelStyle);
            GUILayout.BeginArea(new Rect(modalRect.x + 32f, modalRect.y + 24f, modalRect.width - 64f, modalRect.height - 48f));

            GUILayout.Label("MISSION PAUSED", modalTitleStyle);
            GUILayout.Space(16);

            if (GUILayout.Button("RESUME", buttonSciFiStyle, GUILayout.Height(48)))
            {
                AudioManager.Instance?.PlayUISound();
                TogglePause();
            }

            GUILayout.Space(10);

            if (GUILayout.Button("RESTART MISSION", buttonSciFiStyle, GUILayout.Height(48)))
            {
                AudioManager.Instance?.PlayUISound();
                Time.timeScale = 1f;
                LevelManager.Instance?.RestartLevel();
            }

            GUILayout.Space(10);

            if (GUILayout.Button("SETTINGS", buttonSciFiStyle, GUILayout.Height(48)))
            {
                AudioManager.Instance?.PlayUISound();
                showSettings = true;
            }

            GUILayout.Space(10);

            if (GUILayout.Button("MAIN MENU", buttonSciFiStyle, GUILayout.Height(48)))
            {
                AudioManager.Instance?.PlayUISound();
                Time.timeScale = 1f;
                SceneManager.LoadScene("MainMenu");
            }

            GUILayout.EndArea();
        }

        private void DrawSettingsModal()
        {
            DrawModalBackdrop();

            float w = 560f;
            float h = 560f;
            Rect modalRect = new Rect((viewWidth - w) / 2f, (viewHeight - h) / 2f, w, h);

            GUI.Box(modalRect, GUIContent.none, panelStyle);
            GUILayout.BeginArea(new Rect(modalRect.x + 32f, modalRect.y + 20f, modalRect.width - 64f, modalRect.height - 40f));

            GUILayout.Label("SETTINGS", modalTitleStyle);
            GUILayout.Space(12);

            // AUDIO SECTION
            GUILayout.Label($"MASTER VOLUME: {(int)(SettingsManager.MasterVolume * 100)}%", cardHeaderStyle);
            float newMaster = GUILayout.HorizontalSlider(SettingsManager.MasterVolume, 0f, 1f);
            if (Mathf.Abs(newMaster - SettingsManager.MasterVolume) > 0.01f) SettingsManager.SetMasterVolume(newMaster);

            GUILayout.Space(6);
            GUILayout.BeginHorizontal();
            GUILayout.Label($"SFX: {(int)(SettingsManager.SFXVolume * 100)}%", cardBodyStyle, GUILayout.Width(130));
            float newSfx = GUILayout.HorizontalSlider(SettingsManager.SFXVolume, 0f, 1f);
            if (Mathf.Abs(newSfx - SettingsManager.SFXVolume) > 0.01f) SettingsManager.SetSFXVolume(newSfx);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label($"MUSIC: {(int)(SettingsManager.MusicVolume * 100)}%", cardBodyStyle, GUILayout.Width(130));
            float newMusic = GUILayout.HorizontalSlider(SettingsManager.MusicVolume, 0f, 1f);
            if (Mathf.Abs(newMusic - SettingsManager.MusicVolume) > 0.01f) SettingsManager.SetMusicVolume(newMusic);
            GUILayout.EndHorizontal();

            GUILayout.Space(8);
            GUILayout.BeginHorizontal();
            bool muteToggle = GUILayout.Toggle(SettingsManager.IsMuted, " MUTE ALL AUDIO", GUI.skin.toggle);
            if (muteToggle != SettingsManager.IsMuted) SettingsManager.SetMuted(muteToggle);

            bool warnToggle = GUILayout.Toggle(SettingsManager.CountdownAudioEnabled, " COUNTDOWN AUDIO", GUI.skin.toggle);
            if (warnToggle != SettingsManager.CountdownAudioEnabled) SettingsManager.SetCountdownAudio(warnToggle);
            GUILayout.EndHorizontal();

            GUILayout.Space(12);

            // GAMEPLAY & DISPLAY
            GUILayout.BeginHorizontal();
            bool shakeToggle = GUILayout.Toggle(SettingsManager.ScreenShakeEnabled, " SCREEN SHAKE", GUI.skin.toggle);
            if (shakeToggle != SettingsManager.ScreenShakeEnabled) SettingsManager.SetScreenShake(shakeToggle);

            bool vfxToggle = GUILayout.Toggle(SettingsManager.VisualEffectsEnabled, " VISUAL EFFECTS", GUI.skin.toggle);
            if (vfxToggle != SettingsManager.VisualEffectsEnabled) SettingsManager.SetVisualEffects(vfxToggle);
            GUILayout.EndHorizontal();

            GUILayout.Space(8);
            if (GUILayout.Button(Screen.fullScreen ? "SWITCH TO WINDOWED" : "SWITCH TO FULLSCREEN", buttonSciFiStyle, GUILayout.Height(36)))
            {
                SettingsManager.ToggleFullscreen();
            }

            GUILayout.Space(10);
            GUILayout.Label($"DIFFICULTY LEVEL: {SettingsManager.CurrentDifficulty.ToString().ToUpper()}", cardHeaderStyle);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("EASY", SettingsManager.CurrentDifficulty == GameDifficulty.Easy ? pauseBtnStyle : buttonSciFiStyle, GUILayout.Height(36)))
            {
                SettingsManager.SetDifficulty(GameDifficulty.Easy);
            }
            GUILayout.Space(6);
            if (GUILayout.Button("MEDIUM", SettingsManager.CurrentDifficulty == GameDifficulty.Medium ? pauseBtnStyle : buttonSciFiStyle, GUILayout.Height(36)))
            {
                SettingsManager.SetDifficulty(GameDifficulty.Medium);
            }
            GUILayout.Space(6);
            if (GUILayout.Button("HARD (1.5x PTS)", SettingsManager.CurrentDifficulty == GameDifficulty.Hard ? pauseBtnStyle : buttonSciFiStyle, GUILayout.Height(36)))
            {
                SettingsManager.SetDifficulty(GameDifficulty.Hard);
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(10);
            GUILayout.Label("CONTROLS: [WASD] Thrust  |  [Mouse] Aim  |  [Space/LMB] Launch Capsule  |  [R] Quick Restart", smallLabelStyle);

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("‹ BACK", buttonSciFiStyle, GUILayout.Height(46)))
            {
                AudioManager.Instance?.PlayUISound();
                showSettings = false;
            }

            GUILayout.EndArea();
        }

        // ==========================================
        // 6. MISSION RESULTS SCREEN (VICTORY)
        // Shows:
        // - Total score & breakdown
        // - Relays activated / required
        // - Capsules used and remaining
        // - Mission completion time
        // - NEXT MISSION and RETRY options
        // ==========================================
        private void DrawResultsModal(LevelManager lm)
        {
            DrawModalBackdrop();

            float w = 580f;
            float h = 540f;
            Rect modalRect = new Rect((viewWidth - w) / 2f, (viewHeight - h) / 2f, w, h);

            GUI.Box(modalRect, GUIContent.none, panelStyle);
            GUILayout.BeginArea(new Rect(modalRect.x + 36f, modalRect.y + 24f, modalRect.width - 72f, modalRect.height - 48f));

            GUIStyle winTitle = new GUIStyle(modalTitleStyle) { normal = { textColor = greenNeon } };
            GUILayout.Label("MISSION ACCOMPLISHED!", winTitle);
            GUILayout.Label($"Sector {lm.LevelNumber:D2} : {lm.LevelTitle} fully restored.", modalBodyStyle);

            GUILayout.Space(18);

            ScoreManager sm = ScoreManager.Instance;
            int totalScore = sm != null ? sm.TotalScore : 0;
            int baseRelay = sm != null ? sm.BaseRelayScore : 0;
            int compBonus = sm != null ? sm.CompletionBonus : 0;
            int timeBonus = sm != null ? sm.TimeBonus : 0;
            int capBonus = sm != null ? sm.CapsuleBonus : 0;

            int activeRelays = lm.Rings != null ? lm.Rings.ActivatedRelaysCount : 0;
            int totalRelays = lm.Rings != null ? lm.Rings.TotalRelays : 0;

            CapsuleLauncher launcher = lm.Launcher;
            int capsRemaining = launcher != null ? launcher.RemainingCapsules : 0;
            int capsUsed = launcher != null ? launcher.CapsulesUsed : 0;

            int timeElapsedSec = Mathf.FloorToInt(lm.ElapsedTime);
            int eMins = timeElapsedSec / 60;
            int eSecs = timeElapsedSec % 60;

            GUIStyle statHeader = new GUIStyle(cardHeaderStyle) { fontSize = 17, normal = { textColor = cyanNeon } };
            GUIStyle statBody = new GUIStyle(cardBodyStyle) { fontSize = 16 };

            // Stat Table
            GUILayout.Label($"• RELAYS SYNCHRONIZED: {activeRelays}/{totalRelays} (+{baseRelay} pts)", statBody);
            GUILayout.Label($"• TIME ELAPSED: {eMins:D2}:{eSecs:D2} (Time Bonus: +{timeBonus} pts)", statBody);
            GUILayout.Label($"• CAPSULES USED: {capsUsed}  |  REMAINING: {capsRemaining} (+{capBonus} pts)", statBody);
            GUILayout.Label($"• MISSION COMPLETION BONUS: +{compBonus} pts", statBody);
            int diffBonus = sm != null ? sm.DifficultyBonus : 0;
            if (diffBonus > 0)
            {
                GUILayout.Label($"• DIFFICULTY BONUS (HARD 1.5x): +{diffBonus} pts", statBody);
            }

            GUILayout.Space(14);

            GUIStyle bigTotal = new GUIStyle(valueBoldStyle)
            {
                fontSize = 28,
                normal = { textColor = amberNeon },
                alignment = TextAnchor.MiddleCenter
            };
            GUILayout.Label($"TOTAL SCORE: {totalScore}", bigTotal);

            GUILayout.Space(20);

            int maxSectors = GameManager.Instance != null ? GameManager.Instance.MaxRelays : 5;
            string advanceText = (lm.LevelNumber < maxSectors) ? "NEXT MISSION  ›" : "GALACTIC VICTORY";

            GUILayout.BeginHorizontal();
            if (GUILayout.Button(advanceText, buttonSciFiStyle, GUILayout.Height(50)))
            {
                AudioManager.Instance?.PlayUISound();
                Time.timeScale = 1f;
                lm.AdvanceToNextLevel();
            }

            GUILayout.Space(12);

            if (GUILayout.Button("RETRY MISSION", buttonSciFiStyle, GUILayout.Height(50)))
            {
                AudioManager.Instance?.PlayUISound();
                Time.timeScale = 1f;
                lm.RestartLevel();
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(8);

            if (GUILayout.Button("MAIN MENU", buttonSciFiStyle, GUILayout.Height(42)))
            {
                AudioManager.Instance?.PlayUISound();
                Time.timeScale = 1f;
                SceneManager.LoadScene("MainMenu");
            }

            GUILayout.EndArea();
        }

        // ==========================================
        // 7. FAILURE MODAL
        // ==========================================
        private void DrawFailureModal(LevelManager lm)
        {
            DrawModalBackdrop();

            float w = 500f;
            float h = 380f;
            Rect modalRect = new Rect((viewWidth - w) / 2f, (viewHeight - h) / 2f, w, h);

            GUI.Box(modalRect, GUIContent.none, panelStyle);
            GUILayout.BeginArea(new Rect(modalRect.x + 32f, modalRect.y + 24f, modalRect.width - 64f, modalRect.height - 48f));

            GUIStyle failTitle = new GUIStyle(modalTitleStyle) { normal = { textColor = redDanger } };
            GUILayout.Label("MISSION FAILED", failTitle);
            GUILayout.Space(12);

            string failText = string.IsNullOrEmpty(lm.FailureReason) ? "Ship destroyed or power exhausted." : lm.FailureReason.ToUpper();
            GUIStyle failReasonStyle = new GUIStyle(modalBodyStyle)
            {
                fontSize = 20,
                normal = { textColor = Color.white }
            };
            GUILayout.Label(failText, failReasonStyle);

            GUILayout.Space(24);

            if (GUILayout.Button("RETRY [R]", buttonSciFiStyle, GUILayout.Height(50)))
            {
                AudioManager.Instance?.PlayUISound();
                Time.timeScale = 1f;
                lm.RestartLevel();
            }

            GUILayout.Space(12);

            if (GUILayout.Button("MAIN MENU", buttonSciFiStyle, GUILayout.Height(46)))
            {
                AudioManager.Instance?.PlayUISound();
                Time.timeScale = 1f;
                SceneManager.LoadScene("MainMenu");
            }

            GUILayout.EndArea();
        }

        private void DrawModalBackdrop()
        {
            DrawRect(new Rect(0, 0, viewWidth, viewHeight), new Color(0f, 0.02f, 0.06f, 0.88f));
        }

        private void DrawRect(Rect r, Color c)
        {
            Color prev = GUI.color;
            GUI.color = c;
            GUI.DrawTexture(r, whiteTex);
            GUI.color = prev;
        }

        private Texture2D MakeTex(int width, int height, Color col)
        {
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; i++) pix[i] = col;
            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }

        private Texture2D MakeBorderedTex(int width, int height, Color fill, Color border, int borderWidth)
        {
            Texture2D result = new Texture2D(width, height);
            Color[] pix = new Color[width * height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    bool isBorder = (x < borderWidth || x >= width - borderWidth || y < borderWidth || y >= height - borderWidth);
                    pix[y * width + x] = isBorder ? border : fill;
                }
            }
            result.SetPixels(pix);
            result.Apply();
            return result;
        }
    }
}
