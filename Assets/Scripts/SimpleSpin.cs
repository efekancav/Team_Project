using UnityEngine;

public class SimpleSpin : MonoBehaviour
{
    [Header("Spin Settings")]
    public float spinSpeed = 150f;

    void Update()
    {
        // Rotates the object on its Z-axis (perfect for 2D)
        transform.Rotate(0, 0, spinSpeed * Time.deltaTime);
    }
}