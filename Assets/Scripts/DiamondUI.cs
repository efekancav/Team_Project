using TMPro;
using UnityEngine;

public class DiamondUI : MonoBehaviour
{
    public TextMeshProUGUI diamondText;

    private void Start()
    {
        UpdateDiamondText();
    }

    private void Update()
    {
        UpdateDiamondText();
    }

    public void UpdateDiamondText()
    {
        if (DiamondManager.instance != null && diamondText != null)
        {
            diamondText.text = "x" + DiamondManager.instance.GetDiamondCount();
        }
    }
}