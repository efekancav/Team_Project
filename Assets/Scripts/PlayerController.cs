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
    public float wallSlideSpeed = 0f; // 0 = duvarda neredeyse yapışık kalır

    [Header("Checks")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;

    public Transform wallCheck;
    public float wallCheckRadius = 0.25f;

    public LayerMask surfaceLayer; // Ground + Wall tek layer

    [Header("References")]
    public Rigidbody2D rb;
    public Animator animator;

    private float moveInput;
    private bool isGrounded;
    private bool wasGrounded;
    private bool isRunning;
    private bool isCrouching;
    private bool isRolling;
    private bool isWallSliding;
    private bool isShielding;
    private bool isDead;

    private float rollTimer;
    private int facingDirection = 1; // 1 = sağ, -1 = sol

    // Hesaplanan jump değerleri
    private float jumpVelocity;
    private float jumpHorizontalVelocity;

    void Start()
    {
        CalculateJumpValues();
    }

    void Update()
    {
        if (isDead)
            return;

        CheckEnvironment();
        HandleLanding();
        HandleInput();
        HandleJump();
        HandleRoll();
        HandleAttack();
        HandleShield();
        HandleDeathDebug();
        HandleFacing();
        UpdateAnimator();
    }

    void FixedUpdate()
    {
        if (isDead)
            return;

        if (isRolling)
        {
            rb.velocity = new Vector2(facingDirection * rollSpeed, rb.velocity.y);
            return;
        }

        HandleMovement();
        HandleWallSlide();
    }

    void CalculateJumpValues()
    {
        float jumpHeight = jumpHeightInTiles * tileSize;
        float jumpDistance = jumpForwardDistanceInTiles * tileSize;

        float gravity = Mathf.Abs(Physics2D.gravity.y * rb.gravityScale);

        // 2.5 tile yukarı çıkmak için gereken ilk dikey hız
        jumpVelocity = Mathf.Sqrt(2f * gravity * jumpHeight);

        // Aynı yükseklikte inen bir sıçramada toplam havada kalış süresi
        float timeToApex = jumpVelocity / gravity;
        float totalAirTime = timeToApex * 2f;

        // 5 tile ileri gitmek için gereken yatay hız
        jumpHorizontalVelocity = jumpDistance / totalAirTime;
    }

    void CheckEnvironment()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, surfaceLayer);

        bool touchingWall = Physics2D.OverlapCircle(wallCheck.position, wallCheckRadius, surfaceLayer);

        // Yerde değilse ve yana temas varsa wall slide
        isWallSliding = !isGrounded && touchingWall;
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

        isRunning = Input.GetKey(KeyCode.LeftShift) && Mathf.Abs(moveInput) > 0.1f && !isCrouching;
        isCrouching = Input.GetKey(KeyCode.S) && isGrounded && !isRolling && !isShielding;
    }

    void HandleMovement()
    {
        if (isShielding)
        {
            rb.velocity = new Vector2(0f, rb.velocity.y);
            return;
        }

        // Duvarda yapışık kalırken normal movement uygulama
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

        if (!jumpPressed || isRolling || isShielding)
            return;

        // Ground jump
        if (isGrounded)
        {
            float horizontal = 0f;

            if (Mathf.Abs(moveInput) > 0.1f)
                horizontal = Mathf.Sign(moveInput) * jumpHorizontalVelocity;

            rb.velocity = new Vector2(horizontal, jumpVelocity);
            animator.SetTrigger("Jump");
            return;
        }

        // Infinite wall jump
        if (isWallSliding)
        {
            int jumpDirection = -facingDirection; // duvardan ters yöne zıpla
            rb.velocity = new Vector2(jumpDirection * jumpHorizontalVelocity, jumpVelocity);
            isWallSliding = false;
            animator.SetTrigger("Jump");
        }
    }

    void HandleRoll()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl) && isGrounded && !isRolling && !isCrouching && !isShielding)
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

        if (attackPressed && !isRolling && !isShielding && !isDead)
        {
            int randomAttack = Random.Range(1, 4);
            animator.SetInteger("AttackIndex", randomAttack);
            animator.SetTrigger("Attack");
        }
    }

    void HandleShield()
    {
        isShielding = (Input.GetKey(KeyCode.K) || Input.GetMouseButton(1)) && !isRolling && !isDead;

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
        rb.velocity = Vector2.zero;
        animator.SetTrigger("Die");
    }

    void HandleFacing()
    {
        // Duvarda da karakter yön değiştirebilsin
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

    void UpdateAnimator()
    {
        animator.SetFloat("Speed", Mathf.Abs(moveInput));
        animator.SetFloat("YVelocity", rb.velocity.y);

        animator.SetBool("IsGrounded", isGrounded);
        animator.SetBool("IsRunning", isRunning);
        animator.SetBool("IsCrouching", isCrouching);
        animator.SetBool("IsWallSliding", isWallSliding);
        animator.SetBool("IsShielding", isShielding);
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
    }
}