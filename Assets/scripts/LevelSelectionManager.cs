using UnityEngine;
using UnityEngine.UI;

public class LevelSelectionManager : MonoBehaviour
{
    [Header("Level Buttons")]
    public LevelButton[] levelButtons;

    private void Start()
    {
        UpdateAllLevelButtons();
    }

    private void OnEnable()
    {
        UpdateAllLevelButtons();
    }

    public void UpdateAllLevelButtons()
    {
        if (levelButtons != null)
        {
            foreach (var button in levelButtons)
            {
                button?.UpdateButtonState();
            }
        }
    }

    /// <summary>
    /// Method to reset all progress for testing purposes
    /// </summary>
    public void ResetAllProgress()
    {
        if (LevelProgressManager.Instance != null)
        {
            LevelProgressManager.Instance.ResetAllProgress();
            UpdateAllLevelButtons();
        }
    }
}