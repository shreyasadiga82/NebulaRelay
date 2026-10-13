using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using NebulaRelay.Core;
using NebulaRelay.Effects;

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
    /// Includes comprehensive Settings menu powered by SettingsManager and PlayerPrefs.
    /// </summary>
    [SelectionBase]
    public class MainMenuController : MonoBehaviour
    {
        [Header("Sci-Fi Palette")]
        [SerializeField] private Color cyanNeon = new Color(0f, 0.92f, 1f, 1f);
        [SerializeField] private Color amberNeon = new Color(1f, 0.75f, 0f, 1f);
        [SerializeField] private Color greenNeon = new Color(0.18f, 0.95f, 0.5f, 1f);
        [SerializeField] private Color redDanger = new Color(1f, 0.22f, 0.32f, 1f);
        [SerializeField] private Color panelDark = new Color(0.03f, 0.05f, 0.1f, 0.92f);

        private MenuState currentState = MenuState.Title;
        private int selectedMissionIndex = 1; // 1 to 5
        private float viewWidth = 1280f;
        private float viewHeight = 720f;

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
                normal = { textColor = cyanNeon, background = MakeBorderedTex(32, 32, new Color(0.04f, 0.08f, 0.16f, 0.92f), cyanNeon, 2) },
                hover = { textColor = Color.white, background = MakeBorderedTex(32, 32, new Color(0f, 0.4f, 0.7f, 0.95f), Color.white, 2) }
            };

            cardSelectStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = UIScale.FontCardHeader, // 18
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = Color.white, background = MakeBorderedTex(32, 32, new Color(0f, 0.35f, 0.6f, 0.95f), cyanNeon, 2) }
            };

            cardUnselectStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = UIScale.FontBodyDetail, // 16
                fontStyle = FontStyle.Normal,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = new Color(0.7f, 0.82f, 0.95f), background = MakeBorderedTex(32, 32, new Color(0.04f, 0.07f, 0.14f, 0.85f), new Color(0.1f, 0.3f, 0.5f, 0.5f), 1) },
                hover = { textColor = Color.white, background = MakeBorderedTex(32, 32, new Color(0.08f, 0.16f, 0.3f, 0.95f), cyanNeon, 1) }
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
        // 1. TITLE SCREEN
        // ==========================================
        private void DrawTitleScreen()
        {
            float totalW = 560f;
            float totalH = 490f;
            float posX = (viewWidth - totalW) / 2f;
            float posY = (viewHeight - totalH) / 2f;

            Rect mainRect = new Rect(posX, posY, totalW, totalH);
            GUI.Box(mainRect, GUIContent.none, panelBoxStyle);

            GUILayout.BeginArea(new Rect(posX + 36, posY + 32, totalW - 72, totalH - 64));

            GUILayout.Label("DRIFT CORE", titleMainStyle);
            GUILayout.Space(8);
            GUILayout.Label("PRECISION COURIER  //  ORBITAL RECONSTRUCTION", subtitleStyle);
            GUILayout.Space(28);

            if (GUILayout.Button("INITIATE MISSION  ›", primaryBtnStyle, GUILayout.Height(54)))
            {
                AudioManager.Instance?.PlayButtonClick();
                currentState = MenuState.MissionSelect;
            }

            GUILayout.Space(14);

            if (GUILayout.Button("SYSTEM SETTINGS", secondaryBtnStyle, GUILayout.Height(50)))
            {
                AudioManager.Instance?.PlayButtonClick();
                currentState = MenuState.Settings;
            }

            GUILayout.Space(14);

            if (GUILayout.Button("DISCONNECT TERMINAL", secondaryBtnStyle, GUILayout.Height(50)))
            {
                AudioManager.Instance?.PlayButtonClick();
                Application.Quit();
            }

            GUILayout.Space(24);
            GUILayout.Label("DEEP SPACE LOGISTICS NETWORK  •  SYSTEM ONLINE", taglineStyle);

            GUILayout.EndArea();
        }

        // ==========================================
        // 2. MISSION SELECT SCREEN
        // ==========================================
        private void DrawMissionSelectScreen()
        {
            float totalW = Mathf.Min(1140f, viewWidth - 60f);
            float totalH = 590f;
            float posX = (viewWidth - totalW) / 2f;
            float posY = (viewHeight - totalH) / 2f;

            Rect mainRect = new Rect(posX, posY, totalW, totalH);
            GUI.Box(mainRect, GUIContent.none, panelBoxStyle);

            GUILayout.BeginArea(new Rect(posX + 28, posY + 20, totalW - 56, totalH - 40));

            // Top Breadcrumb Header
            GUILayout.BeginHorizontal();
            GUIStyle headTitle = new GUIStyle(titleMainStyle) { fontSize = 28, alignment = TextAnchor.MiddleLeft };
            GUILayout.Label("DRIFT CORE  /  SELECT MISSION", headTitle, GUILayout.Height(38));
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

            DrawMissionCard(1, "01  FIRST SIGNAL", "Calm orbital corridor. 4 compulsory relays. 2 capsules.");
            GUILayout.Space(8);
            DrawMissionCard(2, "02  THE DEBRIS FIELD", "Asteroid hazards. Tight routes. 5 compulsory relays.");
            GUILayout.Space(8);
            DrawMissionCard(3, "03  THE ION STORM", "Turbulent moving hazards. 5 compulsory relays.");
            GUILayout.Space(8);
            DrawMissionCard(4, "04  THE GRAVITY WELL", "Gravitational vortex corridors. 5 compulsory relays.");
            GUILayout.Space(8);
            DrawMissionCard(5, "05  THE OMEGA TRANSMISSION", "Grand galactic nexus. 6 compulsory relays. Final goal.");

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("‹  BACK TO MENU", secondaryBtnStyle, GUILayout.Width(200), GUILayout.Height(48)))
            {
                AudioManager.Instance?.PlayButtonClick();
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
                desc = "Sector 1 presents a calm orbital flight corridor. Synchronize all 4 compulsory relays, launch an energy capsule into the receiver dish (max 2 capsules), and dock at the station at <= 1.5 m/s within 2 minutes.";
                targets = "RELAYS: 4 Compulsory  |  CAPSULES: 2 Max  |  TIME: 02:00";
            }
            else if (selectedMissionIndex == 2)
            {
                desc = "Sector 2 traverses an active asteroid debris field. Avoid dense collisions that drain battery shields, synchronize all 5 compulsory relays, and power the second relay station.";
                targets = "RELAYS: 5 Compulsory  |  CAPSULES: 2 Max  |  TIME: 02:00";
            }
            else if (selectedMissionIndex == 3)
            {
                desc = "High-energy ion turbulence creates moving asteroid slaloms. Thread through all 5 compulsory relays under dynamic cross-drift to activate the third relay.";
                targets = "RELAYS: 5 Compulsory  |  CAPSULES: 2 Max  |  TIME: 02:00";
            }
            else if (selectedMissionIndex == 4)
            {
                desc = "Golden gravitational anomalies pull drifting space debris into tight corridors. Precision thrust is required across 5 compulsory relays before unlocking the exit.";
                targets = "RELAYS: 5 Compulsory  |  CAPSULES: 2 Max  |  TIME: 02:00";
            }
            else
            {
                desc = "The master galactic nexus hub. Synchronize 6 compulsory relays across an epic asteroid gauntlet and deliver the final master signal to achieve total galactic recovery.";
                targets = "RELAYS: 6 Compulsory  |  CAPSULES: 2 Max  |  TIME: 02:00";
            }

            GUILayout.Label(desc, bodyStyle);
            GUILayout.Space(14);
            GUILayout.Label(targets, headerSubStyle);

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("START MISSION  ›", primaryBtnStyle, GUILayout.Height(52)))
            {
                AudioManager.Instance?.PlayButtonClick();
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
                AudioManager.Instance?.PlayButtonClick();
            }
        }

        // ==========================================
        // 3. SETTINGS SCREEN
        // ==========================================
        private void DrawSettingsScreen()
        {
            float w = 620f;
            float h = 570f;
            float x = (viewWidth - w) / 2f;
            float y = (viewHeight - h) / 2f;
            Rect r = new Rect(x, y, w, h);

            GUI.Box(r, GUIContent.none, panelBoxStyle);
            GUILayout.BeginArea(new Rect(r.x + 36, r.y + 24, r.width - 72, r.height - 48));

            GUIStyle setHead = new GUIStyle(titleMainStyle) { fontSize = 28 };
            GUILayout.Label("SETTINGS & TELEMETRY", setHead);
            GUILayout.Space(14);

            // Audio sliders
            GUILayout.Label($"MASTER AUDIO VOLUME: {(int)(SettingsManager.MasterVolume * 100)}%", headerSubStyle);
            float newMaster = GUILayout.HorizontalSlider(SettingsManager.MasterVolume, 0f, 1f);
            if (Mathf.Abs(newMaster - SettingsManager.MasterVolume) > 0.01f) SettingsManager.SetMasterVolume(newMaster);

            GUILayout.Space(6);
            GUILayout.BeginHorizontal();
            GUILayout.Label($"SFX VOLUME: {(int)(SettingsManager.SFXVolume * 100)}%", bodyStyle, GUILayout.Width(180));
            float newSfx = GUILayout.HorizontalSlider(SettingsManager.SFXVolume, 0f, 1f);
            if (Mathf.Abs(newSfx - SettingsManager.SFXVolume) > 0.01f) SettingsManager.SetSFXVolume(newSfx);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label($"MUSIC VOLUME: {(int)(SettingsManager.MusicVolume * 100)}%", bodyStyle, GUILayout.Width(180));
            float newMusic = GUILayout.HorizontalSlider(SettingsManager.MusicVolume, 0f, 1f);
            if (Mathf.Abs(newMusic - SettingsManager.MusicVolume) > 0.01f) SettingsManager.SetMusicVolume(newMusic);
            GUILayout.EndHorizontal();

            GUILayout.Space(8);
            GUILayout.BeginHorizontal();
            bool mute = GUILayout.Toggle(SettingsManager.IsMuted, " MUTE ALL AUDIO", GUI.skin.toggle);
            if (mute != SettingsManager.IsMuted) SettingsManager.SetMuted(mute);

            bool warn = GUILayout.Toggle(SettingsManager.CountdownAudioEnabled, " COUNTDOWN AUDIO", GUI.skin.toggle);
            if (warn != SettingsManager.CountdownAudioEnabled) SettingsManager.SetCountdownAudio(warn);
            GUILayout.EndHorizontal();

            GUILayout.Space(10);
            GUILayout.BeginHorizontal();
            bool shake = GUILayout.Toggle(SettingsManager.ScreenShakeEnabled, " SCREEN SHAKE", GUI.skin.toggle);
            if (shake != SettingsManager.ScreenShakeEnabled) SettingsManager.SetScreenShake(shake);

            bool vfx = GUILayout.Toggle(SettingsManager.VisualEffectsEnabled, " VISUAL EFFECTS", GUI.skin.toggle);
            if (vfx != SettingsManager.VisualEffectsEnabled) SettingsManager.SetVisualEffects(vfx);
            GUILayout.EndHorizontal();

            GUILayout.Space(10);
            if (GUILayout.Button(Screen.fullScreen ? "SWITCH TO WINDOWED" : "SWITCH TO FULLSCREEN", secondaryBtnStyle, GUILayout.Height(38)))
            {
                SettingsManager.ToggleFullscreen();
            }

            GUILayout.Space(12);
            GUILayout.Label("FLIGHT CONTROLS:", headerSubStyle);
            GUILayout.Label("• [W / A / S / D] or [Arrows] — Drone Inertial Thrusters\n• [Mouse Cursor] — Trajectory Aiming\n• [Space] or [LMB] — Energy Capsule Launch (Max 2)\n• [R] — Quick Sector Restart   |   [Esc] — Pause Menu", bodyStyle);

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("‹  BACK", secondaryBtnStyle, GUILayout.Height(46)))
            {
                AudioManager.Instance?.PlayButtonClick();
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
