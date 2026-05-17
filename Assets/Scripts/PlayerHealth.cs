using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 100;
    public int currentHealth;

    [Header("Lives")]
    public int maxLives = 3;
    public int currentLives;

    [Header("UI")]
    public HealthBarUI healthBarUI;
    public HeartUI heartUI;

    [Header("Damage Flash")]
    public SpriteRenderer playerSprite;
    public Color damageFlashColor = Color.white;
    public float damageFlashDuration = 0.12f;

    [Header("Respawn")]
    public Transform defaultRespawnPoint;
    public float respawnDelay = 1.5f;

    [Header("Invincibility")]
    public float invincibleDuration = 1f;

    [Header("References")]
    public Rigidbody2D rb;
    public Animator animator;
    public PlayerController playerController;

    private bool isDead;
    private bool isInvincible;
    private Color originalPlayerColor;
    private Transform currentRespawnPoint;

    void Awake()
    {
        GetReferences();
    }

    void Start()
    {
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

        UpdateUI();
    }

    public void TakeDamage(int damage)
    {
        if (isDead || isInvincible)
            return;

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        UpdateUI();

        if (playerSprite != null)
            StartCoroutine(DamageFlashRoutine());

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            if (HasAnimatorParameter("Hit"))
                animator.SetTrigger("Hit");

            StartCoroutine(InvincibilityRoutine());
        }

        // Find the Ascent Manager in the scene and abort the flight
        AscentManager flightManager = FindObjectOfType<AscentManager>();
        if (flightManager != null)
        {
            flightManager.AbortFlight();
        }
    }

    public void Heal(int amount)
    {
        if (isDead)
            return;

        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        UpdateUI();
    }

    void Die()
    {
        if (isDead)
            return;

        isDead = true;
        isInvincible = true;

        StopAllCoroutines();

        if (playerController != null)
            playerController.enabled = false;

        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        if (HasAnimatorParameter("Die"))
            animator.SetTrigger("Die");

        currentLives--;

        if (currentLives <= 0)
        {
            currentLives = 0;
            UpdateUI();

            Invoke(nameof(RestartLevel), respawnDelay);
            return;
        }

        UpdateUI();
        Invoke(nameof(Respawn), respawnDelay);
    }

    void Respawn()
    {
        if (currentRespawnPoint != null)
        {
            transform.position = currentRespawnPoint.position;
        }
        else if (defaultRespawnPoint != null)
        {
            transform.position = defaultRespawnPoint.position;
        }

        currentHealth = maxHealth;
        isDead = false;
        isInvincible = false;

        if (animator != null)
        {
            animator.Rebind();
            animator.Update(0f);
        }

        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        if (playerController != null)
            playerController.enabled = true;

        UpdateUI();

        StartCoroutine(InvincibilityRoutine());
    }

    void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    IEnumerator InvincibilityRoutine()
    {
        isInvincible = true;
        yield return new WaitForSeconds(invincibleDuration);
        isInvincible = false;
    }

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
    }

    public bool IsDead()
    {
        return isDead;
    }

    public void SetRespawnPoint(Transform newRespawnPoint)
    {
        currentRespawnPoint = newRespawnPoint;
    }
}