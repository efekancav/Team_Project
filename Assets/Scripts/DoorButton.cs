using UnityEngine;

public class FloorButton : MonoBehaviour
{
    [Header("Button Visuals")]
    public Sprite unpressedSprite;
    public Sprite pressedSprite;
    private SpriteRenderer spriteRenderer;

    [Header("Button Behavior")]
    [Tooltip("If checked, it stays down forever. If unchecked, it pops up when you step off.")]
    public bool staysPressed = false;

    // --- NEW: The Special Rule Toggle ---
    [Tooltip("If checked, ONLY the boulder can press this. Player and boxes are ignored.")]
    public bool boulderOnly = false;

    [Header("Linked Objects")]
    public GameObject objectToEnable;
    public GameObject objectToDisable;

    private int objectsOnButton = 0;
    private bool isPressed = false;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = unpressedSprite;
    }

    // --- NEW: A helper method to keep the trigger logic clean ---
    private bool IsValidTrigger(Collider2D other)
    {
        if (boulderOnly)
        {
            // Remember from our earlier trap setup, your boulder is tagged as "Hazard"!
            return other.CompareTag("Hazard");
        }
        else
        {
            // The classic behavior for all your older rooms
            return other.CompareTag("Player") || other.CompareTag("MovableBox");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Now we just ask our helper method if the object is allowed
        if (IsValidTrigger(other))
        {
            objectsOnButton++;
            UpdateButtonState();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (IsValidTrigger(other))
        {
            objectsOnButton--;
            if (objectsOnButton < 0) objectsOnButton = 0;

            if (!staysPressed)
            {
                UpdateButtonState();
            }
        }
    }

    private void UpdateButtonState()
    {
        if (objectsOnButton > 0 && !isPressed)
        {
            isPressed = true;
            spriteRenderer.sprite = pressedSprite;

            if (objectToDisable != null) objectToDisable.SetActive(false);
            if (objectToEnable != null) objectToEnable.SetActive(true);
        }
        else if (objectsOnButton == 0 && isPressed && !staysPressed)
        {
            isPressed = false;
            spriteRenderer.sprite = unpressedSprite;

            if (objectToDisable != null) objectToDisable.SetActive(true);
            if (objectToEnable != null) objectToEnable.SetActive(false);
        }
    }
}