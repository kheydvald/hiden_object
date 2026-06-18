using UnityEngine;
using UnityEngine.UI;
using System;

public class SearchZone : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("true = зум-зона (по клику показываем зум-объект), false = предметы сразу на фоне")]
    public bool useZoom = true;
    
    [Tooltip("true = активировать автоматически при загрузке уровня (только для зон без зума!)")]
    public bool autoActivateOnStart = false;

    [Header("Zoom Zone Settings (если useZoom = true)")]
    [Tooltip("Дочерний объект с зум-контентом (должен быть на сцене, внутри этой зоны)")]
    public Transform zoomContentRoot;
    
    [Tooltip("Префаб для зума (legacy, не используется если zoomContentRoot назначен)")]
    public GameObject zoomZoneReference;
    public GameObject detailPrefab; // legacy

    [Header("Items")]
    [Tooltip("Список предметов. Если пусто — будут искаться SearchableItem на сцене")]
    public HiddenItemData[] items;

    [Header("UI")]
    [Tooltip("Опционально: если не задан — будет использоваться глобальный из SearchManager")]
    public GameObject wordsPanelOverride;
    public GameObject itemWordPrefabOverride;

    private GameObject currentDetailInstance;
    private bool isCompleted = false;
    private GameObject wordsPanel;
    private GameObject itemWordPrefab;
    private System.Action<HiddenItemData> onItemFoundCallback;
    public event Action OnZoneCompleted;

    // Flag to track if we're currently in zoom mode
    private bool isInZoomMode = false;

    // Список отключённых ZoneImage для последующего включения
    private System.Collections.Generic.List<GameObject> disabledZoneImages = new System.Collections.Generic.List<GameObject>();

    public void Activate(GameObject wordsPanel, GameObject itemWordPrefab)
    {
        if (isCompleted)
        {
#if UNITY_EDITOR || DEBUG
            Debug.Log($"[SearchZone] '{name}' уже завершён. Повторное открытие заблокировано.");
#endif
            return;
        }

        // Respect per-zone overrides if set in inspector
        this.wordsPanel = wordsPanelOverride != null ? wordsPanelOverride : wordsPanel;
        this.itemWordPrefab = itemWordPrefabOverride != null ? itemWordPrefabOverride : itemWordPrefab;

        if (useZoom)
        {
            OpenZoom();
        }
        else
        {
            OpenDirect();
        }
    }

    private void OpenZoom()
    {
        Debug.Log($"[SearchZone] OpenZoom вызван для '{name}'. zoomContentRoot = {(zoomContentRoot != null ? zoomContentRoot.name : "null")}");
        
        // Деактивируем триггерные зоны, чтобы они не блокировали клики по предметам
        DisableTriggers();
        
        // === ВАРИАНТ 1: zoomContentRoot назначен (объект на сцене) ===
        if (zoomContentRoot != null)
        {
            // Показываем зум-контент на месте (не перемещаем!)
            currentDetailInstance = zoomContentRoot.gameObject;
            currentDetailInstance.SetActive(true);

            // Настраиваем
            SetupDetailInstance(currentDetailInstance, false);
        }
        // === ВАРИАНТ 2: zoomZoneReference (префаб, legacy) ===
        else if (zoomZoneReference != null)
        {
            // Используем RectTransform от zoomContentRoot или текущий transform
            RectTransform parentTransform = zoomContentRoot != null ? zoomContentRoot as RectTransform : transform as RectTransform;
            if (parentTransform == null) parentTransform = GetComponent<RectTransform>();

            currentDetailInstance = Instantiate(zoomZoneReference, parentTransform);
            SetupDetailInstance(currentDetailInstance, false);
        }
        // === ВАРИАНТ 3: detailPrefab (legacy) ===
        else if (detailPrefab != null)
        {
            // Используем RectTransform от zoomContentRoot или текущий transform
            RectTransform parentTransform = zoomContentRoot != null ? zoomContentRoot as RectTransform : transform as RectTransform;
            if (parentTransform == null) parentTransform = GetComponent<RectTransform>();

            currentDetailInstance = Instantiate(detailPrefab, parentTransform);
            SetupDetailInstance(currentDetailInstance, false);
        }
        else
        {
            Debug.LogError($"SearchZone '{name}': useZoom=true, но zoomContentRoot, zoomZoneReference и detailPrefab не назначены!");
        }
    }

    private void OpenDirect()
    {
        Debug.Log($"[SearchZone] OpenDirect вызван для '{name}'. wordsPanel = {(wordsPanel != null ? wordsPanel.name : "null")}");
        
        // Просто показываем UI слов на основном экране
        if (wordsPanel == null)
        {
            Debug.LogError($"SearchZone '{name}': OpenDirect called but wordsPanel is null. Cannot show words.");
            return;
        }

        wordsPanel.SetActive(true);
        Debug.Log($"[SearchZone] wordsPanel активирован: {wordsPanel.name}");

        // For non-zoom mode, ensure scene SearchableItem children know their owner/callback
        var sceneItems = GetComponentsInChildren<SearchableItem>(true);
        int assigned = 0;
        foreach (var it in sceneItems)
        {
            if (it == null) continue;
            it.SetOwner(this, OnItemFoundInDirect);
            assigned++;
        }
        Debug.Log($"SearchZone '{name}': OpenDirect assigned {assigned} searchable items as owners.");

        UpdateWordList();
        Debug.Log($"SearchZone '{name}': Режим без зума. UI активирован для '{name}'");
    }


    private void SetupDetailInstance(GameObject instance, bool wasInstantiatedByAddressables)
    {
        // Set zoom mode flag
        isInZoomMode = true;

        // mark ownership on the instance for legacy and new controllers
        var marker = instance.GetComponentInChildren<DetailInstanceMarker>(true);
        if (marker == null) marker = instance.GetComponent<DetailInstanceMarker>();
        if (marker != null)
        {
            marker.ownerSearchZone = this;
            marker.ownerZone = GetComponent<ItemTriggerZone>();
            marker.createdByAddressables = wasInstantiatedByAddressables;
        }

        var closeBtn = instance.GetComponentInChildren<Button>(true);
        if (closeBtn != null)
        {
            closeBtn.onClick.AddListener(() =>
            {
                // 🔥 Просто закрываем зум — НЕ завершаем зону!
                if (SearchManager.Instance != null)
                {
                    SearchManager.Instance.DeactivateCurrentZone();
                }
            });

            // Добавляем компонент автозакрытия зума
            var autoCloser = instance.GetComponent<ZoomAutoCloser>() ?? instance.AddComponent<ZoomAutoCloser>();
            autoCloser.SetSearchZone(this);
            autoCloser.closeButton = closeBtn;
        }

        // Check which items should be hidden due to already being found
        var progressTracker = UnityEngine.Object.FindFirstObjectByType<LevelProgressTracker>();

        // Передаём ссылку на эту зону, чтобы предметы могли вызывать OnItemFound
        var itemsInZoom = instance.GetComponentsInChildren<SearchableItem>();
        foreach (var item in itemsInZoom)
        {
            if (item != null && item.GetItemData() != null && progressTracker != null)
            {
                string itemName = item.GetItemData().itemName;
                if (progressTracker.IsItemFound(itemName))
                {
                    // Mark this item as already found (hide it)
                    item.MarkAsFoundInScene();
#if UNITY_EDITOR || DEBUG
                    Debug.Log($"SearchZone '{name}': Hiding already found item '{itemName}' in zoom instance.");
#endif
                }
            }
            item.SetOwner(this, OnItemFoundInZoom);
        }

        UpdateWordList();
    }

    // Helpers for HintButtonController and legacy callers
    public bool HasUnfoundItems()
    {
        // If we're in zoom mode, check items in the current detail instance
        if (isInZoomMode && currentDetailInstance != null)
        {
            var itemsInZoom = currentDetailInstance.GetComponentsInChildren<SearchableItem>(true);
            foreach (var it in itemsInZoom)
            {
                if (it.gameObject.activeSelf) return true;
            }
        }
        else
        {
            // Otherwise, check items in the original zone
            var itemsInScene = GetComponentsInChildren<SearchableItem>(true);
            foreach (var it in itemsInScene)
            {
                if (it.gameObject.activeSelf) return true;
            }
        }
        return false;
    }

    public SearchableItem GetRandomUnfoundSearchableItem()
    {
        System.Collections.Generic.List<SearchableItem> list = new System.Collections.Generic.List<SearchableItem>();

        // If we're in zoom mode, get items from the current detail instance
        if (isInZoomMode && currentDetailInstance != null)
        {
            var itemsInZoom = currentDetailInstance.GetComponentsInChildren<SearchableItem>(true);
            foreach (var it in itemsInZoom)
            {
                if (it.gameObject.activeSelf)
                    list.Add(it);
            }
        }
        else
        {
            // Otherwise, get items from the original zone
            var itemsInScene = GetComponentsInChildren<SearchableItem>(true);
            foreach (var it in itemsInScene)
            {
                if (it.gameObject.activeSelf)
                    list.Add(it);
            }
        }

        if (list.Count == 0) return null;
        return list[UnityEngine.Random.Range(0, list.Count)];
    }

    public int GetTotalItemsCount()
    {
        return items != null ? items.Length : 0;
    }

    private void UpdateWordList()
    {
        // Очистка и заполнение UI слов
        if (wordsPanel == null)
        {
#if UNITY_EDITOR || DEBUG
            Debug.LogError($"[SearchZone '{name}'] UpdateWordList called but wordsPanel is null. Check SearchManager.SetWordsPanel() was called or wordsPanel assigned in inspector.");
#endif
            return;
        }

        if (!wordsPanel.scene.IsValid())
        {
#if UNITY_EDITOR || DEBUG
            Debug.LogWarning($"SearchZone '{name}': UpdateWordList: wordsPanel '{wordsPanel.name}' is not a scene instance. Clearing reference to avoid MissingReferenceException.");
#endif
            wordsPanel = null;
            return;
        }

        // Ищем Content внутри WordsPanel, если нет — используем сам WordsPanel
        Transform content = wordsPanel.transform.Find("Content");
        if (content == null)
        {
            // Content не найден, используем сам WordsPanel как контейнер
            content = wordsPanel.transform;
#if UNITY_EDITOR || DEBUG
            Debug.Log($"SearchZone '{name}': Content не найден, используем WordsPanel как контейнер.");
#endif
        }

        // remove existing children (scene instances only)
        foreach (Transform child in content)
        {
            if (child.gameObject != null && child.gameObject.scene.IsValid()) Destroy(child.gameObject);
        }

        // Determine source of item data: explicit items[] or scene SearchableItem children
        HiddenItemData[] sourceItems = null;
        if (items != null && items.Length > 0)
        {
            sourceItems = items;
        }
        else
        {
            var sceneSearchables = GetComponentsInChildren<SearchableItem>(true);
            if (sceneSearchables != null && sceneSearchables.Length > 0)
            {
                var list = new System.Collections.Generic.List<HiddenItemData>();
                foreach (var s in sceneSearchables)
                {
                    var d = s.GetItemData();
                    if (d != null) list.Add(d);
                }
                sourceItems = list.ToArray();
            }
        }

        if (sourceItems == null || sourceItems.Length == 0)
        {
#if UNITY_EDITOR || DEBUG
            Debug.Log($"SearchZone '{name}': UpdateWordList: no items to display (items array empty and no SearchableItem children with data).\nCheck that HiddenItemData[] or SearchableItem are set up.");
#endif
            return;
        }

        if (itemWordPrefab == null)
        {
#if UNITY_EDITOR || DEBUG
            Debug.LogError($"SearchZone '{name}': UpdateWordList: itemWordPrefab is null. Assign in SearchManager or override in this zone.");
#endif
            return;
        }

        // Get the LevelProgressTracker to check which items have already been found
        var progressTracker = UnityEngine.Object.FindFirstObjectByType<LevelProgressTracker>();

#if UNITY_EDITOR || DEBUG
        Debug.Log($"SearchZone '{name}': UpdateWordList creating {sourceItems.Length} word items.");
#endif
        for (int i = 0; i < sourceItems.Length; i++)
        {
            var item = sourceItems[i];
            if (item == null) continue;

            // Skip items that have already been found
            if (progressTracker != null && progressTracker.IsItemFound(item.itemName))
            {
#if UNITY_EDITOR || DEBUG
                Debug.Log($"SearchZone '{name}': Skipping already found item '{item.itemName}' from word list.");
#endif
                continue;
            }

            var go = Instantiate(itemWordPrefab, content);
            if (go == null)
            {
#if UNITY_EDITOR || DEBUG
                Debug.LogError($"SearchZone '{name}': Failed to instantiate itemWordPrefab for '{item.itemName}'.");
#endif
                continue;
            }
            var ui = go.GetComponent<ItemWordUI>();
            if (ui != null) ui.SetText(item.itemName);
#if UNITY_EDITOR || DEBUG
            else Debug.LogWarning($"SearchZone '{name}': Instantiated word prefab lacks ItemWordUI component.");
#endif
        }
    }

    public void OnItemFoundInZoom(HiddenItemData item)
    {
        // Register the item as found with the progress tracker
        var progressTracker = UnityEngine.Object.FindFirstObjectByType<LevelProgressTracker>();
        if (progressTracker != null && item != null)
        {
            progressTracker.RegisterItemFound(item.itemName);
        }

        // Обновить UI - удалить слово из списка
        UpdateWordList();

        // 🔥 НЕ вызываем MarkAsCompleted() здесь!
        // Зона завершится автоматически через LevelProgressTracker, когда все предметы на уровне найдены
        // Это предотвращает преждевременное завершение уровня
    }

    public void Close()
    {
        if (currentDetailInstance != null)
        {
            // Просто скрываем зум-контент (не перемещаем и не уничтожаем)
            currentDetailInstance.SetActive(false);
            currentDetailInstance = null;
        }
        // Reset zoom mode flag
        isInZoomMode = false;
        
        // Включаем триггерные зоны обратно
        EnableTriggers();
    }

    public void CloseFromManager()
    {
        if (currentDetailInstance != null)
        {
            // Просто скрываем зум-контент (не перемещаем и не уничтожаем)
            currentDetailInstance.SetActive(false);
            currentDetailInstance = null;
        }
        // Reset zoom mode flag
        isInZoomMode = false;
        
        // Включаем триггерные зоны обратно
        EnableTriggers();
    }

    private bool AllItemsFound()
    {
        var progressTracker = UnityEngine.Object.FindFirstObjectByType<LevelProgressTracker>();
        if (progressTracker == null) return false;

        // Получаем список предметов
        HiddenItemData[] sourceItems = GetSourceItems();
        if (sourceItems == null || sourceItems.Length == 0) return false;

        // Проверяем каждый
        foreach (var item in sourceItems)
        {
            if (item == null) continue;
            if (!progressTracker.IsItemFound(item.itemName))
            {
                return false;
            }
        }

        return true;
    }

    private HiddenItemData[] GetSourceItems()
    {
        if (items != null && items.Length > 0)
            return items;

        var sceneSearchables = GetComponentsInChildren<SearchableItem>(true);
        if (sceneSearchables != null && sceneSearchables.Length > 0)
        {
            var list = new System.Collections.Generic.List<HiddenItemData>();
            foreach (var s in sceneSearchables)
            {
                var d = s.GetItemData();
                if (d != null) list.Add(d);
            }
            return list.ToArray();
        }

        return null;
    }

    /// <summary>
    /// Проверяет, есть ли предметы для поиска в этой зоне
    /// </summary>
    public bool HasItemsToFind()
    {
        // Проверяем, есть ли определенные предметы в массиве items
        if (items != null && items.Length > 0)
        {
            return true;
        }

        // Иначе проверяем, есть ли SearchableItem в сцене
        var itemsInScene = GetComponentsInChildren<SearchableItem>(true);
        return itemsInScene != null && itemsInScene.Length > 0;
    }

    /// <summary>
    /// Проверяет, завершена ли зона (все предметы найдены)
    /// </summary>
    public bool IsZoneCompleted()
    {
        return isCompleted;
    }


    public void MarkAsCompleted()
    {
        if (isCompleted) return;

        isCompleted = true;
        Debug.Log($"[SearchZone] ��� �������� ������� � '{name}'. ��� ����� ������.");

        // ��������� ��������
        LevelProgressTracker.Instance?.SaveLevelStateForZone(this);

        // ���� �� � ���� � ���������
        if (currentDetailInstance != null)
        {
            Close();
        }

        // ���������� ��������
        SearchManager.Instance?.DeactivateCurrentZone();

        OnCompleted();
    }

    private void Start()
    {
        // Проверяем сохранённый прогресс
        if (LevelProgressTracker.Instance != null)
        {
            isCompleted = LevelProgressTracker.Instance.IsZoneCompleted(this);
        }

        // Если уже завершён — можно скрыть UI или подсветить
        if (isCompleted)
        {
            Debug.Log($"[SearchZone] '{name}' загружен как завершённый.");
        }
    }

    private void OnCompleted()
    {
        if (isCompleted) return;
        isCompleted = true;

        // Уведомляем подписчиков
        OnZoneCompleted?.Invoke();
    }

    public void OnItemFoundInDirect(HiddenItemData item)
    {
        // Register the item as found with the progress tracker
        var progressTracker = UnityEngine.Object.FindFirstObjectByType<LevelProgressTracker>();
        if (progressTracker != null && item != null)
        {
            progressTracker.RegisterItemFound(item.itemName);
        }

        OnItemFoundInZoom(item); // та же логика для обновления UI
    }

    public void Deactivate()
    {
        // Use the existing CloseFromManager method to ensure consistent cleanup
        CloseFromManager();
    }

    /// <summary>
    /// Деактивирует триггерные зоны (ZoneImage) по тегу, чтобы они не блокировали клики по предметам в зуме
    /// </summary>
    private void DisableTriggers()
    {
        // Очищаем предыдущий список
        disabledZoneImages.Clear();

        // Находим все GameObject с тегом "ZoneImage"
        var zoneImages = GameObject.FindGameObjectsWithTag("ZoneImage");
        foreach (var zoneImage in zoneImages)
        {
            if (zoneImage.activeSelf)
            {
                // Запоминаем объект и отключаем его
                disabledZoneImages.Add(zoneImage);
                zoneImage.SetActive(false);
#if DEBUG
                Debug.Log($"[SearchZone] '{name}': Деактивирован ZoneImage '{zoneImage.name}' по тегу");
#endif
            }
        }
        
#if DEBUG
        Debug.Log($"[SearchZone] '{name}': Отключено ZoneImage: {disabledZoneImages.Count}");
#endif
    }

    /// <summary>
    /// Включает обратно триггерные зоны после закрытия зума
    /// </summary>
    private void EnableTriggers()
    {
        // Включаем все запомненные ZoneImage
        foreach (var zoneImage in disabledZoneImages)
        {
            if (zoneImage != null && !zoneImage.activeSelf)
            {
                zoneImage.SetActive(true);
#if DEBUG
                Debug.Log($"[SearchZone] '{name}': Включён ZoneImage '{zoneImage.name}'");
#endif
            }
        }
        
        // Очищаем список
        disabledZoneImages.Clear();
        
#if DEBUG
        Debug.Log($"[SearchZone] '{name}': EnableTriggers вызван, включено ZoneImage: {disabledZoneImages.Count}");
#endif
    }
}
