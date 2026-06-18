using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class StoryDialog : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text storyText;
    public GameObject panel;

    [Header("Dialog Text")]
    [TextArea]
    public string directDialogText;

    /// <summary>
    /// Событие вызывается когда все строки диалога прочитаны и диалог закрыт
    /// </summary>
    public event Action OnDialogFinished;

    public int index = 0;
    private string[] dialogLines; // Для хранения строк диалога

    void OnEnable()
    {
        // НЕ очищаем directDialogText здесь — он уже установлен в DialogManager.ShowDialog()
        // DialogManager сам вызывает PrepareDialog() и Show() когда нужно
        
#if DEBUG
        Debug.Log($"StoryDialog: OnEnable called. directDialogText: '{directDialogText}'");
#endif
    }

    public void PrepareDialog()
    {
        // Используем прямой текст
        if (!string.IsNullOrEmpty(directDialogText))
        {
            dialogLines = new string[] { directDialogText };
#if DEBUG
            Debug.Log($"[StoryDialog] PrepareDialog: directDialogText установлен, lines: {dialogLines.Length}");
#endif
        }
        else
        {
            dialogLines = new string[0];
#if DEBUG
            Debug.LogWarning("[StoryDialog] PrepareDialog: directDialogText пустой!");
#endif
        }
    }
    
    /// <summary>
    /// Возвращает количество строк диалога (для отладки)
    /// </summary>
    public int GetDialogLinesCount()
    {
        return dialogLines != null ? dialogLines.Length : 0;
    }

    public void Next()
    {
        if (dialogLines == null || dialogLines.Length == 0)
        {
            Debug.LogWarning("StoryDialog: Нет строк диалога для отображения!");
            return;
        }

        index++;

        if (index >= dialogLines.Length)
        {
            // Все строки прочитаны - закрываем диалог
            CloseDialog();
            return;
        }

        Show();
    }

    public void Show()
    {
        if (dialogLines == null || index < 0 || index >= dialogLines.Length)
        {
            Debug.LogWarning($"StoryDialog: Некорректные данные для отображения! dialogLines={(dialogLines == null ? "null" : dialogLines.Length.ToString())}, index={index}");
            return;
        }

        if (storyText != null)
        {
            storyText.text = dialogLines[index];
#if DEBUG
            Debug.Log($"[StoryDialog] Show(): установлен текст: '{dialogLines[index]}'");
#endif
        }
        else
        {
            Debug.LogError("[StoryDialog] storyText = null!");
        }
    }

    /// <summary>
    /// Закрытие диалога
    /// </summary>
    private void CloseDialog()
    {
#if DEBUG
        Debug.Log("[StoryDialog] CloseDialog: Вызываем OnDialogFinished?.Invoke()");
#endif

        // Сначала вызываем событие завершения диалога (пока GameObject ещё активен!)
        OnDialogFinished?.Invoke();

        // Скрываем весь StoryPanel (вместе с Background, Text и Button)
        gameObject.SetActive(false);

#if DEBUG
        Debug.Log("[StoryDialog] CloseDialog: StoryPanel скрыт");
#endif
    }
}

