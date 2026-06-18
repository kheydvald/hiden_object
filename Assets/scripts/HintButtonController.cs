using UnityEngine;
using UnityEngine.UI;
using YG;

public class HintButtonController : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private int maxHintAttempts = 3;
    [SerializeField] private string hintEffectAddress = "UI_HintAnim";

    [Header("UI References")]
    [SerializeField] private Sprite normalSprite;           // обычная иконка
    [SerializeField] private Sprite adSprite;              // иконка рекламы
    [SerializeField] private Image buttonImage;            // изображение кнопки
    [SerializeField] private Button hintButton;            // кнопка хинта
    [SerializeField] private TMPro.TextMeshProUGUI hintCounterText; // текст счетчика хинтов

    private int remainingAttempts;
    private bool adWatched = false;                        // просмотрена ли реклама
    private static Canvas _mainCanvas; // статический кэш — один на всю игру

    private void Awake()
    {
        if (hintButton == null)
            hintButton = GetComponent<Button>();

        hintButton.onClick.AddListener(OnHintClicked);
        remainingAttempts = maxHintAttempts;
        UpdateButtonState();
    }

    private void OnEnable()
    {
        // Подписываемся на событие успешного просмотра вознагражденной рекламы
        YG2.onRewardAdv += OnRewardedAdReceived;
    }

    private void OnDisable()
    {
        // Отписываемся от события при деактивации
        YG2.onRewardAdv -= OnRewardedAdReceived;
    }

    private void OnDestroy()
    {
        // Отписываемся от события при уничтожении объекта (защита от утечки памяти)
        YG2.onRewardAdv -= OnRewardedAdReceived;
        
        // Отписываемся от клика кнопки
        if (hintButton != null)
            hintButton.onClick.RemoveListener(OnHintClicked);
    }

    private void OnRewardedAdReceived(string adId)
    {
        // Проверяем, что это нужная реклама для сброса хинтов
        if (adId == "hint_reset")
        {
            OnRewardedAdCompleted();
        }
    }

    private void Update()
    {
        UpdateButtonState();
    }

    private void UpdateButtonState()
    {
        var activeZone = SearchManager.Instance != null ? SearchManager.Instance.ActiveZone : null;

        // Если реклама была просмотрена, можно использовать хинты снова
        if (adWatched && remainingAttempts <= 0)
        {
            remainingAttempts = maxHintAttempts; // восстанавливаем все хинты
            adWatched = false; // сбрасываем флаг
        }

        bool hasUnfoundItems = activeZone != null && activeZone.HasUnfoundItems();

        // Если хинты закончились, но есть ненайденные предметы, кнопка активна для рекламы
        bool canUseHint = hasUnfoundItems && remainingAttempts > 0;
        bool canShowAd = hasUnfoundItems && remainingAttempts <= 0;

        // Кнопка активна, если можно использовать хинт или показать рекламу
        hintButton.interactable = canUseHint || canShowAd;

        // Обновляем иконку кнопки
        if (buttonImage != null)
        {
            if (remainingAttempts > 0)
            {
                buttonImage.sprite = normalSprite; // обычная иконка
            }
            else
            {
                buttonImage.sprite = adSprite; // иконка рекламы
            }
        }

        // Обновляем текст счетчика хинтов
        if (hintCounterText != null)
        {
            if (remainingAttempts > 0)
            {
                hintCounterText.text = $"x{remainingAttempts}";
                hintCounterText.gameObject.SetActive(true);
            }
            else
            {
                hintCounterText.gameObject.SetActive(false);
            }
        }
    }

    private Canvas GetMainCanvas()
    {
        if (_mainCanvas == null)
        {
            _mainCanvas = UnityEngine.Object.FindFirstObjectByType<Canvas>();
            if (_mainCanvas == null)
            {
#if DEBUG
                Debug.LogError("HintButtonController: Main Canvas not found in scene!");
#endif
            }
        }
        return _mainCanvas;
    }

    public void OnHintClicked()
    {
#if DEBUG
        Debug.Log("🔍 [Hint] Кнопка хинта нажата");
#endif

        var activeZone = SearchManager.Instance != null ? SearchManager.Instance.ActiveZone : null;
        if (activeZone == null)
        {
#if DEBUG
            Debug.LogWarning("⚠️ [Hint] Нет активной зоны!");
#endif
            return;
        }

        // Если хинты закончились, показываем рекламу
        if (remainingAttempts <= 0)
        {
            ShowRewardAd();
            return;
        }

        if (!activeZone.HasUnfoundItems())
        {
#if DEBUG
            Debug.LogWarning("⚠️ [Hint] Нет ненайденных предметов");
#endif
            return;
        }

        remainingAttempts--;
#if DEBUG
        Debug.Log($"✅ [Hint] Осталось попыток: {remainingAttempts}");
#endif

        var randomItem = activeZone.GetRandomUnfoundSearchableItem();
        if (randomItem == null)
        {
            return;
        }

        var canvas = GetMainCanvas();
        if (canvas == null) return;

        // Загрузка эффекта через Resources
        GameObject hintEffectPrefab = Resources.Load<GameObject>(hintEffectAddress);
        if (hintEffectPrefab != null)
        {
            GameObject effect = Instantiate(hintEffectPrefab, canvas.transform);

            // ✅ Правильное позиционирование для UI-элементов в Canvas
            RectTransform itemRect = randomItem.GetComponent<RectTransform>();
            RectTransform effectRect = effect.GetComponent<RectTransform>();

            if (itemRect != null && effectRect != null)
            {
                // Помещаем хинт внутрь предмета (как дочерний UI-элемент)
                effect.transform.SetParent(itemRect, false);

                // Центрируем хинт по предмету
                effectRect.anchorMin = new Vector2(0.5f, 0.5f);
                effectRect.anchorMax = new Vector2(0.5f, 0.5f);
                effectRect.pivot = new Vector2(0.5f, 0.5f);
                effectRect.anchoredPosition = Vector2.zero;

                // Поднимаем хинт наверх (чтобы был поверх других элементов)
                effect.transform.SetAsLastSibling();

#if DEBUG
                Debug.Log($"[Hint] Эффект '{effect.name}' успешно позиционирован над предметом '{randomItem.name}'");
#endif
            }
            else
            {
#if DEBUG
                Debug.LogWarning("[Hint] Не удалось получить RectTransform у предмета или эффекта!");
#endif
                // На всякий случай — оставим в Canvas по центру
                effect.transform.SetParent(canvas.transform, false);
                if (effectRect != null)
                    effectRect.anchoredPosition = Vector2.zero;
            }

            Destroy(effect, 2f);
        }
        else
        {
#if DEBUG
            Debug.LogError($"❌ Не удалось загрузить UI-хинт: префаб '{hintEffectAddress}' не найден в папке Resources");
#endif
        }

        // Обновляем состояние кнопки и счетчика
        UpdateButtonState();
    }

    /// <summary>
    /// Показать вознагражденную рекламу для получения дополнительных хинтов
    /// </summary>
    private void ShowRewardAd()
    {
#if DEBUG
        Debug.Log("📺 [Hint] Показываем рекламу для получения дополнительных хинтов");
#endif
        YG2.RewardedAdvShow("hint_reset"); // ID рекламы для сброса хинтов
    }

    /// <summary>
    /// Вызывается при успешном просмотре рекламы
    /// </summary>
    public void OnRewardedAdCompleted()
    {
#if DEBUG
        Debug.Log("✅ [Hint] Реклама просмотрена, восстанавливаем хинты");
#endif
        // Восстанавливаем все хинты сразу
        remainingAttempts = maxHintAttempts;
        adWatched = false; // сбрасываем флаг, так как хинты уже восстановлены
        UpdateButtonState();
    }

    public void ForceUpdateState()
    {
        UpdateButtonState();
    }

    /// <summary>
    /// Получить количество оставшихся попыток
    /// </summary>
    public int GetRemainingAttempts()
    {
        return remainingAttempts;
    }

    /// <summary>
    /// Сбросить количество хинтов (для тестирования)
    /// </summary>
    public void ResetHints()
    {
        remainingAttempts = maxHintAttempts;
        adWatched = false;
        UpdateButtonState();
    }

    /// <summary>
    /// Статический метод для внешнего вызова восстановления хинтов
    /// </summary>
    public static void RestoreHintsExternally(HintButtonController controller)
    {
        if (controller != null)
        {
            controller.OnRewardedAdCompleted();
        }
    }
}

