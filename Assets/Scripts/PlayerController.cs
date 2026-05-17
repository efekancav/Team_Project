using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 4f;
    public float runSpeed = 7f;
    public float crouchSpeed = 2f;
    public float rollSpeed = 8f;
    public float rollDuration = 0.4f;

    [Header("Jump Settings")]
    public float jumpHeightInTiles = 2.5f;
    public float jumpForwardDistanceInTiles = 5f;
    public float tileSize = 1f;

    [Header("Wall Slide")]
    public float wallSlideSpeed = 0f;

    [Header("Climb")]
    public float climbSpeed = 3f;

    [Header("Player Attack")]
    public Transform attackPoint;
    public float attackRadius = 0.5f;
    public float attackDamage = 25f;
    public LayerMask enemyLayer;
    public float attackHitDelay = 0.25f;

    [Header("Checks")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public Transform wallCheck;
    public float wallCheckRadius = 0.25f;
    public Transform headCheck;
    public float headCheckRadius = 0.2f;
    public LayerMask surfaceLayer;

    [Header("References")]
    public Rigidbody2D rb;
    public Animator animator;
    public CapsuleCollider2D playerCollider;

    [Header("Crouch Collider")]
    public Vector2 crouchColliderSize = new Vector2(0.55f, 0.35f);
    public Vector2 crouchColliderOffset = new Vector2(0.108f, -0.45f);

    [Header("Roll Collider")]
    public Vector2 rollColliderSize = new Vector2(0.79f, 0.85f);
    public Vector2 rollColliderOffset = new Vector2(0.108f, -0.45f);

    private Vector2 standColliderSize;
    private Vector2 standColliderOffset;

    private float moveInput;
    private float verticalInput;

    private bool isGrounded;
    private bool wasGrounded;
    private bool isRunning;
    private bool isCrouching;
    private bool isRolling;
    private bool isWallSliding;
    private bool isShielding;
    private bool isDead;

    private bool isOnLadder;
    private bool isClimbing;

    private bool isInDropZone;
    private float dropMaxFallSpeed;

    private float rollTimer;
    private int facingDirection = 1;

    private float jumpVelocity;
    private float jumpHorizontalVelocity;
    private float originalGravityScale;

    void Start()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (playerCollider == null)
            playerCollider = GetComponent<CapsuleCollider2D>();

        if (playerCollider != null)
        {
            standColliderSize = playerCollider.size;
            standColliderOffset = playerCollider.offset;
        }

        originalGravityScale = rb.gravityScale;

        CalculateJumpValues();
    }

    void Update()
    {
        if (isDead) return;

        CheckEnvironment();
        HandleLanding();
        HandleInput();
        HandleJump();
        HandleRoll();
        HandleAttack();
        HandleShield();
        HandleDeathDebug();
        HandleFacing();
        UpdateCollider();
        UpdateAnimator();
    }

    void FixedUpdate()
    {
        if (isDead) return;

        if (isRolling)
        {
            rb.velocity = new Vector2(facingDirection * rollSpeed, rb.velocity.y);
            return;
        }

        HandleClimb();
        HandleMovement();
        HandleWallSlide();
        HandleDropZone();
    }

    void CalculateJumpValues()
    {
        float jumpHeight = jumpHeightInTiles * tileSize;
        float jumpDistance = jumpForwardDistanceInTiles * tileSize;
        float gravity = Mathf.Abs(Physics2D.gravity.y * rb.gravityScale);

        jumpVelocity = Mathf.Sqrt(2f * gravity * jumpHeight);

        float timeToApex = jumpVelocity / gravity;
        float totalAirTime = timeToApex * 2f;

        jumpHorizontalVelocity = jumpDistance / totalAirTime;
    }

    void CheckEnvironment()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, surfaceLayer);
        bool touchingWall = Physics2D.OverlapCircle(wallCheck.position, wallCheckRadius, surfaceLayer);

        isWallSliding = !isGrounded && touchingWall && !isClimbing;
    }

    void HandleLanding()
    {
        if (!wasGrounded && isGrounded)
        {
            animator.SetTrigger("Land");
        }

        wasGrounded = isGrounded;
    }

    void HandleInput()
    {
        moveInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        bool crouchPressed = Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow);

        if (crouchPressed && isGrounded && !isRolling && !isShielding && !isClimbing)
        {
            isCrouching = true;
        }
        else if (!crouchPressed)
        {
            if (CanStandUp())
            {
                isCrouching = false;
            }
        }

        isRunning = Input.GetKey(KeyCode.LeftShift) &&
                    Mathf.Abs(moveInput) > 0.1f &&
                    !isCrouching &&
                    !isRolling &&
                    !isClimbing;
    }

    bool CanStandUp()
    {
        if (headCheck == null)
            return true;

        return !Physics2D.OverlapCircle(headCheck.position, headCheckRadius, surfaceLayer);
    }

    void HandleClimb()
    {
        if (!isOnLadder)
        {
            isClimbing = false;

            if (!isInDropZone)
                rb.gravityScale = originalGravityScale;

            return;
        }

        if (Mathf.Abs(verticalInput) > 0.1f)
        {
            isClimbing = true;
        }

        if (isClimbing)
        {
            rb.gravityScale = 0f;
        }
    }

    void HandleMovement()
    {
        if (isClimbing)
        {
            rb.velocity = new Vector2(moveInput * walkSpeed, verticalInput * climbSpeed);

            if (Mathf.Abs(verticalInput) < 0.1f && Mathf.Abs(moveInput) < 0.1f)
            {
                rb.velocity = Vector2.zero;
            }

            return;
        }

        if (isShielding)
        {
            rb.velocity = new Vector2(0f, rb.velocity.y);
            return;
        }

        if (isWallSliding)
        {
            rb.velocity = new Vector2(0f, rb.velocity.y);
            return;
        }

        float currentSpeed = walkSpeed;

        if (isCrouching)
            currentSpeed = crouchSpeed;
        else if (isRunning)
            currentSpeed = runSpeed;

        rb.velocity = new Vector2(moveInput * currentSpeed, rb.velocity.y);
    }

    void HandleJump()
    {
        bool jumpPressed = Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W);

        if (!jumpPressed || isRolling || isShielding || isCrouching || isClimbing)
            return;

        if (isGrounded)
        {
            float horizontal = 0f;

            if (Mathf.Abs(moveInput) > 0.1f)
                horizontal = Mathf.Sign(moveInput) * jumpHorizontalVelocity;

            rb.velocity = new Vector2(horizontal, jumpVelocity);

            SFXManager.Instance.PlaySFX(
                SFXManager.Instance.jump //jumpSound
            );

            animator.SetTrigger("Jump");
            return;
        }

        if (isWallSliding)
        {
            int jumpDirection = -facingDirection;
            rb.velocity = new Vector2(jumpDirection * jumpHorizontalVelocity, jumpVelocity);
            isWallSliding = false;

            SFXManager.Instance.PlaySFX(
                SFXManager.Instance.jump //jumpSound
            );

            animator.SetTrigger("Jump");
        }
    }

    void HandleRoll()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl) &&
            isGrounded &&
            !isRolling &&
            !isCrouching &&
            !isShielding &&
            !isClimbing)
        {
            isRolling = true;
            rollTimer = rollDuration;
            animator.SetTrigger("Roll");
        }

        if (isRolling)
        {
            rollTimer -= Time.deltaTime;

            if (rollTimer <= 0f)
            {
                isRolling = false;
            }
        }
    }

    void HandleAttack()
    {
        bool attackPressed = Input.GetKeyDown(KeyCode.J) || Input.GetMouseButtonDown(0);

        if (attackPressed &&
            !isRolling &&
            !isShielding &&
            !isDead &&
            !isCrouching &&
            !isClimbing)
        {
            int randomAttack = Random.Range(1, 4);
            animator.SetInteger("AttackIndex", randomAttack);

            SFXManager.Instance.PlaySFX(
                SFXManager.Instance.hit
            );

            animator.SetTrigger("Attack");

            Invoke(nameof(DealAttackDamage), attackHitDelay);
        }
    }

    void DealAttackDamage()
    {
        if (attackPoint == null)
            return;

        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint.position, attackRadius, enemyLayer);

        foreach (Collider2D hit in hits)
        {
            EnemyAI enemy = hit.GetComponentInParent<EnemyAI>();

            if (enemy != null)
            {
                enemy.TakeDamage(attackDamage);
            }
        }
    }

    void HandleShield()
    {
        isShielding = (Input.GetKey(KeyCode.K) || Input.GetMouseButton(1)) &&
                      !isRolling &&
                      !isDead &&
                      !isCrouching &&
                      !isClimbing;

        if (isShielding)
        {
            rb.velocity = new Vector2(0f, rb.velocity.y);
        }
    }

    void HandleWallSlide()
    {
        if (isWallSliding)
        {
            if (rb.velocity.y < -wallSlideSpeed)
            {
                rb.velocity = new Vector2(0f, -wallSlideSpeed);
            }
        }
    }

    void HandleDropZone()
    {
        if (!isInDropZone)
            return;

        if (rb.velocity.y < dropMaxFallSpeed)
        {
            rb.velocity = new Vector2(rb.velocity.x, dropMaxFallSpeed);
        }
    }

    public void EnterDropZone(float gravity, float maxFall)
    {
        isInDropZone = true;
        dropMaxFallSpeed = maxFall;

        if (!isClimbing)
            rb.gravityScale = gravity;
    }

    public void ExitDropZone()
    {
        isInDropZone = false;

        if (!isClimbing)
            rb.gravityScale = originalGravityScale;
    }

    void HandleDeathDebug()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            Die();
        }
    }

    void Die()
    {
        isDead = true;
        isShielding = false;
        isRolling = false;
        isClimbing = false;

        rb.velocity = Vector2.zero;
        rb.gravityScale = originalGravityScale;

        animator.SetTrigger("Die");
    }

    void HandleFacing()
    {
        if (moveInput > 0.1f)
        {
            facingDirection = 1;
            transform.localScale = new Vector3(1, 1, 1);
        }
        else if (moveInput < -0.1f)
        {
            facingDirection = -1;
            transform.localScale = new Vector3(-1, 1, 1);
        }
    }

    void UpdateCollider()
    {
        if (playerCollider == null)
            return;

        if (isRolling)
        {
            playerCollider.size = rollColliderSize;
            playerCollider.offset = rollColliderOffset;
            return;
        }

        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            playerCollider.size = crouchColliderSize;
            playerCollider.offset = crouchColliderOffset;
            return;
        }

        playerCollider.size = standColliderSize;
        playerCollider.offset = standColliderOffset;
    }

    void UpdateAnimator()
    {
        animator.SetFloat("Speed", Mathf.Abs(moveInput));
        animator.SetFloat("YVelocity", rb.velocity.y);
        animator.SetFloat("ClimbSpeed", Mathf.Abs(verticalInput));

        animator.SetBool("IsGrounded", isGrounded);
        animator.SetBool("IsRunning", isRunning);
        animator.SetBool("IsCrouching", isCrouching);
        animator.SetBool("IsWallSliding", isWallSliding);
        animator.SetBool("IsShielding", isShielding);
        animator.SetBool("IsClimbing", isClimbing);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Ladder"))
        {
            isOnLadder = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Ladder"))
        {
            isOnLadder = false;
            isClimbing = false;

            if (!isInDropZone)
                rb.gravityScale = originalGravityScale;
        }
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }

        if (wallCheck != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(wallCheck.position, wallCheckRadius);
        }

        if (headCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(headCheck.position, headCheckRadius);
        }

        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
        }
    }
}