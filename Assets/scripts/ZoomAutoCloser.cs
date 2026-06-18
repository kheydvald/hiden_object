using UnityEngine;
using UnityEngine.UI;

public class ZoomAutoCloser : MonoBehaviour
{
    [Tooltip("Кнопка закрытия зума - будет автоматически нажата, когда все предметы найдены")]
    public Button closeButton;

    [Tooltip("Интервал проверки (в секундах)")]
    public float checkInterval = 0.5f;

    private SearchZone currentSearchZone;
    private bool isChecking = false;

    private void OnEnable()
    {
        StartChecking();
    }

    private void OnDisable()
    {
        StopChecking();
    }

    public void SetSearchZone(SearchZone zone)
    {
        currentSearchZone = zone;
        StartChecking();
    }

    private void StartChecking()
    {
        if (!isChecking && currentSearchZone != null)
        {
            isChecking = true;
            // 🔥 Первая проверка через 0.3 сек, чтобы зум успел открыться визуально
            // Затем проверка каждые 0.5 сек
            InvokeRepeating(nameof(CheckAndCloseIfComplete), 0.3f, checkInterval);
        }
    }

    private void StopChecking()
    {
        if (isChecking)
        {
            isChecking = false;
            CancelInvoke(nameof(CheckAndCloseIfComplete));
        }
    }

    private void CheckAndCloseIfComplete()
    {
        if (currentSearchZone != null && !currentSearchZone.IsZoneCompleted())
        {
            // Проверяем, остались ли ненайденные предметы
            if (!currentSearchZone.HasUnfoundItems())
            {
                // Все предметы найдены - закрываем зум через кнопку
                if (closeButton != null)
                {
                    closeButton.onClick.Invoke();
                }
                else
                {
                    // Если кнопка не назначена, вызываем напрямую
                    currentSearchZone.MarkAsCompleted();
                }

                // Останавливаем проверку
                StopChecking();
            }
        }
        else
        {
            // Зона уже завершена или отсутствует - останавливаем проверку
            StopChecking();
        }
    }
}