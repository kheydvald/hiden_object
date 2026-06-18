// LevelProgressTracker.cs
using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

public class LevelProgressTracker : MonoBehaviour
{
    public static LevelProgressTracker Instance { get; private set; }

    public event Action OnAllItemsFound;
    public static event Action<string> OnLevelCompleted;

    private List<SearchZone> allZones = new List<SearchZone>();
    private HashSet<SearchZone> completedZones = new HashSet<SearchZone>();
    private int totalItemsCount = 0;
    private int foundItemsCount = 0;
    private System.Collections.Generic.Dictionary<SearchZone, Action> zoneHandlers = new System.Collections.Generic.Dictionary<SearchZone, Action>();

    // Track found items by name
    private HashSet<string> foundItemNames = new HashSet<string>();

    // Key to store progress in PlayerPrefs
    private string progressKey;

    // Store completed zones
    private HashSet<string> completedZonesSet = new HashSet<string>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            // Если уже есть экземпляр, удаляем этот
            if (this != Instance)
            {
                Destroy(gameObject);
            }
        }
    }

    /// <summary>
    /// Инициализация трекера - находит все зоны на уровне
    /// </summary>
    public void Initialize()
    {
        // Generate a unique key for this level based on the scene name or parent level name
        string levelName = transform.parent?.name ?? gameObject.scene.name;
        // Use the level name from LevelManager if available to ensure uniqueness
        var levelManager = FindFirstObjectByType<LevelManager>();
        if (levelManager != null && levelManager.currentLevelConfig != null)
        {
            levelName = levelManager.currentLevelConfig.LevelName;
        }
        progressKey = $"LevelProgress_{levelName}";

        // Находим все зоны на сцене (используем FindObjectsByType для поиска по всей сцене)
        allZones.Clear();
        allZones.AddRange(FindObjectsByType<SearchZone>(FindObjectsSortMode.None));

        // Подписываемся на события всех зон
        foreach (var zone in allZones)
        {
            zoneHandlers[zone] = () => OnZoneCompleted(zone);
            zone.OnZoneCompleted += zoneHandlers[zone];
        }

        // 🔥 Подсчитываем общее количество предметов ТОЛЬКО через items[] массивы в зонах
        // Это предотвращает дублирование (SearchableItem - это только визуальное представление)
        totalItemsCount = 0;
        HashSet<string> uniqueItems = new HashSet<string>();
        
        foreach (var zone in allZones)
        {
            if (zone != null && zone.items != null)
            {
                foreach (var item in zone.items)
                {
                    if (item != null && !uniqueItems.Contains(item.itemName))
                    {
                        uniqueItems.Add(item.itemName);
                        totalItemsCount++;
                    }
                }
            }
        }

        // Load saved progress
        LoadProgress();

#if UNITY_EDITOR || DEBUG
        Debug.Log($"LevelProgressTracker: Инициализирован. Зон: {allZones.Count}, Предметов (уникальных через items[]): {totalItemsCount}. Found items from save: {foundItemsCount}");
#endif
    }

    /// <summary>
    /// Load progress from PlayerPrefs
    /// </summary>
    private void LoadProgress()
    {
        if (string.IsNullOrEmpty(progressKey)) return;

        // Load the list of found item names
        string foundItemsString = PlayerPrefs.GetString(progressKey, "");
        if (!string.IsNullOrEmpty(foundItemsString))
        {
            foundItemNames = new HashSet<string>(foundItemsString.Split(','));
        }

        // Load completed zones
        string completedZonesString = PlayerPrefs.GetString(progressKey + "_completedZones", "");
        if (!string.IsNullOrEmpty(completedZonesString))
        {
            completedZonesSet = new HashSet<string>(completedZonesString.Split(','));
        }

        // Apply the found state to all searchable items in the scene
        var allSearchableItems = GetComponentsInChildren<SearchableItem>(true);
        foreach (var item in allSearchableItems)
        {
            if (item != null && item.GetItemData() != null)
            {
                string itemName = item.GetItemData().itemName;
                if (foundItemNames.Contains(itemName))
                {
                    item.MarkAsFoundInScene();
                }
            }
        }

        foundItemsCount = foundItemNames.Count;
    }

    /// <summary>
    /// Save progress to PlayerPrefs
    /// </summary>
    private void SaveProgress()
    {
        if (string.IsNullOrEmpty(progressKey)) return;

        // Convert the set of found item names to a comma-separated string
        string foundItemsString = string.Join(",", foundItemNames);

        // Also save completed zones
        string completedZonesString = string.Join(",", completedZonesSet);

        PlayerPrefs.SetString(progressKey, foundItemsString);
        PlayerPrefs.SetString(progressKey + "_completedZones", completedZonesString);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Public method to register when an item is found
    /// </summary>
    public void RegisterItemFound(string itemName)
    {
        if (!foundItemNames.Contains(itemName))
        {
            foundItemNames.Add(itemName);
            foundItemsCount = foundItemNames.Count;
            SaveProgress();

#if UNITY_EDITOR || DEBUG
            Debug.Log($"LevelProgressTracker: Registered item found: {itemName}. Total found: {foundItemsCount}/{totalItemsCount}");
#endif

            // Check if all items have been found
            if (foundItemsCount >= totalItemsCount)
            {
#if UNITY_EDITOR || DEBUG
                Debug.Log("LevelProgressTracker: All items found! Level completed!");
#endif

                // Notify any subscribers that all items have been found
                OnAllItemsFound?.Invoke();

                // Notify LevelManager about level completion
                LevelManager levelManager = FindFirstObjectByType<LevelManager>();
                if (levelManager != null)
                {
#if UNITY_EDITOR || DEBUG
                    Debug.Log("LevelProgressTracker: Notifying LevelManager about level completion");
#endif
                    // Trigger level completion directly
                    levelManager.CompleteLevel();

                    // Notify subscribers about level completion with level name
                    string levelName = levelManager.currentLevelConfig?.LevelName ?? gameObject.scene.name;
                    OnLevelCompleted?.Invoke(levelName);
                }
            }
        }
    }

    /// <summary>
    /// Check if an item has been found
    /// </summary>
    public bool IsItemFound(string itemName)
    {
        return foundItemNames.Contains(itemName);
    }

    /// <summary>
    /// Callback когда зона завершена (все предметы в зоне найдены)
    /// </summary>
    private void OnZoneCompleted(SearchZone zone)
    {
        // Проверяем, не была ли зона уже завершена (защита от повторных вызовов)
        if (completedZones.Contains(zone))
        {
            return;
        }

        completedZones.Add(zone);

        // Обновляем foundItemsCount на основе уже найденных предметов
        foundItemsCount = foundItemNames.Count;

#if UNITY_EDITOR || DEBUG
        Debug.Log($"LevelProgressTracker: Зона '{zone.name}' завершена. Найдено предметов: {foundItemsCount}/{totalItemsCount}");
#endif

        // Save progress after each item is found
        SaveProgress();

        // Проверяем, все ли предметы найдены
        if (foundItemsCount >= totalItemsCount && completedZones.Count == allZones.Count)
        {
#if UNITY_EDITOR || DEBUG
            Debug.Log("LevelProgressTracker: Все предметы найдены во всех зонах! (дублирующая проверка)");
#endif
            // RegisterItemFound() уже вызвал CompleteLevel(), ничего делать не нужно
        }
    }

    private void OnDestroy()
    {
        // Отписываемся от всех зон
        foreach (var kv in zoneHandlers)
        {
            var zone = kv.Key;
            var handler = kv.Value;
            if (zone != null)
            {
                zone.OnZoneCompleted -= handler;
            }
        }
        zoneHandlers.Clear();

        if (Instance == this)
        {
            Instance = null;
        }
    }

    /// <summary>
    /// Проверяет, завершена ли зона
    /// </summary>
    public bool IsZoneCompleted(SearchZone zone)
    {
        string key = GetZoneKey(zone);
        return completedZonesSet.Contains(key);
    }

    /// <summary>
    /// Сохраняет состояние завершения зоны
    /// </summary>
    public void SaveLevelStateForZone(SearchZone zone)
    {
        string key = GetZoneKey(zone);
        completedZonesSet.Add(key);
        SaveProgress(); // → в PlayerPrefs
    }

    /// <summary>
    /// Генерирует уникальный ключ для зоны
    /// </summary>
    private string GetZoneKey(SearchZone zone)
    {
        // Уникальный ключ: Level + Zone
        string levelName = zone.GetComponentInParent<LevelManager>()?.currentLevelConfig?.LevelName ?? "Unknown";
        return $"{levelName}_{zone.name}";
    }

    /// <summary>
    /// Сбросить состояние трекера прогресса
    /// </summary>
    public void ResetTracker()
    {
        // Очистить все списки и счетчики
        allZones.Clear();
        completedZones.Clear();
        foundItemNames.Clear();
        foundItemsCount = 0;
        totalItemsCount = 0;

        // Очистить словарь обработчиков
        foreach (var kv in zoneHandlers)
        {
            var zone = kv.Key;
            var handler = kv.Value;
            if (zone != null)
            {
                zone.OnZoneCompleted -= handler;
            }
        }
        zoneHandlers.Clear();

        // Очистить списки завершенных зон
        completedZonesSet.Clear();

        // Удалить сохраненный прогресс из PlayerPrefs
        if (!string.IsNullOrEmpty(progressKey))
        {
            PlayerPrefs.DeleteKey(progressKey);
            PlayerPrefs.DeleteKey(progressKey + "_completedZones");
            PlayerPrefs.Save();
        }
    }
}

