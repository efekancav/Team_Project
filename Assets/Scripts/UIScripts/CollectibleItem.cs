using UnityEngine;

public class CollectibleItem : MonoBehaviour
{
    [Header("Collectible Settings")]
    public CollectibleType collectibleType;
    public int value = 1;

    [Header("Effects")]
    public GameObject collectEffect;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (CollectibleManager.instance == null)
            return;

        if (CollectibleManager.instance.GetCollectibleType() != collectibleType)
            return;

        CollectibleManager.instance.AddCollectible(value);

        if (collectEffect != null)
        {
            Instantiate(collectEffect, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }
}