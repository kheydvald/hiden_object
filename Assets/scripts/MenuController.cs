using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    [SerializeField] private GameObject menuPanel; // Панель меню
    [SerializeField] private AudioSource musicSource; // Источник музыки

    private bool isMusicOn = true;

    void Start()
    {
        menuPanel.SetActive(false); // Скрыть панель при старте
    }

    public void ToggleMenu()
    {
        menuPanel.SetActive(!menuPanel.activeSelf);
    }

    public void GoHome()
    {
        Time.timeScale = 1f; // Сброс времени (если использовалось замедление)

        Debug.Log($"GoHome called. GameManager.Instance is {(GameManager.Instance != null ? "not null" : "null")}");

        // Сначала скрываем текущее меню
        if (menuPanel != null)
        {
            menuPanel.SetActive(false);
            Debug.Log("Current menu panel deactivated.");
        }

        // Вместо загрузки сцены, просто показываем главное меню
        if (GameManager.Instance != null)
        {
            Debug.Log("Calling GameManager.Instance.ShowMainMenu()");
            GameManager.Instance.ShowMainMenu();
        }
        else
        {
            Debug.LogError("GameManager.Instance is null! Cannot show main menu.");

            // Попробуем найти GameManager вручную
            var gameManager = Object.FindFirstObjectByType<GameManager>();
            if (gameManager != null)
            {
                Debug.Log("GameManager found with FindFirstObjectByType. Calling ShowMainMenu directly.");
                gameManager.ShowMainMenu();
            }
            else
            {
                Debug.LogError("GameManager not found even with FindFirstObjectByType!");
            }
        }
    }

    public void ToggleMusic()
    {
        isMusicOn = !isMusicOn;
        if (musicSource != null)
        {
            if (isMusicOn)
                musicSource.UnPause();
            else
                musicSource.Pause();
        }
    }
}
