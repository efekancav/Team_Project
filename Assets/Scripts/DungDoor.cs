using UnityEngine;
using UnityEngine.SceneManagement;

public class DungDoor : MonoBehaviour
{
    [Header("Door Settings")]
    public string sceneToLoad;
    public int requiredKeys = 1;

    [Header("Door Visuals")]
    public GameObject closedDoor;
    public GameObject openDoor;

    [Header("Teleport (same scene)")]
    public bool useTeleport = false;
    public Transform targetPoint;

    private bool isOpen = false;

    void Start()
    {
        openDoor.SetActive(false);
        closedDoor.SetActive(true);
    }

    void Update()
    {
        if (!isOpen && GameManager.Instance.keysCollected >= requiredKeys)
        {
            OpenDoor();
        }
    }

    void OpenDoor()
    {
        isOpen = true;

        closedDoor.SetActive(false);
        openDoor.SetActive(true);

        Debug.Log("Door opened!");
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && isOpen)
        {
            if (useTeleport && targetPoint != null)
            {
                // 👉 ТЕЛЕПОРТ ВНУТРИ СЦЕНЫ
                collision.transform.position = targetPoint.position;
            }
            else
            {
                // 👉 ПЕРЕХОД НА ДРУГУЮ СЦЕНУ
                SceneManager.LoadScene(sceneToLoad);
            }
        }
    }
}