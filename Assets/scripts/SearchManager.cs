
using UnityEngine;

/// <summary>
/// Менеджер поиска предметов. Управляет активными зонами (SearchZone).
/// </summary>
public class SearchManager : PersistentManager<SearchManager>
{
    [SerializeField] private GameObject wordsPanel; // ← Panel с словами
    [SerializeField] private GameObject itemWordPrefab;
    [SerializeField] private GameObject gameUI; // ← Родитель WordsPanel (для включения)

    private SearchZone activeZone;
    private int activeZoneCount = 0; // Track how many zones are currently active

    public SearchZone ActiveZone => activeZone;

    public event System.Action<HiddenItemData> OnItemFound;

    // Expose current references for runtime assignment/inspection
    public GameObject WordsPanelInstance => wordsPanel;
    public GameObject ItemWordPrefabAsset => itemWordPrefab;

    // Allow runtime assignment (useful when GameUI is instantiated via Addressables)
    public void SetWordsPanel(GameObject panel)
    {
        if (panel == null)
        {
#if DEBUG
            Debug.LogWarning("[SearchManager] SetWordsPanel called with null. Clearing WordsPanel reference.");
#endif
            wordsPanel = null;
            return;
        }

        // Если это префаб (не на сцене), инстанцируем его автоматически
        if (!panel.scene.IsValid())
        {
#if DEBUG
            Debug.LogWarning($"[SearchManager] SetWordsPanel called with prefab '{panel.name}'. Instantiating it on scene...");
#endif

            // Находим Canvas для инстанцирования
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
#if DEBUG
                Debug.LogError("[SearchManager] Canvas not found! Cannot instantiate GameUI prefab.");
#endif
                wordsPanel = null;
                return;
            }

            // Инстанцируем префаб
            GameObject instance = Instantiate(panel, canvas.transform);
            instance.name = panel.name; // Убираем (Clone) из имени
            wordsPanel = instance;
#if DEBUG
            Debug.Log($"[SearchManager] GameUI prefab instantiated: {instance.name} on Canvas '{canvas.name}'");
#endif
        }
        else
        {
            // Это уже инстанс на сцене
            wordsPanel = panel;
#if DEBUG
            Debug.Log($"[SearchManager] WordsPanel assigned (scene instance) -> {panel.name}");
#endif
        }
    }


    public void SetItemWordPrefab(GameObject prefab)
    {
        itemWordPrefab = prefab;
#if DEBUG
        Debug.Log($"SearchManager: ItemWordPrefab assigned at runtime -> {(prefab!=null?prefab.name:"null")}");
#endif
    }

    /// <summary>
    /// Назначает GameUI (родитель WordsPanel) для автоматического включения
    /// </summary>
    public void SetGameUI(GameObject ui)
    {
        gameUI = ui;
#if DEBUG
        Debug.Log($"SearchManager: GameUI assigned -> {(ui!=null?ui.name:"null")}");
#endif
    }

    /// <summary>
    /// Автоматический поиск WordsPanel на сцене
    /// </summary>
    private void FindWordsPanelAutomatically()
    {
        // Ищем по имени
        GameObject found = GameObject.Find("WordsPanel");
        if (found == null) found = GameObject.Find("Panel");
        if (found == null) found = GameObject.Find("GameUI");
        
        // Если не нашли по имени, ищем по компоненту (любой Panel с Content внутри)
        if (found == null)
        {
            Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            foreach (var canvas in canvases)
            {
                // Ищем Panel с дочерним Content
                Transform content = canvas.transform.Find("WordsPanel/Content");
                if (content == null) content = canvas.transform.Find("Panel/Content");
                if (content == null) content = canvas.transform.Find("GameUI/Content");
                if (content == null) content = canvas.transform.Find("GameUI/WordsPanel/Content");
                
                if (content != null)
                {
                    found = content.parent.gameObject;
#if DEBUG
                    Debug.Log($"[SearchManager] Found WordsPanel by Content search: {found.name}");
#endif
                    break;
                }
            }
        }

        // Рекурсивный поиск по всей иерархии Canvas
        if (found == null)
        {
            Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            foreach (var canvas in canvases)
            {
                found = FindPanelWithContent(canvas.transform);
                if (found != null)
                {
#if DEBUG
                    Debug.Log($"[SearchManager] Found WordsPanel by recursive search: {found.name}");
#endif
                    break;
                }
            }
        }

        if (found != null && found.scene.IsValid())
        {
            SetWordsPanel(found);
#if DEBUG
            Debug.Log($"[SearchManager] WordsPanel automatically assigned: {found.name}");
#endif
        }
        else
        {
#if DEBUG
            Debug.LogError("[SearchManager] Could not find WordsPanel automatically. Make sure there's a Panel with 'Content' child in Canvas hierarchy.");
#endif
        }
    }

    /// <summary>
    /// Рекурсивный поиск Panel с дочерним Content
    /// </summary>
    private GameObject FindPanelWithContent(Transform parent)
    {
        foreach (Transform child in parent)
        {
            // Проверяем, есть ли у этого объекта дочерний Content
            if (child.Find("Content") != null)
            {
                return child.gameObject;
            }
            
            // Рекурсивно ищем в детях
            GameObject found = FindPanelWithContent(child);
            if (found != null) return found;
        }
        return null;
    }

    /// <summary>
    /// Активировать зону поиска
    /// </summary>
    public void ActivateZone(SearchZone zone)
    {
        Debug.Log($"[SearchManager] === ActivateZone START ===");
        Debug.Log($"[SearchManager] Zone: {(zone != null ? zone.name : "null")}, useZoom: {(zone != null ? zone.useZoom.ToString() : "N/A")}");
        Debug.Log($"[SearchManager] ActiveZone: {(activeZone != null ? activeZone.name : "null")}");
        Debug.Log($"[SearchManager] wordsPanel: {(wordsPanel != null ? wordsPanel.name : "null")}");
        Debug.Log($"[SearchManager] gameUI: {(gameUI != null ? gameUI.name : "null")}");
        Debug.Log($"[SearchManager] itemWordPrefab: {(itemWordPrefab != null ? itemWordPrefab.name : "null")}");

        if (zone == null)
        {
#if DEBUG
            Debug.LogError("[SearchManager] ActivateZone: zone is null!");
#endif
            return;
        }

        // 🔥 Проверяем, не активен ли диалог — если активен, не активируем зону
        // ИСКЛЮЧЕНИЕ: зоны без зума (useZoom=false) могут активироваться, т.к. GameUI уже показан
        if (DialogManager.Instance != null && DialogManager.Instance.storyDialog != null &&
            DialogManager.Instance.storyDialog.gameObject.activeSelf)
        {
            // Для зон с зумом — блокируем активацию во время диалога
            if (zone.useZoom)
            {
#if DEBUG
                Debug.Log("[SearchManager] Dialog active! Skipping zone activation for zoom zone.");
#endif
                return;
            }
            // Для зон без зума — продолжаем активацию (GameUI будет показан DialogManager)
        }

        // Проверяем, завершена ли зона - если да, не активируем её снова
        if (zone.IsZoneCompleted())
        {
#if DEBUG
            Debug.Log($"[SearchManager] Zone '{zone.name}' is already completed. Skipping activation.");
#endif
            return;
        }

        // Проверяем, есть ли предметы для поиска в зоне - если нет, не активируем
        if (!zone.HasItemsToFind())
        {
#if DEBUG
            Debug.Log($"[SearchManager] Zone '{zone.name}' has no items to find. Skipping activation.");
#endif
            return;
        }

        // Если wordsPanel не назначен, пытаемся найти его автоматически
        if (wordsPanel == null)
        {
#if DEBUG
            Debug.LogWarning("[SearchManager] wordsPanel is null! Attempting to find WordsPanel on scene...");
#endif
            FindWordsPanelAutomatically();
        }

        if (wordsPanel == null)
        {
#if DEBUG
            Debug.LogError("[SearchManager] wordsPanel is not assigned and could not be found automatically! Assign a scene instance of WordsPanel (Panel in Canvas) in inspector or call SetWordsPanel at runtime after GameUI instantiation.");
#endif
            return;
        }

        if (!wordsPanel.scene.IsValid())
        {
#if DEBUG
            Debug.LogError($"[SearchManager] wordsPanel '{wordsPanel.name}' appears to be a prefab asset (not a scene object). Clearing reference. Call SetWordsPanel with instantiated GameObject.");
#endif
            wordsPanel = null;
            return;
        }

        if (itemWordPrefab == null)
        {
#if DEBUG
            Debug.LogError("[SearchManager] itemWordPrefab is not assigned! Assign ItemWordUI prefab in inspector.");
#endif
            return;
        }

        if (activeZone != null)
        {
            if (activeZone == zone)
            {
#if DEBUG
                Debug.Log($"[SearchManager] Zone '{zone.name}' is already active. Skipping activation.");
#endif
                return;
            }
#if DEBUG
            Debug.Log($"[SearchManager] Deactivating previous zone '{activeZone.name}' before activating '{zone.name}'");
#endif
            activeZone.Deactivate();
        }

        activeZone = zone;
        activeZoneCount++; // Increment the active zone counter

        // Включаем GameUI (родитель WordsPanel)
        if (gameUI != null && !gameUI.activeSelf)
        {
            gameUI.SetActive(true);
#if DEBUG
            Debug.Log($"[SearchManager] Activated GameUI: {gameUI.name}");
#endif
        }

        // Ensure the words panel is active when a zone is activated
        if (wordsPanel != null)
        {
            if (!wordsPanel.activeSelf)
            {
                wordsPanel.SetActive(true);
#if DEBUG
                Debug.Log($"[SearchManager] Activated words panel for zone '{zone.name}'");
#endif
            }
            else
            {
#if DEBUG
                Debug.Log($"[SearchManager] Words panel already active: {wordsPanel.name}");
#endif
            }
        }
        else
        {
#if DEBUG
            Debug.LogError("[SearchManager] wordsPanel is null! Cannot activate.");
#endif
        }

#if DEBUG
        Debug.Log($"[SearchManager] Activating zone '{zone.name}' (useZoom={zone.useZoom}), active zones count: {activeZoneCount}");
#endif

        Debug.Log($"[SearchManager] Вызов zone.Activate(wordsPanel={wordsPanel?.name}, itemWordPrefab={itemWordPrefab?.name})");
        zone.Activate(wordsPanel, itemWordPrefab);
        Debug.Log($"[SearchManager] === ActivateZone END ===");
    }

    public void NotifyItemFound(HiddenItemData item)
    {
        OnItemFound?.Invoke(item);

        // Notify LevelProgressTracker to save progress and handle level completion
        var progressTracker = UnityEngine.Object.FindFirstObjectByType<LevelProgressTracker>();
        if (progressTracker != null && item != null)
        {
            progressTracker.RegisterItemFound(item.itemName);
        }
    }

    public void DeactivateCurrentZone()
    {
        if (activeZone != null)
        {
            // Call the special method to close from manager to avoid circular reference
            activeZone.CloseFromManager();

            activeZone = null;
            activeZoneCount--; // Decrement the active zone counter

            // Only deactivate the words panel if no zones are active
            if (activeZoneCount <= 0)
            {
                activeZoneCount = 0; // Ensure it doesn't go below 0
                if (wordsPanel != null && wordsPanel.activeSelf)
                {
                    wordsPanel.SetActive(false);
#if DEBUG
                    Debug.Log("[SearchManager] Deactivated words panel - no active zones");
#endif
                }
            }
            else
            {
#if DEBUG
                Debug.Log($"[SearchManager] Deactivated zone, still have {activeZoneCount} active zones");
#endif
            }
        }

    }

    /// <summary>
    /// Полный сброс состояния SearchManager
    /// </summary>
    public void ResetState()
    {
        DeactivateCurrentZone();
        activeZoneCount = 0;
    }
}
