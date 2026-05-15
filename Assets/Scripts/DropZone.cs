using UnityEngine;

public class DropZone : MonoBehaviour
{
    public float gravityScale = 0.05f;
    public float maxFallSpeed = -1f;

    private void OnTriggerStay2D(Collider2D other)
    {
        PlayerController player = other.GetComponentInParent<PlayerController>();

        if (player != null)
        {
            player.EnterDropZone(gravityScale, maxFallSpeed);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        PlayerController player = other.GetComponentInParent<PlayerController>();

        if (player != null)
        {
            player.ExitDropZone();
        }
    }
}