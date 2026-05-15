using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    public void StartGame()
    {
        SceneManager.LoadScene("10_dung");
    }

    public void ContinueGame()
    {
        Debug.Log("Continue button clicked.");
    }

    public void OpenOptions()
    {
        Debug.Log("Options button clicked.");
    }

    public void OpenControls()
    {
        Debug.Log("Controls button clicked.");
    }
}