using UnityEngine;

public class HealthBarUI : MonoBehaviour
{
    public float maxHealth = 100f;
    public float currentHealth = 100f;

    public RectTransform health;

    public float fullWidth = 180f;

    public void SetHealth(float newHealth)
    {
        currentHealth = Mathf.Clamp(newHealth, 0f, maxHealth);

        float width = (currentHealth / maxHealth) * fullWidth;

        health.sizeDelta = new Vector2(width, health.sizeDelta.y);
    }
}