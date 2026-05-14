using UnityEngine;
using System.Collections;

public class ExtendingLadder : MonoBehaviour
{
    [Header("Ladder Setup")]
    public GameObject[] ladderPieces;

    [Header("Settings")]
    public float dropSpeed = 0.15f;

    private bool isExtended = false;

    private void Start()
    {
        for (int i = 0; i < ladderPieces.Length; i++)
        {
            if (ladderPieces[i] != null)
            {
                ladderPieces[i].SetActive(false);
            }
        }
    }

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

            if (piece == null)
                continue;

            Vector3 finalPosition = piece.transform.localPosition;

            SpriteRenderer spriteRenderer = piece.GetComponent<SpriteRenderer>();
            BoxCollider2D boxCollider = piece.GetComponent<BoxCollider2D>();

            float pieceHeight = 1f;

            if (spriteRenderer != null)
            {
                pieceHeight = spriteRenderer.bounds.size.y;
            }

            Vector3 hiddenPosition = finalPosition + new Vector3(0f, pieceHeight, 0f);

            piece.transform.localPosition = hiddenPosition;
            piece.SetActive(true);

            if (boxCollider != null)
            {
                boxCollider.enabled = true;
                boxCollider.isTrigger = true;
            }

            float timer = 0f;

            while (timer < dropSpeed)
            {
                timer += Time.deltaTime;

                float progress = timer / dropSpeed;

                piece.transform.localPosition = Vector3.Lerp(hiddenPosition, finalPosition, progress);

                yield return null;
            }

            piece.transform.localPosition = finalPosition;
        }
    }
}