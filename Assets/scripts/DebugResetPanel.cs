using UnityEngine;

/// <summary>
/// Скрипт для отладки - показывает GUI кнопки для тестирования сброса прогресса
/// 
/// НАСТРОЙКА:
/// 1. Создайте пустой объект в любой сцене
/// 2. Добавьте этот скрипт
/// 3. Запустите игру - в левом верхнем углу появятся кнопки для теста
/// 
/// ВАЖНО: Удалите этот скрипт перед сборкой игры!
/// </summary>
public class DebugResetPanel : MonoBehaviour
{
    private Rect windowRect = new Rect(10, 10, 250, 300);
    private string logMessage = "";

    void OnGUI()
    {
        // Создаём окно в левом верхнем углу
        windowRect = GUI.Window(0, windowRect, DrawWindow, "🧪 Debug Reset Panel");
    }

    void DrawWindow(int windowID)
    {
        GUI.Label(new Rect(10, 20, 230, 20), "Тест сброса прогресса");

        // Кнопка 1: Проверка Instance
        if (GUI.Button(new Rect(10, 50, 230, 30), "Проверить Instance"))
        {
            if (ResetProgressManager.Instance != null)
            {
                logMessage = "✅ ResetProgressManager.Instance найден";
                Debug.Log(logMessage);
            }
            else
            {
                logMessage = "❌ ResetProgressManager.Instance = null";
                Debug.LogError(logMessage);
            }
        }

        // Кнопка 2: Сброс прогресса
        GUI.Label(new Rect(10, 90, 230, 20), "Сброс прогресса:");
        
        if (GUI.Button(new Rect(10, 115, 230, 30), "🔴 СБРОСИТЬ ВСЁ"))
        {
            if (ResetProgressManager.Instance != null)
            {
                ResetProgressManager.Instance.ResetAllProgressImmediate();
                logMessage = "✅ Прогресс сброшен!";
                Debug.Log(logMessage);
            }
            else
            {
                logMessage = "❌ ResetProgressManager не найден!";
                Debug.LogError(logMessage);
            }
        }

        // Кнопка 3: Сброс только уровней
        if (GUI.Button(new Rect(10, 155, 230, 30), "Сбросить уровни"))
        {
            if (ResetProgressManager.Instance != null)
            {
                ResetProgressManager.Instance.ResetLevelProgressOnly();
                logMessage = "✅ Прогресс уровней сброшен";
                Debug.Log(logMessage);
            }
            else
            {
                logMessage = "❌ ResetProgressManager не найден!";
                Debug.LogError(logMessage);
            }
        }

        // Кнопка 4: Проверка PlayerPrefs
        if (GUI.Button(new Rect(10, 195, 230, 30), "Проверить PlayerPrefs"))
        {
            string levelProgress = PlayerPrefs.GetString("LevelProgress", "ПУСТО");
            logMessage = $"LevelProgress: {levelProgress}";
            Debug.Log(logMessage);
        }

        // Кнопка 5: Удалить все PlayerPrefs
        if (GUI.Button(new Rect(10, 235, 230, 30), "⚠️ Удалить все PlayerPrefs"))
        {
            PlayerPrefs.DeleteAll();
            logMessage = "✅ Все PlayerPrefs удалены";
            Debug.Log(logMessage);
        }

        // Лог сообщений
        GUI.Label(new Rect(10, 275, 230, 20), logMessage);

        // Dragable заголовок
        GUI.DragWindow(new Rect(0, 0, 10000, 1000));
    }
}
