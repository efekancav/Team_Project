using UnityEngine;
using UnityEngine.SceneManagement;

public class CollectibleDoor : MonoBehaviour
{
    [Header("Scene Settings")]
    public string sceneToLoad;
    public float delayInSeconds = 0.3f;

    [Header("Door Settings")]
    public bool requiresCollectibles = true;

    private bool isLoading = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isLoading)
            return;

        if (!other.CompareTag("Player"))
            return;

        if (requiresCollectibles)
        {
            if (CollectibleManager.instance == null)
            {
                Debug.LogWarning("CollectibleManager bulunamadı!");
                return;
            }

            if (!CollectibleManager.instance.HasEnoughCollectibles())
            {
                Debug.Log("Yeterli collectible yok!");
                return;
            }
        }

        isLoading = true;
        Invoke(nameof(LoadNextScene), delayInSeconds);
    }

    private void LoadNextScene()
    {
        SceneManager.LoadScene(sceneToLoad);
    }
}