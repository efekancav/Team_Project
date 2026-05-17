using UnityEngine;

public class Key : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // Eski sistemi bozmayalım
            GameManager.Instance.keysCollected++;
            Debug.Log("Ключ подобран! Всего: " + GameManager.Instance.keysCollected);

            // Yeni Collectible UI sistemi
            if (CollectibleManager.instance != null)
            {
                CollectibleManager.instance.AddCollectible(1);
            }

            Destroy(gameObject);
        }
    }
}