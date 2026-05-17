using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathMenu : MonoBehaviour
{
    [Header("UI")]
    public GameObject deathPanel;

    [Header("Scene Names")]
    public string mainMenuSceneName = "MainMenu";

    private void Start()
    {
        HideDeathPanel();
    }

    public void ShowDeathPanel()
    {
        if (deathPanel != null)
            deathPanel.SetActive(true);

        Time.timeScale = 0f;
    }

    public void HideDeathPanel()
    {
        if (deathPanel != null)
            deathPanel.SetActive(false);

        Time.timeScale = 1f;
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }
}