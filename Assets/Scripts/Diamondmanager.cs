using UnityEngine;

public class DiamondManager : MonoBehaviour
{
    public static DiamondManager instance;

    public int diamondCount = 0;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddDiamond(int amount)
    {
        diamondCount += amount;
    }

    public int GetDiamondCount()
    {
        return diamondCount;
    }
}