using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using UnityEngine.AddressableAssets; // Нужно для Addressables.LoadSceneAsync
using UnityEngine.ResourceManagement.AsyncOperations; // Нужно для AsyncOperationStatus

/// <summary>
/// Главный менеджер игры. Сохраняется между сценами (DontDestroyOnLoad).
/// Управляет загрузкой уровней, навигацией между сценами и глобальным состоянием.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("UI Settings")]
    [SerializeField] private GameObject gameUI; // ← Просто ссылка на объект на сцене
    [SerializeField] private GameObject loadingScreen;

    [Header("UI Panels")]
    public GameObject panel_MainMenu;
    public GameObject panel_LevelSelect;

    [Header("Scene Management")]
    [Tooltip("Имя сцены главного меню")]
    [SerializeField] private string mainMenuSceneName = "main_menu";
    [Tooltip("Имя сцены выбора уровней")]
    [SerializeField] private string levelSelectSceneName = "select_level";

    private bool musicOn = true;
    private bool sfxOn = true;

    /// <summary>
    /// Устанавливает экземпляр GameUI (вызывается из BootstrapManager после загрузки)
    /// </summary>
    public void SetGameUIInstance(GameObject uiInstance)
    {
        gameUI = uiInstance;
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad вызывается в BootstrapManager для корневого объекта PersistentManagers
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        LoadAudioSettings();
    }

    private void Start()
    {
        FindPanelsIfNeeded();
        ShowMainMenu();

        // GameUI уже загружен и назначен в BootstrapManager
        if (gameUI != null)
        {
#if DEBUG
            Debug.Log($"[GameManager] Start: GameUI назначен: {gameUI.name}");
#endif

            // Скрываем UI до загрузки уровня
            gameUI.SetActive(false);
        }
        else
        {
#if DEBUG
            Debug.LogWarning("[GameManager] Start: GameUI не загружен. Проверьте настройки BootstrapManager.");
#endif
        }
    }

    /// <summary>
    /// Загружает уровень по LevelConfig через Addressables.
    /// Каждая сцена — Addressable объект.
    /// </summary>
    public void LoadLevel(LevelConfig levelConfig)
    {
        if (levelConfig == null)
        {
            Debug.LogError("[GameManager] ❌ LevelConfig is null!");
            return;
        }

        Debug.Log($"[GameManager] ▶️ Загружаем сцену уровня: '{levelConfig.LevelName}'");
        Debug.Log($"[GameManager] 📋 sceneReference.AssetGUID: {levelConfig.sceneReference?.AssetGUID ?? "NULL"}");
        Debug.Log($"[GameManager] 📋 IsAddressableScene: {levelConfig.IsAddressableScene}");
        Debug.Log($"[GameManager] 📋 LevelPrefab: {(levelConfig.LevelPrefab != null ? levelConfig.LevelPrefab.name : "NULL (сцена)")}");

        ShowLoadingScreen(true);

        // Скрываем ВСЕ UI панели перед загрузкой уровня
        HideMenuPanels();

        // Скрываем GameUI и StoryPanel
        if (gameUI != null)
            gameUI.SetActive(false);

        var dialogManager = DialogManager.Instance;
        if (dialogManager != null)
        {
            dialogManager.HideStoryPanel();
        }

        // Выгружаем предыдущий уровень
        UnloadCurrentLevel();

        // Загружаем сцену ТОЛЬКО через Addressables (для удалённых сцен)
        if (levelConfig.sceneReference != null && !string.IsNullOrEmpty(levelConfig.sceneReference.AssetGUID))
        {
            Debug.Log($"[GameManager] 🌐 Загрузка через Addressables (сцена на облаке)");
            LoadLevelFromAddressables(levelConfig);
        }
        else
        {
            Debug.LogError($"[GameManager] ❌ sceneReference не настроен для уровня '{levelConfig.LevelName}'! " +
                          $"Назначьте сцену в Addressables и создайте AssetReference в LevelConfig.");
            ShowLoadingScreen(false);
        }
    }

    /// <summary>
    /// Скрывает панели меню и выбора уровней
    /// </summary>
    private void HideMenuPanels()
    {
        if (panel_MainMenu != null)
            panel_MainMenu.SetActive(false);

        if (panel_LevelSelect != null)
            panel_LevelSelect.SetActive(false);
    }

    /// <summary>
    /// Загружает уровень через Addressables
    /// </summary>
       public async void LoadLevelFromAddressables(LevelConfig config)
    {
        // 0. Проверка режима загрузки
        if (!config.IsAddressableScene)
        {
            Debug.LogError($"[GameManager] ❌ Конфигурация '{config.LevelName}' не является Addressable сценой! Проверь поле sceneReference.");
            // Можно добавить фоллбэк на префаб, если нужно, но пока просто выходим
            return;
        }

        // 1. Получаем менеджер загрузки
        LoadingScreenManager loadingManager = FindFirstObjectByType<LoadingScreenManager>();

        if (loadingManager == null)
        {
            Debug.LogError("[GameManager] ❌ LoadingScreenManager не найден в сцене!");
        }
        else
        {
            // 2. ПОКАЗЫВАЕМ экран загрузки
            loadingManager.ShowLoadingScreen();
            Debug.Log("[GameManager] 🟠 LoadingScreen показан");
        }

        Debug.Log($"[GameManager] 📥 Начало загрузки сцены: {config.LevelName} (Address: {config.sceneReference.AssetGUID})");

        try
        {
            // 3. Загружаем сцену через AssetReference
            // Используем config.sceneReference.LoadSceneAsync напрямую
            var handle = config.sceneReference.LoadSceneAsync(LoadSceneMode.Additive);

            // Ждем полного завершения загрузки
            await handle.Task;

            if (handle.Status == AsyncOperationStatus.Succeeded) // 4. Теперь статус работает
            {
                Debug.Log($"[GameManager] ✅ Сцена '{config.LevelName}' успешно загружена!");

                // 5. Активируем новую сцену как основную
                SceneManager.SetActiveScene(handle.Result.Scene);

                // Опционально: выгрузить старую сцену (меню), если она не нужна
                // Но пока оставим, чтобы не ломать переходы назад
                // SceneManager.UnloadSceneAsync("main_menu"); 

                // 6. СКРЫВАЕМ экран загрузки
                if (loadingManager != null)
                {
                    loadingManager.HideLoadingScreen();
                    Debug.Log("[GameManager] 🟢 LoadingScreen скрыт");
                }

                // Инициализируем LevelManager в новой сцене
                LevelManager levelManager = FindFirstObjectByType<LevelManager>();
                if (levelManager != null)
                {
                    Debug.Log($"[GameManager] 🔍 LevelManager найден: {levelManager.name}");
                    // levelManager.Initialize(); 
                }
                else
                {
                    Debug.LogWarning("[GameManager] ️ LevelManager не найден в загруженной сцене!");
                }
            }
            else
            {
                Debug.LogError($"[GameManager] ❌ Ошибка загрузки сцены. Статус: {handle.Status}");
                if (loadingManager != null) loadingManager.HideLoadingScreen();
            }
            
            handle.Release(); 
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[GameManager] 💥 Критическая ошибка при загрузке: {e.Message}\n{e.StackTrace}");
            if (loadingManager != null) loadingManager.HideLoadingScreen();
        }
    }


    public void UnloadCurrentLevel()
    {
        // При загрузке сцен просто скрываем UI
        if (gameUI != null)
        {
            gameUI.SetActive(false);
        }

        // Также убеждаемся, что SearchManager деактивирует текущую зону
        if (SearchManager.Instance != null)
        {
            SearchManager.Instance.DeactivateCurrentZone();
        }
    }

    private void OnLevelCompleted()
    {
#if DEBUG
        Debug.Log("Уровень завершён! Вызван OnLevelCompleted");
#endif

        // Ищем LevelManager на текущей сцене
        var levelManager = FindFirstObjectByType<LevelManager>();
        if (levelManager != null)
        {
#if DEBUG
            Debug.Log("levelManager найден, отписываюсь от события");
#endif
            levelManager.OnLevelCompleted -= OnLevelCompleted;

            // Проверяем, есть ли OutroDialog
            string outroText = levelManager.currentLevelConfig?.OutroDialog;
#if DEBUG
            Debug.Log($"OutroDialog: '{outroText}', IsNullOrEmpty: {string.IsNullOrEmpty(outroText)}");
#endif

            // Показываем аутро-диалог при завершении уровня
            if (!string.IsNullOrEmpty(outroText))
            {
#if DEBUG
                Debug.Log("Показываем ShowOutroDialog");
#endif
                ShowOutroDialog(levelManager.currentLevelConfig);
            }
            else
            {
#if DEBUG
                Debug.Log("OutroDialog пустой, вызываем UnloadCurrentLevel и ShowLevelSelect");
#endif
                // Если нет аутро-диалога, сразу переходим к выбору уровня
                UnloadCurrentLevel();
                ShowLevelSelect();
            }
        }
        else
        {
#if DEBUG
            Debug.Log("levelManager не найден, вызываем UnloadCurrentLevel и ShowLevelSelect");
#endif
            UnloadCurrentLevel();
            ShowLevelSelect();
        }
    }

    private void ShowLoadingScreen(bool show)
    {
        if (show)
        {
            LoadingScreenManager.Instance?.ShowLoadingScreen();
        }
        else
        {
            LoadingScreenManager.Instance?.HideLoadingScreen();
        }
    }

    private void ShowIntroDialog(LevelConfig levelConfig)
    {
        // Используем статический Instance для доступа к DialogManager
        if (DialogManager.Instance != null)
        {
#if DEBUG
            Debug.Log($"[GameManager] DialogManager найден, показываем IntroDialog: '{levelConfig.IntroDialog}'");
#endif
            DialogManager.Instance.ShowDialog(levelConfig.IntroDialog);
        }
        else
        {
#if DEBUG
            Debug.LogWarning("[GameManager] DialogManager.Instance = null! Проверьте, что DialogManager есть на сцене bootstrap.");
#endif
        }
    }

    private void ShowOutroDialog(LevelConfig levelConfig)
    {
        // Используем статический Instance для доступа к DialogManager
        if (DialogManager.Instance != null)
        {
#if DEBUG
            Debug.Log("GameManager: DialogManager found, proceeding with outro dialog.");
#endif

            // Check if outro dialog is not empty
            if (string.IsNullOrEmpty(levelConfig?.OutroDialog))
            {
#if DEBUG
                Debug.LogWarning("GameManager: OutroDialog is null or empty. Proceeding to unload level.");
#endif
                // If no outro dialog, just unload the level
                UnloadCurrentLevel();
                ShowLevelSelect();
                return;
            }

#if DEBUG
            Debug.Log($"GameManager: Showing outro dialog with text: '{levelConfig.OutroDialog}'");
#endif

            // Subscribe to dialog finished event to unload level after dialog
            // restoreGameUIAfter = false, т.к. после OutroDialog переходим на select_level
            DialogManager.Instance.OnDialogClosed += OnOutroDialogClosed;
            DialogManager.Instance.ShowDialog(levelConfig.OutroDialog, restoreGameUIAfter: false);
        }
        else
        {
#if DEBUG
            Debug.LogWarning("DialogManager not found in scene!");
#endif
            // If no dialog manager, just unload the level
            UnloadCurrentLevel();
            ShowLevelSelect();
        }
    }

    private void OnOutroDialogClosed()
    {
#if DEBUG
        Debug.Log("[GameManager] OnOutroDialogClosed: START");
#endif

        if (DialogManager.Instance != null)
        {
            DialogManager.Instance.OnDialogClosed -= OnOutroDialogClosed;
#if DEBUG
            Debug.Log("[GameManager] OnOutroDialogClosed: Отписка от события DialogManager");
#endif
        }

        // Отмечаем уровень как пройденный
        var levelManager = FindFirstObjectByType<LevelManager>();
        if (levelManager != null && levelManager.currentLevelConfig != null && LevelProgressManager.Instance != null)
        {
            LevelProgressManager.Instance.MarkLevelCompleted(levelManager.currentLevelConfig.LevelName);
#if DEBUG
            Debug.Log($"[GameManager] OnOutroDialogClosed: Уровень {levelManager.currentLevelConfig.LevelName} отмечен как пройденный");
#endif
        }

#if DEBUG
        Debug.Log("[GameManager] OnOutroDialogClosed: Вызываем UnloadCurrentLevel()");
#endif
        UnloadCurrentLevel();

#if DEBUG
        Debug.Log("[GameManager] OnOutroDialogClosed: Вызываем ShowLevelSelect()");
#endif
        ShowLevelSelect();
    }

    private void FindPanelsIfNeeded()
    {
        if (panel_MainMenu == null)
        {
            GameObject found = GameObject.Find("Panel_MainMenu");
            if (found == null) found = GameObject.Find("panel_MainMenu");
            if (found != null) panel_MainMenu = found;
        }

        if (panel_LevelSelect == null)
        {
            GameObject found = GameObject.Find("Panel_LevelSelect");
            if (found == null) found = GameObject.Find("panel_LevelSelect");
            if (found != null) panel_LevelSelect = found;
        }
    }

    [Header("Menu Panels")]
    [SerializeField] private GameObject currentMenuPanel; // Панель текущего меню (например, Pause Menu)

    /// <summary>
    /// Загружает сцену главного меню
    /// </summary>
    public void ShowMainMenu()
    {
#if DEBUG
        Debug.Log("[GameManager] ShowMainMenu: Loading main menu scene");
#endif

        // Сначала выгружаем текущий уровень
        UnloadCurrentLevel();

        // Скрываем GameUI (он будет показан только на уровне)
        if (gameUI != null)
            gameUI.SetActive(false);

        // Загружаем сцену главного меню
        if (!string.IsNullOrEmpty(mainMenuSceneName))
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }
        else
        {
            Debug.LogError("[GameManager] mainMenuSceneName not set!");
        }
    }

    /// <summary>
    /// Загружает сцену выбора уровней
    /// </summary>
    public void ShowLevelSelect()
    {
#if DEBUG
        Debug.Log("[GameManager] ShowLevelSelect: Loading level select scene");
#endif

        // Сначала выгружаем текущий уровень
        UnloadCurrentLevel();

        // Скрываем GameUI (он будет показан только на уровне)
        if (gameUI != null)
            gameUI.SetActive(false);

        // Загружаем сцену выбора уровней
        if (!string.IsNullOrEmpty(levelSelectSceneName))
        {
            SceneManager.LoadScene(levelSelectSceneName);
        }
        else
        {
            Debug.LogError("[GameManager] levelSelectSceneName not set!");
        }
    }

    /// <summary>
    /// Загружает указанную сцену по имени
    /// </summary>
    public void LoadScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("[GameManager] LoadScene: sceneName is null or empty!");
            return;
        }

#if DEBUG
        Debug.Log($"[GameManager] LoadScene: {sceneName}");
#endif

        UnloadCurrentLevel();
        SceneManager.LoadScene(sceneName);
    }

    public void ToggleMusic(bool isOn)
    {
        musicOn = isOn;
        SaveAudioSettings();
#if DEBUG
        Debug.Log("Музыка: " + (musicOn ? "вкл" : "выкл"));
#endif
    }

    public void ToggleSFX(bool isOn)
    {
        sfxOn = isOn;
        SaveAudioSettings();
#if DEBUG
        Debug.Log("Звуки: " + (sfxOn ? "вкл" : "выкл"));
#endif
    }

    private void SaveAudioSettings()
    {
        PlayerPrefs.SetInt("MusicOn", musicOn ? 1 : 0);
        PlayerPrefs.SetInt("SFXOn", sfxOn ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void LoadAudioSettings()
    {
        musicOn = PlayerPrefs.GetInt("MusicOn", 1) == 1;
        sfxOn = PlayerPrefs.GetInt("SFXOn", 1) == 1;
    }

    private void OnDestroy()
    {
        // Отписываемся от события при уничтожении
        var levelManager = FindFirstObjectByType<LevelManager>();
        if (levelManager != null)
        {
            levelManager.OnLevelCompleted -= OnLevelCompleted;
        }
    }
}

/// <summary>
/// Alternative implementation of UIZoneTrigger that finds ItemTriggerZone in children at runtime
/// This solves the issue where UI zone triggers on background prefabs can't reference scene objects
/// </summary>
public class RuntimeUIZoneTrigger : UnityEngine.MonoBehaviour, UnityEngine.EventSystems.IPointerClickHandler
{
    [UnityEngine.Tooltip("Optional: assign ItemTriggerZone manually. If null, will search in children at runtime.")]
    public ItemTriggerZone targetZone;

    public void OnPointerClick(UnityEngine.EventSystems.PointerEventData eventData)
    {
        // If targetZone wasn't assigned in inspector, try to find it in children
        if (targetZone == null)
        {
            targetZone = GetComponentInChildren<ItemTriggerZone>();

            if (targetZone == null)
            {
                // If not found in children, try to find in siblings or parent
                targetZone = GetComponentInParent<ItemTriggerZone>();

                if (targetZone == null)
                {
#if DEBUG
                    Debug.LogError($"RuntimeUIZoneTrigger: ItemTriggerZone not found for '{name}'. Make sure it's in the hierarchy.");
#endif
                    return;
                }
            }
        }

        if (targetZone != null)
        {
            // Проверяем, есть ли SearchZone в той же иерархии, чтобы выполнить проверки
            SearchZone searchZone = targetZone.GetComponent<SearchZone>() ?? targetZone.GetComponentInParent<SearchZone>() ?? targetZone.GetComponentInChildren<SearchZone>();

            if (searchZone != null)
            {
                // Проверяем, завершена ли зона - если да, не активируем её
                if (searchZone.IsZoneCompleted())
                {
#if DEBUG
                    Debug.Log($"RuntimeUIZoneTrigger: Zone '{searchZone.name}' is already completed. Skipping activation.");
#endif
                    return;
                }

                // Проверяем, есть ли предметы для поиска в зоне - если нет, не активируем
                if (!searchZone.HasItemsToFind())
                {
#if DEBUG
                    Debug.Log($"RuntimeUIZoneTrigger: Zone '{searchZone.name}' has no items to find. Skipping activation.");
#endif
                    return;
                }
            }

            targetZone.ShowDetail();
        }
    }
}
