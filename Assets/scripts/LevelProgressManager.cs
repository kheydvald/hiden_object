using UnityEngine;
using System.Collections.Generic;
using YG.Utils;

/// <summary>
/// Менеджер прогресса уровней. Сохраняется между сценами (DontDestroyOnLoad).
/// Управляет разблокировкой уровней и сохранением прогресса в Yandex Games LocalStorage.
/// </summary>
public class LevelProgressManager : PersistentManager<LevelProgressManager>
{
    [Header("Level Unlock Configuration")]
    [Tooltip("Список конфигураций уровней для разблокировки. Если пустой — все уровни разблокированы по умолчанию")]
    public List<LevelConfig> levelConfigs;

    // Событие, которое вызывается при разблокировке уровня
    public static event System.Action<string> OnLevelUnlocked;

    private const string SAVE_KEY = "LevelProgress";
    private Dictionary<string, bool> unlockedLevels = new Dictionary<string, bool>();
    private Dictionary<string, bool> completedLevels = new Dictionary<string, bool>();

    protected override void OnInit()
    {
        LoadProgress();
    }

    /// <summary>
    /// Load saved level progress from Yandex Games LocalStorage
    /// </summary>
    public void LoadProgress()
    {
        string savedData = LocalStorage.GetKey(SAVE_KEY, "");

#if UNITY_EDITOR || DEBUG
        Debug.Log($"LevelProgressManager: Loading progress, raw data: {(string.IsNullOrEmpty(savedData) ? "empty" : savedData)}");
#endif

        if (!string.IsNullOrEmpty(savedData))
        {
            try
            {
                var tempDict = JsonYG.DeserializeDictionary(savedData);
                unlockedLevels = new Dictionary<string, bool>();
                completedLevels = new Dictionary<string, bool>();

                foreach (var kvp in tempDict)
                {
                    if (kvp.Key.StartsWith("COMPLETED_"))
                    {
                        string levelName = kvp.Key.Substring("COMPLETED_".Length);
                        completedLevels[levelName] = kvp.Value == 1;
#if UNITY_EDITOR || DEBUG
                        Debug.Log($"LevelProgressManager: Loaded completed level '{levelName}' with status: {kvp.Value == 1}");
#endif
                    }
                    else
                    {
                        unlockedLevels[kvp.Key] = kvp.Value == 1;
#if UNITY_EDITOR || DEBUG
                        Debug.Log($"LevelProgressManager: Loaded level '{kvp.Key}' with unlock status: {kvp.Value == 1}");
#endif
                    }
                }

#if UNITY_EDITOR || DEBUG
                Debug.Log($"LevelProgressManager: Successfully loaded {unlockedLevels.Count} levels from storage, {completedLevels.Count} completed levels");
#endif
            }
            catch (System.Exception e)
            {
#if DEBUG
                Debug.LogError($"Error loading level progress: {e.Message}");
#endif
                InitializeDefaultLevels();
            }
        }
        else
        {
#if UNITY_EDITOR || DEBUG
            Debug.Log("LevelProgressManager: No saved data found, initializing defaults");
#endif
            InitializeDefaultLevels();
        }

#if UNITY_EDITOR || DEBUG
        Debug.Log($"LevelProgressManager: Final unlocked levels count: {unlockedLevels.Count}, completed levels count: {completedLevels.Count}");
#endif
    }

    /// <summary>
    /// Initialize default levels (all levels without dependencies are unlocked by default)
    /// </summary>
    private void InitializeDefaultLevels()
    {
        unlockedLevels.Clear();

        if (levelConfigs != null && levelConfigs.Count > 0)
        {
            foreach (var levelConfig in levelConfigs)
            {
                if (levelConfig == null) continue;

                if (levelConfig.requiredPreviousLevels == null || levelConfig.requiredPreviousLevels.Count == 0)
                {
                    unlockedLevels[levelConfig.LevelName] = true;
#if UNITY_EDITOR || DEBUG
                    Debug.Log($"LevelProgressManager: Initialized default - unlocked level without dependencies: {levelConfig.LevelName}");
#endif
                }
                else
                {
#if UNITY_EDITOR || DEBUG
                    Debug.Log($"LevelProgressManager: Level '{levelConfig.LevelName}' has dependencies: [{string.Join(", ", levelConfig.requiredPreviousLevels)}], keeping locked initially");
#endif
                }
            }

            if (unlockedLevels.Count == 0)
            {
                string firstLevelName = levelConfigs[0].LevelName;
                unlockedLevels[firstLevelName] = true;
#if UNITY_EDITOR || DEBUG
                Debug.Log($"LevelProgressManager: Initialized default - unlocked first level as fallback: {firstLevelName}");
#endif
            }
        }
        else
        {
#if UNITY_EDITOR || DEBUG
            Debug.LogWarning("LevelProgressManager: No level unlock data found! Unlocking all levels by default.");
#endif
        }
    }

    /// <summary>
    /// Save current level progress to Yandex Games LocalStorage
    /// </summary>
    public void SaveProgress()
    {
        var saveData = new Dictionary<string, int>();

        foreach (var kvp in unlockedLevels)
        {
            saveData[kvp.Key] = kvp.Value ? 1 : 0;
        }

        foreach (var kvp in completedLevels)
        {
            string completedKey = "COMPLETED_" + kvp.Key;
            saveData[completedKey] = kvp.Value ? 1 : 0;
        }

        string jsonData = JsonYG.SerializeDictionary(saveData);
        LocalStorage.SetKey(SAVE_KEY, jsonData);

#if UNITY_EDITOR || DEBUG
        Debug.Log($"LevelProgressManager: Saved progress for {unlockedLevels.Count} levels, {completedLevels.Count} completed levels");
#endif
    }

    /// <summary>
    /// Check if a specific level is unlocked
    /// </summary>
    public bool IsLevelUnlocked(string levelName)
    {
        if (levelConfigs == null || levelConfigs.Count == 0)
        {
            return true;
        }

        return unlockedLevels.ContainsKey(levelName) && unlockedLevels[levelName];
    }

    /// <summary>
    /// Check if a specific level is completed (passed)
    /// </summary>
    public bool IsLevelCompleted(string levelName)
    {
        return completedLevels.ContainsKey(levelName) && completedLevels[levelName];
    }

    /// <summary>
    /// Mark a specific level as completed
    /// </summary>
    public void MarkLevelCompleted(string levelName)
    {
#if UNITY_EDITOR || DEBUG
        Debug.Log($"LevelProgressManager: MarkLevelCompleted called for level '{levelName}'");
#endif

        completedLevels[levelName] = true;
        SaveProgress();
        OnLevelCompleted?.Invoke(levelName);
        CheckForNewlyUnlockedLevels(levelName);
    }

    /// <summary>
    /// Event triggered when a level is completed
    /// </summary>
    public static event System.Action<string> OnLevelCompleted;

    /// <summary>
    /// Unlock a specific level and save progress
    /// </summary>
    public void UnlockLevel(string levelName)
    {
        bool wasAlreadyUnlocked = unlockedLevels.ContainsKey(levelName) && unlockedLevels[levelName];

#if DEBUG
        Debug.Log($"LevelProgressManager: Attempting to unlock level '{levelName}'. Was already unlocked: {wasAlreadyUnlocked}");
#endif

        unlockedLevels[levelName] = true;
        SaveProgress();
        CheckForNewlyUnlockedLevels(levelName);

        if (!wasAlreadyUnlocked)
        {
#if DEBUG
            Debug.Log($"LevelProgressManager: Invoking OnLevelUnlocked event for '{levelName}'");
#endif
            OnLevelUnlocked?.Invoke(levelName);
        }
    }

    /// <summary>
    /// Check if any levels should be unlocked based on a specific level being unlocked
    /// </summary>
    private void CheckForNewlyUnlockedLevels(string justUnlockedLevel)
    {
        if (levelConfigs == null) return;

        bool changesMade = false;

        foreach (var levelConfig in levelConfigs)
        {
            if (levelConfig == null) continue;

            if (!IsLevelUnlocked(levelConfig.LevelName))
            {
                if (DoesLevelDependOn(levelConfig, justUnlockedLevel))
                {
                    bool canUnlock = CheckDependencies(levelConfig);

                    if (canUnlock)
                    {
                        unlockedLevels[levelConfig.LevelName] = true;
                        changesMade = true;

#if DEBUG
                        Debug.Log($"LevelProgressManager: Unlocked level '{levelConfig.LevelName}' due to dependency d on '{justUnlockedLevel}'");
#endif

                        OnLevelUnlocked?.Invoke(levelConfig.LevelName);
                    }
                }
            }
        }

        if (changesMade)
        {
            SaveProgress();
        }
    }

    /// <summary>
    /// Проверяет, зависит ли уровень от указанного уровня
    /// </summary>
    private bool DoesLevelDependOn(LevelConfig levelConfig, string dependencyLevel)
    {
        if (string.IsNullOrEmpty(dependencyLevel) || levelConfig.requiredPreviousLevels == null)
        {
            return false;
        }

        foreach (string reqLevel in levelConfig.requiredPreviousLevels)
        {
            if (reqLevel == dependencyLevel)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Check if all dependencies for a level are met
    /// </summary>
    private bool CheckDependencies(LevelConfig levelConfig)
    {
        if (levelConfig.requiredPreviousLevels == null || levelConfig.requiredPreviousLevels.Count == 0)
        {
            return true;
        }

        foreach (string requiredLevel in levelConfig.requiredPreviousLevels)
        {
            if (!IsLevelUnlocked(requiredLevel))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Get all unlocked level names
    /// </summary>
    public List<string> GetAllUnlockedLevels()
    {
        List<string> unlocked = new List<string>();

        foreach (var kvp in unlockedLevels)
        {
            if (kvp.Value)
            {
                unlocked.Add(kvp.Key);
            }
        }

        return unlocked;
    }

    /// <summary>
    /// Reset all progress (for debugging purposes)
    /// </summary>
    public void ResetAllProgress()
    {
        LocalStorage.DeleteKey(SAVE_KEY);
        unlockedLevels.Clear();
        completedLevels.Clear();
        InitializeDefaultLevels();
        SaveProgress();

#if DEBUG
        Debug.Log("[LevelProgressManager] Progress reset successfully");
#endif
    }

    /// <summary>
    /// Reset completed levels only
    /// </summary>
    public void ResetCompletedLevels()
    {
        completedLevels.Clear();
        SaveProgress();
    }
}
