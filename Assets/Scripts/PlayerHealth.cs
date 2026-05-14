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

    void Awake()
    {
        GetReferences();
    }

    void Start()
    {
<<<<<<< Updated upstream
        currentHealth = maxHealth;
        currentLives = maxLives;
=======
        currentRespawnPoint = defaultRespawnPoint;
        StartCoroutine(InitializeHealthRoutine());
    }

    IEnumerator InitializeHealthRoutine()
    {
        yield return null;

        FindUIReferences();
        ResetHealthAndLives();
    }

    void GetReferences()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (playerController == null)
            playerController = GetComponent<PlayerController>();

        if (playerSprite == null)
            playerSprite = GetComponentInChildren<SpriteRenderer>();

        if (playerSprite != null)
            originalPlayerColor = playerSprite.color;
    }

    void FindUIReferences()
    {
        if (healthBarUI == null)
            healthBarUI = FindObjectOfType<HealthBarUI>();

        if (heartUI == null)
            heartUI = FindObjectOfType<HeartUI>();
    }

    public void ResetHealthAndLives()
    {
        currentHealth = maxHealth;
        currentLives = maxLives;

        isDead = false;
        isInvincible = false;

        if (playerController != null)
            playerController.enabled = true;

        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

>>>>>>> Stashed changes
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
<<<<<<< Updated upstream
=======
            currentLives = 0;
            UpdateUI();

>>>>>>> Stashed changes
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
        isInvincible = false;

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

<<<<<<< Updated upstream
    void UpdateUI()
    {
        Debug.Log("Health: " + currentHealth + " / " + maxHealth);
        Debug.Log("Lives: " + currentLives + " / " + maxLives);
=======
    IEnumerator DamageFlashRoutine()
    {
        if (playerSprite == null)
            yield break;

        playerSprite.color = damageFlashColor;

        yield return new WaitForSeconds(damageFlashDuration);

        playerSprite.color = originalPlayerColor;
    }

    void UpdateUI()
    {
        if (healthBarUI == null || heartUI == null)
            FindUIReferences();

        if (healthBarUI != null)
            healthBarUI.SetHealth(currentHealth);

        if (heartUI != null)
            heartUI.UpdateHearts(currentLives);
    }

    bool HasAnimatorParameter(string parameterName)
    {
        if (animator == null)
            return false;

        foreach (AnimatorControllerParameter parameter in animator.parameters)
        {
            if (parameter.name == parameterName)
                return true;
        }

        return false;
>>>>>>> Stashed changes
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