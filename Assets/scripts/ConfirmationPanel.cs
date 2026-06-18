using UnityEngine;
using UnityEngine.UI;

public class ConfirmationPanel : MonoBehaviour
{
    [Header("UI Elements")]
    public Text questionText; // Текст с вопросом
    public Button yesButton;  // Кнопка "Да"
    public Button noButton;   // Кнопка "Нет"

    [Header("Confirmation Settings")]
    public string confirmationMessage = "Вы уверены, что хотите сбросить прогресс?";

    private System.Action onConfirm; // Действие при подтверждении
    private System.Action onCancel;  // Действие при отмене

    private void Awake()
    {
        // Убедиться, что панель изначально неактивна
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Показать панель подтверждения
    /// </summary>
    /// <param name="onConfirm">Действие при подтверждении</param>
    /// <param name="onCancel">Действие при отмене</param>
    public void Show(System.Action onConfirm, System.Action onCancel)
    {
        this.onConfirm = onConfirm;
        this.onCancel = onCancel;

        // Установить текст вопроса
        if (questionText != null)
        {
            questionText.text = confirmationMessage;
        }

        // Настроить кнопки
        if (yesButton != null)
        {
            yesButton.onClick.RemoveAllListeners();
            yesButton.onClick.AddListener(() => Confirm());
        }

        if (noButton != null)
        {
            noButton.onClick.RemoveAllListeners();
            noButton.onClick.AddListener(() => Cancel());
        }

        // Показать панель
        gameObject.SetActive(true);
    }

    /// <summary>
    /// Подтвердить действие
    /// </summary>
    private void Confirm()
    {
        if (onConfirm != null)
        {
            onConfirm();
        }

        // Скрыть панель
        Hide();
    }

    /// <summary>
    /// Отменить действие
    /// </summary>
    private void Cancel()
    {
        if (onCancel != null)
        {
            onCancel();
        }

        // Скрыть панель
        Hide();
    }

    /// <summary>
    /// Скрыть панель
    /// </summary>
    public void Hide()
    {
        gameObject.SetActive(false);
    }
}