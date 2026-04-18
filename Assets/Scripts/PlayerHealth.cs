using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 100;
    public int currentHealth;

    [Header("Lives Settings")]
    public int maxLives = 3;
    public int currentLives;

    [Header("Respawn Settings")]
    public Transform respawnPoint;
    public float respawnDelay = 1f;

    [Header("Invincibility Settings")]
    public float invincibleDuration = 1f;

    [Header("References")]
    public Rigidbody2D rb;
    public Animator animator;

    private bool isDead = false;
    private bool isInvincible = false;

    void Start()
    {
        currentHealth = maxHealth;
        currentLives = maxLives;
        UpdateUI();
    }

    public void TakeDamage(int damage)
    {
        if (isDead || isInvincible)
            return;

        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
        else
        {
            StartCoroutine(InvincibilityCoroutine());
        }

        UpdateUI();
    }

    public void Heal(int amount)
    {
        if (isDead)
            return;

        currentHealth += amount;

        if (currentHealth > maxHealth)
            currentHealth = maxHealth;

        UpdateUI();
    }

    void Die()
    {
        isDead = true;

        if (animator != null)
        {
            animator.SetTrigger("Die");
        }

        if (rb != null)
        {
            rb.velocity = Vector2.zero;
        }

        if (currentLives > 0)
        {
            currentLives--;
            Invoke(nameof(Respawn), respawnDelay);
        }
        else
        {
            Invoke(nameof(RestartLevel), respawnDelay);
        }

        UpdateUI();
    }

    void Respawn()
    {
        if (respawnPoint != null)
        {
            transform.position = respawnPoint.position;
        }

        currentHealth = maxHealth;
        isDead = false;

        if (rb != null)
        {
            rb.velocity = Vector2.zero;
        }

        StartCoroutine(InvincibilityCoroutine());
        UpdateUI();
    }

    void RestartLevel()
    {
        currentLives = maxLives;
        currentHealth = maxHealth;

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    IEnumerator InvincibilityCoroutine()
    {
        isInvincible = true;

        yield return new WaitForSeconds(invincibleDuration);

        isInvincible = false;
    }

    void UpdateUI()
    {
        Debug.Log("Health: " + currentHealth + " / " + maxHealth);
        Debug.Log("Lives: " + currentLives + " / " + maxLives);
    }

    public bool IsDead()
    {
        return isDead;
    }

    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    public int GetCurrentLives()
    {
        return currentLives;
    }
}