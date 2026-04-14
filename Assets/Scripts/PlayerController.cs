using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 4f;
    public float runSpeed = 7f;
    public float crouchSpeed = 2f;
    public float jumpForce = 11f;
    public float wallSlideSpeed = 2f;
    public float rollSpeed = 8f;
    public float rollDuration = 0.4f;

    [Header("Checks")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    public Transform wallCheck;
    public float wallCheckRadius = 0.2f;
    public LayerMask wallLayer;

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

    private bool canDoubleJump;
    private float rollTimer;
    private int facingDirection = 1; // 1 = right, -1 = left

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

    void CheckEnvironment()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        bool touchingWall = Physics2D.OverlapCircle(wallCheck.position, wallCheckRadius, wallLayer);

        isWallSliding = !isGrounded && touchingWall && rb.velocity.y < 0f && Mathf.Abs(moveInput) > 0.1f;

        if (isGrounded)
        {
            canDoubleJump = true;
        }
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
        isCrouching = Input.GetKey(KeyCode.S) && isGrounded && !isRolling;
    }

    void HandleMovement()
    {
        if (isShielding)
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

        if (isGrounded)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            animator.SetTrigger("Jump");
        }
        else if (canDoubleJump && !isWallSliding)
        {
            canDoubleJump = false;
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            animator.SetTrigger("Jump");
        }
        else if (isWallSliding)
        {
            rb.velocity = new Vector2(-facingDirection * walkSpeed, jumpForce);
            animator.SetTrigger("Jump");
            isWallSliding = false;
            canDoubleJump = true;
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
            int randomAttack = Random.Range(1, 4); // 1, 2, or 3
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
                rb.velocity = new Vector2(rb.velocity.x, -wallSlideSpeed);
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