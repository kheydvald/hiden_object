using UnityEngine;

/// <summary>
/// Менеджер экрана загрузки.
/// Ищет готовый объект на сцене (в Canvas) и просто включает/выключает его.
/// </summary>
public class LoadingScreenManager : PersistentManager<LoadingScreenManager>
{
    [Header("Loading Screen Settings")]
    [Tooltip("Объект экрана загрузки на сцене (внутри Canvas). Назначь в инспекторе или оставь пустым для авто-поиска.")]
    public GameObject loadingScreenObject;

    [Tooltip("Имя объекта для поиска, если поле выше пустое (например, 'LoadingScreenPanel')")]
    [SerializeField] private string searchObjectName = "LoadingScreen";

    [Tooltip("Время минимального показа экрана загрузки (секунды)")]
    [SerializeField] private float minDisplayTime = 0.5f;

    private float showTime;
    private bool isWaiting = false;
    private GameObject spinnerObj; // Ссылка на спиннер внутри панели

    protected override void OnInit()
    {
        // 1. Если объект не назначен вручную, ищем его на сцене
        if (loadingScreenObject == null)
        {
            loadingScreenObject = GameObject.Find(searchObjectName);
            
            if (loadingScreenObject == null)
            {
                Debug.LogError($"[LoadingScreenManager] ❌ Объект '{searchObjectName}' не найден на сцене! Проверь имя или назначь префаб в инспекторе.");
                return;
            }
            else
            {
                Debug.Log($"[LoadingScreenManager] ✅ Экран загрузки найден на сцене: {loadingScreenObject.name}");
            }
        }
        else
        {
            Debug.Log($"[LoadingScreenManager] ✅ Экран загрузки назначен вручную: {loadingScreenObject.name}");
        }

        // 2. Убеждаемся, что при старте он выключен
        loadingScreenObject.SetActive(false);

        // 3. Находим спиннер внутри этого объекта (для вращения)
        // Предполагаем, что спиннер называется "LoadingSpinner" или имеет тег/компонент
        Transform spinnerTransform = loadingScreenObject.transform.Find("Panel/LoadingSpinner"); 
        // Если у тебя другая структура, поправь путь выше (например, просто "LoadingSpinner")
        
        if (spinnerTransform != null)
        {
            spinnerObj = spinnerTransform.gameObject;
            var rotator = spinnerObj.GetComponent<LoadingSpinnerRotator>();
            if (rotator == null)
                rotator = spinnerObj.AddComponent<LoadingSpinnerRotator>();
            rotator.enabled = false; // Изначально выключен
        }
        else
        {
            Debug.LogWarning("[LoadingScreenManager] ⚠️ Спиннер не найден внутри экрана загрузки. Вращение работать не будет.");
        }
    }

    /// <summary>
    /// Показать экран загрузки
    /// </summary>
    public void ShowLoadingScreen()
    {
        if (loadingScreenObject == null)
        {
            Debug.LogError("[LoadingScreenManager] Объект лоадера не найден!");
            return;
        }

        loadingScreenObject.SetActive(true);
        showTime = Time.time;
        isWaiting = true;

        // Включаем вращение спиннера
        if (spinnerObj != null)
        {
            var rotator = spinnerObj.GetComponent<LoadingSpinnerRotator>();
            if (rotator != null) rotator.enabled = true;
        }

#if DEBUG
        Debug.Log("[LoadingScreenManager] ShowLoadingScreen");
#endif
    }

    /// <summary>
    /// Скрыть экран загрузки
    /// </summary>
    public void HideLoadingScreen()
    {
        if (loadingScreenObject == null) return;

        // Проверяем минимальное время показа
        float elapsedTime = Time.time - showTime;

        if (elapsedTime < minDisplayTime && isWaiting)
        {
            Invoke(nameof(HideLoadingScreenInternal), minDisplayTime - elapsedTime);
        }
        else
        {
            HideLoadingScreenInternal();
        }
    }

    private void HideLoadingScreenInternal()
    {
        if (loadingScreenObject == null) return;

        loadingScreenObject.SetActive(false);
        isWaiting = false;

        // Выключаем вращение
        if (spinnerObj != null)
        {
            var rotator = spinnerObj.GetComponent<LoadingSpinnerRotator>();
            if (rotator != null) rotator.enabled = false;
        }

#if DEBUG
        Debug.Log("[LoadingScreenManager] HideLoadingScreen");
#endif
    }

    /// <summary>
    /// Компонент для вращения спиннера
    /// </summary>
    private class LoadingSpinnerRotator : MonoBehaviour
    {
        [SerializeField] private float rotationSpeed = 180f;

        private void Update()
        {
            transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);
        }
    }
}