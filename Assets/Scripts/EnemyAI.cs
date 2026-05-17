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
    [Tooltip("Seconds to wait after an attack ends (animation reset) before the next attack is allowed.")]
    public float attackCooldown = 2f;
    [Tooltip("Extra radius padding so players squeezed between pivot and AttackPoint are still detected.")]
    public float closeCombatPadding = 0.15f;
    [Tooltip("Melee uses |dx| and |dy| vs full 2D distance so different pivot heights still count as in range.")]
    public float meleeVerticalTolerance = 1.85f;
    [Tooltip("Extra horizontal reach beyond Attack Range for entering melee (hugging / wide colliders).")]
    public float meleeHorizontalExtra = 0.35f;
    [Tooltip("Hysteresis: after melee engages, allow this much extra |dx| before exiting melee (reduces chase/attack flicker).")]
    public float meleeReleaseHorizontalExtra = 0.45f;
    [Tooltip("Hysteresis: extra |dy| allowed before exiting melee.")]
    public float meleeReleaseVerticalExtra = 0.55f;
    [Tooltip("Far away, enemy won't aggro through walls. Close range always chases (better platformer feel).")]
    public bool requireLineOfSight = true;
    [Tooltip("Within this fraction of Detection Range, always chase even without LOS (e.g. 0.55 = 55%).")]
    [Range(0.25f, 1f)]
    public float alwaysChaseWithinDetectionFraction = 0.55f;
    [Tooltip("After seeing the player, keep chasing this long when LOS is briefly blocked.")]
    public float chaseMemoryAfterLostSight = 2.5f;
    [Tooltip("LOS ray origin height above pivot.")]
    public float lineOfSightOriginYOffset = 0.45f;

    [Header("Knockback (when hit)")]
    public float knockbackHorizontalForce = 4f;
    public float knockbackVerticalForce = 2f;
    [Tooltip("Seconds after a hit where chase/melee won't overwrite knockback velocity.")]
    public float knockbackStunDuration = 0.35f;
    [Tooltip("Show floating damage numbers when this enemy takes damage.")]
    public bool showDamageNumbers = true;

    [Header("Patrol")]
    public bool enablePatrol = true;
    public Transform[] patrolPoints;
    public float patrolMoveSpeedMultiplier = 0.65f;
    public float patrolPointReachedDistance = 0.4f;
    public float patrolWaitTime = 1.25f;

    [Header("References")]
    public Transform visual;
    public Animator animator;
    public Transform attackPoint;
    public Transform groundCheck;
    public Transform wallCheck;
    public Transform edgeCheck;
    public float attackRadius = 0.5f;
    public LayerMask playerLayer;
    public LayerMask groundLayer;
    public float checkRadius = 0.2f;

    [Header("Pathfinding")]
    public float nextWaypointDistance = 1f;
    public float pathUpdateRate = 0.5f;
    [Tooltip("If the player is on unwalkable nodes (e.g. mid-air), still return the closest reachable path.")]
    public bool allowPartialPaths = true;
    [Tooltip("Log path calculation failures (check Grid Graph + Scan in Unity).")]
    public bool logPathFailures = true;
    [Tooltip("Minimum seconds between repeated path failure logs.")]
    public float pathFailLogInterval = 2f;
    [Tooltip("During Play, draw this unit's current path polyline in the Scene view (lime). Seeker gizmos also need Scene view + Play; this is a backup.")]
    public bool drawActivePathInScene = true;

    [Header("Anti-stuck (chase)")]
    [Tooltip("If waypoint and enemy share almost the same X, use player direction instead of Sign(0).")]
    public float waypointSameXEpsilon = 0.04f;
    [Tooltip("If |enemyX - playerX| is below this, treat as 'under player' and allow horizontal wiggle.")]
    public float underPlayerHorizontalDeadZone = 0.14f;
    [Tooltip("Minimum vertical gap before wiggling sideways under the player.")]
    public float underPlayerWiggleMinDy = 0.22f;
    [Tooltip("How fast to alternate left/right when wiggling out from under a ledge.")]
    public float underPlayerWiggleHz = 1.85f;

    [Header("Chase jump")]
    [Tooltip("If the player is at least this much higher (m), jump while grounded to reach ledges.")]
    public float chaseJumpVerticalThreshold = 0.4f;
    [Tooltip("Also jump when the path waypoint is at least this much higher than the enemy (filters A* grid Y noise).")]
    public float pathWaypointJumpDy = 0.65f;
    [Tooltip("Path-only chase jumps still require the player to be at least this much higher (stops flat-floor hops).")]
    public float pathJumpMinPlayerDy = 0.12f;

    [Header("Ledge / wall jump")]
    [Tooltip("Ray down from EdgeCheck: if no ground in this range, treat as platform edge.")]
    public float edgeDropDetectDistance = 0.45f;
    [Tooltip("Ray from WallCheck in move direction for a blocking wall.")]
    public float wallAheadCheckDistance = 0.22f;
    [Tooltip("Minimum player height advantage before ledge/wall auto-jump.")]
    public float ledgeJumpMinPlayerDy = 0.18f;
    [Tooltip("Minimum waypoint height advantage (path) before ledge/wall auto-jump.")]
    public float ledgeJumpMinWaypointDy = 0.12f;
    [Tooltip("When a wall blocks ahead, min height checks are multiplied by this (easier wall hop).")]
    [Range(0.2f, 1f)]
    public float wallJumpRelaxedHeightFactor = 0.55f;
    [Tooltip("EdgeCheck must lie roughly ahead of movement (dot threshold).")]
    public float ledgeForwardDotThreshold = 0.2f;
    [Tooltip("Extra upward speed for jumps triggered at a ledge or wall (multiplies Jump Force for that jump only).")]
    public float ledgeJumpVerticalMultiplier = 1.28f;

    [Header("Spike / hazard")]
    [Tooltip("Seconds before spike knockback can fire again (prevents rapid re-trigger).")]
    public float spikeImmunityDuration = 0.55f;
    [Tooltip("Jump cooldown applied after spike knockback; keep low so the AI can chain-jump out of a pit.")]
    public float spikeKnockbackJumpCooldown = 0.45f;
    [Tooltip("Chase jump cooldown when grounded only on spikes (only if Suppress Chase Jump On Spikes Only is off).")]
    public float chaseJumpCooldownOnSpikeFooting = 0.65f;
    [Tooltip("While only touching spikes, do not chase-jump upward — walk horizontally to safe ground first, then jump.")]
    public bool suppressChaseJumpOnSpikesOnly = true;
    [Tooltip("Horizontal ray length from feet to find the nearest non-spike collider (safe floor).")]
    public float spikeEscapeHorizonRayLength = 3.5f;
    [Tooltip("Ray origin lift from ground check to avoid starting inside a collider.")]
    public float spikeEscapeRayStartYOffset = 0.12f;
    private float currentHealth;
    private float attackTimer;
    private float jumpCooldown;
    private bool isDead;
    private bool isAttacking;
    private bool isFacingRight = true;
    private Transform player;
    private Rigidbody2D rb;

    private Seeker seeker;
    private EnemyTrapAwarePathing trapPathing;
    private Path path;
    private int currentWaypoint = 0;
    private float pathUpdateTimer;

    private Vector2 lastPosition;
    private float stuckTimer;
    private float wallStuckTimer;

    private bool spikeImmune = false;
    private float knockbackTimer;
    private bool meleeEngaged;
    private float lastPathFailLogTime = -999f;
    private float chaseMemoryTimer;
    private int patrolIndex;
    private float patrolWaitTimer;
    private bool waitingAtPatrolPoint;

    // ── Start ────────────────────────────────────────────────────────────────
    void Start()
    {
        currentHealth = maxHealth;
        attackTimer = 0f;

        rb = GetComponent<Rigidbody2D>();
        seeker = GetComponent<Seeker>();
        trapPathing = GetComponent<EnemyTrapAwarePathing>();

        if (visual == null)
        {
            Transform foundVisual = transform.Find("Visual");
            if (foundVisual != null) visual = foundVisual;
        }

        if (animator == null)
        {
            if (visual != null) animator = visual.GetComponent<Animator>();
            if (animator == null) animator = GetComponentInChildren<Animator>();
        }

        if (wallCheck == null)
        {
            Transform w = transform.Find("WallCheck");
            if (w != null) wallCheck = w;
        }

        if (edgeCheck == null)
        {
            Transform e = transform.Find("EdgeCheck");
            if (e != null) edgeCheck = e;
        }

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;

        lastPosition = transform.position;
    }

    // ── Update ───────────────────────────────────────────────────────────────
    void Update()
    {
        if (isDead) return;

        attackTimer     -= Time.deltaTime;
        jumpCooldown    -= Time.deltaTime;
        pathUpdateTimer -= Time.deltaTime;
        knockbackTimer  -= Time.deltaTime;

        if (knockbackTimer > 0f)
        {
            if (rb != null)
                SetSpeed(Mathf.Abs(rb.velocity.x));
            return;
        }

        if (player == null)
        {
            UpdatePatrolOrIdle();
            return;
        }

        Vector2 enemyPos = rb != null ? rb.position : (Vector2)transform.position;
        float dist = Vector2.Distance(enemyPos, (Vector2)player.position);
        Vector2 toPlayer = (Vector2)player.position - enemyPos;
        UpdateMeleeEngagement(toPlayer);

        bool playerInRange = dist <= detectionRange;
        bool hasLos = HasLineOfSight();
        bool closeEnoughToAlwaysChase = dist <= detectionRange * alwaysChaseWithinDetectionFraction;

        if (playerInRange && (hasLos || closeEnoughToAlwaysChase))
            chaseMemoryTimer = chaseMemoryAfterLostSight;
        else
            chaseMemoryTimer -= Time.deltaTime;

        bool shouldChase = playerInRange && ShouldChasePlayer(hasLos, closeEnoughToAlwaysChase);

        if (meleeEngaged && playerInRange)
        {
            rb.velocity = new Vector2(0f, rb.velocity.y);
            SetSpeed(0f);
            FacePlayer();

            if (attackTimer <= 0f && !isAttacking)
                StartAttack();
        }
        else if (shouldChase)
            RunChaseMovement(enemyPos);
        else if (playerInRange)
            SteerTowardPlayerDirect(enemyPos);
        else
            UpdatePatrolOrIdle();
    }

    bool ShouldChasePlayer(bool hasLos, bool closeEnoughToAlwaysChase)
    {
        if (!requireLineOfSight)
            return true;
        if (closeEnoughToAlwaysChase)
            return true;
        if (hasLos)
            return true;
        if (chaseMemoryTimer > 0f)
            return true;
        return false;
    }

    void RunChaseMovement(Vector2 enemyPos)
    {
        if (isAttacking) return;

        if (pathUpdateTimer <= 0f)
        {
            pathUpdateTimer = pathUpdateRate;
            if (trapPathing != null)
                trapPathing.RequestPath(transform.position, player.position, OnPathComplete);
            else if (seeker != null)
            {
                ABPath ab = ABPath.Construct(transform.position, player.position, null);
                ab.calculatePartial = allowPartialPaths;
                seeker.StartPath(ab, OnPathComplete);
            }
        }

        if (path != null && path.vectorPath != null && path.vectorPath.Count > 0)
            FollowPath();
        else
            SteerTowardPlayerDirect(enemyPos);
    }

    void UpdatePatrolOrIdle()
    {
        if (enablePatrol && patrolPoints != null && patrolPoints.Length > 0)
            UpdatePatrol();
        else
        {
            if (rb != null)
                rb.velocity = new Vector2(0f, rb.velocity.y);
            SetSpeed(0f);
        }
    }

    void UpdatePatrol()
    {
        if (patrolPoints == null || patrolPoints.Length == 0) return;

        if (waitingAtPatrolPoint)
        {
            patrolWaitTimer -= Time.deltaTime;
            if (rb != null)
                rb.velocity = new Vector2(0f, rb.velocity.y);
            SetSpeed(0f);
            if (patrolWaitTimer <= 0f)
                waitingAtPatrolPoint = false;
            return;
        }

        Transform waypoint = patrolPoints[patrolIndex];
        if (waypoint == null)
        {
            AdvancePatrolIndex();
            return;
        }

        Vector2 myPos = rb != null ? rb.position : (Vector2)transform.position;
        float dx = waypoint.position.x - myPos.x;
        float dy = waypoint.position.y - myPos.y;
        float dirX = Mathf.Abs(dx) < patrolPointReachedDistance ? 0f : Mathf.Sign(dx);

        if (dirX > 0f && !isFacingRight) Flip();
        else if (dirX < 0f && isFacingRight) Flip();

        float patrolSpeed = moveSpeed * patrolMoveSpeedMultiplier;
        if (rb != null)
            rb.velocity = new Vector2(dirX * patrolSpeed, rb.velocity.y);
        SetSpeed(Mathf.Abs(dirX) > 0.01f ? patrolSpeed : 0f);

        if (Mathf.Abs(dx) <= patrolPointReachedDistance && Mathf.Abs(dy) < 1.75f)
        {
            waitingAtPatrolPoint = true;
            patrolWaitTimer = patrolWaitTime;
            AdvancePatrolIndex();
        }
    }

    void AdvancePatrolIndex()
    {
        if (patrolPoints == null || patrolPoints.Length == 0) return;
        patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
    }

    bool HasLineOfSight()
    {
        if (!requireLineOfSight || player == null) return true;

        Vector2 origin = (rb != null ? rb.position : (Vector2)transform.position)
            + Vector2.up * lineOfSightOriginYOffset;
        Vector2 target = (Vector2)player.position + Vector2.up * lineOfSightOriginYOffset * 0.5f;
        Vector2 delta = target - origin;
        float dist = delta.magnitude;
        if (dist < 0.08f) return true;

        Vector2 dir = delta / dist;
        float checkDist = Mathf.Min(dist, detectionRange);

        RaycastHit2D[] hits = Physics2D.RaycastAll(origin, dir, checkDist, groundLayer);
        if (hits == null || hits.Length == 0) return true;

        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
        foreach (RaycastHit2D h in hits)
        {
            if (h.collider == null) continue;
            if (ColliderBelongsToThisUnit(h.collider)) continue;
            if (ColliderBelongsToPlayer(h.collider)) continue;
            return false;
        }

        return true;
    }

    // ── Pathfinding ──────────────────────────────────────────────────────────
    void OnPathComplete(Path p)
    {
        if (p.error)
        {
            path = null;
            currentWaypoint = 0;

            if (logPathFailures && Time.unscaledTime - lastPathFailLogTime >= pathFailLogInterval)
            {
                lastPathFailLogTime = Time.unscaledTime;
                Debug.LogWarning("[EnemyAI] Path error: " + p.errorLog + " | Ensure A* graph is scanned and covers this area (AstarPath object).", this);
            }
            return;
        }

        path = p;
        currentWaypoint = 0;
    }

    void FollowPath()
    {
        Vector2 myPos = rb != null ? rb.position : (Vector2)transform.position;

        if (path == null || currentWaypoint >= path.vectorPath.Count)
        {
            SteerTowardPlayerDirect(myPos);
            return;
        }

        Vector2 target = path.vectorPath[currentWaypoint];

        float dirY = target.y - myPos.y;

        bool isGrounded = IsGrounded(out bool spikeOnlyFooting);
        float dirX;
        bool faceByMoveDirOnly;
        if (isGrounded && spikeOnlyFooting && TryGetHorizontalTowardNearestNonSpikeGround(myPos, out float escapeDir))
        {
            dirX = escapeDir;
            faceByMoveDirOnly = true;
        }
        else
        {
            dirX = ComputeChaseDirX(myPos, target.x);
            faceByMoveDirOnly = false;
        }

        ApplyFacingForChase(dirX, myPos, faceByMoveDirOnly);

        TryJumpForVerticalChase(myPos, dirY, isGrounded, spikeOnlyFooting);
        TryLedgeOrWallJump(myPos, dirX, dirY, isGrounded, spikeOnlyFooting);

        rb.velocity = new Vector2(dirX * moveSpeed, rb.velocity.y);
        SetSpeed(Mathf.Abs(rb.velocity.x));

        float dist = Vector2.Distance(myPos, target);
        if (dist < nextWaypointDistance)
        {
            currentWaypoint++;
            if (trapPathing != null && path != null)
                currentWaypoint = trapPathing.SkipUnsafeWaypoints(path, currentWaypoint);
        }

        bool wallBlocksNav = IsWallBlockingAhead(dirX);
        bool playerAboveForJump = player != null && player.position.y - myPos.y > chaseJumpVerticalThreshold * 0.45f;

        // ── Wall stuck recovery (only when a wall actually blocks movement) ───
        bool movingButStuck = wallBlocksNav
            && Mathf.Abs(rb.velocity.x) < 0.14f
            && Mathf.Abs(dirX) > 0.06f;

        if (movingButStuck)
        {
            wallStuckTimer += Time.deltaTime;

            if (wallStuckTimer > 0.45f)
            {
                rb.velocity = new Vector2(-dirX * moveSpeed * 2f, rb.velocity.y);

                if (isGrounded && jumpCooldown <= 0f && !(suppressChaseJumpOnSpikesOnly && spikeOnlyFooting))
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

        // ── General stuck recovery ─────────────────────────────────────────
        float moved = Vector2.Distance((Vector2)transform.position, lastPosition);

        if (moved < 0.02f)
        {
            stuckTimer += Time.deltaTime;

            if (stuckTimer > 1.35f && isGrounded && jumpCooldown <= 0f
                && !(suppressChaseJumpOnSpikesOnly && spikeOnlyFooting)
                && (wallBlocksNav || playerAboveForJump))
            {
                Jump();
                stuckTimer = 0f;
                path = null;
                pathUpdateTimer = 0f;
            }
        }
        else
        {
            stuckTimer = 0f;
        }

        lastPosition = transform.position;
    }

    /// <summary>
    /// When the path is missing or consumed, keep moving on X toward the player so the AI does not freeze.
    /// </summary>
    void SteerTowardPlayerDirect(Vector2 myPos)
    {
        if (player == null) return;

        if (path != null && currentWaypoint >= path.vectorPath.Count)
        {
            path = null;
            pathUpdateTimer = 0f;
        }

        bool isGrounded = IsGrounded(out bool spikeOnlyFooting);
        float dirX;
        bool faceByMoveDirOnly;
        if (isGrounded && spikeOnlyFooting && TryGetHorizontalTowardNearestNonSpikeGround(myPos, out float escapeDir))
        {
            dirX = escapeDir;
            faceByMoveDirOnly = true;
        }
        else
        {
            dirX = ComputeChaseDirX(myPos, player.position.x);
            faceByMoveDirOnly = false;
        }

        ApplyFacingForChase(dirX, myPos, faceByMoveDirOnly);

        float waypointDy = 0f;
        if (path != null && currentWaypoint < path.vectorPath.Count)
            waypointDy = path.vectorPath[currentWaypoint].y - myPos.y;
        TryJumpForVerticalChase(myPos, waypointDy, isGrounded, spikeOnlyFooting);
        TryLedgeOrWallJump(myPos, dirX, waypointDy, isGrounded, spikeOnlyFooting);

        rb.velocity = new Vector2(dirX * moveSpeed, rb.velocity.y);
        SetSpeed(Mathf.Abs(rb.velocity.x));

        bool wallBlocksNav = IsWallBlockingAhead(dirX);
        bool playerAboveForJump = player != null && player.position.y - myPos.y > chaseJumpVerticalThreshold * 0.45f;

        bool movingButStuck = wallBlocksNav
            && Mathf.Abs(rb.velocity.x) < 0.14f
            && Mathf.Abs(dirX) > 0.06f;
        if (movingButStuck)
        {
            wallStuckTimer += Time.deltaTime;
            if (wallStuckTimer > 0.45f)
            {
                rb.velocity = new Vector2(-dirX * moveSpeed * 2f, rb.velocity.y);
                if (isGrounded && jumpCooldown <= 0f && !(suppressChaseJumpOnSpikesOnly && spikeOnlyFooting))
                    Jump();
                path = null;
                pathUpdateTimer = 0f;
                wallStuckTimer = 0f;
            }
        }
        else
            wallStuckTimer = 0f;

        float moved = Vector2.Distance((Vector2)transform.position, lastPosition);
        if (moved < 0.02f)
        {
            stuckTimer += Time.deltaTime;
            if (stuckTimer > 1.35f && isGrounded && jumpCooldown <= 0f
                && !(suppressChaseJumpOnSpikesOnly && spikeOnlyFooting)
                && (wallBlocksNav || playerAboveForJump))
            {
                Jump();
                stuckTimer = 0f;
                path = null;
                pathUpdateTimer = 0f;
            }
        }
        else
            stuckTimer = 0f;

        lastPosition = transform.position;
    }

    /// <summary>
    /// Stable horizontal chase direction: avoids Sign(0) when waypoint shares X with the enemy,
    /// and wiggles slightly when the player is directly above (spike pit / shaft).
    /// </summary>
    float ComputeChaseDirX(Vector2 myPos, float waypointOrPlayerX)
    {
        if (player == null) return 0f;

        float dxW = waypointOrPlayerX - myPos.x;
        float dirX;
        if (Mathf.Abs(dxW) > waypointSameXEpsilon)
            dirX = Mathf.Sign(dxW);
        else
        {
            float toPx = player.position.x - myPos.x;
            dirX = Mathf.Abs(toPx) > waypointSameXEpsilon ? Mathf.Sign(toPx) : 0f;
        }

        float toPy = player.position.y - myPos.y;
        float toPx2 = player.position.x - myPos.x;
        if (Mathf.Abs(dirX) < 0.01f && toPy > underPlayerWiggleMinDy
            && Mathf.Abs(toPx2) < underPlayerHorizontalDeadZone)
            dirX = Mathf.Sign(Mathf.Sin(Time.time * underPlayerWiggleHz * Mathf.PI * 2f));

        return dirX;
    }

    void ApplyFacingForChase(float moveDirX, Vector2 myPos, bool useMoveDirectionOnly = false)
    {
        if (player == null) return;

        if (useMoveDirectionOnly)
        {
            if (moveDirX > 0.01f && !isFacingRight) Flip();
            else if (moveDirX < -0.01f && isFacingRight) Flip();
            return;
        }

        float pdx = player.position.x - myPos.x;
        if (Mathf.Abs(pdx) <= underPlayerHorizontalDeadZone * 0.55f)
            FacePlayer();
        else
        {
            if (moveDirX > 0.01f && !isFacingRight) Flip();
            else if (moveDirX < -0.01f && isFacingRight) Flip();
        }
    }

    /// <summary>
    /// From the feet, ray left/right along ground layers; first non-Spike hit picks retreat toward safe floor.
    /// </summary>
    bool TryGetHorizontalTowardNearestNonSpikeGround(Vector2 myPos, out float dirX)
    {
        dirX = 0f;
        int mask = groundLayer.value;
        if (mask == 0) return false;

        Vector2 origin = GetGroundCheckWorldPos() + Vector2.up * spikeEscapeRayStartYOffset;

        bool Sample(float sign, out float firstSafeDist)
        {
            firstSafeDist = float.MaxValue;
            RaycastHit2D[] hits = Physics2D.RaycastAll(origin, new Vector2(sign, 0f), spikeEscapeHorizonRayLength, mask);
            if (hits == null || hits.Length == 0) return false;
            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
            foreach (RaycastHit2D h in hits)
            {
                if (h.collider == null) continue;
                if (ColliderBelongsToThisUnit(h.collider)) continue;
                if (!h.collider.CompareTag("Spike"))
                {
                    firstSafeDist = h.distance;
                    return true;
                }
            }
            return false;
        }

        bool okL = Sample(-1f, out float distL);
        bool okR = Sample(1f, out float distR);

        if (okL && !okR)
        {
            dirX = -1f;
            return true;
        }

        if (!okL && okR)
        {
            dirX = 1f;
            return true;
        }

        if (okL && okR)
        {
            dirX = distL <= distR ? -1f : 1f;
            return true;
        }

        return false;
    }

    void UpdateMeleeEngagement(Vector2 toPlayer)
    {
        float ax = Mathf.Abs(toPlayer.x);
        float ay = Mathf.Abs(toPlayer.y);

        float enterH = attackRange + meleeHorizontalExtra;
        float enterV = meleeVerticalTolerance;
        float exitH = enterH + meleeReleaseHorizontalExtra;
        float exitV = enterV + meleeReleaseVerticalExtra;

        bool insideEnter = ax <= enterH && ay <= enterV;
        bool insideExit = ax <= exitH && ay <= exitV;

        if (insideEnter)
            meleeEngaged = true;
        else if (!insideExit)
            meleeEngaged = false;
    }

    Vector2 GetGroundCheckWorldPos()
    {
        return groundCheck != null
            ? (Vector2)groundCheck.position
            : (Vector2)transform.position + new Vector2(0f, -1.1f);
    }

    bool ColliderBelongsToThisUnit(Collider2D c)
    {
        if (c == null) return true;
        Transform t = c.transform;
        return t == transform || t.IsChildOf(transform);
    }

    bool ColliderBelongsToPlayer(Collider2D c)
    {
        if (c == null || player == null) return false;
        Transform t = c.transform;
        return t == player || t.IsChildOf(player);
    }

    /// <summary>Raycast that ignores this enemy's colliders and the player (so WallCheck doesn't hit self).</summary>
    RaycastHit2D RaycastExternal(Vector2 origin, Vector2 direction, float distance, LayerMask mask)
    {
        if (direction.sqrMagnitude < 1e-10f || distance <= 0f) return default;

        Vector2 nd = direction.normalized;
        RaycastHit2D[] hits = Physics2D.RaycastAll(origin, nd, distance, mask);
        if (hits == null || hits.Length == 0) return default;

        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
        foreach (RaycastHit2D h in hits)
        {
            if (h.collider == null) continue;
            if (ColliderBelongsToThisUnit(h.collider)) continue;
            if (ColliderBelongsToPlayer(h.collider)) continue;
            return h;
        }

        return default;
    }

    bool IsWallBlockingAhead(float moveDirX)
    {
        if (wallCheck == null || Mathf.Abs(moveDirX) < 0.04f) return false;

        RaycastHit2D h = RaycastExternal(
            wallCheck.position,
            new Vector2(Mathf.Sign(moveDirX), 0f),
            wallAheadCheckDistance + 0.08f,
            groundLayer);

        return h.collider != null && !h.collider.CompareTag("Spike");
    }

    /// <summary>
    /// True if ground check overlaps the ground mask. spikeOnlyFooting = only spike colliders (no safe floor).
    /// </summary>
    bool IsGrounded(out bool spikeOnlyFooting)
    {
        spikeOnlyFooting = false;
        Vector2 pos = GetGroundCheckWorldPos();
        Collider2D[] hits = Physics2D.OverlapCircleAll(pos, checkRadius, groundLayer);

        if (hits == null || hits.Length == 0)
        {
            if (rb == null) return false;
            if (Mathf.Abs(rb.velocity.y) > 0.35f) return false;
            RaycastHit2D down = Physics2D.Raycast(pos + Vector2.up * 0.08f, Vector2.down, checkRadius + 0.35f, groundLayer);
            if (down.collider == null) return false;
            spikeOnlyFooting = down.collider.CompareTag("Spike");
            return true;
        }

        bool anySpike = false;
        bool anyNonSpike = false;
        foreach (Collider2D h in hits)
        {
            if (h == null) continue;
            if (h.CompareTag("Spike")) anySpike = true;
            else anyNonSpike = true;
        }

        spikeOnlyFooting = anySpike && !anyNonSpike;
        return true;
    }

    void TryJumpForVerticalChase(Vector2 myPos, float waypointDyAboveSelf, bool isGrounded, bool spikeOnlyFooting)
    {
        if (player == null || !isGrounded || jumpCooldown > 0f) return;
        if (suppressChaseJumpOnSpikesOnly && spikeOnlyFooting) return;

        float playerDy = player.position.y - myPos.y;
        float needPlayerDy = spikeOnlyFooting
            ? chaseJumpVerticalThreshold * 0.55f
            : chaseJumpVerticalThreshold;
        float needWaypointDy = spikeOnlyFooting
            ? pathWaypointJumpDy * 0.55f
            : pathWaypointJumpDy;

        if (playerDy > needPlayerDy)
        {
            Jump(spikeOnlyFooting ? chaseJumpCooldownOnSpikeFooting : -1f);
            return;
        }

        // Path waypoint Y alone is often grid noise on flat floors — require player slightly above too.
        if (waypointDyAboveSelf > needWaypointDy && playerDy > pathJumpMinPlayerDy)
            Jump(spikeOnlyFooting ? chaseJumpCooldownOnSpikeFooting : -1f);
    }

    bool HasFootingBelow(float rayLength = 0.55f)
    {
        Vector2 origin = GetGroundCheckWorldPos() + Vector2.up * 0.06f;
        RaycastHit2D hit = RaycastExternal(origin, Vector2.down, rayLength, groundLayer);
        return hit.collider != null && !hit.collider.CompareTag("Spike");
    }

    /// <summary>
    /// Uses EdgeCheck (void below) and WallCheck (solid ahead) for climb jumps when something above is worth reaching.
    /// </summary>
    void TryLedgeOrWallJump(Vector2 myPos, float moveDirX, float waypointDyAboveSelf, bool isGrounded, bool spikeOnlyFooting)
    {
        if (player == null || !isGrounded || jumpCooldown > 0f) return;
        if (suppressChaseJumpOnSpikesOnly && spikeOnlyFooting) return;
        if (Mathf.Abs(moveDirX) < 0.05f) return;

        float playerDy = player.position.y - myPos.y;

        bool wallBlocked = IsWallBlockingAhead(moveDirX);
        float minPlayerDy = ledgeJumpMinPlayerDy;
        float minWaypointDy = ledgeJumpMinWaypointDy;
        if (wallBlocked)
        {
            minPlayerDy *= wallJumpRelaxedHeightFactor;
            minWaypointDy *= wallJumpRelaxedHeightFactor;
        }

        bool playerAbove = playerDy >= minPlayerDy;
        bool pathAbove = waypointDyAboveSelf >= minWaypointDy;
        bool worthClimbing = playerAbove || pathAbove;

        Vector2 moveDir = new Vector2(Mathf.Sign(moveDirX), 0f);

        if (edgeCheck != null)
        {
            RaycastHit2D edgeDown = RaycastExternal(edgeCheck.position, Vector2.down, edgeDropDetectDistance, groundLayer);
            if (edgeDown.collider == null && HasFootingBelow())
            {
                Vector2 toEdge = (Vector2)edgeCheck.position - myPos;
                if (toEdge.sqrMagnitude > 0.0001f && Vector2.Dot(toEdge.normalized, moveDir) >= ledgeForwardDotThreshold)
                {
                    Jump(-1f, true);
                    return;
                }
            }
        }

        if (!worthClimbing)
            return;

        if (wallCheck != null)
        {
            RaycastHit2D wallHit = RaycastExternal(wallCheck.position, moveDir, wallAheadCheckDistance + 0.06f, groundLayer);
            if (wallHit.collider != null && !wallHit.collider.CompareTag("Spike"))
                Jump(-1f, true);
        }
    }

    // ── Jump ─────────────────────────────────────────────────────────────────
    /// <param name="jumpCooldownOverride">If &gt;= 0, used as jump cooldown; otherwise default 2.5s.</param>
    /// <param name="useLedgeJumpBoost">When true, vertical speed is Jump Force × Ledge Jump Vertical Multiplier.</param>
    void Jump(float jumpCooldownOverride = -1f, bool useLedgeJumpBoost = false)
    {
        float vy = useLedgeJumpBoost ? jumpForce * ledgeJumpVerticalMultiplier : jumpForce;
        rb.velocity = new Vector2(rb.velocity.x, vy);
        jumpCooldown = jumpCooldownOverride >= 0f ? jumpCooldownOverride : 2.5f;
    }

    // ── Attack ───────────────────────────────────────────────────────────────
    void StartAttack()
    {
        isAttacking = true;

        rb.velocity = new Vector2(0f, rb.velocity.y);

        SetBool("IsAttacking", true);

        CancelInvoke(nameof(DealDamage));
        CancelInvoke(nameof(ResetAttack));

        // Hit frame timing — tune to match your attack animation
        Invoke(nameof(DealDamage),  0.35f);
        // End of attack animation — tune to match your attack animation
        Invoke(nameof(ResetAttack), 0.7f);
    }

    void DealDamage()
    {
        if (isDead) return;

        int mask = playerLayer.value != 0 ? (int)playerLayer : ~0;
        Vector2 pivot = transform.position;

        Vector2 primaryOrigin = attackPoint != null
            ? (Vector2)attackPoint.position
            : pivot + new Vector2(isFacingRight ? 0.6f : -0.6f, 0f);

        if (ApplyDamageFromOverlap(primaryOrigin, attackRadius, mask))
            return;

        // Point-blank: player between pivot and AttackPoint can sit outside the AttackPoint circle alone.
        if (attackPoint != null)
        {
            float hugRadius = Vector2.Distance(pivot, (Vector2)attackPoint.position) + attackRadius + closeCombatPadding;
            ApplyDamageFromOverlap(pivot, hugRadius, mask);
        }
    }

    bool ApplyDamageFromOverlap(Vector2 origin, float radius, int mask)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(origin, radius, mask);

        foreach (Collider2D hit in hits)
        {
            PlayerHealth ph = hit.GetComponent<PlayerHealth>();
            if (ph == null) ph = hit.GetComponentInParent<PlayerHealth>();

            if (ph != null)
            {
                ph.TakeDamage(Mathf.RoundToInt(attackDamage));
                return true;
            }
        }

        return false;
    }

    void ResetAttack()
    {
        isAttacking = false;
        SetBool("IsAttacking", false);
        attackTimer = attackCooldown;
    }

    // ── Take Damage ──────────────────────────────────────────────────────────
    public void TakeDamage(float amount)
    {
        TakeDamage(amount, Vector2.zero);
    }

    public void TakeDamage(float amount, Vector2 hitDirection)
    {
        if (isDead) return;

        currentHealth -= amount;
        currentHealth  = Mathf.Clamp(currentHealth, 0f, maxHealth);

        if (showDamageNumbers)
            FloatingDamageNumber.Spawn(transform.position, Mathf.RoundToInt(amount));

        if (hitDirection.sqrMagnitude > 0.0001f && rb != null && rb.simulated)
        {
            Vector2 dir = hitDirection.normalized;
            rb.velocity = new Vector2(dir.x * knockbackHorizontalForce, knockbackVerticalForce);
            knockbackTimer = knockbackStunDuration;
        }

        if (currentHealth <= 0f)
            Die();
        else
        {
            StopCoroutine(nameof(HitRoutine));
            StartCoroutine(HitRoutine());
        }
    }

    IEnumerator HitRoutine()
    {
        SetBool("IsHit", true);
        yield return new WaitForSeconds(0.2f);
        SetBool("IsHit", false);
    }

    // ── Die ──────────────────────────────────────────────────────────────────
    void Die()
    {
        if (isDead) return;

        isDead = true;

        CancelInvoke();
        StopAllCoroutines();

        rb.velocity  = Vector2.zero;
        rb.simulated = false;

        // Clear other bools; only IsDead stays true
        SetBool("IsAttacking", false);
        SetBool("IsHit",       false);
        SetBool("IsDead",      true);
        SetSpeed(0f);

        Destroy(gameObject, 2f);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────
    void FacePlayer()
    {
        if (player == null) return;

        float dx = player.position.x - transform.position.x;
        const float faceEps = 0.05f;
        if (dx > faceEps && !isFacingRight) Flip();
        else if (dx < -faceEps && isFacingRight) Flip();
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
        if (animator != null) animator.SetFloat("Speed", value);
    }

    /// <summary>
    /// Sets a bool on the Animator only if a matching bool parameter exists (avoids errors).
    /// </summary>
    void SetBool(string paramName, bool value)
    {
        if (animator == null) return;

        foreach (AnimatorControllerParameter p in animator.parameters)
        {
            if (p.type == AnimatorControllerParameterType.Bool && p.name == paramName)
            {
                animator.SetBool(paramName, value);
                return;
            }
        }

        // No matching parameter — skip quietly to avoid console spam
    }

    // ── Spike knockback ──────────────────────────────────────────────────────
    void OnTriggerEnter2D(Collider2D col)
    {
        if (isDead) return;
        if (spikeImmune) return;

        if (col.CompareTag("Spike"))
            StartCoroutine(SpikeKnockback());
    }

    IEnumerator SpikeKnockback()
    {
        spikeImmune = true;

        float towardX = 0f;
        if (player != null)
        {
            towardX = Mathf.Sign(player.position.x - transform.position.x);
            if (Mathf.Abs(towardX) < 0.02f)
                towardX = isFacingRight ? 1f : -1f;
        }
        else
            towardX = isFacingRight ? -1f : 1f;

        float upVel = Mathf.Max(jumpForce * 1.55f, jumpForce + 1.25f);
        rb.velocity = new Vector2(towardX * moveSpeed * 2.8f, upVel);
        jumpCooldown = spikeKnockbackJumpCooldown;
        yield return new WaitForSeconds(spikeImmunityDuration);
        spikeImmune = false;
    }

    // ── Gizmos ───────────────────────────────────────────────────────────────
    void OnDrawGizmos()
    {
        if (!drawActivePathInScene || !Application.isPlaying || path == null) return;
        if (path.vectorPath == null || path.vectorPath.Count < 2) return;

        Gizmos.color = new Color(0.15f, 1f, 0.25f, 1f);
        for (int i = 0; i < path.vectorPath.Count - 1; i++)
        {
            Vector3 a = path.vectorPath[i];
            Vector3 b = path.vectorPath[i + 1];
            Gizmos.DrawLine(a, b);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        float enterH = attackRange + meleeHorizontalExtra;
        float enterV = meleeVerticalTolerance;
        Vector3 c = transform.position;
        Gizmos.DrawWireCube(c, new Vector3(enterH * 2f, enterV * 2f, 0f));

        Gizmos.color = new Color(1f, 0.4f, 0.4f, 0.9f);
        float exitH = enterH + meleeReleaseHorizontalExtra;
        float exitV = enterV + meleeReleaseVerticalExtra;
        Gizmos.DrawWireCube(c, new Vector3(exitH * 2f, exitV * 2f, 0f));

        Vector2 atkOrigin = attackPoint != null
            ? (Vector2)attackPoint.position
            : (Vector2)transform.position + new Vector2(isFacingRight ? 0.6f : -0.6f, 0f);

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(atkOrigin, attackRadius);

        if (attackPoint != null)
        {
            float hugR = Vector2.Distance(transform.position, attackPoint.position) + attackRadius + closeCombatPadding;
            Gizmos.color = new Color(1f, 0.5f, 0f, 1f);
            Gizmos.DrawWireSphere(transform.position, hugR);
        }

        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, checkRadius);
        }

        if (edgeCheck != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(edgeCheck.position, edgeCheck.position + Vector3.down * edgeDropDetectDistance);
        }

        if (wallCheck != null)
        {
            float face = isFacingRight ? 1f : -1f;
            Gizmos.color = new Color(0.4f, 0.8f, 1f, 1f);
            Vector3 p = wallCheck.position;
            Gizmos.DrawLine(p, p + new Vector3(face * wallAheadCheckDistance, 0f, 0f));
        }
    }
}