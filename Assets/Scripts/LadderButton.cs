using UnityEngine;

public class LadderButton : MonoBehaviour
{
    [Header("Button Visuals")]
    public Sprite unpressedSprite;
    public Sprite pressedSprite;

    [Header("What Ladder?")]
    public ExtendingLadder targetLadder; // Link to our new ladder script!

    private bool isPressed = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Only trigger once, and allow the Player or a Box to press it
        if (!isPressed && (other.CompareTag("Player") || other.CompareTag("MovableBox")))
        {
            isPressed = true;
            GetComponent<SpriteRenderer>().sprite = pressedSprite;

            // Tell the ladder to do its animation!
            if (targetLadder != null)
            {
                targetLadder.RollOut();
            }
        }
    }
}