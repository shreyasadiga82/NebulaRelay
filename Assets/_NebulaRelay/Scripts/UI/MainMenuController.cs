using UnityEngine;
using UnityEngine.SceneManagement;
using NebulaRelay.Core;

namespace NebulaRelay.UI
{
    public enum MenuState
    {
        Title,
        MissionSelect,
        Settings
    }

    /// <summary>
    /// MainMenuController provides the cinematic neon sci-fi title and mission selection screen
    /// matching the reference image designs (Screens 1 & 2).
    /// </summary>
    [SelectionBase]
    public class MainMenuController : MonoBehaviour
    {
        [Header("Sci-Fi Palette")]
        [SerializeField] private Color cyanNeon = new Color(0f, 0.92f, 1f, 1f);
        [SerializeField] private Color amberNeon = new Color(1f, 0.75f, 0f, 1f);
        [SerializeField] private Color greenNeon = new Color(0.18f, 0.95f, 0.5f, 1f);
        [SerializeField] private Color panelDark = new Color(0.03f, 0.05f, 0.1f, 0.92f);

        private MenuState currentState = MenuState.Title;
        private int selectedMissionIndex = 1; // 1, 2, or 3
        private float audioVolume = 1.0f;

        // Styles
        private GUIStyle titleMainStyle;
        private GUIStyle subtitleStyle;
        private GUIStyle primaryBtnStyle;
        private GUIStyle secondaryBtnStyle;
        private GUIStyle cardSelectStyle;
        private GUIStyle cardUnselectStyle;
        private GUIStyle panelBoxStyle;
        private GUIStyle headerSubStyle;
        private GUIStyle bodyStyle;
        private Texture2D whiteTex;
        private bool stylesInit = false;

        private void InitStyles()
        {
            if (stylesInit) return;

            whiteTex = MakeTex(2, 2, Color.white);

            titleMainStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 38,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = cyanNeon }
            };

            subtitleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(0.6f, 0.75f, 0.9f) }
            };

            headerSubStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                normal = { textColor = amberNeon }
            };

            bodyStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                wordWrap = true,
                normal = { textColor = new Color(0.85f, 0.92f, 1f) }
            };

            panelBoxStyle = new GUIStyle(GUI.skin.box);
            panelBoxStyle.normal.background = MakeBorderedTex(64, 64, panelDark, new Color(0f, 0.7f, 1f, 0.4f), 1);
            panelBoxStyle.border = new RectOffset(6, 6, 6, 6);

            primaryBtnStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 15,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white, background = MakeBorderedTex(32, 32, new Color(0f, 0.4f, 0.7f, 0.95f), cyanNeon, 2) },
                hover = { textColor = cyanNeon, background = MakeBorderedTex(32, 32, new Color(0f, 0.55f, 0.9f, 0.95f), Color.white, 2) }
            };

            secondaryBtnStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 13,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = cyanNeon, background = MakeBorderedTex(32, 32, new Color(0.04f, 0.08f, 0.16f, 0.9f), new Color(0f, 0.5f, 0.8f, 0.5f), 1) },
                hover = { textColor = Color.white, background = MakeBorderedTex(32, 32, new Color(0f, 0.35f, 0.6f, 0.95f), cyanNeon, 1) }
            };

            cardSelectStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 13,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = Color.white, background = MakeBorderedTex(32, 32, new Color(0.06f, 0.14f, 0.28f, 0.95f), cyanNeon, 2) }
            };

            cardUnselectStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = new Color(0.7f, 0.8f, 0.9f), background = MakeBorderedTex(32, 32, new Color(0.03f, 0.06f, 0.12f, 0.85f), new Color(0.1f, 0.25f, 0.45f, 0.4f), 1) },
                hover = { textColor = Color.white, background = MakeBorderedTex(32, 32, new Color(0.05f, 0.1f, 0.2f, 0.9f), cyanNeon, 1) }
            };

            stylesInit = true;
        }

        private void OnGUI()
        {
            InitStyles();

            switch (currentState)
            {
                case MenuState.Title:
                    DrawTitleScreen();
                    break;
                case MenuState.MissionSelect:
                    DrawMissionSelectScreen();
                    break;
                case MenuState.Settings:
                    DrawSettingsScreen();
                    break;
            }
        }

        // ==========================================
        // 1. OPENING SCREEN (TITLE / START)
        // ==========================================
        private void DrawTitleScreen()
        {
            float w = 440f;
            float h = 400f;
            float x = 60f; // Left side of widescreen like reference 1
            float y = (Screen.height - h) / 2f + 20f;
            Rect menuRect = new Rect(x, y, w, h);

            GUI.Box(menuRect, GUIContent.none, panelBoxStyle);
            GUILayout.BeginArea(new Rect(menuRect.x + 28, menuRect.y + 24, menuRect.width - 56, menuRect.height - 48));

            // Logo & Title
            GUILayout.Label("NEBULA RELAY", titleMainStyle);
            GUILayout.Label("THE LAST SIGNAL", subtitleStyle);

            GUILayout.Space(24);

            // Button Deck
            if (GUILayout.Button("NEW TRANSMISSION  ›", primaryBtnStyle, GUILayout.Height(44)))
            {
                selectedMissionIndex = 1;
                currentState = MenuState.MissionSelect;
            }

            GUILayout.Space(10);

            if (GUILayout.Button("CONTINUE MISSION", secondaryBtnStyle, GUILayout.Height(38)))
            {
                currentState = MenuState.MissionSelect;
            }

            GUILayout.Space(10);

            if (GUILayout.Button("SETTINGS", secondaryBtnStyle, GUILayout.Height(38)))
            {
                currentState = MenuState.Settings;
            }

            GUILayout.Space(10);

            if (GUILayout.Button("QUIT", secondaryBtnStyle, GUILayout.Height(38)))
            {
                Application.Quit();
            }

            GUILayout.FlexibleSpace();

            GUIStyle tagStyle = new GUIStyle(subtitleStyle)
            {
                fontSize = 10,
                normal = { textColor = new Color(0.4f, 0.6f, 0.8f) }
            };
            GUILayout.Label("FIVE RELAYS. ONE SIGNAL. NO SECOND CHANCES.", tagStyle);

            GUILayout.EndArea();
        }

        // ==========================================
        // 2. MISSION SELECTION SCREEN
        // ==========================================
        private void DrawMissionSelectScreen()
        {
            float totalW = Mathf.Min(980f, Screen.width - 40f);
            float totalH = 520f;
            float posX = (Screen.width - totalW) / 2f;
            float posY = (Screen.height - totalH) / 2f;

            Rect mainRect = new Rect(posX, posY, totalW, totalH);
            GUI.Box(mainRect, GUIContent.none, panelBoxStyle);

            GUILayout.BeginArea(new Rect(posX + 24, posY + 16, totalW - 48, totalH - 32));

            // Top Breadcrumb Header
            GUILayout.BeginHorizontal();
            GUILayout.Label("NEBULA RELAY  /  SELECT MISSION", titleMainStyle, GUILayout.Height(34));
            GUILayout.FlexibleSpace();
            int totalRestored = GameManager.Instance != null ? GameManager.Instance.TotalRelaysRestored : 0;
            int maxR = GameManager.Instance != null ? GameManager.Instance.MaxRelays : 5;
            GUILayout.Label($"STABILITY 100%  |  RELAYS {totalRestored}/{maxR}", headerSubStyle);
            GUILayout.EndHorizontal();

            GUILayout.Space(12);

            GUILayout.BeginHorizontal();

            // LEFT COLUMN: 5 MISSION CARDS
            float colW = (totalW - 64f) * 0.52f;
            GUILayout.BeginVertical(GUILayout.Width(colW));

            DrawMissionCard(1, "01  FIRST SIGNAL", "Calm orbital corridor. Sequential checkpoints.");
            GUILayout.Space(6);
            DrawMissionCard(2, "02  THE DEBRIS FIELD", "Asteroid hazards. Tight routes. Shield preservation.");
            GUILayout.Space(6);
            DrawMissionCard(3, "03  THE ION STORM", "Turbulent moving hazard slaloms. Precision flight.");
            GUILayout.Space(6);
            DrawMissionCard(4, "04  THE GRAVITY WELL", "Gravitational vortex corridors. Dense obstacles.");
            GUILayout.Space(6);
            DrawMissionCard(5, "05  THE OMEGA TRANSMISSION", "Grand galactic nexus. Final master relay station.");

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("‹  BACK TO MENU", secondaryBtnStyle, GUILayout.Width(180), GUILayout.Height(32)))
            {
                currentState = MenuState.Title;
            }

            GUILayout.EndVertical();

            GUILayout.Space(20);

            // RIGHT COLUMN: MISSION OVERVIEW PANEL
            float overW = (totalW - 64f) * 0.48f;
            GUILayout.BeginVertical(panelBoxStyle, GUILayout.Width(overW), GUILayout.Height(totalH - 80f));
            GUILayout.Space(8);

            GUILayout.Label("MISSION OVERVIEW", headerSubStyle);
            GUILayout.Space(8);

            string desc = "";
            string targets = "";
            if (selectedMissionIndex == 1)
            {
                desc = "Sector 1 presents a calm orbital flight corridor. Fly through 4 rings in sequential order, aim the energy capsule at the receiver dish, and dock at the relay station at <= 1.5 m/s.";
                targets = "RINGS: 4  |  HAZARDS: None  |  SAFE SPEED: <= 1.5 m/s";
            }
            else if (selectedMissionIndex == 2)
            {
                desc = "Sector 2 traverses an active asteroid debris field. Avoid dense collisions that drain battery shields, navigate 5 checkpoint gates, and power the second relay station.";
                targets = "RINGS: 5  |  HAZARDS: 3 Asteroid Belts  |  SAFE SPEED: <= 1.5 m/s";
            }
            else if (selectedMissionIndex == 3)
            {
                desc = "High-energy ion turbulence creates moving asteroid slaloms. Thread through 5 orbital gates under dynamic cross-drift to activate the third relay.";
                targets = "RINGS: 5  |  HAZARDS: Moving Asteroids  |  SAFE SPEED: <= 1.5 m/s";
            }
            else if (selectedMissionIndex == 4)
            {
                desc = "Golden gravitational anomalies pull drifting space debris into tight corridors. Precision thrust is required across 5 checkpoints to prevent kinetic damage.";
                targets = "RINGS: 5  |  HAZARDS: Orbiting Hazards  |  SAFE SPEED: <= 1.5 m/s";
            }
            else
            {
                desc = "The master galactic nexus hub. Navigate 6 checkpoint rings across an epic asteroid gauntlet and deliver the final master signal to achieve total galactic recovery.";
                targets = "RINGS: 6  |  HAZARDS: Gauntlet Belt  |  SAFE SPEED: <= 1.5 m/s";
            }

            GUILayout.Label(desc, bodyStyle);
            GUILayout.Space(10);
            GUILayout.Label(targets, headerSubStyle);

            GUILayout.FlexibleSpace();

            // Large Start Mission Button
            if (GUILayout.Button("START MISSION  ›", primaryBtnStyle, GUILayout.Height(44)))
            {
                SceneManager.LoadScene($"Level_0{selectedMissionIndex}");
            }

            GUILayout.Space(8);
            GUILayout.EndVertical();

            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        private void DrawMissionCard(int index, string title, string subtitle)
        {
            bool isSelected = (selectedMissionIndex == index);
            GUIStyle cardStyle = isSelected ? cardSelectStyle : cardUnselectStyle;

            if (GUILayout.Button($"   {title}  —  {subtitle}", cardStyle, GUILayout.Height(52)))
            {
                selectedMissionIndex = index;
            }
        }

        // ==========================================
        // 3. SETTINGS SCREEN
        // ==========================================
        private void DrawSettingsScreen()
        {
            float w = 460f;
            float h = 380f;
            float x = (Screen.width - w) / 2f;
            float y = (Screen.height - h) / 2f;
            Rect r = new Rect(x, y, w, h);

            GUI.Box(r, GUIContent.none, panelBoxStyle);
            GUILayout.BeginArea(new Rect(r.x + 28, r.y + 24, r.width - 56, r.height - 48));

            GUILayout.Label("SETTINGS & TELEMETRY", titleMainStyle);
            GUILayout.Space(16);

            GUILayout.Label($"MASTER AUDIO VOLUME: {(int)(audioVolume * 100)}%", headerSubStyle);
            audioVolume = GUILayout.HorizontalSlider(audioVolume, 0f, 1f);
            AudioListener.volume = audioVolume;

            GUILayout.Space(16);

            GUILayout.Label("FLIGHT CONTROLS:", headerSubStyle);
            GUILayout.Label("• [W / A / S / D] or [Arrows] — Drone Inertial Thrusters\n• [Mouse Cursor] — Real-Time Trajectory Aim\n• [Space] or [LMB] — Energy Capsule Launch\n• [R] — Quick Sector Restart\n• [Escape] — Mission Pause", bodyStyle);

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("‹  BACK", secondaryBtnStyle, GUILayout.Height(36)))
            {
                currentState = MenuState.Title;
            }

            GUILayout.EndArea();
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
