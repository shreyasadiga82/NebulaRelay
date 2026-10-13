using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using NebulaRelay.Core;
using NebulaRelay.Player;
using NebulaRelay.Delivery;
using NebulaRelay.World;

namespace NebulaRelay.UI
{
    /// <summary>
    /// HUDController provides the cinematic neon sci-fi HUD matching the game reference designs.
    /// Scaled via UIScale (1280x720 reference) for readability on laptops, 1080p, 1440p, 4K, and ultra-wides.
    /// Features top objective & relay cards, bottom telemetry dock (radial battery, speed dial,
    /// capsule status, next ring badge), and contextual popup cards (Slow Down, Low Battery, Relay Activated, Pause).
    /// </summary>
    [SelectionBase]
    public class HUDController : MonoBehaviour
    {
        [Header("Sci-Fi Palette")]
        [SerializeField] private Color cyanNeon = new Color(0f, 0.92f, 1f, 1f);      // #00EAFF
        [SerializeField] private Color magentaNeon = new Color(1f, 0.18f, 0.65f, 1f); // #FF2EA6
        [SerializeField] private Color amberNeon = new Color(1f, 0.75f, 0f, 1f);      // #FFBF00
        [SerializeField] private Color greenNeon = new Color(0.18f, 0.95f, 0.5f, 1f);  // #2EF280
        [SerializeField] private Color redDanger = new Color(1f, 0.22f, 0.32f, 1f);    // #FF3852
        [SerializeField] private Color panelDark = new Color(0.03f, 0.05f, 0.1f, 0.92f); // #080D1A

        // GUI Styles (All font sizes in 1280x720 reference space >= 14)
        private GUIStyle panelStyle;
        private GUIStyle dockStyle;
        private GUIStyle titleHeaderStyle;
        private GUIStyle sectorSubStyle;
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

        private Texture2D panelTex;
        private Texture2D dockTex;
        private Texture2D whiteTex;
        private bool isStylesInit = false;

        // Pause state
        private bool isPaused = false;
        private bool showSettings = false;
        private float masterVolume = 1.0f;
        private float viewWidth = 1280f;
        private float viewHeight = 720f;

        private void InitStyles()
        {
            if (isStylesInit) return;

            whiteTex = MakeTex(2, 2, Color.white);
            panelTex = MakeBorderedTex(64, 64, panelDark, new Color(0f, 0.6f, 0.85f, 0.45f), 1);
            dockTex = MakeBorderedTex(64, 64, new Color(0.02f, 0.04f, 0.08f, 0.94f), new Color(0f, 0.75f, 1f, 0.4f), 1);

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

            sectorSubStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = UIScale.FontBodyDetail, // 16
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.6f, 0.75f, 0.9f, 0.9f) }
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
            Time.timeScale = isPaused ? 0f : 1f;
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
                DrawTopLeftMissionObjective(lm);
                DrawTopRightRelayIndicator(lm);
                DrawBottomTelemetryDock(lm);
                DrawContextualPopups(lm);

                if (isPaused)
                {
                    DrawPauseModal();
                }
                else if (lm.State == LevelState.Completed)
                {
                    DrawVictoryModal(lm);
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
        // 1. TOP-LEFT: MISSION OBJECTIVE CARD
        // ==========================================
        private void DrawTopLeftMissionObjective(LevelManager lm)
        {
            float width = 450f;
            float height = 145f;
            Rect cardRect = new Rect(24f, 24f, width, height);

            GUI.Box(cardRect, GUIContent.none, panelStyle);
            GUILayout.BeginArea(new Rect(cardRect.x + 18, cardRect.y + 12, cardRect.width - 36, cardRect.height - 24));

            // Title and Sector subheader
            GUILayout.Label("NEBULA RELAY", titleHeaderStyle);
            GUILayout.Label($"SECTOR 0{lm.LevelNumber} — {lm.LevelTitle.ToUpper()}", sectorSubStyle);

            GUILayout.Space(6);

            // Objective description (font size >= 20)
            GUILayout.BeginHorizontal();
            GUILayout.Label("◉", iconStyle, GUILayout.Width(24));
            GUILayout.Label(lm.CurrentObjectiveText, objectiveTextStyle);
            GUILayout.EndHorizontal();

            GUILayout.Space(4);

            // Checkpoint indicators (e.g. RINGS: ● ◎ ○)
            if (lm.Rings != null)
            {
                GUILayout.BeginHorizontal();
                int currentRing = lm.Rings.CurrentExpectedOrder;
                int totalRings = lm.Rings.TotalRings;

                string ringDots = "";
                for (int i = 1; i <= totalRings; i++)
                {
                    if (i < currentRing || lm.Rings.AllRingsCompleted) ringDots += "● ";
                    else if (i == currentRing) ringDots += "◎ ";
                    else ringDots += "○ ";
                }

                GUIStyle dotStyle = new GUIStyle(cardBodyStyle)
                {
                    fontSize = UIScale.FontBodyDetail, // 16
                    fontStyle = FontStyle.Bold,
                    normal = { textColor = cyanNeon }
                };
                GUILayout.Label($"RINGS: {ringDots}", dotStyle);
                GUILayout.EndHorizontal();
            }

            GUILayout.EndArea();
        }

        // ==========================================
        // 2. TOP-RIGHT: RELAY STATUS & PAUSE BUTTON
        // ==========================================
        private void DrawTopRightRelayIndicator(LevelManager lm)
        {
            float cardW = 280f;
            float cardH = 56f;
            float marginX = viewWidth - cardW - 24f;
            Rect area = new Rect(marginX, 24f, cardW, cardH);

            GUI.Box(area, GUIContent.none, panelStyle);
            GUILayout.BeginArea(new Rect(area.x + 14, area.y + 8, area.width - 28, area.height - 16));

            GUILayout.BeginHorizontal();

            int restored = GameManager.Instance != null ? GameManager.Instance.TotalRelaysRestored : (lm.LevelNumber - 1);
            if (lm.State == LevelState.Completed) restored = lm.LevelNumber;
            int total = GameManager.Instance != null ? GameManager.Instance.MaxRelays : 5;

            string relayCircles = "";
            for (int i = 1; i <= total; i++)
            {
                relayCircles += (i <= restored) ? "◆ " : "◇ ";
            }

            GUIStyle relayLabelStyle = new GUIStyle(cardBodyStyle)
            {
                fontSize = UIScale.FontCardHeader, // 18
                fontStyle = FontStyle.Bold,
                normal = { textColor = greenNeon },
                alignment = TextAnchor.MiddleLeft
            };
            GUILayout.Label($"⚡ {restored}/{total} RELAYS  {relayCircles}", relayLabelStyle);

            GUILayout.FlexibleSpace();

            // Pause button [ || ]
            if (GUILayout.Button("❚❚", pauseBtnStyle, GUILayout.Width(44), GUILayout.Height(40)))
            {
                TogglePause();
            }

            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        // ==========================================
        // 3. BOTTOM TELEMETRY DOCK
        // ==========================================
        private void DrawBottomTelemetryDock(LevelManager lm)
        {
            float dockW = Mathf.Min(1040f, viewWidth - 48f);
            float dockH = 92f;
            float dockX = (viewWidth - dockW) / 2f;
            float dockY = viewHeight - dockH - 24f; // 24px safe margin from bottom edge
            Rect dockRect = new Rect(dockX, dockY, dockW, dockH);

            GUI.Box(dockRect, GUIContent.none, dockStyle);

            float totalInnerW = dockW - 32f;
            float batW = 310f;
            float remainingW = totalInnerW - batW;
            float subW = remainingW / 3f;

            // Section A: Battery (with 220x24 px bar)
            DrawDockBattery(new Rect(dockX + 16f, dockY + 12f, batW, dockH - 24f), lm);

            // Section B: Speed Dial (turns red above 1.5 m/s)
            DrawDockSpeed(new Rect(dockX + 16f + batW, dockY + 12f, subW, dockH - 24f), lm);

            // Section C: Capsule Status
            DrawDockCapsule(new Rect(dockX + 16f + batW + subW, dockY + 12f, subW, dockH - 24f), lm);

            // Section D: Next Ring
            DrawDockRing(new Rect(dockX + 16f + batW + subW * 2f, dockY + 12f, subW, dockH - 24f), lm);
        }

        private void DrawDockBattery(Rect r, LevelManager lm)
        {
            float pct = lm.Battery != null ? lm.Battery.BatteryPercentage : 1f;
            Color batCol = pct > 0.4f ? cyanNeon : (pct > 0.2f ? amberNeon : redDanger);

            GUIStyle pctStyle = new GUIStyle(valueBoldStyle)
            {
                fontSize = UIScale.FontValue, // 22
                normal = { textColor = batCol }
            };

            GUI.Label(new Rect(r.x, r.y + 16f, 62f, 32f), $"{(int)(pct * 100)}%", pctStyle);

            // Label & Bar
            GUI.Label(new Rect(r.x + 68f, r.y, r.width - 68f, 20f), "BATTERY SHIELD", smallLabelStyle);

            // Battery bar at least 220x24 reference px
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

            // Turns RED above the 1.5 u/s safe docking limit
            Color spdCol = (speed > 1.5f) ? redDanger : greenNeon;

            GUIStyle spdVal = new GUIStyle(valueBoldStyle)
            {
                fontSize = UIScale.FontValue, // 22
                normal = { textColor = spdCol }
            };
            GUI.Label(new Rect(r.x + 36f, r.y + 22f, r.width - 36f, 28f), $"{speed:F1} m/s", spdVal);

            GUIStyle icon = new GUIStyle(iconStyle) { normal = { textColor = spdCol }, fontSize = 24 };
            GUI.Label(new Rect(r.x, r.y + 12f, 32f, 36f), "◷", icon);
        }

        private void DrawDockCapsule(Rect r, LevelManager lm)
        {
            bool ready = true;
            CapsuleLauncher launcher = lm.Drone != null ? lm.Drone.GetComponent<CapsuleLauncher>() : null;
            if (launcher != null)
            {
                ready = launcher.CanFire;
            }

            GUI.Label(new Rect(r.x + 36f, r.y, r.width - 36f, 20f), "CAPSULE", smallLabelStyle);

            GUIStyle capVal = new GUIStyle(valueBoldStyle)
            {
                fontSize = UIScale.FontButton, // 20
                normal = { textColor = ready ? magentaNeon : new Color(0.5f, 0.5f, 0.65f) }
            };
            GUI.Label(new Rect(r.x + 36f, r.y + 22f, r.width - 36f, 28f), ready ? "READY" : "CHARGING", capVal);

            GUIStyle icon = new GUIStyle(iconStyle) { normal = { textColor = magentaNeon }, fontSize = 22 };
            GUI.Label(new Rect(r.x, r.y + 12f, 32f, 36f), "⬢", icon);
        }

        private void DrawDockRing(Rect r, LevelManager lm)
        {
            int nextRing = lm.Rings != null ? lm.Rings.CurrentExpectedOrder : 1;
            bool allDone = lm.Rings != null && lm.Rings.AllRingsCompleted;

            GUI.Label(new Rect(r.x + 36f, r.y, r.width - 36f, 20f), "NEXT GATE", smallLabelStyle);

            GUIStyle ringVal = new GUIStyle(valueBoldStyle)
            {
                fontSize = UIScale.FontValue, // 22
                normal = { textColor = allDone ? greenNeon : amberNeon }
            };
            string text = allDone ? "ALL CLEAR" : $"GATE 0{nextRing}";
            GUI.Label(new Rect(r.x + 36f, r.y + 22f, r.width - 36f, 28f), text, ringVal);

            GUIStyle icon = new GUIStyle(iconStyle) { normal = { textColor = amberNeon }, fontSize = 22 };
            GUI.Label(new Rect(r.x, r.y + 12f, 32f, 36f), "◎", icon);
        }

        // ==========================================
        // 4. CONTEXTUAL POPUP NOTIFICATIONS
        // ==========================================
        private void DrawContextualPopups(LevelManager lm)
        {
            float popW = 340f;
            float popH = 76f;
            float popX = viewWidth - popW - 24f;
            float popY = 96f;

            // A. Slow Down Warning (if drone is near relay at dangerous speed)
            if (lm.Relay != null && lm.Drone != null && lm.Relay.IsDroneInActivationRadius(lm.Drone.transform.position))
            {
                Rigidbody2D rb = lm.Drone.GetComponent<Rigidbody2D>();
                if (rb != null && rb.linearVelocity.magnitude > lm.Relay.MaxSafeSpeed && lm.Relay.State == RelayState.ReadyForDroneActivation)
                {
                    DrawAlertCard(new Rect(popX, popY, popW, popH), redDanger, "⚠ SLOW DOWN!", "Docking limit: <= 1.5 m/s");
                    popY += popH + 12f;
                }
            }

            // B. Low Battery Warning
            if (lm.Battery != null && lm.Battery.IsLowBattery && lm.Battery.CurrentBattery > 0f)
            {
                DrawAlertCard(new Rect(popX, popY, popW, popH), magentaNeon, "⚡ LOW BATTERY", $"{(int)lm.Battery.CurrentBattery}% reserves remaining");
                popY += popH + 12f;
            }

            // C. Relay Restored Notification
            if (lm.Relay != null && lm.Relay.State == RelayState.Activated)
            {
                DrawAlertCard(new Rect(popX, popY, popW, popH), greenNeon, "✔ RELAY ONLINE", $"Sector 0{lm.LevelNumber} network restored!");
            }
        }

        private void DrawAlertCard(Rect r, Color accent, string title, string detail)
        {
            GUIStyle card = new GUIStyle(panelStyle);
            card.normal.background = MakeBorderedTex(32, 32, new Color(0.05f, 0.08f, 0.14f, 0.95f), accent, 2);
            GUI.Box(r, GUIContent.none, card);

            GUIStyle hStyle = new GUIStyle(cardHeaderStyle) { normal = { textColor = accent } };
            GUI.Label(new Rect(r.x + 16f, r.y + 12f, r.width - 32f, 24f), title, hStyle);
            GUI.Label(new Rect(r.x + 16f, r.y + 38f, r.width - 32f, 26f), detail, cardBodyStyle);
        }

        // ==========================================
        // 5. PAUSE MENU MODAL
        // ==========================================
        private void DrawPauseModal()
        {
            DrawModalBackdrop();

            float w = 440f;
            float h = showSettings ? 380f : 340f;
            Rect modalRect = new Rect((viewWidth - w) / 2f, (viewHeight - h) / 2f, w, h);

            GUI.Box(modalRect, GUIContent.none, panelStyle);
            GUILayout.BeginArea(new Rect(modalRect.x + 32f, modalRect.y + 28f, modalRect.width - 64f, modalRect.height - 56f));

            GUILayout.Label("PAUSED", modalTitleStyle);
            GUILayout.Space(20);

            if (!showSettings)
            {
                if (GUILayout.Button("RESUME FLIGHT", buttonSciFiStyle, GUILayout.Height(50)))
                {
                    TogglePause();
                }

                GUILayout.Space(12);

                if (GUILayout.Button("SETTINGS", buttonSciFiStyle, GUILayout.Height(48)))
                {
                    showSettings = true;
                }

                GUILayout.Space(12);

                if (GUILayout.Button("QUIT TO MENU", buttonSciFiStyle, GUILayout.Height(48)))
                {
                    Time.timeScale = 1f;
                    SceneManager.LoadScene("MainMenu");
                }
            }
            else
            {
                GUILayout.Label($"MASTER AUDIO: {(int)(masterVolume * 100)}%", cardBodyStyle);
                masterVolume = GUILayout.HorizontalSlider(masterVolume, 0f, 1f);
                AudioListener.volume = masterVolume;

                GUILayout.Space(24);

                if (GUILayout.Button("BACK", buttonSciFiStyle, GUILayout.Height(48)))
                {
                    showSettings = false;
                }
            }

            GUILayout.EndArea();
        }

        // ==========================================
        // 6. VICTORY & FAILURE MODALS
        // ==========================================
        private void DrawVictoryModal(LevelManager lm)
        {
            DrawModalBackdrop();

            float w = 480f;
            float h = 320f;
            Rect modalRect = new Rect((viewWidth - w) / 2f, (viewHeight - h) / 2f, w, h);

            GUI.Box(modalRect, GUIContent.none, panelStyle);
            GUILayout.BeginArea(new Rect(modalRect.x + 32f, modalRect.y + 28f, modalRect.width - 64f, modalRect.height - 56f));

            GUILayout.Label("MISSION COMPLETE", modalTitleStyle);
            GUILayout.Space(10);
            GUILayout.Label($"Relay Station 0{lm.LevelNumber} successfully brought online!", modalBodyStyle);

            GUILayout.Space(24);

            int maxSectors = GameManager.Instance != null ? GameManager.Instance.MaxRelays : 5;
            string advanceText = (lm.LevelNumber < maxSectors) ? "NEXT SECTOR  ›" : "GALACTIC VICTORY";
            if (GUILayout.Button(advanceText, buttonSciFiStyle, GUILayout.Height(52)))
            {
                Time.timeScale = 1f;
                if (lm.LevelNumber < maxSectors)
                {
                    SceneManager.LoadScene($"Level_0{lm.LevelNumber + 1}");
                }
                else
                {
                    SceneManager.LoadScene("MainMenu");
                }
            }

            GUILayout.Space(12);

            if (GUILayout.Button("RETURN TO MAIN MENU", buttonSciFiStyle, GUILayout.Height(48)))
            {
                Time.timeScale = 1f;
                SceneManager.LoadScene("MainMenu");
            }

            GUILayout.EndArea();
        }

        private void DrawFailureModal(LevelManager lm)
        {
            DrawModalBackdrop();

            float w = 460f;
            float h = 320f;
            Rect modalRect = new Rect((viewWidth - w) / 2f, (viewHeight - h) / 2f, w, h);

            GUI.Box(modalRect, GUIContent.none, panelStyle);
            GUILayout.BeginArea(new Rect(modalRect.x + 32f, modalRect.y + 28f, modalRect.width - 64f, modalRect.height - 56f));

            GUIStyle failTitle = new GUIStyle(modalTitleStyle) { normal = { textColor = redDanger } };
            GUILayout.Label("MISSION FAILED", failTitle);
            GUILayout.Space(10);
            GUILayout.Label(string.IsNullOrEmpty(lm.FailureReason) ? "Ship destroyed or power exhausted." : lm.FailureReason, modalBodyStyle);

            GUILayout.Space(24);

            if (GUILayout.Button("RETRY SECTOR [R]", buttonSciFiStyle, GUILayout.Height(52)))
            {
                Time.timeScale = 1f;
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }

            GUILayout.Space(12);

            if (GUILayout.Button("RETURN TO MAIN MENU", buttonSciFiStyle, GUILayout.Height(48)))
            {
                Time.timeScale = 1f;
                SceneManager.LoadScene("MainMenu");
            }

            GUILayout.EndArea();
        }

        private void DrawModalBackdrop()
        {
            DrawRect(new Rect(0, 0, viewWidth, viewHeight), new Color(0f, 0.02f, 0.06f, 0.85f));
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
