using UnityEngine;
using UnityEngine.SceneManagement;

namespace NebulaRelay.Core
{
    public enum GameState
    {
        MainMenu,
        Playing,
        LevelComplete,
        GameOver,
        Victory
    }

    /// <summary>
    /// GameManager is the persistent singleton overseeing global game progression across scenes.
    /// Tracks total relays restored (0/3 to 3/3), current level, and scene loading.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Global Progression")]
        [Tooltip("Total relays restored across the orbital network (0 to 5).")]
        [SerializeField] private int totalRelaysRestored = 0;

        [Tooltip("Maximum relays required to achieve victory across all 5 sectors.")]
        [SerializeField] private int maxRelays = 5;

        public GameState CurrentState { get; private set; } = GameState.Playing;
        public int TotalRelaysRestored => totalRelaysRestored;
        public int MaxRelays => maxRelays;
        public bool IsGameWon => totalRelaysRestored >= maxRelays;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void SetState(GameState newState)
        {
            CurrentState = newState;
        }

        /// <summary>
        /// Registers a completed relay station and checks for grand victory.
        /// </summary>
        public void RegisterRelayRestored(int levelRelayIndex)
        {
            totalRelaysRestored = Mathf.Clamp(levelRelayIndex, totalRelaysRestored, maxRelays);

            if (totalRelaysRestored >= maxRelays)
            {
                SetState(GameState.Victory);
            }
            else
            {
                SetState(GameState.LevelComplete);
            }
        }

        public void LoadLevel(int levelNumber)
        {
            SetState(GameState.Playing);
            string sceneName = $"Level_{levelNumber:D2}";
            SceneManager.LoadScene(sceneName);
        }

        public void LoadNextLevel()
        {
            int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
            int nextIndex = currentSceneIndex + 1;

            if (nextIndex < SceneManager.sceneCountInBuildSettings)
            {
                SetState(GameState.Playing);
                SceneManager.LoadScene(nextIndex);
            }
            else
            {
                // Reached the end: loop back or return to menu
                LoadMainMenu();
            }
        }

        public void RestartCurrentLevel()
        {
            SetState(GameState.Playing);
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void LoadMainMenu()
        {
            SetState(GameState.MainMenu);
            SceneManager.LoadScene("MainMenu");
        }

        public void ResetProgress()
        {
            totalRelaysRestored = 0;
            SetState(GameState.Playing);
        }

        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
