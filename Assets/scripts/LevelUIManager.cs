using UnityEngine;

/// <summary>
/// Менеджер UI уровня. Управляет интерфейсом во время геймплея.
/// Вешается на пустой объект на сцене уровня или на Canvas.
/// </summary>
public class LevelUIManager : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Панель со словами (список предметов для поиска)")]
    [SerializeField] private GameObject wordsPanel;
    
    [Tooltip("Контейнер для зум-объектов")]
    [SerializeField] private RectTransform zoomContainer;
    
    [Tooltip("Контейнер для фона уровня")]
    [SerializeField] private RectTransform backgroundContainer;

    [Header("Buttons")]
    [Tooltip("Кнопка подсказки")]
    [SerializeField] private HintButtonController hintButton;
    
    [Tooltip("Кнопка закрытия зума")]
    [SerializeField] private CloseZoomButton closeZoomButton;

    [Header("Panels")]
    [Tooltip("Панель паузы (опционально)")]
    [SerializeField] private GameObject pausePanel;
    
    [Tooltip("Панель завершения уровня (опционально)")]
    [SerializeField] private GameObject completionPanel;

    private bool isPaused = false;

    public RectTransform ZoomContainer => zoomContainer;
    public RectTransform BackgroundContainer => backgroundContainer;
    public GameObject WordsPanel => wordsPanel;

    private void Awake()
    {
        // Инициализация ссылок через SearchManager
        if (SearchManager.Instance != null)
        {
            if (wordsPanel != null)
            {
                SearchManager.Instance.SetWordsPanel(wordsPanel);
            }
        }
    }

    /// <summary>
    /// Показать панель паузы
    /// </summary>
    public void ShowPausePanel()
    {
        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
            isPaused = true;
            Time.timeScale = 0f;
        }
    }

    /// <summary>
    /// Скрыть панель паузы
    /// </summary>
    public void HidePausePanel()
    {
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
            isPaused = false;
            Time.timeScale = 1f;
        }
    }

    /// <summary>
    /// Переключить состояние паузы
    /// </summary>
    public void TogglePause()
    {
        if (isPaused)
            HidePausePanel();
        else
            ShowPausePanel();
    }

    /// <summary>
    /// Показать панель завершения уровня
    /// </summary>
    public void ShowCompletionPanel()
    {
        if (completionPanel != null)
        {
            completionPanel.SetActive(true);
        }
    }

    /// <summary>
    /// Скрыть панель завершения уровня
    /// </summary>
    public void HideCompletionPanel()
    {
        if (completionPanel != null)
        {
            completionPanel.SetActive(false);
        }
    }

    /// <summary>
    /// Получить кнопку подсказки
    /// </summary>
    public HintButtonController GetHintButton()
    {
        return hintButton;
    }

    /// <summary>
    /// Получить кнопку закрытия зума
    /// </summary>
    public CloseZoomButton GetCloseZoomButton()
    {
        return closeZoomButton;
    }

    private void OnDestroy()
    {
        // Сбрасываем Time.timeScale при уничтожении
        Time.timeScale = 1f;
    }
}
