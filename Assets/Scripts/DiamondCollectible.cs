using UnityEngine;

public class DiamondCollectible : MonoBehaviour
{
    public int diamondValue = 1;
    public GameObject collectEffect;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<PlayerHealth>() != null)
        {
            if (DiamondManager.instance != null)
            {
                DiamondManager.instance.AddDiamond(diamondValue);
            }

            if (collectEffect != null)
            {
                Instantiate(collectEffect, transform.position, Quaternion.identity);
            }

            Destroy(gameObject);
        }
    }
}