using UnityEngine;
using UnityEngine.UI;

public class LevelProgressTest : MonoBehaviour
{
    public Button testCompleteLevelButton;
    public Text debugText;
    public string testLevelName = "Level1";

    private void Start()
    {
        if (testCompleteLevelButton != null)
        {
            testCompleteLevelButton.onClick.AddListener(TestCompleteLevel);
        }
    }

    private void Update()
    {
        if (debugText != null)
        {
            debugText.text = "Current unlocked levels: " + GetUnlockedLevelsText();
        }
    }

    private string GetUnlockedLevelsText()
    {
        if (LevelProgressManager.Instance != null)
        {
            var unlocked = LevelProgressManager.Instance.GetAllUnlockedLevels();
            return string.Join(", ", unlocked);
        }
        return "None";
    }

    public void TestCompleteLevel()
    {
        if (LevelProgressManager.Instance != null)
        {
            LevelProgressManager.Instance.UnlockLevel(testLevelName);
            Debug.Log("Test: Completed level " + testLevelName);
        }
    }
}