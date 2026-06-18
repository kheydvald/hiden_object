// LevelButton.cs
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class LevelButton : MonoBehaviour
{
    public LevelConfig levelConfig;
    public Image levelImage; // Reference to the image component showing the level thumbnail
    public Image lockOverlay; // Reference to the lock overlay image
    public GameObject lockIcon; // Optional lock icon GameObject

    private Button _button;

    private void Awake()
    {
        _button = GetComponent<Button>();
        if (_button == null)
        {
#if DEBUG
            Debug.LogError("LevelButton: Button component missing on " + name);
#endif
            return;
        }
        _button.onClick.AddListener(OnClick);
    }

    private void Start()
    {
#if DEBUG
        Debug.Log($"LevelButton {gameObject.name}: Starting - Active in hierarchy: {gameObject.activeInHierarchy}, Active self: {gameObject.activeSelf}");
#endif
        UpdateButtonState();
#if DEBUG
        Debug.Log($"LevelButton {gameObject.name}: After UpdateButtonState - Active in hierarchy: {gameObject.activeInHierarchy}, Active self: {gameObject.activeSelf}");
#endif

        // Подписываемся на события разблокировки и завершения уровней
        LevelProgressManager.OnLevelUnlocked += OnLevelUnlocked;
        LevelProgressManager.OnLevelCompleted += OnLevelCompleted;

        // Подписываемся на событие сброса прогресса
        ResetProgressManager.OnResetCompleted += OnResetCompleted;
    }

    public void UpdateButtonState()
    {
        // Проверяем, что кнопка инициализирована
        if (_button == null)
        {
#if DEBUG
            Debug.LogWarning($"LevelButton on {gameObject.name}: _button is null! Пропускаем обновление.");
#endif
            return;
        }

        if (levelConfig == null)
        {
#if DEBUG
            Debug.LogWarning($"LevelButton on {gameObject.name}: levelConfig is null! Пропускаем обновление.");
#endif
            return;
        }

        // Проверяем, что LevelProgressManager инициализирован
        if (LevelProgressManager.Instance == null)
        {
#if DEBUG
            Debug.LogWarning($"LevelButton on {gameObject.name}: LevelProgressManager.Instance ещё не инициализирован. Пропускаем обновление.");
#endif
            return;
        }

        // Check if level is unlocked using LevelProgressManager
        bool isUnlocked = LevelProgressManager.Instance != null && LevelProgressManager.Instance.IsLevelUnlocked(levelConfig.LevelName);

        // Check if level is completed (passed)
        bool isCompleted = LevelProgressManager.Instance != null && LevelProgressManager.Instance.IsLevelCompleted(levelConfig.LevelName);

#if DEBUG
        Debug.Log($"LevelButton: Level '{levelConfig.LevelName}' - Unlocked: {isUnlocked}, Completed: {isCompleted}, Button: {gameObject.name}");
#endif

        // Update button interactability
        _button.interactable = isUnlocked && !isCompleted;

        // Update visuals based on unlock state
        if (lockOverlay != null)
        {
            // Проверяем, не является ли lockOverlay самой кнопкой
            if (lockOverlay.gameObject == gameObject)
            {
#if DEBUG
                Debug.LogError($"LevelButton: ERROR - lockOverlay references the button itself! This will hide the button when unlocked. Level: {levelConfig?.LevelName}");
#endif
            }
            else
            {
                // Показываем overlay только если уровень заблокирован (не пройден, но заблокирован)
                lockOverlay.gameObject.SetActive(!isUnlocked && !isCompleted);
#if DEBUG
                Debug.Log($"LevelButton: Setting lockOverlay active to {!isUnlocked && !isCompleted}");
#endif
            }
        }

        if (lockIcon != null)
        {
            // Проверяем, не является ли lockIcon самой кнопкой
            if (lockIcon == gameObject)
            {
#if DEBUG
                Debug.LogError($"LevelButton: ERROR - lockIcon references the button itself! This will hide the button when unlocked. Level: {levelConfig?.LevelName}");
#endif
            }
            else
            {
                // Показываем иконку только если уровень заблокирован (не пройден, но заблокирован)
                lockIcon.SetActive(!isUnlocked && !isCompleted);
#if DEBUG
                Debug.Log($"LevelButton: Setting lockIcon active to {!isUnlocked && !isCompleted}");
#endif
            }
        }

        // Change button appearance based on unlock state
        if (levelImage != null)
        {
            // Используем спрайты из LevelConfig
            if (levelConfig != null)
            {
                if (isCompleted && levelConfig.completedSprite != null)
                {
                    // Если уровень пройден, используем спрайт завершенного уровня
                    levelImage.sprite = levelConfig.completedSprite;
#if DEBUG
                    Debug.Log($"LevelButton: Setting completed sprite for {levelConfig.LevelName}");
#endif
                }
                else if (isUnlocked && levelConfig.unlockedSprite != null)
                {
                    levelImage.sprite = levelConfig.unlockedSprite;
#if DEBUG
                    Debug.Log($"LevelButton: Setting unlocked sprite for {levelConfig.LevelName}");
#endif
                }
                else if (!isUnlocked && levelConfig.lockedSprite != null)
                {
                    levelImage.sprite = levelConfig.lockedSprite;
#if DEBUG
                    Debug.Log($"LevelButton: Setting locked sprite for {levelConfig.LevelName}");
#endif
                }

                // Если уровень пройден, делаем изображение неактивным (серым)
                if (isCompleted)
                {
                    levelImage.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
#if DEBUG
                    Debug.Log($"LevelButton: Level '{levelConfig.LevelName}' is completed, image set to inactive (gray)");
#endif
                }
                else
                {
                    levelImage.color = Color.white;
                }
            }
        }

        // Для пройденных уровней не скрываем кнопку, а делаем её неактивной визуально
        if (isCompleted)
        {
            // Кнопка неинтерактивна, но видима
            _button.interactable = false;

#if DEBUG
            Debug.Log($"LevelButton: Level '{levelConfig.LevelName}' is completed, button disabled but visible");
#endif
        }
    }

    public void OnClick()
    {
        if (levelConfig == null)
        {
            Debug.LogError($"[LevelButton] ❌ LevelConfig не назначен на кнопке {name}");
            return;
        }

        Debug.Log($"[LevelButton] 🖱️ Клик по кнопке уровня: '{levelConfig.LevelName}'");

        // Check if level is unlocked before allowing to play
        bool isUnlocked = LevelProgressManager.Instance?.IsLevelUnlocked(levelConfig.LevelName) ?? false;

        Debug.Log($"[LevelButton] 🔓 Уровень разблокирован: {isUnlocked}");

        if (!isUnlocked)
        {
            Debug.Log($"[LevelButton] 🚫 Уровень '{levelConfig.LevelName}' заблокирован!");
            return;
        }

        if (GameManager.Instance == null)
        {
            Debug.LogError("[LevelButton] ❌ GameManager.Instance is null!");
            return;
        }

        Debug.Log($"[LevelButton] ▶️ Вызов GameManager.LoadLevel()");
        GameManager.Instance.LoadLevel(levelConfig);
    }

    private void OnLevelUnlocked(string levelName)
    {
#if DEBUG
        Debug.Log($"LevelButton: Received OnLevelUnlocked event for level '{levelName}'. My level: {levelConfig?.LevelName}");
#endif

        // Обновляем состояние кнопки, если разблокирован уровень, связанный с этой кнопкой
        if (levelConfig != null && levelConfig.LevelName == levelName)
        {
#if DEBUG
            Debug.Log($"LevelButton: Updating button state for level '{levelName}'");
#endif
            UpdateButtonState();
        }
        else
        {
#if DEBUG
            Debug.Log($"LevelButton: Level name mismatch - event for '{levelName}', my level: {levelConfig?.LevelName}");
#endif
        }
    }

    private void OnLevelCompleted(string levelName)
    {
#if DEBUG
        Debug.Log($"LevelButton: Received OnLevelCompleted event for level '{levelName}'. My level: {levelConfig?.LevelName}");
#endif

        // Обновляем состояние кнопки, если уровень, связанный с этой кнопкой, завершен
        if (levelConfig != null && levelConfig.LevelName == levelName)
        {
#if DEBUG
            Debug.Log($"LevelButton: Updating button state for completed level '{levelName}'");
#endif
            UpdateButtonState();
        }
        else
        {
#if DEBUG
            Debug.Log($"LevelButton: Level name mismatch - event for '{levelName}', my level: {levelConfig?.LevelName}");
#endif
        }
    }

    private void OnResetCompleted()
    {
        // Обновляем состояние кнопки при сбросе прогресса
        UpdateButtonState();
    }

    private void OnDestroy()
    {
        if (_button != null)
        {
            _button.onClick.RemoveListener(OnClick);
        }

        // Отписываемся от событий разблокировки и завершения уровней
        LevelProgressManager.OnLevelUnlocked -= OnLevelUnlocked;
        LevelProgressManager.OnLevelCompleted -= OnLevelCompleted;

        // Отписываемся от события сброса прогресса
        ResetProgressManager.OnResetCompleted -= OnResetCompleted;
    }
}