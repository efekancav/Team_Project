using UnityEngine;

public class Dung_LadderButton : MonoBehaviour
{
    public Sprite unpressedSprite;
    public Sprite pressedSprite;

    public Dung_ExtendingLadder targetLadder;

    private int objectsOnButton = 0;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") || other.CompareTag("MovableBox"))
        {
            objectsOnButton++;

            SFXManager.Instance.PlaySFX(
                SFXManager.Instance.buttonPress
            );

            GetComponent<SpriteRenderer>().sprite = pressedSprite;

            if (targetLadder != null)
            {
                targetLadder.RollOut();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") || other.CompareTag("MovableBox"))
        {
            objectsOnButton--;

            SFXManager.Instance.PlaySFX(
                SFXManager.Instance.buttonRelease
            );

            if (objectsOnButton <= 0)
            {
                GetComponent<SpriteRenderer>().sprite = unpressedSprite;

                if (targetLadder != null)
                {
                    targetLadder.RollBack(); // 🔥 ВОТ ГЛАВНАЯ СТРОКА
                }
            }
        }
    }
}