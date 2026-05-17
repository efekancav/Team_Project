using UnityEngine;
using System.Collections;

public class AscentManager : MonoBehaviour
{
    [Header("Phase 1: Takeoff Settings")]
    public float takeoffWaitTime = 2f;

    [Header("Phase 2: Flight Settings")]
    public float upwardSpeed = 5f;
    public float dodgeSpeed = 6f;

    [Header("Phase 3: Landing Settings")]
    public Transform landingTarget;
    public float landingSmoothTime = 2f;

    [Header("References")]
    public GameObject player;
    public PlayerController playerControllerScript;

    [HideInInspector] public bool isAscending = false;
    private bool isLanding = false;
    private float landingTimer = 0f;
    private Vector3 landingStartPos;

    private Rigidbody2D playerRb;
    private Animator playerAnim;

    void Start()
    {
        playerRb = player.GetComponent<Rigidbody2D>();
        playerAnim = player.GetComponentInChildren<Animator>();
    }

    public void StartFlight()
    {
        if (isAscending || isLanding) return;
        StartCoroutine(TakeoffRoutine());
    }

    private IEnumerator TakeoffRoutine()
    {
        // 1. Disable normal movement
        if (playerControllerScript != null)
            playerControllerScript.enabled = false;

        // 2. Stop falling
        playerRb.gravityScale = 0f;
        playerRb.velocity = Vector2.zero;

        // 3. PHYSICS FIX: Keep Rigidbody awake so DamageZones (boulders) don't pass through her
        playerRb.sleepMode = RigidbodySleepMode2D.NeverSleep;
        playerRb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        // 4. Reset walking animation and force Idle
        if (playerAnim != null)
        {
            playerAnim.SetFloat("Speed", 0f);
            playerAnim.Play("player_Idle");
        }

        // 5. TRIGGER SPRITE SHAKE
        CameraShake spriteShake = player.GetComponentInChildren<CameraShake>();
        if (spriteShake != null)
        {
            spriteShake.Shake(takeoffWaitTime, 0.2f);
        }

        // 6. Wait for the countdown
        yield return new WaitForSeconds(takeoffWaitTime);

        // 7. Blast off
        isAscending = true;
    }

    public void StartLanding()
    {
        if (!isAscending) return;

        isAscending = false;
        isLanding = true;
        landingTimer = 0f;

        landingStartPos = player.transform.position;
        playerRb.velocity = Vector2.zero;
    }

    void Update()
    {
        if (isAscending)
        {
            // Move left/right and constantly fly up
            float horizontalInput = Input.GetAxisRaw("Horizontal");
            playerRb.velocity = new Vector2(horizontalInput * dodgeSpeed, upwardSpeed);

            // Force the single flat climbing frame
            if (playerAnim != null)
            {
                playerAnim.Play("Player_Climbing", 0, 0.25f);
            }
        }
        else if (isLanding)
        {
            landingTimer += Time.deltaTime;
            float percentComplete = landingTimer / landingSmoothTime;

            // Smoothly glide to the landing platform
            player.transform.position = Vector3.Lerp(landingStartPos, landingTarget.position, percentComplete);

            // Return to standing pose
            if (playerAnim != null)
            {
                playerAnim.Play("player_Idle");
            }

            // Once the glide is 100% finished
            if (percentComplete >= 1f)
            {
                isLanding = false;

                // Return gravity
                playerRb.gravityScale = 3f;

                // RESTORE NORMAL PHYSICS VALUES so the game runs normally again
                playerRb.sleepMode = RigidbodySleepMode2D.StartAwake;
                playerRb.collisionDetectionMode = CollisionDetectionMode2D.Discrete;

                // Give control back to the player
                if (playerControllerScript != null)
                    playerControllerScript.enabled = true;
            }
        }
    }

    // Call this from your health script when she dies!
    // Call this from your health script when she dies!
    public void AbortFlight()
    {
        isAscending = false;
        isLanding = false;
        StopAllCoroutines(); // Stops the shake/takeoff if she dies during countdown

        if (playerRb != null)
        {
            playerRb.gravityScale = 3f; // Let her body fall
            playerRb.sleepMode = RigidbodySleepMode2D.StartAwake;
            playerRb.collisionDetectionMode = CollisionDetectionMode2D.Discrete;
        }

        // --- THE MISSING FIX: Turn her movement script back on! ---
        if (playerControllerScript != null)
        {
            playerControllerScript.enabled = true;
        }
    }
}