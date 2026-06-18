using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Универсальный обработчик кликов для активации зон поиска.
/// Заменяет: BackgroundTrigger, UIZoneTrigger, ZoomTrigger.
/// 
/// НАЗНАЧЕНИЕ:
/// - Вешается на любой кликабельный объект (фон, UI-зона, 3D-объект)
/// - При клике активирует SearchZone
/// 
/// НАСТРОЙКА:
/// 1. Добавьте на объект с Collider2D или Image (с Raycast Target)
/// 2. Назначьте targetZone (или оставьте пустым для автопоиска в родителях)
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class ClickHandler : MonoBehaviour, IPointerClickHandler
{
    [Tooltip("Целевая зона поиска. Если не назначена — будет найдена в родителях автоматически.")]
    [SerializeField] private SearchZone targetZone;

    private void Awake()
    {
        // Автопоиск зоны в родителях, если не назначена вручную
        if (targetZone == null)
        {
            targetZone = GetComponentInParent<SearchZone>();
            
            if (targetZone == null)
            {
                // Пробуем найти через legacy ItemTriggerZone
                var legacyZone = GetComponentInParent<ItemTriggerZone>();
                if (legacyZone != null)
                {
                    targetZone = legacyZone.GetComponent<SearchZone>();
                }
            }
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // 🔥 Блокируем клики если зум уже открыт (чтобы нельзя было открыть другую зум-зону)
        if (SearchManager.Instance != null && SearchManager.Instance.ActiveZone != null)
        {
#if DEBUG
            Debug.Log($"[ClickHandler] Зум уже открыт ('{SearchManager.Instance.ActiveZone.name}'), клик заблокирован.");
#endif
            return;
        }

        // Проверяем, завершена ли зона
        if (targetZone == null)
        {
#if DEBUG
            Debug.LogError($"[ClickHandler] SearchZone не найдена для объекта '{name}'. Назначьте targetZone в инспекторе или добавьте SearchZone в иерархию.");
#endif
            return;
        }

        // Проверяем, завершена ли зона
        if (targetZone.IsZoneCompleted())
        {
#if DEBUG
            Debug.Log($"[ClickHandler] Зона '{targetZone.name}' уже завершена. Клик проигнорирован.");
#endif
            return;
        }

        // Проверяем, есть ли предметы для поиска
        if (!targetZone.HasItemsToFind())
        {
#if DEBUG
            Debug.Log($"[ClickHandler] Зона '{targetZone.name}' не имеет предметов для поиска. Клик проигнорирован.");
#endif
            return;
        }

        // Активируем зону через SearchManager
        SearchManager.Instance?.ActivateZone(targetZone);
    }
}
