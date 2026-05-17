using UnityEngine;

public class TriggerZoneConnector : MonoBehaviour
{
    public AscentManager manager;
    public bool isLandingTrigger = false; // False = Takeoff, True = Landing

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (!isLandingTrigger)
                manager.StartFlight();
            else
                manager.StartLanding();
        }
    }
}