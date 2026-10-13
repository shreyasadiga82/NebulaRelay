using UnityEngine;
using System.IO;

namespace NebulaRelay.Core
{
    /// <summary>
    /// Runtime logger that logs all Application messages and errors to crash_log.txt
    /// in Application.persistentDataPath for standalone build debugging.
    /// </summary>
    public class RuntimeFileLogger : MonoBehaviour
    {
        private static RuntimeFileLogger instance;
        private static string logFilePath;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void AutoInitialize()
        {
            if (instance != null) return;
            GameObject go = new GameObject("[RuntimeFileLogger]");
            instance = go.AddComponent<RuntimeFileLogger>();
            DontDestroyOnLoad(go);

            try
            {
                logFilePath = Path.Combine(Application.persistentDataPath, "crash_log.txt");
                File.WriteAllText(logFilePath, $"=== Drift Core Runtime Log Started at {System.DateTime.Now:yyyy-MM-dd HH:mm:ss} ===\nPlatform: {Application.platform}, Version: {Application.unityVersion}\nPersistentDataPath: {Application.persistentDataPath}\n\n");
            }
            catch { }

            Application.logMessageReceived += OnLogMessageReceived;
        }

        private static void OnLogMessageReceived(string condition, string stackTrace, LogType type)
        {
            if (string.IsNullOrEmpty(logFilePath)) return;
            try
            {
                string line = $"[{System.DateTime.Now:HH:mm:ss.fff}] [{type}] {condition}\n";
                if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
                {
                    line += $"Stack: {stackTrace}\n";
                }
                File.AppendAllText(logFilePath, line);
            }
            catch { }
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                Application.logMessageReceived -= OnLogMessageReceived;
            }
        }
    }
}
