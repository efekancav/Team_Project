using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CollectibleUI : MonoBehaviour
{
    [Header("UI References")]
    public Image collectibleIcon;
    public TextMeshProUGUI collectibleText;

    [Header("Collectible Icons")]
    public Sprite keyIcon;
    public Sprite diamondIcon;
    public Sprite flowerIcon;

    private void Start()
    {
        UpdateCollectibleUI();
    }

    public void UpdateCollectibleUI()
    {
        if (CollectibleManager.instance == null)
            return;

        UpdateIcon();
        UpdateText();
    }

    private void UpdateIcon()
    {
        if (collectibleIcon == null)
            return;

        CollectibleType type = CollectibleManager.instance.GetCollectibleType();

        if (type == CollectibleType.Key)
            collectibleIcon.sprite = keyIcon;
        else if (type == CollectibleType.Diamond)
            collectibleIcon.sprite = diamondIcon;
        else if (type == CollectibleType.Flower)
            collectibleIcon.sprite = flowerIcon;
    }

    private void UpdateText()
    {
        if (collectibleText == null)
            return;

        int currentAmount = CollectibleManager.instance.GetCurrentAmount();
        int requiredAmount = CollectibleManager.instance.GetRequiredAmount();

        collectibleText.text = currentAmount + " / " + requiredAmount;
    }
}