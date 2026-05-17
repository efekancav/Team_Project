using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

/// <summary>
/// Marks traps/hazards on the A* grid as unwalkable and helps the Seeker avoid them.
/// Add to the same GameObject as Seeker + EnemyAI. Re-scan when you add new trap types to a level.
/// </summary>
[RequireComponent(typeof(Seeker))]
public class EnemyTrapAwarePathing : MonoBehaviour
{
    [Header("Hazard detection")]
    [Tooltip("Tags treated as traps (e.g. Spike). Objects with these tags get unwalkable graph updates.")]
    public string[] hazardTags = { "Spike" };
    [Tooltip("Layers that count as hazards in overlap checks (optional; leave empty to use tags only).")]
    public LayerMask hazardLayers;
    [Tooltip("Also scan all DamageZone components in the scene.")]
    public bool includeDamageZones = true;

    [Header("A* graph updates")]
    [Tooltip("Expand trap bounds when marking nodes unwalkable.")]
    public float graphUpdatePadding = 0.35f;
    [Tooltip("Re-scan scene for traps on start and at this interval (0 = only on start).")]
    public float hazardRescanInterval = 0f;
    [Tooltip("Extra penalty cost for hazard-tagged nodes if your graph uses A* tags (optional).")]
    public int hazardTagPenalty = 20000;

    [Header("Path requests")]
    public bool allowPartialPaths = true;
    [Tooltip("Radius used to reject path waypoints that sit on a hazard collider.")]
    public float waypointHazardCheckRadius = 0.28f;

    [Header("Debug")]
    public bool logScanResults = false;

    Seeker seeker;
    float rescanTimer;
    readonly HashSet<int> processedColliderIds = new HashSet<int>();

    void Awake()
    {
        seeker = GetComponent<Seeker>();
    }

    void Start()
    {
        ApplySeekerTagPenalties();
        ScanAndUpdateGraph();
    }

    void Update()
    {
        if (hazardRescanInterval <= 0f) return;
        rescanTimer -= Time.deltaTime;
        if (rescanTimer <= 0f)
        {
            rescanTimer = hazardRescanInterval;
            ScanAndUpdateGraph();
        }
    }

    /// <summary>Call after spawning traps at runtime.</summary>
    public void ScanAndUpdateGraph()
    {
        if (AstarPath.active == null)
        {
            Debug.LogWarning("[EnemyTrapAwarePathing] No AstarPath in scene — add and Scan your grid first.", this);
            return;
        }

        processedColliderIds.Clear();
        int count = 0;

        if (hazardTags != null)
        {
            foreach (string tag in hazardTags)
            {
                if (string.IsNullOrEmpty(tag)) continue;
                try
                {
                    GameObject[] tagged = GameObject.FindGameObjectsWithTag(tag);
                    foreach (GameObject go in tagged)
                        count += MarkHazardFromObject(go);
                }
                catch (UnityException)
                {
                    // Tag not defined in project — skip
                }
            }
        }

        if (includeDamageZones)
        {
            DamageZone[] zones = FindObjectsOfType<DamageZone>();
            foreach (DamageZone zone in zones)
            {
                if (zone != null)
                    count += MarkHazardFromObject(zone.gameObject);
            }
        }

        if (logScanResults)
            Debug.Log($"[EnemyTrapAwarePathing] Marked {count} hazard collider region(s) unwalkable.", this);
    }

    int MarkHazardFromObject(GameObject root)
    {
        if (root == null) return 0;
        int n = 0;
        Collider2D[] cols = root.GetComponentsInChildren<Collider2D>(true);
        foreach (Collider2D col in cols)
        {
            if (col == null || !col.enabled) continue;
            if (!IsHazardCollider(col)) continue;
            if (processedColliderIds.Add(col.GetInstanceID()))
            {
                MarkColliderUnwalkable(col);
                n++;
            }
        }
        return n;
    }

    bool IsHazardCollider(Collider2D col)
    {
        if (col.GetComponentInParent<DamageZone>() != null)
            return true;

        if (hazardTags != null)
        {
            foreach (string tag in hazardTags)
            {
                if (!string.IsNullOrEmpty(tag) && col.CompareTag(tag))
                    return true;
            }
        }

        if (hazardLayers.value != 0 && ((1 << col.gameObject.layer) & hazardLayers.value) != 0)
            return true;

        return false;
    }

    void MarkColliderUnwalkable(Collider2D col)
    {
        Bounds b = col.bounds;
        b.Expand(graphUpdatePadding);

        var guo = new GraphUpdateObject(b)
        {
            modifyWalkability = true,
            setWalkability = false,
            updatePhysics = false
        };

        AstarPath.active.UpdateGraphs(guo);
    }

    void ApplySeekerTagPenalties()
    {
        if (seeker == null || hazardTagPenalty <= 0) return;

        if (seeker.tagPenalties == null || seeker.tagPenalties.Length < 32)
            seeker.tagPenalties = new int[32];

        string[] astarTagNames = AstarPath.FindTagNames();
        if (astarTagNames == null) return;

        foreach (string tagName in hazardTags)
        {
            if (string.IsNullOrEmpty(tagName)) continue;
            for (int i = 0; i < astarTagNames.Length && i < seeker.tagPenalties.Length; i++)
            {
                if (astarTagNames[i] == tagName)
                {
                    seeker.tagPenalties[i] = hazardTagPenalty;
                    break;
                }
            }
        }
    }

    /// <summary>Request a path through the Seeker (used by EnemyAI).</summary>
    public void RequestPath(Vector3 from, Vector3 to, OnPathDelegate onComplete)
    {
        if (seeker == null)
        {
            onComplete?.Invoke(null);
            return;
        }

        ABPath ab = ABPath.Construct(from, to, null);
        ab.calculatePartial = allowPartialPaths;
        seeker.StartPath(ab, onComplete);
    }

    /// <summary>True if nothing hazardous overlaps this point (for skipping bad waypoints).</summary>
    public bool IsWaypointSafe(Vector2 worldPos)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(worldPos, waypointHazardCheckRadius);
        if (hits == null) return true;

        foreach (Collider2D h in hits)
        {
            if (h == null || !h.enabled) continue;
            if (IsHazardCollider(h))
                return false;
        }

        return true;
    }

    /// <summary>Advance waypoint index until a safe point or end of path.</summary>
    public int SkipUnsafeWaypoints(Path p, int startIndex)
    {
        if (p == null || p.vectorPath == null) return startIndex;

        int i = startIndex;
        int safety = 0;
        while (i < p.vectorPath.Count && !IsWaypointSafe(p.vectorPath[i]) && safety < 32)
        {
            i++;
            safety++;
        }

        return i;
    }

    void OnDrawGizmosSelected()
    {
        if (hazardTags == null) return;
        Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.35f);
        foreach (string tag in hazardTags)
        {
            if (string.IsNullOrEmpty(tag)) continue;
            try
            {
                GameObject[] tagged = GameObject.FindGameObjectsWithTag(tag);
                foreach (GameObject go in tagged)
                {
                    Collider2D[] cols = go.GetComponentsInChildren<Collider2D>();
                    foreach (Collider2D c in cols)
                    {
                        if (c != null)
                            Gizmos.DrawWireCube(c.bounds.center, c.bounds.size + Vector3.one * graphUpdatePadding * 2f);
                    }
                }
            }
            catch (UnityException) { }
        }
    }
}
