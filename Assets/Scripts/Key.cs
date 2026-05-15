using UnityEngine;

public class Key : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            GameManager.Instance.keysCollected++;
            Debug.Log("Ключ подобран! Всего: " + GameManager.Instance.keysCollected);

            Destroy(gameObject);
        }
    }
}