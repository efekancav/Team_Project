using UnityEngine;

public class Chest : MonoBehaviour
{
    public Sprite closedSprite;
    public Sprite openedSprite;

    public bool hasKey = true;

    public GameObject keyPrefab;
    public Transform spawnPoint;

    private bool isOpened = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isOpened && other.CompareTag("Player"))
        {
            OpenChest();
        }
    }

    void OpenChest()
    {
        isOpened = true;

        // меняем спрайт
        GetComponent<SpriteRenderer>().sprite = openedSprite;

        // если есть ключ → создаём
        if (hasKey && keyPrefab != null)
        {
            Instantiate(keyPrefab, spawnPoint.position, Quaternion.identity);
        }
    }
}