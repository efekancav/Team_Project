using UnityEngine;
using System.Collections;

public class LocalTeleport : MonoBehaviour
{
    [Header("Settings")]
    public Transform destination;      // Drag the exit marker here
    public float animationSpeed = 0.3f; // Speed of shrink/grow
    public float cooldownTime = 0.5f;   // How long to wait before doors work again

    // The Safety Lock: 'static' means all doors share this one lock
    private static bool isGlobalTeleporting = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Only trigger if it's the Player and the Lock is OPEN
        if (other.CompareTag("Player") && !isGlobalTeleporting)
        {
            StartCoroutine(TeleportSequence(other.transform));
        }
    }

    IEnumerator TeleportSequence(Transform player)
    {
        isGlobalTeleporting = true; // CLOSE the lock

        // 1. Get components
        var moveScript = player.GetComponent<MonoBehaviour>();
        var sr = player.GetComponent<SpriteRenderer>();
        Vector3 originalScale = player.localScale;

        // 2. Freeze & Go "Inside" (Behind door)
        if (moveScript != null) moveScript.enabled = false;
        if (sr != null) sr.sortingOrder = -3; // Assuming door is -2

        // 3. Shrink Animation
        float timer = 0;
        while (timer < animationSpeed)
        {
            player.localScale = Vector3.Lerp(originalScale, Vector3.zero, timer / animationSpeed);
            timer += Time.deltaTime;
            yield return null;
        }
        player.localScale = Vector3.zero;

        // 4. Move to Destination
        player.position = destination.position;

        // 5. Grow Animation
        timer = 0;
        while (timer < animationSpeed)
        {
            player.localScale = Vector3.Lerp(Vector3.zero, originalScale, timer / animationSpeed);
            timer += Time.deltaTime;
            yield return null;
        }
        player.localScale = originalScale;

        // 6. Unfreeze & Come back to Front
        if (sr != null) sr.sortingOrder = 0;
        if (moveScript != null) moveScript.enabled = true;

        // 7. Wait for cooldown, then OPEN the lock
        yield return new WaitForSeconds(cooldownTime);
        isGlobalTeleporting = false;
    }
}