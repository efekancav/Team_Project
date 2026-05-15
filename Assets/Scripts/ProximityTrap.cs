using UnityEngine;

public class ProximityTrap : MonoBehaviour
{
    [Header("Trap Setup")]
    public Transform player;
    public float triggerDistance = 10f;
    public float rollSpeed = 7f;

    private Rigidbody2D rb;
    private bool isTriggered = false;

    // NEW: A timer to give the boulder time to start falling before we check its speed
    private float settleTimer = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
    }

    void Update()
    {
        // 1. The Trap Logic (Waiting to be triggered)
        if (!isTriggered && player != null)
        {
            float distance = Vector2.Distance(transform.position, player.position);
            bool isPlayerOnLeft = player.position.x < transform.position.x;

            if (distance <= triggerDistance && isPlayerOnLeft)
            {
                isTriggered = true;

                // Unfreeze gravity and push left
                rb.bodyType = RigidbodyType2D.Dynamic;
                rb.velocity = new Vector2(-rollSpeed, 0);

                Collider2D playerCol = player.GetComponent<Collider2D>();
                Collider2D[] boulderCols = GetComponents<Collider2D>();

                if (playerCol != null)
                {
                    foreach (Collider2D bc in boulderCols)
                    {
                        if (bc.isTrigger == false)
                        {
                            Physics2D.IgnoreCollision(playerCol, bc, true);
                        }
                    }
                }
            }
        }
        // 2. The Disarm Logic (After it drops)
        else if (isTriggered)
        {
            settleTimer += Time.deltaTime;

            // Wait 1 second to ensure the boulder has actually started falling/rolling
            if (settleTimer > 1f)
            {
                // Check if the boulder has basically stopped moving
                if (Mathf.Abs(rb.velocity.x) < 0.1f && Mathf.Abs(rb.velocity.y) < 0.1f)
                {
                    Collider2D playerCol = player.GetComponent<Collider2D>();
                    Collider2D[] boulderCols = GetComponents<Collider2D>();

                    foreach (Collider2D bc in boulderCols)
                    {
                        if (bc.isTrigger == true)
                        {
                            // 1. Disarm the trap by turning off the death aura
                            bc.enabled = false;
                        }
                        else if (playerCol != null)
                        {
                            // 2. NEW: Turn the solid physics BACK ON so it becomes a bridge!
                            Physics2D.IgnoreCollision(playerCol, bc, false);
                        }
                    }

                    // Turn this script off so it stops running in the background
                    this.enabled = false;
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, triggerDistance);
    }
}