using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LevelConfig))]
public class LevelConfigEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        LevelConfig config = (LevelConfig)target;

        if (config.LevelPrefab != null && GUILayout.Button("Apply LevelManager to Prefab"))
        {
            ApplyLevelManagerToPrefab(config.LevelPrefab);
        }
    }

    private void ApplyLevelManagerToPrefab(GameObject prefab)
    {
        if (prefab != null && prefab.GetComponent<LevelManager>() == null)
        {
            prefab.AddComponent<LevelManager>();
            EditorUtility.SetDirty(prefab);
            Debug.Log($"LevelManager добавлен к {prefab.name}");
        }
        else if (prefab == null)
        {
            Debug.LogError($"LevelConfigEditor: Префаб не назначен в LevelConfig");
        }
    }
}
