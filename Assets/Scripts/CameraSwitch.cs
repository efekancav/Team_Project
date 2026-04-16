using UnityEngine;

public class CameraSwitch : MonoBehaviour
{
    [Header("Cameras to Swap")]
    public GameObject cameraToTurnOff;
    public GameObject cameraToTurnOn;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Turn on the new room's camera
            cameraToTurnOn.SetActive(true);

            // Turn off the old room's camera
            cameraToTurnOff.SetActive(false);
        }
    }
}