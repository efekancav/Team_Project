using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    // This is where we store the coordinates for Room 1
    public Vector3 lastRoom1Position;
    public bool hasStoredPosition = false;

    private void Awake()
    {
        // This makes sure there is only ever one Manager and it stays alive between scenes
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (GetComponent<AstarSceneBootstrap>() == null)
                gameObject.AddComponent<AstarSceneBootstrap>();
        }
        else
        {
            Destroy(gameObject);
        }
    }
}