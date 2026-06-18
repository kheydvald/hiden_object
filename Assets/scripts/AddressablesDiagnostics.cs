using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;

/// <summary>
/// Простая диагностика Addressables.
/// Показывает статус инициализации.
/// Добавьте на сцену bootstrap.unity
/// </summary>
public class AddressablesDiagnostics : MonoBehaviour
{
    [Header("Diagnostics")]
    [SerializeField] private bool logToConsole = true;

    private AsyncOperationHandle initHandle;

    private void Start()
    {
        Debug.Log("╔═══════════════════════════════════════════════════════════╗");
        Debug.Log("║  🔍 Addressables Diagnostics Started                      ║");
        Debug.Log("╚═══════════════════════════════════════════════════════════╝");

        Log($"[AddressablesDiagnostics] Runtime Path: {Addressables.RuntimePath}");

        // Проверяем инициализацию Addressables
        CheckAddressablesInitialization();
    }

    private async void CheckAddressablesInitialization()
    {
        try
        {
            Log($"[AddressablesDiagnostics] 🔄 Инициализация Addressables...");

            initHandle = Addressables.InitializeAsync();
            await initHandle.Task;

            if (initHandle.Status == AsyncOperationStatus.Succeeded)
            {
                Log($"[AddressablesDiagnostics] ✅ Addressables инициализирован успешно");

                // Проверяем наличие сцен уровней через загрузку
                string[] levelNames = new string[]
                {
                    "Assets/Scenes/1_office.unity",
                    "Assets/Scenes/2_door_park.unity",
                    "Assets/Scenes/3_cupol_cirka.unity",
                    "Assets/Scenes/4_carusel.unity",
                    "Assets/Scenes/5_gorki.unity",
                    "Assets/Scenes/6_strah_room.unity",
                    "Assets/Scenes/7_peshera.unity",
                    "Assets/Scenes/8_opushka.unity",
                    "Assets/Scenes/9_hata.unity"
                };

                Log($"[AddressablesDiagnostics] 📋 Проверка сцен уровней:");

                foreach (var levelName in levelNames)
                {
                    try
                    {
                        var loadHandle = Addressables.LoadSceneAsync(levelName, LoadSceneMode.Additive, true);
                        await loadHandle.Task;

                        if (loadHandle.Status == AsyncOperationStatus.Succeeded)
                        {
                            Log($"[AddressablesDiagnostics]   ✅ {levelName} (доступна)");
                            // Сразу выгружаем, чтобы не занимать память
                            Addressables.UnloadSceneAsync(loadHandle.Result);
                        }
                        else
                        {
                            var exceptionMsg = loadHandle.OperationException != null ? loadHandle.OperationException.Message : "неизвестная";
                            Log($"[AddressablesDiagnostics]   ❌ {levelName} (ошибка: {exceptionMsg})");
                        }
                    }
                    catch (System.Exception)
                    {
                        Log($"[AddressablesDiagnostics]   ❌ {levelName} (НЕ НАЙДЕН в Addressables!)");
                    }
                }
            }
            else
            {
                Debug.LogError($"[AddressablesDiagnostics] ❌ Ошибка инициализации Addressables");
                Debug.LogError($"[AddressablesDiagnostics] 📋 Status: {initHandle.Status}");
                Debug.LogError($"[AddressablesDiagnostics] 📋 Exception: {initHandle.OperationException}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[AddressablesDiagnostics] 💥 ИСКЛЮЧЕНИЕ: {e.GetType().Name}");
            Debug.LogError($"[AddressablesDiagnostics] 💥 Сообщение: {e.Message}");
            Debug.LogError($"[AddressablesDiagnostics] 💥 StackTrace: {e.StackTrace}");
        }
    }

    private void Log(string message)
    {
        if (logToConsole)
        {
            Debug.Log(message);
        }
    }

    private void OnDestroy()
    {
        if (initHandle.IsValid())
        {
            Addressables.Release(initHandle);
        }
    }
}
