using UnityEngine;

public class DamageZone : MonoBehaviour
{
    [Header("Damage")]
    public int damage = 25;
    public bool instantKill = false;

    [Header("Settings")]
    public bool damageOverTime = false;
    public float damageInterval = 1f;

    private float damageTimer;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        PlayerHealth playerHealth = other.GetComponentInParent<PlayerHealth>();

        if (playerHealth == null)
            return;

        if (instantKill)
        {
            playerHealth.TakeDamage(9999);
        }
        else if (!damageOverTime)
        {
            playerHealth.TakeDamage(damage);
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!damageOverTime)
            return;

        if (!other.CompareTag("Player"))
            return;

        damageTimer -= Time.deltaTime;

        if (damageTimer > 0f)
            return;

        PlayerHealth playerHealth = other.GetComponentInParent<PlayerHealth>();

        if (playerHealth == null)
            return;

        if (instantKill)
            playerHealth.TakeDamage(9999);
        else
            playerHealth.TakeDamage(damage);

        damageTimer = damageInterval;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        damageTimer = 0f;
    }
}