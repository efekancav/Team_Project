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

    [Header("Linked Objects")]
    public GameObject objectToEnable;
    public GameObject objectToDisable;

    // We use a counter so if a Player AND a Box are on it, it doesn't unpress when only one leaves!
    private int objectsOnButton = 0;
    private bool isPressed = false;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = unpressedSprite; // Set to default picture
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Make sure your movable block has the tag "MovableBox"
        if (other.CompareTag("Player") || other.CompareTag("MovableBox"))
        {
            objectsOnButton++;
            UpdateButtonState();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") || other.CompareTag("MovableBox"))
        {
            objectsOnButton--;
            if (objectsOnButton < 0) objectsOnButton = 0;

            // Only unpress if the rule allows it
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
            // PRESS IT DOWN
            isPressed = true;
            spriteRenderer.sprite = pressedSprite;

            if (objectToDisable != null) objectToDisable.SetActive(false);
            if (objectToEnable != null) objectToEnable.SetActive(true);
        }
        else if (objectsOnButton == 0 && isPressed && !staysPressed)
        {
            // POP IT BACK UP
            isPressed = false;
            spriteRenderer.sprite = unpressedSprite;

            if (objectToDisable != null) objectToDisable.SetActive(true); // Door comes back!
            if (objectToEnable != null) objectToEnable.SetActive(false);  // Ladder vanishes!
        }
    }
}