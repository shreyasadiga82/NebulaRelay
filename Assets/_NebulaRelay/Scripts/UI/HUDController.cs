using UnityEngine;
using NebulaRelay.Core;
using NebulaRelay.Player;
using NebulaRelay.World;

namespace NebulaRelay.UI
{
    /// <summary>
    /// HUDController provides a sleek, zero-dependency sci-fi on-screen HUD.
    /// Displays speed, battery telemetry, current objective, relay counter, alerts, and victory/fail modals.
    /// </summary>
    [SelectionBase]
    public class HUDController : MonoBehaviour
    {
        [Header("Theme Palette")]
        [SerializeField] private Color primaryCyan = new Color(0f, 0.9f, 1f, 1f);
        [SerializeField] private Color successGreen = new Color(0.16f, 0.85f, 0.47f, 1f);
        [SerializeField] private Color warningAmber = new Color(1f, 0.83f, 0f, 1f);
        [SerializeField] private Color dangerRed = new Color(1f, 0.2f, 0.35f, 1f);

        // GUI Styles
        private GUIStyle panelBoxStyle;
        private GUIStyle titleStyle;
        private GUIStyle bodyStyle;
        private GUIStyle valueStyle;
        private GUIStyle bannerStyle;
        private GUIStyle modalTitleStyle;
        private GUIStyle modalBodyStyle;
        private GUIStyle buttonStyle;
        private Texture2D barBackgroundTex;
        private Texture2D barFillTex;
        private bool stylesInitialized = false;

        private void InitStyles()
        {
            if (stylesInitialized) return;

            barBackgroundTex = MakeTex(2, 2, new Color(0.08f, 0.12f, 0.18f, 0.9f));
            barFillTex = MakeTex(2, 2, Color.white);

            panelBoxStyle = new GUIStyle(GUI.skin.box);
            panelBoxStyle.normal.background = MakeTex(2, 2, new Color(0.04f, 0.06f, 0.1f, 0.85f));
            panelBoxStyle.border = new RectOffset(4, 4, 4, 4);

            titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13,
                fontStyle = FontStyle.Bold,
                normal = { textColor = primaryCyan }
            };

            bodyStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                normal = { textColor = new Color(0.85f, 0.9f, 0.95f) }
            };

            valueStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleRight,
                normal = { textColor = Color.white }
            };

            bannerStyle = new GUIStyle(GUI.skin.box)
            {
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white }
            };
            bannerStyle.normal.background = MakeTex(2, 2, new Color(0.05f, 0.08f, 0.14f, 0.95f));

            modalTitleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 20,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = primaryCyan }
            };

            modalBodyStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white }
            };

            buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 14,
                fontStyle = FontStyle.Bold
            };

            stylesInitialized = true;
        }

        private void OnGUI()
        {
            InitStyles();

            LevelManager lm = LevelManager.Instance;
            if (lm == null) return;

            DrawObjectiveCard(lm);
            DrawTelemetryCard(lm);
            DrawCenterAlertBanner(lm);

            if (lm.State == LevelState.Completed)
            {
                DrawLevelCompleteModal(lm);
            }
            else if (lm.State == LevelState.Failed)
            {
                DrawFailureModal(lm);
            }
        }

        private void DrawObjectiveCard(LevelManager lm)
        {
            float width = 380f;
            float height = 90f;
            Rect area = new Rect(20, 20, width, height);

            GUI.Box(area, GUIContent.none, panelBoxStyle);
            GUILayout.BeginArea(new Rect(area.x + 12, area.y + 8, area.width - 24, area.height - 16));

            GUILayout.Label($"MISSION {lm.LevelNumber}: {lm.LevelTitle.ToUpper()}", titleStyle);
            GUILayout.Space(2);

            // Objective text with dynamic color
            Color objColor = lm.State == LevelState.Completed ? successGreen : primaryCyan;
            GUIStyle objStyle = new GUIStyle(bodyStyle)
            {
                fontStyle = FontStyle.Bold,
                normal = { textColor = objColor }
            };
            GUILayout.Label(lm.CurrentObjectiveText, objStyle);

            GUILayout.FlexibleSpace();
            GUIStyle hintStyle = new GUIStyle(bodyStyle)
            {
                fontSize = 10,
                normal = { textColor = new Color(0.6f, 0.7f, 0.8f) }
            };
            GUILayout.Label("Controls: [WASD] Thrust  |  [Mouse] Aim  |  [Space/LMB] Launch  |  [R] Restart", hintStyle);

            GUILayout.EndArea();
        }

        private void DrawTelemetryCard(LevelManager lm)
        {
            float width = 300f;
            float height = 115f;
            Rect area = new Rect(Screen.width - width - 20, 20, width, height);

            GUI.Box(area, GUIContent.none, panelBoxStyle);
            GUILayout.BeginArea(new Rect(area.x + 12, area.y + 8, area.width - 24, area.height - 16));

            // 1. Relays Restored Counter
            int restoredCount = GameManager.Instance != null ? GameManager.Instance.TotalRelaysRestored : (lm.LevelNumber - 1);
            if (lm.State == LevelState.Completed) restoredCount = lm.LevelNumber;
            int maxRelays = GameManager.Instance != null ? GameManager.Instance.MaxRelays : 3;

            GUILayout.BeginHorizontal();
            GUILayout.Label("ORBITAL NETWORK:", titleStyle);
            GUILayout.Label($"{restoredCount}/{maxRelays} RELAYS", valueStyle);
            GUILayout.EndHorizontal();

            GUILayout.Space(6);

            // 2. Battery Meter
            float batteryPct = lm.Battery != null ? lm.Battery.BatteryPercentage : 1f;
            float batteryVal = lm.Battery != null ? lm.Battery.CurrentBattery : 100f;
            Color batColor = batteryPct > 0.5f ? primaryCyan : (batteryPct > 0.2f ? warningAmber : dangerRed);

            GUILayout.BeginHorizontal();
            GUILayout.Label("BATTERY:", bodyStyle);
            GUIStyle batValStyle = new GUIStyle(valueStyle) { normal = { textColor = batColor } };
            GUILayout.Label($"{batteryVal:F0}%", batValStyle);
            GUILayout.EndHorizontal();

            DrawProgressBar(batteryPct, batColor);

            GUILayout.Space(6);

            // 3. Drone Speedometer
            float currentSpeed = lm.Drone != null ? lm.Drone.Speed : 0f;
            float maxSpeed = lm.Drone != null ? lm.Drone.MaxSpeed : 8f;
            float speedRatio = Mathf.Clamp01(currentSpeed / maxSpeed);

            GUILayout.BeginHorizontal();
            GUILayout.Label("DRONE SPEED:", bodyStyle);
            GUILayout.Label($"{currentSpeed:F1} / {maxSpeed:F0} u/s", valueStyle);
            GUILayout.EndHorizontal();

            DrawProgressBar(speedRatio, primaryCyan);

            GUILayout.EndArea();
        }

        private void DrawProgressBar(float fillPct, Color fillColor)
        {
            Rect barRect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(8));
            GUI.DrawTexture(barRect, barBackgroundTex);

            Rect fillRect = new Rect(barRect.x, barRect.y, barRect.width * fillPct, barRect.height);
            Color oldColor = GUI.color;
            GUI.color = fillColor;
            GUI.DrawTexture(fillRect, barFillTex);
            GUI.color = oldColor;
        }

        private void DrawCenterAlertBanner(LevelManager lm)
        {
            if (!lm.IsAlertActive()) return;

            float width = 460f;
            float height = 50f;
            Rect area = new Rect((Screen.width - width) / 2f, 80, width, height);

            Color alertColor = lm.IsCurrentAlertWarning ? dangerRed : successGreen;
            GUIStyle activeBannerStyle = new GUIStyle(bannerStyle)
            {
                normal = { textColor = alertColor }
            };

            GUI.Box(area, lm.CurrentAlertMessage, activeBannerStyle);
        }

        private void DrawLevelCompleteModal(LevelManager lm)
        {
            float width = 460f;
            float height = 180f;
            Rect area = new Rect((Screen.width - width) / 2f, (Screen.height - height) / 2f, width, height);

            GUI.Box(area, GUIContent.none, panelBoxStyle);
            GUILayout.BeginArea(new Rect(area.x + 20, area.y + 20, area.width - 40, area.height - 40));

            bool isGrandVictory = lm.LevelNumber >= 3;

            GUIStyle title = new GUIStyle(modalTitleStyle)
            {
                normal = { textColor = isGrandVictory ? successGreen : primaryCyan }
            };
            GUILayout.Label(isGrandVictory ? "★ ORBITAL NETWORK RESTORED ★" : $"RELAY {lm.LevelNumber} RESTORED!", title);

            GUILayout.Space(6);
            string subText = isGrandVictory
                ? "All 3 Relays are back online. Couriers can now navigate safely."
                : $"Sector {lm.LevelNumber} energy relay is fully online and synchronized.";
            GUILayout.Label(subText, modalBodyStyle);

            GUILayout.FlexibleSpace();

            string buttonLabel = isGrandVictory ? "RETURN TO MENU" : "PROCEED TO NEXT SECTOR  [SPACE]";
            if (GUILayout.Button(buttonLabel, buttonStyle, GUILayout.Height(38)))
            {
                lm.AdvanceToNextLevel();
            }

            GUILayout.EndArea();
        }

        private void DrawFailureModal(LevelManager lm)
        {
            float width = 440f;
            float height = 170f;
            Rect area = new Rect((Screen.width - width) / 2f, (Screen.height - height) / 2f, width, height);

            GUI.Box(area, GUIContent.none, panelBoxStyle);
            GUILayout.BeginArea(new Rect(area.x + 20, area.y + 20, area.width - 40, area.height - 40));

            GUIStyle failTitle = new GUIStyle(modalTitleStyle)
            {
                normal = { textColor = dangerRed }
            };
            GUILayout.Label("MISSION COMPROMISED", failTitle);

            GUILayout.Space(6);
            GUILayout.Label(lm.FailureReason, modalBodyStyle);

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("RETRY LEVEL  [R]", buttonStyle, GUILayout.Height(38)))
            {
                lm.RestartLevel();
            }

            GUILayout.EndArea();
        }

        private static Texture2D MakeTex(int width, int height, Color col)
        {
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; i++) pix[i] = col;
            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }
    }
}
