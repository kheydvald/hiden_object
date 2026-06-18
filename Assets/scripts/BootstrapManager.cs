using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Менеджер загрузочной сцены (bootstrap.unity)
/// Инициализирует постоянные менеджеры и загружает главное меню
///
/// НАСТРОЙКА В ИНСПЕКТОРЕ:
/// 1. firstSceneName: "main_menu" (по умолчанию)
/// 2. persistentManagersPrefab: (оставьте пустым для автоматического создания)
///
/// ВАЖНО:
/// - Разместите Canvas, GameUI и StoryPanel на сцене bootstrap.unity
/// - Они будут сохранены через DontDestroyOnLoad
/// - Удалите Canvas, GameUI и StoryPanel из других сцен (main_menu, select_level, уровни)
/// </summary>
public class BootstrapManager : MonoBehaviour
{
    [Header("Scene Management")]
    [Tooltip("Имя сцены, которая загрузится после bootstrap (по умолчанию \"main_menu\")")]
    [SerializeField] private string firstSceneName = "main_menu";

    [Header("Persistent Managers")]
    [Tooltip("Опционально: префаб с менеджерами. Оставьте пустым для автоматического создания.")]
    [SerializeField] private GameObject persistentManagersPrefab;

    [Header("UI References")]
    [Tooltip("Canvas на сцене bootstrap. Найдётся автоматически, если не назначен.")]
    [SerializeField] private Canvas canvasInstance;

    [Tooltip("GameUI на сцене bootstrap. Найдётся автоматически, если не назначен.")]
    [SerializeField] private GameObject gameUIInstance;

    [Tooltip("StoryPanel на сцене bootstrap. Найдётся автоматически, если не назначен.")]
    [SerializeField] private GameObject storyPanelInstance;

    [Tooltip("ItemWordUI префаб. Назначьте из Assets/Prefab/UI/ItemWordUI.prefab")]
    [SerializeField] private GameObject itemWordUIPrefab;

    [Tooltip("LoadingScreen префаб. Назначьте из Assets/Prefab/UI/LoadingScreen.prefab")]
    [SerializeField] private GameObject loadingScreenPrefab;

    private static bool _managersCreated = false;

    private void Awake()
    {
        // Создаём постоянные менеджеры только один раз
        if (!_managersCreated)
        {
            CreatePersistentManagers();
            _managersCreated = true;
        }
        else
        {
            // Если менеджеры уже созданы, уничтожаем дубликат BootstrapManager
            Destroy(gameObject);
            return;
        }

        // Не уничтожаем этот объект при загрузке сцен
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Создаёт постоянные менеджеры для игры
    /// </summary>
    private void CreatePersistentManagers()
    {
        // Если назначен префаб - используем его
        if (persistentManagersPrefab != null)
        {
            GameObject managersInstance = Instantiate(persistentManagersPrefab, Vector3.zero, Quaternion.identity);
            DontDestroyOnLoad(managersInstance);
            Debug.Log("[BootstrapManager] Persistent managers instantiated from prefab: " + persistentManagersPrefab.name);
            // Загружаем сцену сразу
            LoadFirstScene();
            return;
        }

        // Создаём объект для постоянных менеджеров
        GameObject managersObject = new GameObject("PersistentManagers");

        // === 1. Создаём менеджеров ===
        
        // Создаём GameManager
        GameObject gameManagerObject = new GameObject("GameManager");
        gameManagerObject.transform.SetParent(managersObject.transform);
        GameManager gameManager = gameManagerObject.AddComponent<GameManager>();

        // Создаём SearchManager
        GameObject searchManagerObject = new GameObject("SearchManager");
        searchManagerObject.transform.SetParent(managersObject.transform);
        SearchManager searchManager = searchManagerObject.AddComponent<SearchManager>();

        // Создаём SoundManager
        GameObject soundManagerObject = new GameObject("SoundManager");
        soundManagerObject.transform.SetParent(managersObject.transform);
        soundManagerObject.AddComponent<SoundManager>();

        // Создаём LevelProgressManager
        GameObject progressManagerObject = new GameObject("LevelProgressManager");
        progressManagerObject.transform.SetParent(managersObject.transform);
        progressManagerObject.AddComponent<LevelProgressManager>();

        // Создаём InterstitialAdManager
        GameObject adManagerObject = new GameObject("InterstitialAdManager");
        adManagerObject.transform.SetParent(managersObject.transform);
        adManagerObject.AddComponent<InterstitialAdManager>();

        // Создаём ResetProgressManager
        GameObject resetManagerObject = new GameObject("ResetProgressManager");
        resetManagerObject.transform.SetParent(managersObject.transform);
        resetManagerObject.AddComponent<ResetProgressManager>();

        // Создаём LoadingScreenManager
        GameObject loadingScreenManagerObject = new GameObject("LoadingScreenManager");
        loadingScreenManagerObject.transform.SetParent(managersObject.transform);
        LoadingScreenManager loadingScreenManager = loadingScreenManagerObject.AddComponent<LoadingScreenManager>();
        
        // Назначаем префаб LoadingScreen, если он указан
        if (loadingScreenPrefab != null)
        {
            loadingScreenManager.loadingScreenObject = loadingScreenPrefab; // Присваиваем префаб в новое поле объекта
            Debug.Log($"[BootstrapManager] LoadingScreenPrefab назначен: {loadingScreenPrefab.name}");
        }

        // Создаём DialogManager (для диалогов на уровнях)
        GameObject dialogManagerObject = new GameObject("DialogManager");
        dialogManagerObject.transform.SetParent(managersObject.transform);
        DialogManager dialogManager = dialogManagerObject.AddComponent<DialogManager>();
        
        // Сразу настраиваем DialogManager!
        if (storyPanelInstance != null)
        {
            var storyDialog = storyPanelInstance.GetComponentInChildren<StoryDialog>(true);
            if (storyDialog != null)
            {
                dialogManager.storyDialog = storyDialog;
            }
            dialogManager.SetStoryPanel(storyPanelInstance);
        }
        dialogManager.gameUI = gameUIInstance;
        
        var menuPanelObj = GameObject.Find("Panel_MainMenu");
        if (menuPanelObj != null)
        {
            dialogManager.menuPanel = menuPanelObj;
        }
        
        var levelSelectPanelObj = GameObject.Find("Panel_LevelSelect");
        if (levelSelectPanelObj != null)
        {
            dialogManager.levelSelectPanel = levelSelectPanelObj;
        }
        
        Debug.Log("[BootstrapManager] DialogManager создан и настроен");
        
        // Скрываем StoryPanel сразу - он будет показан только при загрузке уровня
        if (storyPanelInstance != null)
        {
            storyPanelInstance.SetActive(false);
            Debug.Log("[BootstrapManager] StoryPanel скрыт (будет показан при загрузке уровня)");
        }

        // Применяем DontDestroyOnLoad к корневому объекту менеджеров
        DontDestroyOnLoad(managersObject);

        // === 2. Настраиваем UI ===
        
        SetupUI(gameManager, searchManager);

        // === 3. Загружаем первую сцену ===
        
        LoadFirstScene();

        Debug.Log("[BootstrapManager] Persistent managers and UI setup completed");
    }

    /// <summary>
    /// Настраивает UI (находит на сцене и назначает в менеджеры)
    /// </summary>
    private void SetupUI(GameManager gameManager, SearchManager searchManager)
    {
        // DialogManager уже настроен в CreatePersistentManagers()
        // Проверяем, что он существует
        var dialogManager = FindFirstObjectByType<DialogManager>();
        if (dialogManager == null)
        {
            Debug.LogError("[BootstrapManager] DialogManager не найден! Проверьте CreatePersistentManagers.");
            return;
        }
        
        Debug.Log("[BootstrapManager] DialogManager найден, проверки настроек...");
        Debug.Log($"[BootstrapManager] storyDialog: {(dialogManager.storyDialog != null ? dialogManager.storyDialog.name : "null")}");
        Debug.Log($"[BootstrapManager] gameUI: {(dialogManager.gameUI != null ? dialogManager.gameUI.name : "null")}");

        // Находим Canvas на сцене
        if (canvasInstance == null)
        {
            canvasInstance = FindFirstObjectByType<Canvas>();
        }

        if (canvasInstance == null)
        {
            Debug.LogWarning("[BootstrapManager] Canvas не найден на сцене! Создаём автоматически.");
            CreateDefaultCanvas();
        }
        else
        {
            Debug.Log($"[BootstrapManager] Canvas найден: {canvasInstance.name}");
            DontDestroyOnLoad(canvasInstance.gameObject);

            // Устанавливаем высокий Sorting Order, чтобы UI был поверх всех сцен
            var canvasComponent = canvasInstance.GetComponent<Canvas>();
            if (canvasComponent != null)
            {
                canvasComponent.sortingOrder = 1000; // Высокий приоритет
                Debug.Log($"[BootstrapManager] Установлен Sorting Order: 1000");
            }
            
            // Убеждаемся, что Canvas активен
            canvasInstance.gameObject.SetActive(true);
            Debug.Log("[BootstrapManager] Canvas активирован");
        }

        // Находим GameUI на сцене
        if (gameUIInstance == null)
        {
            gameUIInstance = GameObject.Find("GameUI");
        }

        if (gameUIInstance == null)
        {
            Debug.LogError("[BootstrapManager] GameUI не найден на сцене! Разместите GameUI на сцене bootstrap.unity");
        }
        else
        {
            // Назначаем Canvas родителем, если ещё не назначен
            if (gameUIInstance.transform.parent == null)
            {
                gameUIInstance.transform.SetParent(canvasInstance.transform, false);
            }

            // Выводим GameUI на передний план
            gameUIInstance.transform.SetAsLastSibling();

            // 🔥 Применяем DontDestroyOnLoad к Canvas, а GameUI останется внутри него
            DontDestroyOnLoad(canvasInstance.gameObject);

            gameManager.SetGameUIInstance(gameUIInstance);
            Debug.Log($"[BootstrapManager] GameUI найден и назначен: {gameUIInstance.name}");
        }

        // Настраиваем ItemWordUI
        if (itemWordUIPrefab == null)
        {
            Debug.LogWarning("[BootstrapManager] itemWordUIPrefab не назначен! SearchManager не будет работать.");
        }
        else
        {
            searchManager.SetItemWordPrefab(itemWordUIPrefab);
            Debug.Log($"[BootstrapManager] ItemWordUI назначен: {itemWordUIPrefab.name}");
        }

        // Настраиваем SearchManager (WordsPanel)
        if (searchManager != null && gameUIInstance != null)
        {
            // Находим WordsPanel внутри GameUI
            Transform wordsPanelTransform = FindWordsPanelRecursive(gameUIInstance.transform);
            if (wordsPanelTransform != null)
            {
                searchManager.SetWordsPanel(wordsPanelTransform.gameObject);
                searchManager.SetGameUI(gameUIInstance); // Назначаем GameUI для включения
                Debug.Log($"[BootstrapManager] WordsPanel назначен в SearchManager: {wordsPanelTransform.name}");
            }
            else
            {
                Debug.LogWarning("[BootstrapManager] WordsPanel не найден в GameUI!");
            }
        }

        // Настраиваем StoryPanel (диалоги)
        if (storyPanelInstance == null)
        {
            storyPanelInstance = GameObject.Find("StoryPanel");
        }

        if (storyPanelInstance != null)
        {
            // Назначаем Canvas родителем, если ещё не назначен
            if (storyPanelInstance.transform.parent == null && canvasInstance != null)
            {
                storyPanelInstance.transform.SetParent(canvasInstance.transform, false);
            }

            // 🔥 Canvas уже сохранён через DontDestroyOnLoad, StoryPanel останется внутри
            Debug.Log($"[BootstrapManager] StoryPanel найден и сохранён: {storyPanelInstance.name}");
        }
        else
        {
            Debug.LogWarning("[BootstrapManager] StoryPanel не найден! Диалоги не будут работать.");
        }
    }

    /// <summary>
    /// Создаёт Canvas по умолчанию (если не найден на сцене)
    /// </summary>
    private void CreateDefaultCanvas()
    {
        GameObject canvasObject = new GameObject("Canvas");
        canvasInstance = canvasObject.AddComponent<Canvas>();
        canvasInstance.renderMode = RenderMode.ScreenSpaceOverlay;
        
        canvasObject.AddComponent<UnityEngine.UI.CanvasScaler>();
        canvasObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        
        // Создаём EventSystem
        GameObject eventSystem = new GameObject("EventSystem");
        eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
        eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        
        DontDestroyOnLoad(canvasObject);
        DontDestroyOnLoad(eventSystem);
        
        Debug.Log("[BootstrapManager] Canvas и EventSystem созданы автоматически");
    }

    /// <summary>
    /// Рекурсивный поиск WordsPanel в иерархии
    /// </summary>
    private Transform FindWordsPanelRecursive(Transform parent)
    {
        foreach (Transform child in parent)
        {
            // Ищем по имени
            if (child.name.Contains("WordsPanel") || child.name.Contains("Words"))
            {
                return child;
            }

            Transform found = FindWordsPanelRecursive(child);
            if (found != null) return found;
        }
        return null;
    }

    /// <summary>
    /// Загружает первую сцену (главное меню)
    /// </summary>
    private void LoadFirstScene()
    {
        Debug.Log("[BootstrapManager] ▶️ Загрузка первой сцены...");
        Debug.Log($"[BootstrapManager] 📋 firstSceneName: {firstSceneName}");
        
        if (!string.IsNullOrEmpty(firstSceneName))
        {
            Debug.Log($"[BootstrapManager] 📥 Загрузка сцены: {firstSceneName}");
            SceneManager.LoadScene(firstSceneName);
            Debug.Log($"[BootstrapManager] ✅ Сцена {firstSceneName} загружена");
        }
        else
        {
            Debug.LogError("[BootstrapManager] ❌ firstSceneName не настроен! Установите значение в инспекторе.");
        }
    }
}
