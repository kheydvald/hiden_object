using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;

/// <summary>
/// Единая конфигурация уровня.
/// Заменяет LevelConfig + LevelUnlockData.
/// Поддерживает три режима загрузки уровня:
/// 1. LevelPrefab — загрузка префаба из Resources или Addressables
/// 2. sceneReference — загрузка сцены через Addressables (рекомендуется)
/// 3. legacyLevelKey — устаревший режим через Resources.Load
/// </summary>
[CreateAssetMenu(fileName = "LevelConfig", menuName = "Configs/Level Config")]
public class LevelConfig : ScriptableObject
{
    [Header("Level Info")]
    [Tooltip("Уникальное имя уровня (например: Level_BG1). Должно совпадать с Addressable Address сцены.")]
    public string LevelName;

    [Header("Level Prefab")]
    [Tooltip("Префаб уровня (сцена или префаб с фоном и зонами). Приоритет над sceneReference.")]
    public GameObject LevelPrefab;

    [Header("Addressable Scene (рекомендуется)")]
    [Tooltip("Addressable ссылка на сцену уровня. Используется, если LevelPrefab не назначен.")]
    public AssetReference sceneReference;

    [Header("UI")]
    [Tooltip("Опционально: UI для уровня. Если не назначен, используется GameUI из сцены")]
    public GameObject GameUI;

    [Header("Dialogs")]
    [Tooltip("Вступительный диалог при начале уровня")]
    [TextArea(3, 5)]
    public string IntroDialog;

    [Tooltip("Заключительный диалог при завершении уровня")]
    [TextArea(3, 5)]
    public string OutroDialog;

    [Header("Level Settings")]
    [Tooltip("Общее количество предметов для поиска")]
    public int TotalItemsCount = 5;

    [Tooltip("Задержка перед показом OutroDialog (секунды)")]
    public float OutroDelay = 2f;

    [Header("Level Unlock Settings")]
    [Tooltip("Уровни, которые должны быть пройдены перед разблокировкой этого уровня")]
    public List<string> requiredPreviousLevels = new List<string>();

    [Header("Level Button Sprites")]
    [Tooltip("Спрайт для разблокированного уровня на кнопке")]
    public Sprite unlockedSprite;

    [Tooltip("Спрайт для заблокированного уровня на кнопке")]
    public Sprite lockedSprite;

    [Tooltip("Спрайт для пройденного уровня на кнопке")]
    public Sprite completedSprite;

    [Tooltip("Иконка замка для заблокированного уровня")]
    public Sprite lockIcon;

    [Header("Legacy (for compatibility)")]
    [Tooltip("Устаревший ключ Addressables — игнорируется, если назначен LevelPrefab или sceneReference")]
    public string legacyLevelKey;

    #region Load Mode Detection

    /// <summary>
    /// Проверка: используется ли загрузка через Addressable сцену
    /// </summary>
    public bool IsAddressableScene => sceneReference != null && sceneReference.AssetGUID != null && string.IsNullOrEmpty(sceneReference.AssetGUID) == false && LevelPrefab == null;

    /// <summary>
    /// Проверка: используется ли загрузка через префаб
    /// </summary>
    public bool IsPrefabMode => LevelPrefab != null;

    /// <summary>
    /// Проверка: используется ли legacy режим (Resources.Load)
    /// </summary>
    public bool IsLegacyMode => legacyLevelKey != null && LevelPrefab == null && (sceneReference == null || string.IsNullOrEmpty(sceneReference.AssetGUID));

    public string GetLegacyLevelKey() => legacyLevelKey;
    public bool HasLegacyKey() => !string.IsNullOrEmpty(legacyLevelKey) && LevelPrefab == null && (sceneReference == null || string.IsNullOrEmpty(sceneReference.AssetGUID));

    #endregion
}