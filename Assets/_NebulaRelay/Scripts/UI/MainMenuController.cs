using UnityEngine;
using UnityEngine.SceneManagement;
using NebulaRelay.Core;

namespace NebulaRelay.UI
{
    /// <summary>
    /// MainMenuController renders the title screen, briefing, level selector, and quit button.
    /// </summary>
    [SelectionBase]
    public class MainMenuController : MonoBehaviour
    {
        [Header("Palette")]
        [SerializeField] private Color cyanColor = new Color(0f, 0.9f, 1f, 1f);
        [SerializeField] private Color greenColor = new Color(0.16f, 0.85f, 0.47f, 1f);
        [SerializeField] private Color amberColor = new Color(1f, 0.83f, 0f, 1f);

        private GUIStyle titleStyle;
        private GUIStyle subtitleStyle;
        private GUIStyle headerStyle;
        private GUIStyle textStyle;
        private GUIStyle boxStyle;
        private GUIStyle mainButtonStyle;
        private GUIStyle smallButtonStyle;
        private bool stylesInit = false;

        private void InitStyles()
        {
            if (stylesInit) return;

            titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 32,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = cyanColor }
            };

            subtitleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                fontStyle = FontStyle.Italic,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(0.7f, 0.85f, 0.95f) }
            };

            headerStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                normal = { textColor = amberColor }
            };

            textStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                normal = { textColor = new Color(0.85f, 0.9f, 0.95f) }
            };

            boxStyle = new GUIStyle(GUI.skin.box);
            Texture2D bg = new Texture2D(2, 2);
            for (int y = 0; y < 2; y++)
                for (int x = 0; x < 2; x++)
                    bg.SetPixel(x, y, new Color(0.04f, 0.06f, 0.12f, 0.92f));
            bg.Apply();
            boxStyle.normal.background = bg;

            mainButtonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 16,
                fontStyle = FontStyle.Bold
            };

            smallButtonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 13
            };

            stylesInit = true;
        }

        private void OnGUI()
        {
            InitStyles();

            float screenW = Screen.width;
            float screenH = Screen.height;

            // Main Menu Box Container
            float boxW = 560f;
            float boxH = 460f;
            Rect boxRect = new Rect((screenW - boxW) / 2f, (screenH - boxH) / 2f, boxW, boxH);

            GUI.Box(boxRect, GUIContent.none, boxStyle);
            GUILayout.BeginArea(new Rect(boxRect.x + 24, boxRect.y + 20, boxRect.width - 48, boxRect.height - 40));

            // Title Header
            GUILayout.Label("NEBULA RELAY", titleStyle);
            GUILayout.Label("THE LAST SIGNAL — 2D SPACE PHYSICS ADVENTURE", subtitleStyle);

            GUILayout.Space(14);

            // Mission Briefing & Mechanics Card
            GUILayout.Label("MISSION BRIEFING & DIRECTIVES", headerStyle);
            GUILayout.Label("• Pilot the courier drone using thrust and orbital inertia.", textStyle);
            GUILayout.Label("• Aim and launch energy capsules through sequential orbital rings.", textStyle);
            GUILayout.Label("• Strike the energy receiver to deliver power.", textStyle);
            GUILayout.Label("• Decelerate to safe docking speed (≤ 1.5 u/s) to reactivate the relay.", textStyle);
            GUILayout.Label("• Restore all 3 Relays before your battery is exhausted.", textStyle);

            GUILayout.Space(8);
            GUILayout.Label("FLIGHT CONTROLS", headerStyle);
            GUILayout.Label("• [W, A, S, D / Arrows]: Thrusters & Braking", textStyle);
            GUILayout.Label("• [Mouse Cursor]: Aim Projectile Launcher", textStyle);
            GUILayout.Label("• [Space / Left Mouse Button]: Launch Energy Capsule", textStyle);
            GUILayout.Label("• [R]: Quick Mission Restart", textStyle);

            GUILayout.FlexibleSpace();

            // Main Launch Button
            if (GUILayout.Button("LAUNCH MISSION (LEVEL 1)  ▶", mainButtonStyle, GUILayout.Height(42)))
            {
                StartGame(1);
            }

            GUILayout.Space(6);

            // Level Selector Row & Quit
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Sector 1: First Signal", smallButtonStyle, GUILayout.Height(28))) StartGame(1);
            if (GUILayout.Button("Sector 2: Debris Field", smallButtonStyle, GUILayout.Height(28))) StartGame(2);
            if (GUILayout.Button("Sector 3: Transmission", smallButtonStyle, GUILayout.Height(28))) StartGame(3);
            if (GUILayout.Button("Quit", smallButtonStyle, GUILayout.Height(28), GUILayout.Width(70)))
            {
                QuitGame();
            }
            GUILayout.EndHorizontal();

            GUILayout.EndArea();
        }

        private void StartGame(int level)
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ResetProgress();
                GameManager.Instance.LoadLevel(level);
            }
            else
            {
                SceneManager.LoadScene($"Level_{level:D2}");
            }
        }

        private void QuitGame()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.QuitGame();
            }
            else
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            }
        }
    }
}
