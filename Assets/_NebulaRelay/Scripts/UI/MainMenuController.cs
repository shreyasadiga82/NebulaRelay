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
    /// Scaled via UIScale (1280x720 reference) for readability across laptops and high-res displays.
    /// Dynamically scales MainMenu_CosmicBG to cover the screen on all aspect ratios.
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
        private int selectedMissionIndex = 1; // 1 to 5
        private float audioVolume = 1.0f;
        private float viewWidth = 1280f;
        private float viewHeight = 720f;

        // Styles (All font sizes in 1280x720 reference space >= 14)
        private GUIStyle titleMainStyle;
        private GUIStyle subtitleStyle;
        private GUIStyle primaryBtnStyle;
        private GUIStyle secondaryBtnStyle;
        private GUIStyle cardSelectStyle;
        private GUIStyle cardUnselectStyle;
        private GUIStyle panelBoxStyle;
        private GUIStyle headerSubStyle;
        private GUIStyle bodyStyle;
        private GUIStyle taglineStyle;
        private Texture2D whiteTex;
        private bool stylesInit = false;

        // Dynamic background reference
        private Transform bgTransform;
        private SpriteRenderer bgRenderer;

        private void Start()
        {
            GameObject bgObj = GameObject.Find("MainMenu_Background");
            if (bgObj != null)
            {
                bgTransform = bgObj.transform;
                bgRenderer = bgObj.GetComponent<SpriteRenderer>();
            }
            UpdateBackgroundScale();
        }

        private void Update()
        {
            UpdateBackgroundScale();
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

        private void InitStyles()
        {
            if (stylesInit) return;

            whiteTex = MakeTex(2, 2, Color.white);

            titleMainStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = UIScale.FontMenuMainTitle, // 64
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = cyanNeon }
            };

            subtitleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = UIScale.FontMenuSubtitle, // 20
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(0.65f, 0.8f, 0.95f) }
            };

            headerSubStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = UIScale.FontCardHeader, // 18
                fontStyle = FontStyle.Bold,
                normal = { textColor = amberNeon }
            };

            bodyStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = UIScale.FontBodyDetail, // 16
                fontStyle = FontStyle.Bold,
                wordWrap = true,
                normal = { textColor = new Color(0.85f, 0.92f, 1f) }
            };

            taglineStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = UIScale.FontSmallLabel, // 14
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(0.5f, 0.7f, 0.9f) }
            };

            panelBoxStyle = new GUIStyle(GUI.skin.box);
            panelBoxStyle.normal.background = MakeBorderedTex(64, 64, panelDark, new Color(0f, 0.7f, 1f, 0.45f), 2);
            panelBoxStyle.border = new RectOffset(6, 6, 6, 6);

            primaryBtnStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = UIScale.FontButton, // 20
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white, background = MakeBorderedTex(32, 32, new Color(0f, 0.45f, 0.75f, 0.95f), cyanNeon, 2) },
                hover = { textColor = cyanNeon, background = MakeBorderedTex(32, 32, new Color(0f, 0.6f, 0.95f, 0.95f), Color.white, 2) }
            };

            secondaryBtnStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = UIScale.FontButton, // 20
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = cyanNeon, background = MakeBorderedTex(32, 32, new Color(0.04f, 0.08f, 0.16f, 0.92f), new Color(0f, 0.55f, 0.85f, 0.6f), 2) },
                hover = { textColor = Color.white, background = MakeBorderedTex(32, 32, new Color(0f, 0.4f, 0.65f, 0.95f), cyanNeon, 2) }
            };

            cardSelectStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = UIScale.FontCardHeader, // 18
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = Color.white, background = MakeBorderedTex(32, 32, new Color(0.06f, 0.16f, 0.32f, 0.95f), cyanNeon, 2) }
            };

            cardUnselectStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = UIScale.FontBodyDetail, // 16
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = new Color(0.75f, 0.85f, 0.95f), background = MakeBorderedTex(32, 32, new Color(0.03f, 0.06f, 0.12f, 0.9f), new Color(0.1f, 0.25f, 0.45f, 0.5f), 1) },
                hover = { textColor = Color.white, background = MakeBorderedTex(32, 32, new Color(0.05f, 0.12f, 0.24f, 0.92f), cyanNeon, 1) }
            };

            stylesInit = true;
        }

        private void OnGUI()
        {
            InitStyles();

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
        // 1. OPENING SCREEN (TITLE / START)
        // ==========================================
        private void DrawTitleScreen()
        {
            // Panel placed on the left with safe margin, leaving planet focal point on right visible on all aspects
            float w = 460f;
            float h = 530f;
            float x = Mathf.Max(28f, 40f);
            float y = (viewHeight - h) / 2f;
            Rect menuRect = new Rect(x, y, w, h);

            GUI.Box(menuRect, GUIContent.none, panelBoxStyle);
            GUILayout.BeginArea(new Rect(menuRect.x + 32, menuRect.y + 28, menuRect.width - 64, menuRect.height - 56));

            // Logo & Title
            GUILayout.Label("NEBULA RELAY", titleMainStyle);
            GUILayout.Label("THE LAST SIGNAL", subtitleStyle);

            GUILayout.Space(26);

            // Primary buttons: at least 48px tall
            if (GUILayout.Button("NEW TRANSMISSION  ›", primaryBtnStyle, GUILayout.Height(52)))
            {
                selectedMissionIndex = 1;
                currentState = MenuState.MissionSelect;
            }

            GUILayout.Space(12);

            if (GUILayout.Button("CONTINUE MISSION", secondaryBtnStyle, GUILayout.Height(48)))
            {
                currentState = MenuState.MissionSelect;
            }

            GUILayout.Space(12);

            if (GUILayout.Button("SETTINGS", secondaryBtnStyle, GUILayout.Height(48)))
            {
                currentState = MenuState.Settings;
            }

            GUILayout.Space(12);

            if (GUILayout.Button("QUIT", secondaryBtnStyle, GUILayout.Height(48)))
            {
                Application.Quit();
            }

            GUILayout.FlexibleSpace();

            GUILayout.Label("FIVE RELAYS. ONE SIGNAL. NO SECOND CHANCES.", taglineStyle);

            GUILayout.EndArea();
        }

        // ==========================================
        // 2. MISSION SELECTION SCREEN
        // ==========================================
        private void DrawMissionSelectScreen()
        {
            float totalW = Mathf.Min(1120f, viewWidth - 48f);
            float totalH = Mathf.Min(620f, viewHeight - 48f);
            float posX = (viewWidth - totalW) / 2f;
            float posY = (viewHeight - totalH) / 2f;

            Rect mainRect = new Rect(posX, posY, totalW, totalH);
            GUI.Box(mainRect, GUIContent.none, panelBoxStyle);

            GUILayout.BeginArea(new Rect(posX + 28, posY + 20, totalW - 56, totalH - 40));

            // Top Breadcrumb Header
            GUILayout.BeginHorizontal();
            GUIStyle headTitle = new GUIStyle(titleMainStyle) { fontSize = 28, alignment = TextAnchor.MiddleLeft };
            GUILayout.Label("NEBULA RELAY  /  SELECT MISSION", headTitle, GUILayout.Height(38));
            GUILayout.FlexibleSpace();
            int totalRestored = GameManager.Instance != null ? GameManager.Instance.TotalRelaysRestored : 0;
            int maxR = GameManager.Instance != null ? GameManager.Instance.MaxRelays : 5;
            GUILayout.Label($"STABILITY 100%  |  RELAYS {totalRestored}/{maxR}", headerSubStyle);
            GUILayout.EndHorizontal();

            GUILayout.Space(14);

            GUILayout.BeginHorizontal();

            // LEFT COLUMN: 5 MISSION CARDS
            float colW = (totalW - 80f) * 0.52f;
            GUILayout.BeginVertical(GUILayout.Width(colW));

            DrawMissionCard(1, "01  FIRST SIGNAL", "Calm orbital corridor. 4 checkpoints.");
            GUILayout.Space(8);
            DrawMissionCard(2, "02  THE DEBRIS FIELD", "Asteroid hazards. Tight routes. 5 checkpoints.");
            GUILayout.Space(8);
            DrawMissionCard(3, "03  THE ION STORM", "Turbulent moving hazards. 5 checkpoints.");
            GUILayout.Space(8);
            DrawMissionCard(4, "04  THE GRAVITY WELL", "Gravitational vortex corridors. 5 checkpoints.");
            GUILayout.Space(8);
            DrawMissionCard(5, "05  THE OMEGA TRANSMISSION", "Grand galactic nexus. 6 checkpoints. Final relay.");

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("‹  BACK TO MENU", secondaryBtnStyle, GUILayout.Width(200), GUILayout.Height(48)))
            {
                currentState = MenuState.Title;
            }

            GUILayout.EndVertical();

            GUILayout.Space(24);

            // RIGHT COLUMN: MISSION OVERVIEW PANEL
            float overW = (totalW - 80f) * 0.48f;
            GUILayout.BeginVertical(panelBoxStyle, GUILayout.Width(overW), GUILayout.Height(totalH - 96f));
            GUILayout.Space(12);

            GUILayout.Label("MISSION OVERVIEW", headerSubStyle);
            GUILayout.Space(10);

            string desc;
            string targets;
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
            GUILayout.Space(14);
            GUILayout.Label(targets, headerSubStyle);

            GUILayout.FlexibleSpace();

            // Large Start Mission Button (>= 48px tall)
            if (GUILayout.Button("START MISSION  ›", primaryBtnStyle, GUILayout.Height(52)))
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

            if (GUILayout.Button($"   {title}\n   {subtitle}", cardStyle, GUILayout.Height(56)))
            {
                selectedMissionIndex = index;
            }
        }

        // ==========================================
        // 3. SETTINGS SCREEN
        // ==========================================
        private void DrawSettingsScreen()
        {
            float w = 540f;
            float h = 480f;
            float x = (viewWidth - w) / 2f;
            float y = (viewHeight - h) / 2f;
            Rect r = new Rect(x, y, w, h);

            GUI.Box(r, GUIContent.none, panelBoxStyle);
            GUILayout.BeginArea(new Rect(r.x + 36, r.y + 28, r.width - 72, r.height - 56));

            GUIStyle setHead = new GUIStyle(titleMainStyle) { fontSize = 28 };
            GUILayout.Label("SETTINGS & TELEMETRY", setHead);
            GUILayout.Space(20);

            GUILayout.Label($"MASTER AUDIO VOLUME: {(int)(audioVolume * 100)}%", headerSubStyle);
            audioVolume = GUILayout.HorizontalSlider(audioVolume, 0f, 1f);
            AudioListener.volume = audioVolume;

            GUILayout.Space(20);

            GUILayout.Label("FLIGHT CONTROLS:", headerSubStyle);
            GUILayout.Label("• [W / A / S / D] or [Arrows] — Drone Inertial Thrusters\n• [Mouse Cursor] — Real-Time Trajectory Aim\n• [Space] or [LMB] — Energy Capsule Launch\n• [R] — Quick Sector Restart\n• [Escape] — Mission Pause\n• [F11] or [Alt+Enter] — Fullscreen / Windowed", bodyStyle);

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("‹  BACK", secondaryBtnStyle, GUILayout.Height(48)))
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
