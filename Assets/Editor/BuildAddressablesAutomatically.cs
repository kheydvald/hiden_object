using UnityEditor;

public static class BuildAddressablesAutomatically
{
    [InitializeOnLoadMethod]
    static void AutoBuild()
    {
        // Подключите при необходимости
        // EditorApplication.playModeStateChanged += OnPlayModeState;
    }

    static void OnPlayModeState(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingEditMode)
        {
            // Addressables больше не используется, удаляем вызов
            // AddressableAssetSettings.BuildPlayerContent();
            UnityEngine.Debug.Log("Addressables: больше не используется, вызов BuildPlayerContent() удален.");
        }
    }
}
