using UnityEngine;

public class JumpingBoulder : MonoBehaviour
{
    [Header("Jump Settings")]
    public float jumpHeight = 4f;
    public float speed = 3f;
    public float timeOffset = 0f;

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        // Mathf.Sin creates a smooth wave. 
        // Mathf.Abs forces the bottom of the wave to sharply bounce up instead of going underground.
        float bounceMath = Mathf.Abs(Mathf.Sin((Time.time + timeOffset) * speed));

        float currentHeight = bounceMath * jumpHeight;

        transform.position = startPos + new Vector3(0, currentHeight, 0);
    }
}