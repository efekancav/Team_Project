using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneTrigger : MonoBehaviour
{
    public string sceneToLoad;
    public float delayInSeconds = 0.5f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // If we are in the FIRST room, save the position before leaving
            if (SceneManager.GetActiveScene().name == "1Room")
            {
                // ADDITION: We add a Vector3 offset so the player spawns safely OUTSIDE the door when they return.
                // You might need to change -2f to +2f depending on which side of the door the player should stand!
                GameManager.Instance.lastRoom1Position = other.transform.position + new Vector3(0f, -2f, 0f);

                GameManager.Instance.hasStoredPosition = true;
            }

            StartCoroutine(LoadSceneWithDelay());
        }
    }

    IEnumerator LoadSceneWithDelay()
    {
        SFXManager.Instance.PlaySFX(
            SFXManager.Instance.levelFinish
        );

        yield return new WaitForSeconds(delayInSeconds);

        SceneManager.LoadScene(sceneToLoad);
    }
}