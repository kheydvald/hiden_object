using UnityEngine;

/// <summary>
/// Менеджер уровня. Управляет логикой конкретного уровня.
/// Вешается на объект уровня на сцене (не сохраняется между сценами).
/// </summary>
public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [field: SerializeField] public LevelConfig currentLevelConfig { get; set; }
    public int TotalItemsCount { get; private set; }
    private int itemsFoundCount = 0;
    private bool levelCompleted = false;

    public event System.Action OnLevelCompleted;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // НЕ используем DontDestroyOnLoad - LevelManager должен быть на сцене уровня
        }
        else
        {
            Debug.LogWarning($"[LevelManager] Duplicate instance detected. Destroying {gameObject.name}");
            Destroy(gameObject);
            return;
        }
        // Awake no longer auto-activates zones. Call Initialize() from GameManager when the scene-level UI is ready.
    }

    /// <summary>
    /// Initialize level: scan zones, compute totals and optionally auto-activate non-zoom zones.
    /// Call this after the level GameUI (words panel) has been instantiated and assigned to SearchManager.
    /// </summary>
    public void Initialize()
    {
#if DEBUG
        Debug.Log("[LevelManager] Initialize() called");
#endif

        // Инициализируем LevelProgressTracker (если есть на сцене)
        var progressTracker = FindFirstObjectByType<LevelProgressTracker>();
        if (progressTracker != null)
        {
            progressTracker.Initialize();
#if DEBUG
            Debug.Log("[LevelManager] LevelProgressTracker.Initialize() вызван");
#endif
        }
        else
        {
#if DEBUG
            Debug.LogWarning("[LevelManager] LevelProgressTracker не найден на сцене! Прогресс уровня не будет отслеживаться.");
#endif
        }

        // Ищем все SearchZone на сцене (используем FindObjectsByType для поиска по всей сцене)
        SearchZone[] zones = FindObjectsByType<SearchZone>(FindObjectsSortMode.None);
#if DEBUG
        Debug.Log($"[LevelManager] Found {zones.Length} SearchZone(s) in scene");
#endif

        int totalItems = 0;
        bool hasNonZoomZones = false;

        foreach (var zone in zones)
        {
            if (zone == null) continue;

            int zoneItems = zone.items != null ? zone.items.Length : 0;
            totalItems += zoneItems;

            // Также учитываем SearchableItem в сцене (если они не дублируют items)
            var searchableItems = zone.GetComponentsInChildren<SearchableItem>(true);
            foreach (var item in searchableItems)
            {
                if (item.GetItemData() != null)
                {
                    // Проверяем, не является ли этот предмет дубликатом из массива items
                    bool isDuplicate = false;
                    if (zone.items != null)
                    {
                        foreach (var zoneItem in zone.items)
                        {
                            if (zoneItem.itemName == item.GetItemData().itemName)
                            {
                                isDuplicate = true;
                                break;
                            }
                        }
                    }

                    if (!isDuplicate)
                    {
                        totalItems++;
                    }
                }
            }

#if DEBUG
            Debug.Log($"[LevelManager] Zone '{zone.name}': useZoom={zone.useZoom}, autoActivateOnStart={zone.autoActivateOnStart}, items={zoneItems}");
#endif

            // Проверяем, есть ли предметы для поиска в зоне
            bool hasItemsToFind = zone.HasItemsToFind();
#if DEBUG
            Debug.Log($"[LevelManager] Zone '{zone.name}': hasItemsToFind={hasItemsToFind}");
#endif

            // ✅ Авто-активация ТОЛЬКО для зон БЕЗ зума (useZoom = false)
            // Зоны с зумом НИКОГДА не активируются автоматически - только по клику!
            // Также не активируем, если в зоне нет предметов для поиска
            if (zone.autoActivateOnStart && !zone.useZoom && hasItemsToFind)
            {
                hasNonZoomZones = true;
#if DEBUG
                Debug.Log($"[LevelManager] Авто-активация SearchZone (без зума): {zone.name}");
#endif

                if (SearchManager.Instance == null)
                {
#if DEBUG
                    Debug.LogError("[LevelManager] SearchManager.Instance is null! Cannot auto-activate zone.");
#endif
                }
                else
                {
                    SearchManager.Instance.ActivateZone(zone);
                }
            }
            else if (zone.useZoom && zone.autoActivateOnStart)
            {
#if DEBUG
                Debug.LogWarning($"[LevelManager] Zone '{zone.name}' has useZoom=true but autoActivateOnStart=true. Зум-зоны не должны активироваться автоматически! Отключите autoActivateOnStart.");
#endif
            }
            else if (!hasItemsToFind && zone.autoActivateOnStart)
            {
#if DEBUG
                Debug.LogWarning($"[LevelManager] Zone '{zone.name}' has no items to find but autoActivateOnStart=true. Зона не будет активирована автоматически.");
#endif
            }
        }

        TotalItemsCount = totalItems;
        itemsFoundCount = 0; // Reset found count
#if DEBUG
        Debug.Log($"[LevelManager] Total items calculated: {TotalItemsCount}. Has non-zoom zones: {hasNonZoomZones}");
#endif
    }

    public void ItemFound()
    {
        // Защита от двойного завершения уровня
        if (levelCompleted) return;

        itemsFoundCount++;
        int remainingItems = TotalItemsCount - itemsFoundCount;
#if DEBUG
        Debug.Log($"LevelManager: Найдено предметов: {itemsFoundCount}/{TotalItemsCount}. Осталось: {remainingItems}");
#endif

        if (remainingItems <= 0)
        {
#if DEBUG
            Debug.Log("LevelManager: Уровень завершён!");
#endif

            CompleteLevel();
        }
    }

    /// <summary>
    /// Принудительное завершение уровня (например, когда все предметы уже найдены)
    /// </summary>
    public void CompleteLevel()
    {
        if (levelCompleted) return;

        levelCompleted = true;
        itemsFoundCount = TotalItemsCount; // Устанавливаем счетчик в максимальное значение

#if DEBUG
        Debug.Log("LevelManager: Уровень завершён!");
#endif

        // Level completed - notify LevelProgressManager to unlock next levels
        if (currentLevelConfig != null && LevelProgressManager.Instance != null)
        {
            LevelProgressManager.Instance.UnlockLevel(currentLevelConfig.LevelName);
        }

        OnLevelCompleted?.Invoke();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}

