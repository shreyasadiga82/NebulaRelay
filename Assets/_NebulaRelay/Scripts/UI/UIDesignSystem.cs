using System;
using System.Collections.Generic;
using UnityEngine;
using NebulaRelay.Core;

namespace NebulaRelay.UI
{
    public enum ProceduralIcon
    {
        Beacon,
        Reticle,
        WarningTriangle,
        Singularity,
        Gate,
        Capsule,
        Battery,
        Speedometer,
        Dock,
        Lock,
        Checkmark
    }

    /// <summary>
    /// Mini Design System providing cached procedural textures, shared GUIStyles,
    /// 8-px spacing tokens, semantic colors, text auto-fitting, and UI primitives.
    /// Allocates textures ONCE; produces ZERO per-frame GC allocations in OnGUI.
    /// </summary>
    public static class UIDesignSystem
    {
        // ==========================================
        // 1. SEMANTIC COLOR TOKENS
        // ==========================================
        public static readonly Color ColorNavyBg       = new Color(0.024f, 0.043f, 0.094f, 0.88f); // ~88% dark navy
        public static readonly Color ColorNavyOverlay  = new Color(0.02f, 0.035f, 0.08f, 0.70f);  // ~70% modal dimmer
        public static readonly Color ColorBorderCyan   = new Color(0f, 0.85f, 1f, 0.38f);          // 38% cyan border
        public static readonly Color ColorBorderGlow   = new Color(0f, 0.95f, 1f, 0.85f);          // Bright cyan glow
        public static readonly Color ColorCyan         = new Color(0f, 0.92f, 1f, 1f);             // #00EAFF Primary
        public static readonly Color ColorGreenSafe     = new Color(0.18f, 0.95f, 0.50f, 1f);        // #2EF280 Safe/Complete
        public static readonly Color ColorAmberWarning  = new Color(1f, 0.75f, 0f, 1f);             // #FFBF00 Warning
        public static readonly Color ColorRedCritical   = new Color(1f, 0.22f, 0.32f, 1f);          // #FF3852 Critical/Danger
        public static readonly Color ColorMagenta       = new Color(1f, 0.18f, 0.65f, 1f);          // #FF2EA6 Capsules
        public static readonly Color ColorTextMuted     = new Color(0.55f, 0.65f, 0.78f, 1f);       // Muted blue-grey
        public static readonly Color ColorTextBright    = new Color(0.92f, 0.96f, 1f, 1f);          // Off-white bright
        public static readonly Color ColorTextShadow    = new Color(0.01f, 0.02f, 0.05f, 0.95f);    // Deep shadow

        // Spacing tokens (8-px grid)
        public const float SpaceXXS = 4f;
        public const float SpaceXS  = 8f;
        public const float SpaceS   = 16f;
        public const float SpaceM   = 24f;
        public const float SpaceL   = 32f;
        public const float SpaceXL  = 48f;
        public const float MarginSafe = 24f;

        // Cached Textures
        public static Texture2D TexWhite { get; private set; }
        public static Texture2D TexDimOverlay { get; private set; }
        public static Texture2D TexPanel { get; private set; }
        public static Texture2D TexPanelSelected { get; private set; }
        public static Texture2D TexButtonNormal { get; private set; }
        public static Texture2D TexButtonHover { get; private set; }
        public static Texture2D TexButtonPressed { get; private set; }
        public static Texture2D TexButtonSecondary { get; private set; }
        public static Texture2D TexButtonSecondaryHover { get; private set; }
        public static Texture2D TexPillBg { get; private set; }
        public static Texture2D TexBarBg { get; private set; }
        public static Texture2D TexBarFill { get; private set; }

        private static readonly Dictionary<ProceduralIcon, Texture2D> iconTextures = new Dictionary<ProceduralIcon, Texture2D>();

        // Shared GUIStyles
        public static GUIStyle StylePanel { get; private set; }
        public static GUIStyle StylePanelSelected { get; private set; }
        public static GUIStyle StyleTitle { get; private set; }
        public static GUIStyle StyleHeader { get; private set; }
        public static GUIStyle StyleLabelMuted { get; private set; }
        public static GUIStyle StyleLabelBright { get; private set; }
        public static GUIStyle StyleValueBold { get; private set; }
        public static GUIStyle StyleValueLarge { get; private set; }
        public static GUIStyle StyleButtonPrimary { get; private set; }
        public static GUIStyle StyleButtonSecondary { get; private set; }
        public static GUIStyle StyleCard { get; private set; }
        public static GUIStyle StyleCardSelected { get; private set; }
        public static GUIStyle StyleModalTitle { get; private set; }
        public static GUIStyle StyleTableRowLabel { get; private set; }
        public static GUIStyle StyleTableRowValue { get; private set; }

        // Specialized cached styles to guarantee ZERO per-frame OnGUI allocations
        public static GUIStyle StyleMenuMainTitle { get; private set; }
        public static GUIStyle StyleMenuSubtitle { get; private set; }
        public static GUIStyle StyleFooter { get; private set; }
        public static GUIStyle StyleSectionHeader { get; private set; }
        public static GUIStyle StyleCardTitle { get; private set; }
        public static GUIStyle StyleCardSubtitle { get; private set; }
        public static GUIStyle StyleCardDesc { get; private set; }
        public static GUIStyle StyleCardValue { get; private set; }
        public static GUIStyle StyleProgressText { get; private set; }
        public static GUIStyle StyleGridTitle { get; private set; }
        public static GUIStyle StyleGridDesc { get; private set; }
        public static GUIStyle StyleGridMeta { get; private set; }
        public static GUIStyle StyleDetailBriefing { get; private set; }
        public static GUIStyle StyleObjectiveBanner { get; private set; }
        public static GUIStyle StyleTimerValue { get; private set; }
        public static GUIStyle StyleFloatingScore { get; private set; }
        public static GUIStyle StyleAlertBanner { get; private set; }
        public static GUIStyle StyleModalSubtitle { get; private set; }
        public static GUIStyle StyleTotalScoreValue { get; private set; }
        public static GUIStyle StyleBestScoreText { get; private set; }
        public static GUIStyle StyleFailReason { get; private set; }
        public static GUIStyle StyleDifficultyBadge { get; private set; }

        private static bool isInitialized = false;

        public static void EnsureInitialized()
        {
            if (isInitialized) return;

            InitTextures();
            InitStyles();
            isInitialized = true;
        }

        private static void InitTextures()
        {
            TexWhite = CreateSolidTexture(2, 2, Color.white);
            TexDimOverlay = CreateSolidTexture(2, 2, ColorNavyOverlay);
            TexBarBg = CreateSolidTexture(2, 2, new Color(0.04f, 0.08f, 0.16f, 0.85f));
            TexBarFill = CreateSolidTexture(2, 2, Color.white);
            TexPillBg = CreateRoundedBoxTex(32, 16, 6, new Color(0.05f, 0.10f, 0.20f, 0.90f), ColorBorderCyan, 1);

            TexPanel = CreateSciFiPanelTex(64, 64, ColorNavyBg, ColorBorderCyan, 1, true);
            TexPanelSelected = CreateSciFiPanelTex(64, 64, new Color(0.03f, 0.07f, 0.15f, 0.95f), ColorBorderGlow, 2, true);

            TexButtonNormal = CreateSciFiPanelTex(48, 48, new Color(0.04f, 0.12f, 0.24f, 0.95f), ColorCyan, 2, false);
            TexButtonHover = CreateSciFiPanelTex(48, 48, new Color(0f, 0.45f, 0.75f, 0.98f), Color.white, 2, false);
            TexButtonPressed = CreateSciFiPanelTex(48, 48, new Color(0f, 0.35f, 0.60f, 0.98f), ColorCyan, 2, false);

            TexButtonSecondary = CreateSciFiPanelTex(48, 48, new Color(0.03f, 0.06f, 0.13f, 0.92f), ColorBorderCyan, 1, false);
            TexButtonSecondaryHover = CreateSciFiPanelTex(48, 48, new Color(0.06f, 0.16f, 0.30f, 0.96f), ColorCyan, 1, false);

            // Procedural icons
            iconTextures[ProceduralIcon.Beacon] = CreateBeaconIcon(48, 48);
            iconTextures[ProceduralIcon.Reticle] = CreateReticleIcon(48, 48);
            iconTextures[ProceduralIcon.WarningTriangle] = CreateWarningTriangleIcon(48, 48);
            iconTextures[ProceduralIcon.Singularity] = CreateSingularityIcon(48, 48);
            iconTextures[ProceduralIcon.Gate] = CreateGateIcon(32, 32);
            iconTextures[ProceduralIcon.Capsule] = CreateCapsuleIcon(32, 32);
            iconTextures[ProceduralIcon.Battery] = CreateBatteryIcon(32, 32);
            iconTextures[ProceduralIcon.Speedometer] = CreateSpeedometerIcon(32, 32);
            iconTextures[ProceduralIcon.Dock] = CreateDockIcon(32, 32);
            iconTextures[ProceduralIcon.Lock] = CreateLockIcon(32, 32);
            iconTextures[ProceduralIcon.Checkmark] = CreateCheckmarkIcon(32, 32);
        }

        private static void InitStyles()
        {
            StylePanel = new GUIStyle
            {
                normal = { background = TexPanel },
                border = new RectOffset(10, 10, 10, 10),
                padding = new RectOffset(16, 16, 16, 16)
            };

            StylePanelSelected = new GUIStyle
            {
                normal = { background = TexPanelSelected },
                border = new RectOffset(10, 10, 10, 10),
                padding = new RectOffset(16, 16, 16, 16)
            };

            StyleTitle = new GUIStyle
            {
                fontSize = UIScale.FontHUDTitle, // 26
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = ColorCyan }
            };

            StyleHeader = new GUIStyle
            {
                fontSize = UIScale.FontCardHeader, // 18
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = ColorAmberWarning }
            };

            StyleLabelMuted = new GUIStyle
            {
                fontSize = UIScale.FontSmallLabel, // 14
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = ColorTextMuted }
            };

            StyleLabelBright = new GUIStyle
            {
                fontSize = UIScale.FontBodyDetail, // 16
                fontStyle = FontStyle.Normal,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = ColorTextBright }
            };

            StyleValueBold = new GUIStyle
            {
                fontSize = UIScale.FontValue, // 22
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = Color.white }
            };

            StyleValueLarge = new GUIStyle
            {
                fontSize = 28,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = ColorCyan }
            };

            StyleButtonPrimary = new GUIStyle
            {
                fontSize = UIScale.FontButton, // 20
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white, background = TexButtonNormal },
                hover = { textColor = ColorCyan, background = TexButtonHover },
                active = { textColor = Color.white, background = TexButtonPressed }
            };

            StyleButtonSecondary = new GUIStyle
            {
                fontSize = UIScale.FontButton, // 20
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = ColorCyan, background = TexButtonSecondary },
                hover = { textColor = Color.white, background = TexButtonSecondaryHover },
                active = { textColor = ColorCyan, background = TexButtonSecondary }
            };

            StyleCard = new GUIStyle
            {
                fontSize = UIScale.FontBodyDetail,
                fontStyle = FontStyle.Normal,
                alignment = TextAnchor.UpperLeft,
                normal = { textColor = ColorTextBright, background = TexButtonSecondary },
                hover = { textColor = Color.white, background = TexButtonSecondaryHover }
            };

            StyleCardSelected = new GUIStyle
            {
                fontSize = UIScale.FontBodyDetail,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.UpperLeft,
                normal = { textColor = Color.white, background = TexPanelSelected }
            };

            StyleModalTitle = new GUIStyle
            {
                fontSize = UIScale.FontModalTitle, // 32
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = ColorCyan }
            };

            StyleTableRowLabel = new GUIStyle
            {
                fontSize = UIScale.FontBodyDetail, // 16
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = ColorTextMuted }
            };

            StyleTableRowValue = new GUIStyle
            {
                fontSize = UIScale.FontCardHeader, // 18
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleRight,
                normal = { textColor = ColorTextBright }
            };

            // Specialized cached styles
            StyleMenuMainTitle = new GUIStyle(StyleTitle)
            {
                fontSize = 52,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = ColorCyan }
            };

            StyleMenuSubtitle = new GUIStyle(StyleLabelBright)
            {
                fontSize = 16,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = ColorTextMuted }
            };

            StyleFooter = new GUIStyle(StyleLabelMuted)
            {
                fontSize = 14,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = ColorTextMuted }
            };

            StyleSectionHeader = new GUIStyle(StyleTitle)
            {
                fontSize = 24,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = ColorCyan }
            };

            StyleCardTitle = new GUIStyle(StyleHeader)
            {
                fontSize = 18,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = ColorTextBright }
            };

            StyleCardSubtitle = new GUIStyle(StyleLabelMuted)
            {
                fontSize = 14,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = ColorTextMuted }
            };

            StyleCardDesc = new GUIStyle(StyleLabelBright)
            {
                fontSize = 14,
                wordWrap = true,
                alignment = TextAnchor.UpperLeft,
                normal = { textColor = ColorTextBright }
            };

            StyleCardValue = new GUIStyle(StyleTableRowValue)
            {
                fontSize = 14,
                alignment = TextAnchor.MiddleRight,
                normal = { textColor = ColorTextBright }
            };

            StyleProgressText = new GUIStyle(StyleHeader)
            {
                fontSize = 16,
                alignment = TextAnchor.MiddleRight,
                normal = { textColor = ColorAmberWarning }
            };

            StyleGridTitle = new GUIStyle(StyleHeader)
            {
                fontSize = 15,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = ColorTextBright }
            };

            StyleGridDesc = new GUIStyle(StyleLabelMuted)
            {
                fontSize = 14, // Minimum 14 px floor
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = ColorTextMuted }
            };

            StyleGridMeta = new GUIStyle(StyleLabelMuted)
            {
                fontSize = 14, // Minimum 14 px floor
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = ColorTextMuted }
            };

            StyleDetailBriefing = new GUIStyle(StyleLabelBright)
            {
                fontSize = 15,
                wordWrap = true,
                alignment = TextAnchor.UpperLeft,
                normal = { textColor = ColorTextBright }
            };

            StyleObjectiveBanner = new GUIStyle(StyleLabelBright)
            {
                fontSize = 20,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = ColorTextBright }
            };

            StyleTimerValue = new GUIStyle(StyleValueLarge)
            {
                fontSize = 28,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleRight,
                normal = { textColor = ColorTextBright }
            };

            StyleFloatingScore = new GUIStyle(StyleHeader)
            {
                fontSize = 20,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = ColorGreenSafe }
            };

            StyleAlertBanner = new GUIStyle(StyleHeader)
            {
                fontSize = 22,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = ColorRedCritical }
            };

            StyleModalSubtitle = new GUIStyle(StyleHeader)
            {
                fontSize = 16,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = ColorCyan }
            };

            StyleTotalScoreValue = new GUIStyle(StyleValueLarge)
            {
                fontSize = 32,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleRight,
                normal = { textColor = ColorGreenSafe }
            };

            StyleBestScoreText = new GUIStyle(StyleLabelMuted)
            {
                fontSize = 14,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = ColorTextMuted }
            };

            StyleFailReason = new GUIStyle(StyleHeader)
            {
                fontSize = 18,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = ColorAmberWarning }
            };

            StyleDifficultyBadge = new GUIStyle(StyleLabelMuted)
            {
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
        }

        // ==========================================
        // 2. TEXT FITTING & LABELS
        // ==========================================
        public static void DrawShadowedLabel(Rect r, string text, GUIStyle style, Color textColor, float offset = 1.5f)
        {
            Color prev = style.normal.textColor;
            style.normal.textColor = ColorTextShadow;
            GUI.Label(new Rect(r.x + offset, r.y + offset, r.width, r.height), text, style);
            style.normal.textColor = textColor;
            GUI.Label(r, text, style);
            style.normal.textColor = prev;
        }

        public static void DrawFittedTitle(Rect r, string text, GUIStyle style, Color textColor, int maxFont = 26, int minFont = 14)
        {
            int origFont = style.fontSize;
            int chosenFont = maxFont;
            GUIContent content = new GUIContent(text);

            for (int f = maxFont; f >= minFont; f -= 2)
            {
                style.fontSize = f;
                Vector2 size = style.CalcSize(content);
                if (size.x <= r.width || f == minFont)
                {
                    chosenFont = f;
                    break;
                }
            }

            style.fontSize = chosenFont;
            DrawShadowedLabel(r, text, style, textColor);
            style.fontSize = origFont;
        }

        // ==========================================
        // 3. UI PRIMITIVES: PILLS, BADGES, BARS
        // ==========================================
        public static void DrawBadge(Rect r, string text, Color bgColor, Color textColor, int fontSize = 14)
        {
            Color prevBg = GUI.backgroundColor;
            GUI.backgroundColor = bgColor;
            GUI.Box(r, GUIContent.none, StylePanel);
            GUI.backgroundColor = prevBg;

            StyleDifficultyBadge.fontSize = fontSize;
            DrawShadowedLabel(r, text, StyleDifficultyBadge, textColor, 1.0f);
        }

        public static void DrawSegmentedBar(Rect r, float fill01, int segments, Color fillColor, Color emptyColor)
        {
            // Background slot
            GUI.DrawTexture(r, TexBarBg);

            fill01 = Mathf.Clamp01(fill01);
            int activeSegments = Mathf.CeilToInt(fill01 * segments);
            if (fill01 <= 0.001f) activeSegments = 0;

            float spacing = 2f;
            float totalSpacing = spacing * (segments - 1);
            float segWidth = (r.width - 4f - totalSpacing) / segments;
            float segHeight = r.height - 4f;
            float startX = r.x + 2f;
            float startY = r.y + 2f;

            Color prevCol = GUI.color;
            for (int i = 0; i < segments; i++)
            {
                Rect segRect = new Rect(startX + i * (segWidth + spacing), startY, segWidth, segHeight);
                GUI.color = (i < activeSegments) ? fillColor : emptyColor;
                GUI.DrawTexture(segRect, TexBarFill);
            }
            GUI.color = prevCol;
        }

        public static void DrawProgressBar(Rect r, float fill01, Color fillColor, Color emptyColor)
        {
            GUI.DrawTexture(r, TexBarBg);
            fill01 = Mathf.Clamp01(fill01);
            if (fill01 > 0f)
            {
                Rect fillRect = new Rect(r.x + 1f, r.y + 1f, (r.width - 2f) * fill01, r.height - 2f);
                Color prev = GUI.color;
                GUI.color = fillColor;
                GUI.DrawTexture(fillRect, TexBarFill);
                GUI.color = prev;
            }
        }

        public static void DrawSpeedometerBar(Rect r, float currentSpeed, float maxSpeed, float dockSpeedLimit)
        {
            GUI.DrawTexture(r, TexBarBg);

            float fill = Mathf.Clamp01(currentSpeed / Mathf.Max(1f, maxSpeed));
            Color speedColor = (currentSpeed <= dockSpeedLimit) ? ColorGreenSafe : ColorRedCritical;

            if (fill > 0f)
            {
                Rect fillRect = new Rect(r.x + 1f, r.y + 1f, (r.width - 2f) * fill, r.height - 2f);
                Color prev = GUI.color;
                GUI.color = speedColor;
                GUI.DrawTexture(fillRect, TexBarFill);
                GUI.color = prev;
            }

            // Dock speed tick marker
            float tickX = r.x + 1f + (r.width - 2f) * Mathf.Clamp01(dockSpeedLimit / maxSpeed);
            Rect tickRect = new Rect(tickX - 1f, r.y - 2f, 2f, r.height + 4f);
            Color pCol = GUI.color;
            GUI.color = ColorAmberWarning;
            GUI.DrawTexture(tickRect, TexBarFill);
            GUI.color = pCol;
        }

        public static void DrawIcon(Rect r, ProceduralIcon icon, Color tint)
        {
            if (iconTextures.TryGetValue(icon, out Texture2D tex) && tex != null)
            {
                Color prev = GUI.color;
                GUI.color = tint;
                GUI.DrawTexture(r, tex, ScaleMode.ScaleToFit);
                GUI.color = prev;
            }
        }

        public static void DrawDimScreenOverlay(float viewW, float viewH)
        {
            GUI.DrawTexture(new Rect(0, 0, viewW, viewH), TexDimOverlay);
        }

        // ==========================================
        // 4. PROCEDURAL TEXTURE GENERATION
        // ==========================================
        private static Texture2D CreateSolidTexture(int w, int h, Color col)
        {
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[w * h];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = col;
            tex.SetPixels(pixels);
            tex.Apply();
            return tex;
        }

        private static Texture2D CreateRoundedBoxTex(int w, int h, int radius, Color bg, Color border, int borderWidth)
        {
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    // Check distance to 4 rounded corners
                    float dx = Mathf.Min(x, w - 1 - x);
                    float dy = Mathf.Min(y, h - 1 - y);

                    if (dx < radius && dy < radius)
                    {
                        float d = Vector2.Distance(new Vector2(dx, dy), new Vector2(radius, radius));
                        if (d > radius)
                        {
                            tex.SetPixel(x, y, Color.clear);
                            continue;
                        }
                    }

                    bool isBorder = (x < borderWidth || x >= w - borderWidth || y < borderWidth || y >= h - borderWidth);
                    tex.SetPixel(x, y, isBorder ? border : bg);
                }
            }
            tex.Apply();
            return tex;
        }

        private static Texture2D CreateSciFiPanelTex(int w, int h, Color bg, Color border, int bWidth, bool cornerBrackets)
        {
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            int bracketLen = 8;

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    // Subtle top highlight gradient
                    float topGradient = (y >= h - 4) ? 0.08f : 0f;
                    Color currentBg = new Color(bg.r + topGradient, bg.g + topGradient, bg.b + topGradient, bg.a);

                    bool isEdge = (x < bWidth || x >= w - bWidth || y < bWidth || y >= h - bWidth);
                    bool isBracket = false;

                    if (cornerBrackets)
                    {
                        bool nearLeft = x < bracketLen;
                        bool nearRight = x >= w - bracketLen;
                        bool nearBottom = y < bracketLen;
                        bool nearTop = y >= h - bracketLen;

                        if ((nearLeft || nearRight) && (y < bWidth + 2 || y >= h - (bWidth + 2))) isBracket = true;
                        if ((nearTop || nearBottom) && (x < bWidth + 2 || x >= w - (bWidth + 2))) isBracket = true;
                    }

                    if (isBracket)
                    {
                        tex.SetPixel(x, y, ColorCyan);
                    }
                    else if (isEdge)
                    {
                        tex.SetPixel(x, y, border);
                    }
                    else
                    {
                        tex.SetPixel(x, y, currentBg);
                    }
                }
            }
            tex.Apply();
            return tex;
        }

        // ==========================================
        // 5. PROCEDURAL ICONS (64x64 / 32x32)
        // ==========================================
        private static Texture2D CreateBeaconIcon(int w, int h)
        {
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            ClearTexture(tex);
            Vector2 c = new Vector2(w / 2f, h / 2f);

            // Center transmission beacon circle + radiating concentric arc waves
            DrawCircle(tex, c, 5f, ColorGreenSafe, true);
            DrawRing(tex, c, 10f, 13f, ColorGreenSafe);
            DrawRing(tex, c, 17f, 20f, new Color(ColorGreenSafe.r, ColorGreenSafe.g, ColorGreenSafe.b, 0.65f));
            tex.Apply();
            return tex;
        }

        private static Texture2D CreateReticleIcon(int w, int h)
        {
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            ClearTexture(tex);
            Vector2 c = new Vector2(w / 2f, h / 2f);

            DrawRing(tex, c, 12f, 15f, ColorCyan);
            DrawCircle(tex, c, 3f, ColorCyan, true);
            // 4 Crosshair ticks
            DrawLine(tex, new Vector2(c.x - 20, c.y), new Vector2(c.x - 7, c.y), ColorCyan, 2);
            DrawLine(tex, new Vector2(c.x + 7, c.y), new Vector2(c.x + 20, c.y), ColorCyan, 2);
            DrawLine(tex, new Vector2(c.x, c.y - 20), new Vector2(c.x, c.y - 7), ColorCyan, 2);
            DrawLine(tex, new Vector2(c.x, c.y + 7), new Vector2(c.x, c.y + 20), ColorCyan, 2);
            tex.Apply();
            return tex;
        }

        private static Texture2D CreateWarningTriangleIcon(int w, int h)
        {
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            ClearTexture(tex);

            Vector2 p1 = new Vector2(w / 2f, h - 8f);
            Vector2 p2 = new Vector2(8f, 10f);
            Vector2 p3 = new Vector2(w - 8f, 10f);

            DrawLine(tex, p1, p2, ColorAmberWarning, 3);
            DrawLine(tex, p2, p3, ColorAmberWarning, 3);
            DrawLine(tex, p3, p1, ColorAmberWarning, 3);

            // Exclamation mark
            Vector2 c = new Vector2(w / 2f, h / 2f);
            DrawLine(tex, new Vector2(c.x, c.y + 8), new Vector2(c.x, c.y - 2), ColorAmberWarning, 2);
            DrawCircle(tex, new Vector2(c.x, c.y - 7), 2f, ColorAmberWarning, true);
            tex.Apply();
            return tex;
        }

        private static Texture2D CreateSingularityIcon(int w, int h)
        {
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            ClearTexture(tex);
            Vector2 c = new Vector2(w / 2f, h / 2f);

            // Swirling gravitational accretion rings + dark core
            DrawRing(tex, c, 18f, 21f, new Color(ColorMagenta.r, ColorMagenta.g, ColorMagenta.b, 0.5f));
            DrawRing(tex, c, 13f, 16f, ColorMagenta);
            DrawRing(tex, c, 8f, 11f, ColorCyan);
            DrawCircle(tex, c, 6f, new Color(0.02f, 0.04f, 0.08f, 1f), true);
            tex.Apply();
            return tex;
        }

        private static Texture2D CreateGateIcon(int w, int h)
        {
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            ClearTexture(tex);
            Vector2 c = new Vector2(w / 2f, h / 2f);
            DrawRing(tex, c, 9f, 12f, ColorCyan);
            DrawRing(tex, c, 4f, 6f, ColorGreenSafe);
            tex.Apply();
            return tex;
        }

        private static Texture2D CreateCapsuleIcon(int w, int h)
        {
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            ClearTexture(tex);
            Vector2 c = new Vector2(w / 2f, h / 2f);
            DrawCircle(tex, c, 6f, ColorMagenta, true);
            DrawRing(tex, c, 8f, 10f, Color.white);
            tex.Apply();
            return tex;
        }

        private static Texture2D CreateBatteryIcon(int w, int h)
        {
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            ClearTexture(tex);
            // Battery shell outline
            for (int y = 8; y < h - 8; y++)
            {
                for (int x = 6; x < w - 10; x++)
                {
                    bool isBorder = (x == 6 || x == w - 11 || y == 8 || y == h - 9);
                    if (isBorder) tex.SetPixel(x, y, ColorGreenSafe);
                    else if (x < w - 16) tex.SetPixel(x, y, ColorGreenSafe);
                }
            }
            // Battery positive terminal
            for (int y = 12; y < h - 12; y++)
            {
                for (int x = w - 10; x < w - 7; x++)
                {
                    tex.SetPixel(x, y, ColorGreenSafe);
                }
            }
            tex.Apply();
            return tex;
        }

        private static Texture2D CreateSpeedometerIcon(int w, int h)
        {
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            ClearTexture(tex);
            Vector2 c = new Vector2(w / 2f, h / 2f - 2f);
            DrawRing(tex, c, 9f, 12f, ColorCyan);
            DrawLine(tex, c, new Vector2(c.x + 7f, c.y + 7f), ColorAmberWarning, 2);
            tex.Apply();
            return tex;
        }

        private static Texture2D CreateDockIcon(int w, int h)
        {
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            ClearTexture(tex);
            // U-shaped docking cradle
            Vector2 p1 = new Vector2(8f, h - 8f);
            Vector2 p2 = new Vector2(8f, 10f);
            Vector2 p3 = new Vector2(w - 8f, 10f);
            Vector2 p4 = new Vector2(w - 8f, h - 8f);
            DrawLine(tex, p1, p2, ColorCyan, 2);
            DrawLine(tex, p2, p3, ColorCyan, 2);
            DrawLine(tex, p3, p4, ColorCyan, 2);
            // Center beacon dot
            DrawCircle(tex, new Vector2(w / 2f, 18f), 3f, ColorGreenSafe, true);
            tex.Apply();
            return tex;
        }

        private static Texture2D CreateLockIcon(int w, int h)
        {
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            ClearTexture(tex);
            Vector2 c = new Vector2(w / 2f, h / 2f);

            // Shackle arc
            DrawRing(tex, new Vector2(c.x, c.y + 3f), 5f, 7f, ColorRedCritical);
            // Body
            for (int y = (int)c.y - 7; y <= (int)c.y + 2; y++)
            {
                for (int x = (int)c.x - 7; x <= (int)c.x + 7; x++)
                {
                    tex.SetPixel(x, y, ColorRedCritical);
                }
            }
            tex.Apply();
            return tex;
        }

        private static Texture2D CreateCheckmarkIcon(int w, int h)
        {
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            ClearTexture(tex);
            Vector2 p1 = new Vector2(8f, h / 2f);
            Vector2 p2 = new Vector2(w * 0.4f, 10f);
            Vector2 p3 = new Vector2(w - 8f, h - 10f);

            DrawLine(tex, p1, p2, ColorGreenSafe, 3);
            DrawLine(tex, p2, p3, ColorGreenSafe, 3);
            tex.Apply();
            return tex;
        }

        // ==========================================
        // 6. DRAWING HELPERS
        // ==========================================
        private static void ClearTexture(Texture2D tex)
        {
            Color[] clear = new Color[tex.width * tex.height];
            for (int i = 0; i < clear.Length; i++) clear[i] = Color.clear;
            tex.SetPixels(clear);
        }

        private static void DrawCircle(Texture2D tex, Vector2 c, float radius, Color color, bool fill)
        {
            int minX = Mathf.Max(0, (int)(c.x - radius - 1));
            int maxX = Mathf.Min(tex.width - 1, (int)(c.x + radius + 1));
            int minY = Mathf.Max(0, (int)(c.y - radius - 1));
            int maxY = Mathf.Min(tex.height - 1, (int)(c.y + radius + 1));

            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    float d = Vector2.Distance(new Vector2(x, y), c);
                    if (fill && d <= radius)
                    {
                        tex.SetPixel(x, y, color);
                    }
                    else if (!fill && Mathf.Abs(d - radius) < 1f)
                    {
                        tex.SetPixel(x, y, color);
                    }
                }
            }
        }

        private static void DrawRing(Texture2D tex, Vector2 c, float innerRadius, float outerRadius, Color color)
        {
            int minX = Mathf.Max(0, (int)(c.x - outerRadius - 1));
            int maxX = Mathf.Min(tex.width - 1, (int)(c.x + outerRadius + 1));
            int minY = Mathf.Max(0, (int)(c.y - outerRadius - 1));
            int maxY = Mathf.Min(tex.height - 1, (int)(c.y + outerRadius + 1));

            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    float d = Vector2.Distance(new Vector2(x, y), c);
                    if (d >= innerRadius && d <= outerRadius)
                    {
                        tex.SetPixel(x, y, color);
                    }
                }
            }
        }

        private static void DrawLine(Texture2D tex, Vector2 p1, Vector2 p2, Color color, int thickness)
        {
            int steps = Mathf.Max(1, (int)Vector2.Distance(p1, p2) * 2);
            for (int i = 0; i <= steps; i++)
            {
                Vector2 p = Vector2.Lerp(p1, p2, i / (float)steps);
                for (int ty = -thickness / 2; ty <= thickness / 2; ty++)
                {
                    for (int tx = -thickness / 2; tx <= thickness / 2; tx++)
                    {
                        int px = (int)p.x + tx;
                        int py = (int)p.y + ty;
                        if (px >= 0 && px < tex.width && py >= 0 && py < tex.height)
                        {
                            tex.SetPixel(px, py, color);
                        }
                    }
                }
            }
        }
    }
}
