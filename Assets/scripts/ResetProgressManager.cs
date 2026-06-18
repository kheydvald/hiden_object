using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using YG.Utils;

/// <summary>
/// Менеджер сброса прогресса игры.
/// Используется для кнопок "Сбросить прогресс" в настройках или меню.
///
/// НАСТРОЙКА:
/// 1. Добавьте этот скрипт на пустой объект в сцене (например, в main_menu или select_level)
/// 2. Или используйте через ResetProgressManager.Instance
/// 3. На кнопке сброса в OnClick() вызовите ResetAllProgress() или ResetAllProgressImmediate()
///
/// ВАЖНО: Сбрасывает весь прогресс:
/// - Разблокированные уровни
/// - Пройденные уровни
/// - Найденные предметы
/// - Настройки аудио (MusicOn, SFXOn)
/// </summary>
public class ResetProgressManager : PersistentManager<ResetProgressManager>
{
    /// <summary>
    /// Событие, вызываемое после завершения сброса прогресса
    /// Используйте для обновления UI (кнопки уровней, иконки и т.д.)
    /// </summary>
    public static event System.Action OnResetCompleted;

    /// <summary>
    /// Сбросить весь прогресс игры (с небольшой задержкой для визуального эффекта)
    /// Вызывайте из кнопок UI
    /// </summary>
    public void ResetAllProgress()
    {
        StartCoroutine(ResetAllProgressWithDelay());
    }

    private IEnumerator ResetAllProgressWithDelay()
    {
        // Небольшая задержка для визуального эффекта
        yield return new WaitForSeconds(0.1f);

        PerformFullReset();
    }

    /// <summary>
    /// Мгновенный сброс прогресса (без задержек)
    /// Используйте для программного сброса
    /// </summary>
    public void ResetAllProgressImmediate()
    {
        StartCoroutine(PerformFullResetWithRefresh());
    }

    /// <summary>
    /// Выполнить полный сброс прогресса игры с обновлением интерфейса
    /// Использует корутину для постепенного обновления UI
    /// </summary>
    private IEnumerator PerformFullResetWithRefresh()
    {
#if DEBUG
        Debug.Log("[ResetProgressManager] Начинаем сброс прогресса...");
#endif

        // 🔥 ВАЖНО: Сначала очищаем все ключи LevelProgress_* в PlayerPrefs
        // Это нужно для сброса прогресса найденных предметов на уровнях
        PlayerPrefs.DeleteKey("LevelProgress");

        // Сброс всех данных в PlayerPrefs (включая настройки аудио)
        PlayerPrefs.DeleteAll();

        // Очищаем ключ LevelProgress в LocalStorage (для Yandex Games)
        LocalStorage.DeleteKey("LevelProgress");

        // Сохраняем изменения
        PlayerPrefs.Save();

        // Сброс данных LevelProgressManager
        if (LevelProgressManager.Instance != null)
        {
            LevelProgressManager.Instance.ResetAllProgress();
#if DEBUG
            Debug.Log("[ResetProgressManager] LevelProgressManager сброшен");
#endif
        }

        // Сброс компонентов игры
        ResetGameComponents();

        // Ждём конца кадра, чтобы все системы успели обновиться
        yield return new WaitForEndOfFrame();

        // Обновляем все кнопки уровней
        UpdateAllLevelButtons();

        // Дополнительная задержка и повторное обновление
        yield return new WaitForSeconds(0.1f);
        UpdateAllLevelButtons();

        // Принудительное обновление всех Canvas
        yield return new WaitForSeconds(0.1f);
        Canvas.ForceUpdateCanvases();

        // Финальное обновление
        yield return new WaitForSeconds(0.1f);
        UpdateAllLevelButtons();

#if DEBUG
        Debug.Log("[ResetProgressManager] Сброс прогресса завершён");
#endif

        // Сигнал всем подписчикам, что сброс завершён
        OnResetCompleted?.Invoke();
    }

    /// <summary>
    /// Обновить состояние всех кнопок уровней
    /// Вызывает UpdateButtonState() на всех LevelButton в сцене
    /// </summary>
    private void UpdateAllLevelButtons()
    {
        // Обновляем через LevelSelectionManager, если он существует
        LevelSelectionManager levelSelectionManager = Object.FindFirstObjectByType<LevelSelectionManager>();
        if (levelSelectionManager != null)
        {
            levelSelectionManager.UpdateAllLevelButtons();
#if DEBUG
            Debug.Log("[ResetProgressManager] LevelSelectionManager обновлён");
#endif
        }

        // Также вручную обновляем все LevelButton
        LevelButton[] allLevelButtons = Object.FindObjectsByType<LevelButton>(FindObjectsSortMode.None);
        int updatedCount = 0;
        foreach (LevelButton button in allLevelButtons)
        {
            if (button != null)
            {
                button.UpdateButtonState();
                updatedCount++;
            }
        }
#if DEBUG
        Debug.Log($"[ResetProgressManager] Обновлено кнопок уровней: {updatedCount}");
#endif
    }

    /// <summary>
    /// Сбросить состояние всех игровых компонентов
    /// </summary>
    private void ResetGameComponents()
    {
        // Сброс LevelProgressTracker
        LevelProgressTracker[] trackers = Object.FindObjectsByType<LevelProgressTracker>(FindObjectsSortMode.None);
        foreach (LevelProgressTracker tracker in trackers)
        {
            if (tracker != null)
            {
                tracker.ResetTracker();
#if DEBUG
                Debug.Log($"[ResetProgressManager] LevelProgressTracker '{tracker.name}' сброшен");
#endif
            }
        }

        // Сброс SearchManager
        SearchManager searchManager = Object.FindFirstObjectByType<SearchManager>();
        if (searchManager != null)
        {
            searchManager.ResetState();
#if DEBUG
            Debug.Log("[ResetProgressManager] SearchManager сброшен");
#endif
        }

        // 🔥 ВАЖНО: Очищаем все ключи LevelProgress_* в PlayerPrefs
        // Это нужно для сброса прогресса найденных предметов на уровнях
        PlayerPrefs.DeleteKey("LevelProgress");

        // Также можно очистить все ключи, начинающиеся с "LevelProgress_"
        // Но PlayerPrefs не поддерживает enumerate keys, поэтому очищаем только основные
#if DEBUG
        Debug.Log("[ResetProgressManager] LevelProgress ключи очищены из PlayerPrefs");
#endif
    }

    /// <summary>
    /// Выполнить полный сброс прогресса игры (без корутины)
    /// Используйте для немедленного сброса
    /// </summary>
    private void PerformFullReset()
    {
#if DEBUG
        Debug.Log("[ResetProgressManager] Выполняем немедленный сброс прогресса...");
#endif

        // Сброс всех данных в PlayerPrefs (включая настройки аудио и прогресс уровней)
        PlayerPrefs.DeleteAll();

        // Очищаем ключ LevelProgress в LocalStorage (для Yandex Games)
        LocalStorage.DeleteKey("LevelProgress");

        // Сохраняем изменения
        PlayerPrefs.Save();

        if (LevelProgressManager.Instance != null)
        {
            // Сбрасываем внутренние словари LevelProgressManager
            LevelProgressManager.Instance.ResetAllProgress();
#if DEBUG
            Debug.Log("[ResetProgressManager] LevelProgressManager сброшен");
#endif
        }

        // Сброс компонентов игры
        ResetGameComponents();

        // Обновляем UI
        UpdateAllLevelButtons();
        Canvas.ForceUpdateCanvases();

#if DEBUG
        Debug.Log("[ResetProgressManager] Немедленный сброс прогресса завершён");
#endif

        // Сигнал о завершении
        OnResetCompleted?.Invoke();
    }

    /// <summary>
    /// Сбросить только настройки аудио (MusicOn, SFXOn)
    /// </summary>
    public void ResetAudioSettings()
    {
        PlayerPrefs.SetInt("MusicOn", 1);
        PlayerPrefs.SetInt("SFXOn", 1);
        PlayerPrefs.Save();
#if DEBUG
        Debug.Log("[ResetProgressManager] Настройки аудио сброшены");
#endif
    }

    /// <summary>
    /// Сбросить только прогресс уровней (разблокировка и прохождение)
    /// Не затрагивает настройки аудио
    /// </summary>
    public void ResetLevelProgressOnly()
    {
        if (LevelProgressManager.Instance != null)
        {
            LevelProgressManager.Instance.ResetAllProgress();
            LevelProgressManager.Instance.LoadProgress();
#if DEBUG
            Debug.Log("[ResetProgressManager] Прогресс уровней сброшен");
#endif
        }
        else
        {
            LocalStorage.DeleteKey("LevelProgress");
#if DEBUG
            Debug.Log("[ResetProgressManager] LevelProgressManager не найден, очищаем LocalStorage напрямую");
#endif
        }

        // Сброс LevelProgressTracker
        LevelProgressTracker[] trackers = Object.FindObjectsByType<LevelProgressTracker>(FindObjectsSortMode.None);
        foreach (LevelProgressTracker tracker in trackers)
        {
            tracker?.ResetTracker();
        }

        // Обновляем кнопки уровней
        UpdateAllLevelButtons();

        // Сигнал о завершении
        OnResetCompleted?.Invoke();
    }

    /// <summary>
    /// Очистить кэш levelConfigs в LevelProgressManager
    /// Используйте если levelConfigs не назначен в инспекторе
    /// </summary>
    public void ClearLevelConfigsCache()
    {
        if (LevelProgressManager.Instance != null && LevelProgressManager.Instance.levelConfigs != null)
        {
            LevelProgressManager.Instance.levelConfigs.Clear();
#if DEBUG
            Debug.Log("[ResetProgressManager] levelConfigs очищен");
#endif
        }
    }
}