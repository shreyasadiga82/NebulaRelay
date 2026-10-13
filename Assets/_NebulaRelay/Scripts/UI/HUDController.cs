using UnityEngine;
using UnityEngine.SceneManagement;
using NebulaRelay.Core;
using NebulaRelay.Player;
using NebulaRelay.Delivery;
using NebulaRelay.World;

namespace NebulaRelay.UI
{
    /// <summary>
    /// HUDController provides the cinematic neon sci-fi HUD matching the game reference designs.
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
        [SerializeField] private Color panelDark = new Color(0.03f, 0.05f, 0.1f, 0.88f); // #080D1A

        // GUI Styles
        private GUIStyle panelStyle;
        private GUIStyle dockStyle;
        private GUIStyle titleHeaderStyle;
        private GUIStyle sectorSubStyle;
        private GUIStyle cardHeaderStyle;
        private GUIStyle cardBodyStyle;
        private GUIStyle valueBoldStyle;
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

        private void InitStyles()
        {
            if (isStylesInit) return;

            whiteTex = MakeTex(2, 2, Color.white);
            panelTex = MakeBorderedTex(64, 64, panelDark, new Color(0f, 0.6f, 0.85f, 0.4f), 1);
            dockTex = MakeBorderedTex(64, 64, new Color(0.02f, 0.04f, 0.08f, 0.92f), new Color(0f, 0.75f, 1f, 0.35f), 1);

            panelStyle = new GUIStyle(GUI.skin.box);
            panelStyle.normal.background = panelTex;
            panelStyle.border = new RectOffset(6, 6, 6, 6);

            dockStyle = new GUIStyle(GUI.skin.box);
            dockStyle.normal.background = dockTex;
            dockStyle.border = new RectOffset(8, 8, 8, 8);

            titleHeaderStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 18,
                fontStyle = FontStyle.Bold,
                normal = { textColor = cyanNeon }
            };

            sectorSubStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.6f, 0.75f, 0.9f, 0.8f) }
            };

            cardHeaderStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                normal = { textColor = amberNeon }
            };

            cardBodyStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11,
                wordWrap = true,
                normal = { textColor = new Color(0.85f, 0.92f, 1f, 0.95f) }
            };

            valueBoldStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white }
            };

            iconStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = cyanNeon }
            };

            buttonSciFiStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 13,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = cyanNeon, background = MakeBorderedTex(32, 32, new Color(0.04f, 0.09f, 0.18f, 0.95f), cyanNeon, 1) },
                hover = { textColor = Color.white, background = MakeBorderedTex(32, 32, new Color(0f, 0.4f, 0.7f, 0.95f), Color.white, 1) }
            };

            pauseBtnStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = cyanNeon, background = MakeBorderedTex(32, 32, new Color(0.03f, 0.06f, 0.12f, 0.9f), cyanNeon, 1) },
                hover = { textColor = Color.white, background = MakeBorderedTex(32, 32, new Color(0f, 0.5f, 0.8f, 0.95f), Color.white, 1) }
            };

            modalTitleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 20,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = cyanNeon }
            };

            modalBodyStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white }
            };

            isStylesInit = true;
        }

        private void Update()
        {
            // Toggle pause on Escape key
            if (Input.GetKeyDown(KeyCode.Escape))
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

        // ==========================================
        // 1. TOP-LEFT: MISSION OBJECTIVE CARD
        // ==========================================
        private void DrawTopLeftMissionObjective(LevelManager lm)
        {
            float width = 360f;
            float height = 110f;
            Rect cardRect = new Rect(24f, 20f, width, height);

            GUI.Box(cardRect, GUIContent.none, panelStyle);
            GUILayout.BeginArea(new Rect(cardRect.x + 16, cardRect.y + 10, cardRect.width - 32, cardRect.height - 20));

            // Title and Sector subheader
            GUILayout.Label("NEBULA RELAY", titleHeaderStyle);
            GUILayout.Label($"SECTOR 0{lm.LevelNumber} — {lm.LevelTitle.ToUpper()}", sectorSubStyle);

            GUILayout.Space(4);

            // Objective description
            GUILayout.BeginHorizontal();
            GUILayout.Label("◉", iconStyle, GUILayout.Width(16));
            GUILayout.Label(lm.CurrentObjectiveText, cardBodyStyle);
            GUILayout.EndHorizontal();

            GUILayout.Space(6);

            // Checkpoint indicators (e.g. 1/2 ● ○)
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
                    fontSize = 12,
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
            float cardW = 200f;
            float cardH = 44f;
            float marginX = Screen.width - cardW - 24f;
            Rect area = new Rect(marginX, 20f, cardW, cardH);

            GUI.Box(area, GUIContent.none, panelStyle);
            GUILayout.BeginArea(new Rect(area.x + 12, area.y + 6, area.width - 24, area.height - 12));

            GUILayout.BeginHorizontal();

            // Relays count with indicators
            int restored = GameManager.Instance != null ? GameManager.Instance.TotalRelaysRestored : (lm.LevelNumber - 1);
            if (lm.State == LevelState.Completed) restored = lm.LevelNumber;
            int total = GameManager.Instance != null ? GameManager.Instance.MaxRelays : 3;

            string relayCircles = "";
            for (int i = 1; i <= total; i++)
            {
                relayCircles += (i <= restored) ? "◆ " : "◇ ";
            }

            GUIStyle relayLabelStyle = new GUIStyle(cardBodyStyle)
            {
                fontSize = 11,
                fontStyle = FontStyle.Bold,
                normal = { textColor = greenNeon },
                alignment = TextAnchor.MiddleLeft
            };
            GUILayout.Label($"⚡ {restored}/{total} RELAYS  {relayCircles}", relayLabelStyle);

            GUILayout.FlexibleSpace();

            // Pause button [ || ]
            if (GUILayout.Button("❚❚", pauseBtnStyle, GUILayout.Width(32), GUILayout.Height(28)))
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
            float dockW = Mathf.Min(820f, Screen.width - 48f);
            float dockH = 68f;
            float dockX = (Screen.width - dockW) / 2f;
            float dockY = Screen.height - dockH - 20f;
            Rect dockRect = new Rect(dockX, dockY, dockW, dockH);

            GUI.Box(dockRect, GUIContent.none, dockStyle);

            // Sub-sections inside dock
            float sectionW = (dockW - 32f) / 4f;

            // Section A: Battery
            DrawDockBattery(new Rect(dockX + 16f, dockY + 12f, sectionW, dockH - 24f), lm);

            // Section B: Speed Dial
            DrawDockSpeed(new Rect(dockX + 16f + sectionW, dockY + 12f, sectionW, dockH - 24f), lm);

            // Section C: Capsule Status
            DrawDockCapsule(new Rect(dockX + 16f + sectionW * 2f, dockY + 12f, sectionW, dockH - 24f), lm);

            // Section D: Next Ring
            DrawDockRing(new Rect(dockX + 16f + sectionW * 3f, dockY + 12f, sectionW, dockH - 24f), lm);
        }

        private void DrawDockBattery(Rect r, LevelManager lm)
        {
            float pct = lm.Battery != null ? lm.Battery.BatteryPercentage : 1f;
            Color batCol = pct > 0.4f ? cyanNeon : (pct > 0.2f ? amberNeon : redDanger);

            GUIStyle pctStyle = new GUIStyle(valueBoldStyle)
            {
                fontSize = 16,
                normal = { textColor = batCol }
            };

            GUI.Label(new Rect(r.x, r.y, 50f, r.height), $"{(int)(pct * 100)}%", pctStyle);

            // Label & Bar
            GUIStyle subStyle = new GUIStyle(sectorSubStyle) { fontSize = 10 };
            GUI.Label(new Rect(r.x + 55f, r.y - 2f, r.width - 55f, 16f), "BATTERY", subStyle);

            Rect barBg = new Rect(r.x + 55f, r.y + 18f, r.width - 65f, 8f);
            DrawRect(barBg, new Color(0.1f, 0.15f, 0.25f, 0.8f));
            Rect barFill = new Rect(barBg.x, barBg.y, barBg.width * Mathf.Clamp01(pct), barBg.height);
            DrawRect(barFill, batCol);
        }

        private void DrawDockSpeed(Rect r, LevelManager lm)
        {
            float speed = 0f;
            if (lm.Drone != null && lm.Drone.GetComponent<Rigidbody2D>() != null)
            {
                speed = lm.Drone.GetComponent<Rigidbody2D>().linearVelocity.magnitude;
            }

            GUIStyle lbl = new GUIStyle(sectorSubStyle) { fontSize = 10 };
            GUI.Label(new Rect(r.x + 28f, r.y - 2f, r.width - 28f, 16f), "SPEED", lbl);

            Color spdCol = (lm.Relay != null && lm.Relay.IsDroneInActivationRadius(lm.Drone.transform.position) && speed > lm.Relay.MaxSafeSpeed)
                ? amberNeon : Color.white;

            GUIStyle spdVal = new GUIStyle(valueBoldStyle)
            {
                fontSize = 15,
                normal = { textColor = spdCol }
            };
            GUI.Label(new Rect(r.x + 28f, r.y + 14f, r.width - 28f, 22f), $"{speed:F1} m/s", spdVal);

            GUIStyle icon = new GUIStyle(iconStyle) { normal = { textColor = cyanNeon }, fontSize = 18 };
            GUI.Label(new Rect(r.x, r.y + 4f, 26f, 30f), "◷", icon);
        }

        private void DrawDockCapsule(Rect r, LevelManager lm)
        {
            bool ready = true;
            CapsuleLauncher launcher = lm.Drone != null ? lm.Drone.GetComponent<CapsuleLauncher>() : null;
            if (launcher != null)
            {
                ready = launcher.CanFire;
            }

            GUIStyle lbl = new GUIStyle(sectorSubStyle) { fontSize = 10 };
            GUI.Label(new Rect(r.x + 30f, r.y - 2f, r.width - 30f, 16f), "CAPSULE", lbl);

            GUIStyle capVal = new GUIStyle(valueBoldStyle)
            {
                fontSize = 14,
                normal = { textColor = ready ? magentaNeon : new Color(0.5f, 0.5f, 0.6f) }
            };
            GUI.Label(new Rect(r.x + 30f, r.y + 14f, r.width - 30f, 22f), ready ? "READY" : "CHARGING", capVal);

            GUIStyle icon = new GUIStyle(iconStyle) { normal = { textColor = magentaNeon }, fontSize = 16 };
            GUI.Label(new Rect(r.x, r.y + 4f, 26f, 30f), "⬢", icon);
        }

        private void DrawDockRing(Rect r, LevelManager lm)
        {
            int nextRing = lm.Rings != null ? lm.Rings.CurrentExpectedOrder : 1;
            bool allDone = lm.Rings != null && lm.Rings.AllRingsCompleted;

            GUIStyle lbl = new GUIStyle(sectorSubStyle) { fontSize = 10 };
            GUI.Label(new Rect(r.x + 30f, r.y - 2f, r.width - 30f, 16f), "NEXT RING", lbl);

            GUIStyle ringVal = new GUIStyle(valueBoldStyle)
            {
                fontSize = 15,
                normal = { textColor = allDone ? greenNeon : amberNeon }
            };
            string text = allDone ? "CLEAR" : $"0{nextRing}";
            GUI.Label(new Rect(r.x + 30f, r.y + 14f, r.width - 30f, 22f), text, ringVal);

            GUIStyle icon = new GUIStyle(iconStyle) { normal = { textColor = amberNeon }, fontSize = 16 };
            GUI.Label(new Rect(r.x, r.y + 4f, 26f, 30f), "◎", icon);
        }

        // ==========================================
        // 4. CONTEXTUAL POPUP NOTIFICATIONS
        // ==========================================
        private void DrawContextualPopups(LevelManager lm)
        {
            float popW = 280f;
            float popH = 64f;
            float popX = Screen.width - popW - 24f;
            float popY = 80f;

            // A. Slow Down Warning (if drone is near relay at dangerous speed)
            if (lm.Relay != null && lm.Drone != null && lm.Relay.IsDroneInActivationRadius(lm.Drone.transform.position))
            {
                Rigidbody2D rb = lm.Drone.GetComponent<Rigidbody2D>();
                if (rb != null && rb.linearVelocity.magnitude > lm.Relay.MaxSafeSpeed && lm.Relay.State == RelayState.ReadyForDroneActivation)
                {
                    DrawAlertCard(new Rect(popX, popY, popW, popH), amberNeon, "⚠ SLOW DOWN", "Enter the activation zone at <= 1.5 m/s");
                    popY += popH + 12f;
                }
            }

            // B. Low Battery Warning
            if (lm.Battery != null && lm.Battery.IsLowBattery && lm.Battery.CurrentBattery > 0f)
            {
                DrawAlertCard(new Rect(popX, popY, popW, popH), magentaNeon, "⚡ LOW BATTERY", $"{(int)lm.Battery.CurrentBattery}% remaining");
                popY += popH + 12f;
            }

            // C. Relay Restored Notification
            if (lm.Relay != null && lm.Relay.State == RelayState.Activated)
            {
                DrawAlertCard(new Rect(popX, popY, popW, popH), greenNeon, "✔ RELAY ACTIVATED", $"Sector {lm.LevelNumber} signal restored!");
            }
        }

        private void DrawAlertCard(Rect r, Color accent, string title, string detail)
        {
            GUIStyle card = new GUIStyle(panelStyle);
            card.normal.background = MakeBorderedTex(32, 32, new Color(0.05f, 0.08f, 0.14f, 0.95f), accent, 1);
            GUI.Box(r, GUIContent.none, card);

            GUIStyle hStyle = new GUIStyle(cardHeaderStyle) { normal = { textColor = accent } };
            GUI.Label(new Rect(r.x + 14f, r.y + 10f, r.width - 28f, 20f), title, hStyle);
            GUI.Label(new Rect(r.x + 14f, r.y + 32f, r.width - 28f, 24f), detail, cardBodyStyle);
        }

        // ==========================================
        // 5. PAUSE MENU
        // ==========================================
        private void DrawPauseModal()
        {
            DrawModalBackdrop();

            float w = 320f;
            float h = showSettings ? 280f : 240f;
            Rect modalRect = new Rect((Screen.width - w) / 2f, (Screen.height - h) / 2f, w, h);

            GUI.Box(modalRect, GUIContent.none, panelStyle);
            GUILayout.BeginArea(new Rect(modalRect.x + 24f, modalRect.y + 20f, modalRect.width - 48f, modalRect.height - 40f));

            GUILayout.Label("PAUSED", modalTitleStyle);
            GUILayout.Space(14);

            if (!showSettings)
            {
                if (GUILayout.Button("RESUME", buttonSciFiStyle, GUILayout.Height(36)))
                {
                    TogglePause();
                }

                GUILayout.Space(8);

                if (GUILayout.Button("SETTINGS", buttonSciFiStyle, GUILayout.Height(36)))
                {
                    showSettings = true;
                }

                GUILayout.Space(8);

                if (GUILayout.Button("QUIT TO MENU", buttonSciFiStyle, GUILayout.Height(36)))
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

                GUILayout.Space(16);

                if (GUILayout.Button("BACK", buttonSciFiStyle, GUILayout.Height(36)))
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

            float w = 360f;
            float h = 240f;
            Rect modalRect = new Rect((Screen.width - w) / 2f, (Screen.height - h) / 2f, w, h);

            GUI.Box(modalRect, GUIContent.none, panelStyle);
            GUILayout.BeginArea(new Rect(modalRect.x + 24f, modalRect.y + 20f, modalRect.width - 48f, modalRect.height - 40f));

            GUILayout.Label("MISSION COMPLETE", modalTitleStyle);
            GUILayout.Space(6);
            GUILayout.Label($"Relay {lm.LevelNumber} successfully online!", modalBodyStyle);

            GUILayout.Space(16);

            int maxSectors = GameManager.Instance != null ? GameManager.Instance.MaxRelays : 5;
            string advanceText = (lm.LevelNumber < maxSectors) ? "NEXT SECTOR  ›" : "GALACTIC VICTORY — RETURN TO BASE";
            if (GUILayout.Button(advanceText, buttonSciFiStyle, GUILayout.Height(38)))
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

            GUILayout.Space(8);

            if (GUILayout.Button("MAIN MENU", buttonSciFiStyle, GUILayout.Height(32)))
            {
                Time.timeScale = 1f;
                SceneManager.LoadScene("MainMenu");
            }

            GUILayout.EndArea();
        }

        private void DrawFailureModal(LevelManager lm)
        {
            DrawModalBackdrop();

            float w = 340f;
            float h = 230f;
            Rect modalRect = new Rect((Screen.width - w) / 2f, (Screen.height - h) / 2f, w, h);

            GUI.Box(modalRect, GUIContent.none, panelStyle);
            GUILayout.BeginArea(new Rect(modalRect.x + 24f, modalRect.y + 20f, modalRect.width - 48f, modalRect.height - 40f));

            GUIStyle failTitle = new GUIStyle(modalTitleStyle) { normal = { textColor = redDanger } };
            GUILayout.Label("MISSION FAILED", failTitle);
            GUILayout.Space(6);
            GUILayout.Label(string.IsNullOrEmpty(lm.FailureReason) ? "Battery depleted in deep space." : lm.FailureReason, modalBodyStyle);

            GUILayout.Space(16);

            if (GUILayout.Button("RETRY SECTOR [R]", buttonSciFiStyle, GUILayout.Height(38)))
            {
                Time.timeScale = 1f;
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }

            GUILayout.Space(8);

            if (GUILayout.Button("MAIN MENU", buttonSciFiStyle, GUILayout.Height(32)))
            {
                Time.timeScale = 1f;
                SceneManager.LoadScene("MainMenu");
            }

            GUILayout.EndArea();
        }

        private void DrawModalBackdrop()
        {
            DrawRect(new Rect(0, 0, Screen.width, Screen.height), new Color(0f, 0.02f, 0.05f, 0.75f));
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
