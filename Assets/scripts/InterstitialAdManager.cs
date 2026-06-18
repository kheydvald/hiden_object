using UnityEngine;
using YG;

/// <summary>
/// Менеджер полноэкранной рекламы.
/// Показывает рекламу после завершения уровней (начиная с N-ного).
/// </summary>
public class InterstitialAdManager : PersistentManager<InterstitialAdManager>
{
    [Header("Настройки рекламы")]
    [Tooltip("Показывать рекламу начиная с этого уровня (включительно)")]
    [SerializeField] private int showAdFromLevel = 3;

    [Tooltip("ID рекламы (если используется кастомный)")]
    [SerializeField] private string adId = "interstitial"; // можно оставить "interstitial" по умолчанию

    protected override void OnInit()
    {
        // Подписываемся на событие завершения уровня
        LevelProgressTracker.OnLevelCompleted += OnLevelCompleted;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        // Отписываемся
        LevelProgressTracker.OnLevelCompleted -= OnLevelCompleted;
    }

    private void OnLevelCompleted(string levelName)
    {
        int levelNumber = ExtractLevelNumber(levelName);

        // Показываем рекламу, если уровень >= showAdFromLevel
        if (levelNumber >= showAdFromLevel)
        {
            ShowInterstitialAd();
        }
    }

    // Простой парсинг номера уровня из имени, например: "Level_BG6" → 6
    private int ExtractLevelNumber(string levelName)
    {
        string digits = "";
        foreach (char c in levelName)
        {
            if (char.IsDigit(c))
            {
                digits += c;
            }
        }

        if (int.TryParse(digits, out int levelNum))
        {
            return levelNum;
        }

        return 0; // если не удалось распознать
    }

    private void ShowInterstitialAd()
    {
        // Показываем полноэкранную рекламу
        YG2.InterstitialAdvShow();
        
        Debug.Log($"[Ad] Показана интерstitial-реклама после уровня (ID: {adId})");
    }

    // 🔹 Метод для ручного вызова (на случай, если нужно вызвать рекламу вручную)
    public void ShowAdNow()
    {
        YG2.InterstitialAdvShow();
    }
}
