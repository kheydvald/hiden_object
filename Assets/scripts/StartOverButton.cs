using UnityEngine;
using UnityEngine.UI;

public class StartOverButton : MonoBehaviour
{
    [Header("Confirmation Panel")]
    public ConfirmationPanel confirmationPanel; // Панель подтверждения

    private ResetProgressManager resetManager;

    private void Start()
    {
        // Находим или создаем ResetProgressManager
        resetManager = Object.FindFirstObjectByType<ResetProgressManager>();
        if (resetManager == null)
        {
            GameObject resetObj = new GameObject("ResetProgressManager");
            resetManager = resetObj.AddComponent<ResetProgressManager>();
        }

        // Скрыть панель подтверждения при старте (если она есть)
        if (confirmationPanel != null)
        {
            confirmationPanel.Hide();
        }
    }

    /// <summary>
    /// Обработчик нажатия на кнопку "Начать заново"
    /// </summary>
    public void OnStartOverClicked()
    {
        if (confirmationPanel != null)
        {
            // Показать панель подтверждения
            confirmationPanel.Show(
                onConfirm: () => ConfirmStartOver(),
                onCancel: () => CancelStartOver()
            );
        }
        else
        {
            // Прямо сбросить прогресс без подтверждения
            ConfirmStartOver();
        }
    }

    /// <summary>
    /// Подтвердить сброс прогресса
    /// </summary>
    private void ConfirmStartOver()
    {
        if (resetManager != null)
        {
            resetManager.ResetAllProgressImmediate();
        }

        // Скрыть панель подтверждения
        if (confirmationPanel != null)
        {
            confirmationPanel.Hide();
        }
    }

    /// <summary>
    /// Отменить сброс прогресса
    /// </summary>
    private void CancelStartOver()
    {
        // Скрыть панель подтверждения
        if (confirmationPanel != null)
        {
            confirmationPanel.Hide();
        }
    }
}