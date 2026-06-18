using UnityEngine;
using System;

public class DialogManager : MonoBehaviour
{
    public static DialogManager Instance { get; private set; }

    public StoryDialog storyDialog;
    private GameObject storyPanel; // Сохраняем ссылку на StoryPanel

    [Header("UI Panels to Hide During Dialog")]
    [Tooltip("GameUI (панель поиска слов) - скрывается во время диалога")]
    public GameObject gameUI;

    [Tooltip("MenuPanel - скрывается во время диалога")]
    public GameObject menuPanel;

    [Tooltip("LevelSelectPanel - скрывается во время диалога")]
    public GameObject levelSelectPanel;

    // Сохраняем состояние UI до показа диалога
    private bool wasGameUIActive = false;
    private bool wasMenuPanelActive = false;
    private bool wasLevelSelectPanelActive = false;
    private bool isDialogShowing = false;
    private bool shouldRestoreGameUI = false;  // Флаг: нужно ли восстанавливать GameUI после диалога

    /// <summary>
    /// Событие вызывается когда диалог полностью закрыт (все строки прочитаны)
    /// </summary>
    public event Action OnDialogClosed;

    private void Awake()
    {
        // Singleton pattern для доступа из GameManager
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

        // Сохраняем ссылку на StoryPanel (родитель StoryDialog)
        // Если storyPanel ещё не назначен, пробуем найти его через transform.parent
        if (storyPanel == null && storyDialog != null && storyDialog.transform.parent != null)
        {
            // Проверяем, что это не Canvas!
            if (storyDialog.transform.parent.GetComponent<Canvas>() == null)
            {
                storyPanel = storyDialog.transform.parent.gameObject;
            }
        }

        // Подписываемся на событие закрытия диалога
        if (storyDialog != null)
        {
            storyDialog.OnDialogFinished += HandleDialogFinished;
        }
    }
    
    /// <summary>
    /// Устанавливает ссылку на StoryPanel (вызывается из BootstrapManager)
    /// </summary>
    public void SetStoryPanel(GameObject panel)
    {
        storyPanel = panel;
        
        // Находим StoryDialog и подписываемся на событие
        if (storyDialog == null)
        {
            storyDialog = storyPanel.GetComponentInChildren<StoryDialog>(true);
        }
        
        if (storyDialog != null)
        {
            // Отписываемся от старого события (если было)
            storyDialog.OnDialogFinished -= HandleDialogFinished;
            // Подписываемся на новое
            storyDialog.OnDialogFinished += HandleDialogFinished;
#if DEBUG
            Debug.Log($"[DialogManager] SetStoryPanel: Подписка на OnDialogFinished, storyDialog={storyDialog.name}");
#endif
        }
        else
        {
#if DEBUG
            Debug.LogWarning("[DialogManager] SetStoryPanel: StoryDialog не найден!");
#endif
        }
        
#if DEBUG
        Debug.Log($"[DialogManager] SetStoryPanel: {panel.name}");
#endif
    }

    /// <summary>
    /// Показать диалог с прямым текстом (скрывая UI)
    /// </summary>
    /// <param name="dialogText">Текст диалога</param>
    /// <param name="restoreGameUIAfter">Восстанавливать ли GameUI после диалога (true для IntroDialog, false для OutroDialog)</param>
    public void ShowDialog(string dialogText, bool restoreGameUIAfter = true)
    {
        if (storyDialog == null)
        {
#if DEBUG
            Debug.LogError("DialogManager: StoryDialog не назначен!");
#endif
            return;
        }

        if (string.IsNullOrEmpty(dialogText))
        {
#if DEBUG
            Debug.LogError("DialogManager: Dialog text is null or empty!");
#endif
            return;
        }

#if DEBUG
        Debug.Log($"[DialogManager] ShowDialog START");
        Debug.Log($"[DialogManager] dialogText: '{dialogText}'");
        Debug.Log($"[DialogManager] storyPanel: {(storyPanel != null ? storyPanel.name : "null")}");
        Debug.Log($"[DialogManager] storyPanel.activeSelf: {(storyPanel != null ? storyPanel.activeSelf.ToString() : "null")}");
        Debug.Log($"[DialogManager] gameUI: {(gameUI != null ? gameUI.name : "null")}");
        Debug.Log($"[DialogManager] gameUI.activeSelf: {(gameUI != null ? gameUI.activeSelf.ToString() : "null")}");
#endif

        // Сохраняем текущее состояние UI
        SaveUIState();

        // Скрываем UI во время диалога
        HideUIForDialog();

        // Устанавливаем флаг: нужно ли восстанавливать GameUI после диалога
        shouldRestoreGameUI = restoreGameUIAfter;

        // === 1. Устанавливаем текст ДО активации GameObject ===
        // Это важно, т.к. StoryDialog.OnEnable() очищает directDialogText
        storyDialog.directDialogText = dialogText;
        
#if DEBUG
        Debug.Log($"[DialogManager] Установлен directDialogText: '{storyDialog.directDialogText}'");
#endif

        // Подготовим диалог
        storyDialog.PrepareDialog();
        
#if DEBUG
        Debug.Log($"[DialogManager] PrepareDialog вызван. dialogLines.Length: {storyDialog.GetDialogLinesCount()}");
#endif

        // Установим индекс на 0
        storyDialog.index = 0;
        
        // Вызываем Show() явно перед активацией
        storyDialog.Show();
        
#if DEBUG
        if (storyDialog.storyText != null)
        {
            Debug.Log($"[DialogManager] storyText.text после Show(): '{storyDialog.storyText.text}'");
        }
        else
        {
            Debug.LogError("[DialogManager] storyText = null!");
        }
#endif

        // === 2. Показываем StoryPanel и StoryDialog ===
        // Сначала показываем StoryPanel (родитель)
        if (storyPanel != null)
        {
            storyPanel.SetActive(true);
#if DEBUG
            Debug.Log($"[DialogManager] storyPanel.SetActive(true). activeSelf: {storyPanel.activeSelf}");
#endif
            
            // Рекурсивно активируем все дочерние элементы (Background, Text, etc.)
            ShowAllChildren(storyPanel.transform);
        }
        
        // Затем показываем StoryDialog
        if (storyDialog.gameObject != null)
        {
            storyDialog.gameObject.SetActive(true);
#if DEBUG
            Debug.Log($"[DialogManager] storyDialog.SetActive(true). activeSelf: {storyDialog.gameObject.activeSelf}");
#endif
        }

#if DEBUG
        Debug.Log($"[DialogManager] ShowDialog END. StoryDialog активирован с текстом: '{dialogText}'");
#endif

        isDialogShowing = true;
    }

    /// <summary>
    /// Сохраняет текущее состояние UI
    /// </summary>
    private void SaveUIState()
    {
#if DEBUG
        Debug.Log("[DialogManager] SaveUIState called");
#endif
        if (gameUI != null)
        {
            wasGameUIActive = gameUI.activeSelf;
#if DEBUG
            Debug.Log($"[DialogManager] gameUI.activeSelf saved: {wasGameUIActive}");
#endif
        }

        if (menuPanel != null)
        {
            wasMenuPanelActive = menuPanel.activeSelf;
#if DEBUG
            Debug.Log($"[DialogManager] menuPanel.activeSelf saved: {wasMenuPanelActive}");
#endif
        }

        if (levelSelectPanel != null)
        {
            wasLevelSelectPanelActive = levelSelectPanel.activeSelf;
#if DEBUG
            Debug.Log($"[DialogManager] levelSelectPanel.activeSelf saved: {wasLevelSelectPanelActive}");
#endif
        }
    }

    /// <summary>
    /// Скрывает UI во время диалога
    /// </summary>
    private void HideUIForDialog()
    {
#if DEBUG
        Debug.Log("[DialogManager] HideUIForDialog called");
#endif
        if (gameUI != null)
        {
            gameUI.SetActive(false);
#if DEBUG
            Debug.Log($"[DialogManager] gameUI.SetActive(false). activeSelf: {gameUI.activeSelf}");
#endif
        }

        if (menuPanel != null)
        {
            menuPanel.SetActive(false);
#if DEBUG
            Debug.Log($"[DialogManager] menuPanel.SetActive(false). activeSelf: {menuPanel.activeSelf}");
#endif
        }

        if (levelSelectPanel != null)
        {
            levelSelectPanel.SetActive(false);
#if DEBUG
            Debug.Log($"[DialogManager] levelSelectPanel.SetActive(false). activeSelf: {levelSelectPanel.activeSelf}");
#endif
        }
    }

    /// <summary>
    /// Восстанавливает UI после диалога
    /// </summary>
    private void RestoreUIAfterDialog()
    {
        // Проверяем, есть ли на сцене зоны без зума
        SearchZone[] zones = FindObjectsByType<SearchZone>(FindObjectsSortMode.None);
        bool hasNonZoomZones = false;
        
        foreach (var zone in zones)
        {
            if (zone != null && !zone.useZoom && zone.autoActivateOnStart && zone.HasItemsToFind())
            {
                hasNonZoomZones = true;
                break;
            }
        }
        
        // Если есть зоны без зума — показываем GameUI сразу после диалога
        // Если только зум-зоны — GameUI остаётся скрытым до клика на зону
        if (hasNonZoomZones && gameUI != null && !gameUI.activeSelf)
        {
            gameUI.SetActive(true);
#if DEBUG
            Debug.Log("[DialogManager] Найдены зоны без зума — GameUI показан после диалога");
#endif
        }

        isDialogShowing = false;
        shouldRestoreGameUI = false;
    }

    /// <summary>
    /// Скрыть GameUI (вызывается при загрузке уровня)
    /// </summary>
    public void HideGameUI()
    {
        if (gameUI != null)
            gameUI.SetActive(false);
    }

    /// <summary>
    /// Показать GameUI (вызывается после закрытия IntroDialog)
    /// </summary>
    public void ShowGameUI()
    {
        if (gameUI != null)
            gameUI.SetActive(true);
    }
    
    /// <summary>
    /// Скрыть StoryPanel (вызывается при загрузке уровня)
    /// </summary>
    public void HideStoryPanel()
    {
        if (storyPanel != null)
            storyPanel.SetActive(false);
        
        if (storyDialog != null)
            storyDialog.gameObject.SetActive(false);
    }

    private void ShowAllChildren(Transform parent)
    {
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            child.gameObject.SetActive(true);

            // Рекурсивно показываем дочерние элементы
            ShowAllChildren(child);
        }
    }

    private void HandleDialogFinished()
    {
#if DEBUG
        Debug.Log("[DialogManager] HandleDialogFinished START");
        Debug.Log($"[DialogManager] HandleDialogFinished: OnDialogClosed has {OnDialogClosed?.GetInvocationList().Length ?? 0} subscribers");
#endif

        // StoryDialog.CloseDialog() уже скрыл весь StoryPanel...
        if (storyPanel != null && storyPanel.activeSelf)
        {
            storyPanel.SetActive(false);
#if DEBUG
            Debug.Log("[DialogManager] storyPanel принудительно скрыт");
#endif
        }

        // Восстанавливаем UI после диалога
        RestoreUIAfterDialog();

#if DEBUG
        Debug.Log("[DialogManager] HandleDialogFinished: Вызываем OnDialogClosed?.Invoke()");
#endif
        OnDialogClosed?.Invoke();

#if DEBUG
        Debug.Log("[DialogManager] HandleDialogFinished END");
#endif
    }

    private void OnDestroy()
    {
        if (storyDialog != null)
        {
            storyDialog.OnDialogFinished -= HandleDialogFinished;
        }
    }
}
