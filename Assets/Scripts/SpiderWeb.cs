using UnityEngine;

public class SpiderWeb : MonoBehaviour
{
    [Header("Movement Slow")]
    public float walkSlowMultiplier = 0.5f;
    public float runSlowMultiplier = 0.5f;

    [Header("Web Physics")]
    public float webDrag = 8f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            Rigidbody2D rb = other.GetComponent<Rigidbody2D>();

            if (player != null)
            {
                player.walkSpeed *= walkSlowMultiplier;
                player.runSpeed *= runSlowMultiplier;
            }

            if (rb != null)
            {
                rb.drag = webDrag;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            Rigidbody2D rb = other.GetComponent<Rigidbody2D>();

            if (player != null)
            {
                player.walkSpeed /= walkSlowMultiplier;
                player.runSpeed /= runSlowMultiplier;
            }

            if (rb != null)
            {
                rb.drag = 0f;
            }
        }
    }
}