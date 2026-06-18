using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Скрипт для кнопки "Сбросить прогресс".
/// Использует ResetProgressManager для полного сброса игры.
/// 
/// НАСТРОЙКА:
/// 1. Добавьте этот скрипт на GameObject с кнопкой
/// 2. Назначьте компонент Button на эту же кнопку
/// 3. В событии OnClick() кнопки добавьте:
///    - Object: этот GameObject (с ResetProgressButton)
///    - Function: ResetProgressButton.OnClick()
/// 
/// ОПЦИОНАЛЬНО:
/// - confirmationPanel: Панель подтверждения (если нужна)
/// </summary>
public class ResetProgressButton : MonoBehaviour
{
    [Tooltip("Панель подтверждения сброса (опционально)")]
    public ConfirmationPanel confirmationPanel;

    private Button _button;

    private void Awake()
    {
        _button = GetComponent<Button>();
        if (_button == null)
        {
            Debug.LogError("[ResetProgressButton] Button component not found on " + gameObject.name);
            return;
        }

        // Подписываемся на клик
        _button.onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        if (confirmationPanel != null)
        {
            // Показываем панель подтверждения
            confirmationPanel.Show(
                onConfirm: () => PerformReset(),
                onCancel: () => CancelReset()
            );
        }
        else
        {
            // Сбрасываем сразу
            PerformReset();
        }
    }

    private void PerformReset()
    {
        if (ResetProgressManager.Instance == null)
        {
            Debug.LogError("[ResetProgressButton] ResetProgressManager.Instance is null! Убедитесь, что bootstrap.unity загружается первой сценой.");
            return;
        }

        // Выполняем сброс
        ResetProgressManager.Instance.ResetAllProgressImmediate();

#if DEBUG
        Debug.Log("[ResetProgressButton] Прогресс сброшен!");
#endif
    }

    private void CancelReset()
    {
        // Просто скрываем панель (ConfirmationPanel сделает это сам)
#if DEBUG
        Debug.Log("[ResetProgressButton] Сброс отменён");
#endif
    }

    private void OnDestroy()
    {
        if (_button != null)
        {
            _button.onClick.RemoveListener(OnClick);
        }
    }
}
