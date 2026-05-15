using UnityEngine;
using System.Collections;
using Pathfinding;

public class EnemyAI : MonoBehaviour
{
    [Header("Stats")]
    public float maxHealth = 100f;
    public float moveSpeed = 2f;
    public float attackDamage = 10f;
    public float jumpForce = 6f;

    [Header("Detection")]
    public float detectionRange = 6f;
    public float attackRange = 1.2f;
    public float attackCooldown = 1.5f;

    [Header("References")]
    public Transform visual;
    public Animator animator;
    public Transform attackPoint;
    public Transform groundCheck;
    public float attackRadius = 0.5f;
    public LayerMask playerLayer;
    public LayerMask groundLayer;
    public float checkRadius = 0.2f;

    [Header("Pathfinding")]
    public float nextWaypointDistance = 1f;
    public float pathUpdateRate = 0.5f;

    private float currentHealth;
    private float attackTimer;
    private float jumpCooldown;
    private bool isDead;
    private bool isFacingRight = true;
    private Transform player;
    private Rigidbody2D rb;

    private Seeker seeker;
    private Path path;
    private int currentWaypoint = 0;
    private float pathUpdateTimer;

    private Vector2 lastPosition;
    private float stuckTimer;
    private float wallStuckTimer;

    private bool spikeImmune = false;

    void Start()
    {
        currentHealth = maxHealth;

        rb = GetComponent<Rigidbody2D>();
        seeker = GetComponent<Seeker>();

        if (visual == null)
        {
            Transform foundVisual = transform.Find("Visual");
            if (foundVisual != null)
                visual = foundVisual;
        }

        if (animator == null)
        {
            if (visual != null)
                animator = visual.GetComponent<Animator>();

            if (animator == null)
                animator = GetComponentInChildren<Animator>();
        }

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;

        lastPosition = transform.position;
    }

    void Update()
    {
        if (isDead) return;
        if (player == null) return;

        attackTimer -= Time.deltaTime;
        jumpCooldown -= Time.deltaTime;
        pathUpdateTimer -= Time.deltaTime;

        float dist = Vector2.Distance(transform.position, player.position);

        if (dist <= attackRange)
        {
            rb.velocity = new Vector2(0f, rb.velocity.y);
            SetSpeed(0f);

            FacePlayer();

            if (attackTimer <= 0f)
                StartAttack();
        }
        else if (dist <= detectionRange)
        {
            if (pathUpdateTimer <= 0f)
            {
                pathUpdateTimer = pathUpdateRate;

                if (seeker != null)
                    seeker.StartPath(transform.position, player.position, OnPathComplete);
            }

            FollowPath();
        }
        else
        {
            rb.velocity = new Vector2(0f, rb.velocity.y);
            SetSpeed(0f);
        }
    }

    void OnPathComplete(Path p)
    {
        if (!p.error)
        {
            path = p;
            currentWaypoint = 0;
        }
    }

    void FollowPath()
    {
        if (path == null) return;
        if (currentWaypoint >= path.vectorPath.Count) return;

        Vector2 target = path.vectorPath[currentWaypoint];
        Vector2 myPos = transform.position;

        float dirX = Mathf.Sign(target.x - myPos.x);
        float dirY = target.y - myPos.y;

        if (dirX > 0f && !isFacingRight) Flip();
        else if (dirX < 0f && isFacingRight) Flip();

        Vector2 groundCheckPos = groundCheck != null
            ? (Vector2)groundCheck.position
            : (Vector2)transform.position + new Vector2(0f, -1.1f);

        bool isGrounded = Physics2D.OverlapCircle(groundCheckPos, checkRadius, groundLayer);

        if (dirY > 0.5f && isGrounded && jumpCooldown <= 0f)
        {
            Jump();
        }

        rb.velocity = new Vector2(dirX * moveSpeed, rb.velocity.y);
        SetSpeed(Mathf.Abs(rb.velocity.x));

        float dist = Vector2.Distance(myPos, target);

        if (dist < nextWaypointDistance)
            currentWaypoint++;

        bool movingButStuck = Mathf.Abs(rb.velocity.x) < 0.1f && Mathf.Abs(dirX) > 0.5f;

        if (movingButStuck)
        {
            wallStuckTimer += Time.deltaTime;

            if (wallStuckTimer > 0.2f)
            {
                rb.velocity = new Vector2(-dirX * moveSpeed * 3f, rb.velocity.y);

                if (isGrounded && jumpCooldown <= 0f)
                    Jump();

                path = null;
                pathUpdateTimer = 0f;
                wallStuckTimer = 0f;
            }
        }
        else
        {
            wallStuckTimer = 0f;
        }

        float moved = Vector2.Distance((Vector2)transform.position, lastPosition);

        if (moved < 0.02f)
        {
            stuckTimer += Time.deltaTime;

            if (stuckTimer > 1.5f && isGrounded && jumpCooldown <= 0f)
            {
                Jump();
                stuckTimer = 0f;
            }
        }
        else
        {
            stuckTimer = 0f;
        }

        lastPosition = transform.position;
    }

    void Jump()
    {
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        jumpCooldown = 2.5f;
    }

    void StartAttack()
    {
        attackTimer = attackCooldown;
        rb.velocity = new Vector2(0f, rb.velocity.y);

        SetBool("IsAttacking", true);

        CancelInvoke(nameof(DealDamage));
        CancelInvoke(nameof(ResetAttack));

        Invoke(nameof(DealDamage), 0.3f);
        Invoke(nameof(ResetAttack), 0.6f);
    }

    void DealDamage()
    {
        if (isDead) return;
        if (attackPoint == null) return;

        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint.position, attackRadius, playerLayer);

        foreach (Collider2D hit in hits)
        {
            PlayerHealth ph = hit.GetComponentInParent<PlayerHealth>();

            if (ph != null)
            {
                ph.TakeDamage(Mathf.RoundToInt(attackDamage));
                return;
            }
        }
    }

    void ResetAttack()
    {
        SetBool("IsAttacking", false);
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        if (currentHealth <= 0f)
        {
            Die();
        }
        else
        {
            StopCoroutine(nameof(HitRoutine));
            StartCoroutine(HitRoutine());
        }
    }

    IEnumerator HitRoutine()
    {
        SetBool("IsHit", true);
        yield return new WaitForSeconds(0.15f);
        SetBool("IsHit", false);
    }

    void Die()
    {
        if (isDead) return;

        isDead = true;

        rb.velocity = Vector2.zero;
        rb.simulated = false;

        SetBool("IsDead", true);
        SetSpeed(0f);
        SetBool("IsAttacking", false);

        Destroy(gameObject, 2f);
    }

    void FacePlayer()
    {
        if (player == null) return;

        if (player.position.x > transform.position.x && !isFacingRight)
            Flip();
        else if (player.position.x < transform.position.x && isFacingRight)
            Flip();
    }

    void Flip()
    {
        isFacingRight = !isFacingRight;

        Vector3 s = transform.localScale;
        s.x *= -1f;
        transform.localScale = s;
    }

    void SetSpeed(float value)
    {
        if (animator != null)
            animator.SetFloat("Speed", value);
    }

    void SetBool(string parameterName, bool value)
    {
        if (animator != null)
            animator.SetBool(parameterName, value);
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (isDead) return;
        if (spikeImmune) return;

        if (col.CompareTag("Spike"))
        {
            StartCoroutine(SpikeKnockback());
        }
    }

    IEnumerator SpikeKnockback()
    {
        spikeImmune = true;

        float bounceDir = isFacingRight ? -1f : 1f;

        rb.velocity = new Vector2(bounceDir * moveSpeed * 3f, jumpForce * 1.5f);
        jumpCooldown = 2f;

        yield return new WaitForSeconds(2f);

        spikeImmune = false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        if (attackPoint != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
        }

        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, checkRadius);
        }
    }
}