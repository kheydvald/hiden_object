using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Обработчик кликов по фону зум-зоны.
/// Блокирует клики чтобы они не проходили сквозь зум на другие зоны.
/// </summary>
public class ZoomBackgroundClickHandler : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        // 🔥 Блокируем клик — он не пройдёт дальше этого объекта
        eventData.Use();

        // Проигрываем звук промаха
        SoundManager.Instance?.PlayMissSound();
    }
}