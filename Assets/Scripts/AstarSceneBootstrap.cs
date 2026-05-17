using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Pathfinding;

/// <summary>
/// Re-scans the A* grid whenever a new scene loads (room transitions).
/// Attach to GameManager or any DontDestroyOnLoad object, OR place one copy per scene.
/// </summary>
public class AstarSceneBootstrap : MonoBehaviour
{
    [Tooltip("Wait this many frames after load so tilemaps/colliders are ready before Scan.")]
    public int framesToWaitBeforeScan = 2;
    [Tooltip("After graph scan, mark traps unwalkable (EnemyTrapAwarePathing in scene).")]
    public bool refreshTrapPathing = true;
    [Tooltip("Log when a scene scan completes.")]
    public bool logScan = true;

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(ScanCurrentSceneGraph(scene.name));
    }

    /// <summary>Call manually after placing new tilemaps at runtime.</summary>
    public void ScanNow()
    {
        StartCoroutine(ScanCurrentSceneGraph(SceneManager.GetActiveScene().name));
    }

    IEnumerator ScanCurrentSceneGraph(string sceneName)
    {
        for (int i = 0; i < framesToWaitBeforeScan; i++)
            yield return null;

        AstarPath astar = AstarPath.active;
        if (astar == null)
            astar = FindObjectOfType<AstarPath>();

        if (astar == null)
        {
            if (logScan)
                Debug.LogWarning($"[AstarSceneBootstrap] No AstarPath in scene '{sceneName}'. Add AstarPath + Grid Graph to this level.", this);
            yield break;
        }

        astar.Scan();

        if (refreshTrapPathing)
        {
            EnemyTrapAwarePathing[] trappers = FindObjectsOfType<EnemyTrapAwarePathing>();
            foreach (EnemyTrapAwarePathing t in trappers)
            {
                if (t != null)
                    t.ScanAndUpdateGraph();
            }
        }

        if (logScan)
            Debug.Log($"[AstarSceneBootstrap] A* graph scanned for scene '{sceneName}'.", astar);
    }
}
