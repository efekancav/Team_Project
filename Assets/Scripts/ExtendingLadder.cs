using UnityEngine;
using System.Collections;

public class ExtendingLadder : MonoBehaviour
{
    [Header("Ladder Setup")]
    [Tooltip("Drag your hidden ladder pieces here in order, from TOP to BOTTOM.")]
    public GameObject[] ladderPieces;

    [Tooltip("How many seconds does it take for one piece to slide down?")]
    public float dropSpeed = 0.15f;

    private bool isExtended = false;

    public void RollOut()
    {
        if (!isExtended)
        {
            StartCoroutine(RevealPiecesSmoothly());
        }
    }

    private IEnumerator RevealPiecesSmoothly()
    {
        isExtended = true;

        for (int i = 0; i < ladderPieces.Length; i++)
        {
            GameObject piece = ladderPieces[i];

            // 1. Remember the exact perfect spot where you vertex-snapped it
            Vector3 finalPosition = piece.transform.localPosition;

            // 2. Automatically measure how tall the pixel art is
            float pieceHeight = piece.GetComponent<SpriteRenderer>().bounds.size.y;

            // 3. Move it UP so it is perfectly hiding behind the piece above it
            Vector3 hiddenPosition = finalPosition + new Vector3(0, pieceHeight, 0);
            piece.transform.localPosition = hiddenPosition;

            // 4. Turn it on (you won't see it yet because it's hiding!)
            piece.SetActive(true);

            // 5. The Magic Math: Smoothly slide it down to its final position
            float timer = 0f;
            while (timer < dropSpeed)
            {
                // Time.deltaTime makes sure the animation is perfectly smooth on all computers
                timer += Time.deltaTime;
                float progress = timer / dropSpeed;

                // Vector3.Lerp smoothly blends between the hidden spot and the final spot
                piece.transform.localPosition = Vector3.Lerp(hiddenPosition, finalPosition, progress);

                // Pause the code until the next frame of the game is drawn
                yield return null;
            }

            // 6. Ensure it snaps exactly to your perfect position at the end
            piece.transform.localPosition = finalPosition;
        }
    }
}