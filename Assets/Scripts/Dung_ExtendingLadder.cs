using UnityEngine;
using System.Collections;

public class Dung_ExtendingLadder : MonoBehaviour
{
    [Header("Ladder Setup")]
    public GameObject[] ladderPieces;

    public float dropSpeed = 0.15f;

    private bool isExtended = false;

    // 🔥 корутина
    private Coroutine currentCoroutine;

    // 🔥 сохраняем изначальные позиции
    private Vector3[] originalPositions;

    void Start()
    {
        originalPositions = new Vector3[ladderPieces.Length];

        for (int i = 0; i < ladderPieces.Length; i++)
        {
            originalPositions[i] = ladderPieces[i].transform.localPosition;
        }
    }

    // 🔼 ВЫЕЗЖАЕТ
    public void RollOut()
    {
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
        }

        currentCoroutine = StartCoroutine(RevealPiecesSmoothly());
    }

    private IEnumerator RevealPiecesSmoothly()
    {
        isExtended = true;

        for (int i = 0; i < ladderPieces.Length; i++)
        {
            GameObject piece = ladderPieces[i];

            // ✅ используем СОХРАНЕННУЮ позицию
            Vector3 finalPosition = originalPositions[i];

            float pieceHeight = piece.GetComponent<SpriteRenderer>().bounds.size.y;

            Vector3 hiddenPosition = finalPosition + new Vector3(0, pieceHeight, 0);
            piece.transform.localPosition = hiddenPosition;

            piece.SetActive(true);

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

    // 🔽 УЕЗЖАЕТ ОБРАТНО
    public void RollBack()
    {
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
        }

        currentCoroutine = StartCoroutine(HidePiecesSmoothly());
    }

    private IEnumerator HidePiecesSmoothly()
    {
        isExtended = false;

        for (int i = ladderPieces.Length - 1; i >= 0; i--)
        {
            GameObject piece = ladderPieces[i];

            // ✅ тоже используем сохранённую позицию
            Vector3 startPosition = originalPositions[i];

            float pieceHeight = piece.GetComponent<SpriteRenderer>().bounds.size.y;
            Vector3 hiddenPosition = startPosition + new Vector3(0, pieceHeight, 0);

            float timer = 0f;
            while (timer < dropSpeed)
            {
                timer += Time.deltaTime;
                float progress = timer / dropSpeed;

                piece.transform.localPosition = Vector3.Lerp(startPosition, hiddenPosition, progress);
                yield return null;
            }

            piece.transform.localPosition = hiddenPosition;
            piece.SetActive(false);
        }
    }
}