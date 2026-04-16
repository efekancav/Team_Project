using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerSpawning : MonoBehaviour
{
    void Start()
    {
        // Check if the GameManager exists and has a saved position
        if (GameManager.Instance != null && GameManager.Instance.hasStoredPosition)
        {
            // Only teleport if we are back in the main room (1Room)
            if (SceneManager.GetActiveScene().name == "1Room")
            {
                transform.position = GameManager.Instance.lastRoom1Position;
            }
        }
    }
}