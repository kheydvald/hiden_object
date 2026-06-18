using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Скрипт для кнопки "Назад" в сцене выбора уровней.
/// Использует GameManager.Instance для корректной работы между сценами.
/// 
/// НАСТРОЙКА:
/// 1. Добавьте этот скрипт на GameObject с кнопкой (или на саму кнопку)
/// 2. Назначьте компонент Button на эту же кнопку (если ещё не назначен)
/// 3. В событии OnClick() кнопки добавьте:
///    - Object: этот GameObject (с MenuBackButton)
///    - Function: MenuBackButton.OnClick()
/// </summary>
public class MenuBackButton : MonoBehaviour
{
    private Button _button;

    private void Awake()
    {
        _button = GetComponent<Button>();
        if (_button == null)
        {
            Debug.LogError("[MenuBackButton] Button component not found on " + gameObject.name);
            return;
        }

        // Подписываемся на клик
        _button.onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("[MenuBackButton] GameManager.Instance is null! Убедитесь, что bootstrap.unity загружается первой сценой.");
            return;
        }

        GameManager.Instance.ShowMainMenu();
    }

    private void OnDestroy()
    {
        if (_button != null)
        {
            _button.onClick.RemoveListener(OnClick);
        }
    }
}
