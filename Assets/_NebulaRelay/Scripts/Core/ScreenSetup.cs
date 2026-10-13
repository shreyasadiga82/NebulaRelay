using UnityEngine;
using UnityEngine.InputSystem;

namespace NebulaRelay.Core
{
    /// <summary>
    /// Persistent screen manager for Nebula Relay.
    /// Manages window mode, full screen window initialization, resolution clamping for laptops,
    /// and hotkey toggling (F11 and Alt+Enter).
    /// </summary>
    public class ScreenSetup : MonoBehaviour
    {
        private static bool isInitialized = false;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void AutoInitialize()
        {
            if (isInitialized) return;
            isInitialized = true;

            GameObject host = new GameObject("ScreenSetup");
            host.AddComponent<ScreenSetup>();
            DontDestroyOnLoad(host);
        }

        private void Start()
        {
            ApplyInitialScreenMode();
        }

        public static void ApplyInitialScreenMode()
        {
            Resolution nativeRes = Screen.currentResolution;
            if (!PlayerPrefs.HasKey("NebulaRelay_ScreenInit"))
            {
                PlayerPrefs.SetInt("NebulaRelay_ScreenInit", 1);
                PlayerPrefs.Save();
                Screen.SetResolution(nativeRes.width, nativeRes.height, FullScreenMode.FullScreenWindow);
            }
        }

        private void Update()
        {
            // Toggle fullscreen with F11 or Alt+Enter (New Input System)
            bool f11Pressed = Keyboard.current != null && Keyboard.current.f11Key.wasPressedThisFrame;
            bool altEnter = Keyboard.current != null &&
                            (Keyboard.current.leftAltKey.isPressed || Keyboard.current.rightAltKey.isPressed) &&
                            (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.numpadEnterKey.wasPressedThisFrame);

            if (f11Pressed || altEnter)
            {
                ToggleFullscreen();
            }
        }

        public static void ToggleFullscreen()
        {
            if (Screen.fullScreenMode == FullScreenMode.FullScreenWindow || Screen.fullScreenMode == FullScreenMode.ExclusiveFullScreen)
            {
                // Switch to windowed mode: clamp to at most 90% of the display so it fits laptops comfortably
                Resolution current = Screen.currentResolution;
                int maxW = Mathf.FloorToInt(current.width * 0.90f);
                int maxH = Mathf.FloorToInt(current.height * 0.90f);

                int targetW = 1280;
                int targetH = 720;
                if (targetW > maxW || targetH > maxH)
                {
                    float aspect = 16f / 9f;
                    targetW = Mathf.Min(maxW, Mathf.FloorToInt(maxH * aspect));
                    targetH = Mathf.FloorToInt(targetW / aspect);
                }

                Screen.SetResolution(targetW, targetH, FullScreenMode.Windowed);
            }
            else
            {
                Resolution current = Screen.currentResolution;
                Screen.SetResolution(current.width, current.height, FullScreenMode.FullScreenWindow);
            }
        }
    }
}
