using UnityEngine;

/// <summary>
/// Базовый класс для постоянных менеджеров (Singleton).
/// Автоматически уничтожает дубликаты при загрузке сцен.
/// 
/// ВАЖНО: DontDestroyOnLoad должен вызываться в BootstrapManager для корневого объекта.
/// </summary>
/// <typeparam name="T">Тип менеджера (наследник MonoBehaviour)</typeparam>
public abstract class PersistentManager<T> : MonoBehaviour where T : Component
{
    public static T Instance { get; protected set; }

    /// <summary>
    /// Вызывается при первом создании экземпляра (переопределите для инициализации)
    /// </summary>
    protected virtual void OnInit() { }

    protected virtual void Awake()
    {
        if (Instance == null)
        {
            Instance = this as T;
            // DontDestroyOnLoad вызывается в BootstrapManager для корневого объекта PersistentManagers
            OnInit();
        }
        else
        {
            // Уничтожаем дубликат
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Очищает Instance при уничтожении (полезно для тестов)
    /// </summary>
    protected virtual void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
