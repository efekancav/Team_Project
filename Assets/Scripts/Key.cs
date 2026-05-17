using UnityEngine;

public class Key : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // проиграть звук
            SFXManager.Instance.PlaySFX(
                SFXManager.Instance.collect
            );

            GameManager.Instance.keysCollected++;

            Debug.Log("Keys collected! Total: " + GameManager.Instance.keysCollected);

            Destroy(gameObject);
        }
    }
}