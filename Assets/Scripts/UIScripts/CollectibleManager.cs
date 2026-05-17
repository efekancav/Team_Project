using UnityEngine;
using UnityEngine.SceneManagement;

public class CollectibleManager : MonoBehaviour
{
    public static CollectibleManager instance;

    public enum CollectMode
    {
        LevelBased,
        ChapterBased
    }

    [Header("Collectible Settings")]
    public CollectibleType collectibleType;
    public CollectMode collectMode;

    [Header("Requirement")]
    public int requiredAmount = 1;
    public int currentAmount = 0;

    private static int savedDiamondCount = 0;
    private static int savedFlowerCount = 0;

    private void Awake()
    {
        instance = this;
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        InitializeCollectibleCount();
        UpdateUI();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        instance = this;
        InitializeCollectibleCount();
        UpdateUI();
    }

    private void InitializeCollectibleCount()
    {
        if (collectMode == CollectMode.LevelBased)
        {
            currentAmount = 0;
            UpdateUI();
            return;
        }

        if (collectMode == CollectMode.ChapterBased)
        {
            if (collectibleType == CollectibleType.Diamond)
            {
                currentAmount = savedDiamondCount;
            }
            else if (collectibleType == CollectibleType.Flower)
            {
                currentAmount = savedFlowerCount;
            }

            UpdateUI();
        }
    }

    public void AddCollectible(int amount)
    {
        currentAmount += amount;

        if (collectMode == CollectMode.ChapterBased)
        {
            if (collectibleType == CollectibleType.Diamond)
            {
                savedDiamondCount = currentAmount;
            }
            else if (collectibleType == CollectibleType.Flower)
            {
                savedFlowerCount = currentAmount;
            }
        }

        UpdateUI();
    }

    public bool HasEnoughCollectibles()
    {
        return currentAmount >= requiredAmount;
    }

    public int GetCurrentAmount()
    {
        return currentAmount;
    }

    public int GetRequiredAmount()
    {
        return requiredAmount;
    }

    public CollectibleType GetCollectibleType()
    {
        return collectibleType;
    }

    public void ResetSavedChapterCollectibles()
    {
        if (collectibleType == CollectibleType.Diamond)
        {
            savedDiamondCount = 0;
        }
        else if (collectibleType == CollectibleType.Flower)
        {
            savedFlowerCount = 0;
        }

        currentAmount = 0;
        UpdateUI();
    }

    private void UpdateUI()
    {
        CollectibleUI collectibleUI = FindObjectOfType<CollectibleUI>();

        if (collectibleUI != null)
        {
            collectibleUI.UpdateCollectibleUI();
        }
    }
}