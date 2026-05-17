using UnityEngine;

/// <summary>
/// Spawns a simple floating "-25" style damage label at a world position (no prefab required).
/// </summary>
public class FloatingDamageNumber : MonoBehaviour
{
    public float lifetime = 0.85f;
    public float riseSpeed = 1.15f;
    public Color color = new Color(1f, 0.35f, 0.35f, 1f);

    TextMesh textMesh;
    float timer;

    public static void Spawn(Vector3 worldPosition, int damage)
    {
        if (damage <= 0) return;

        GameObject go = new GameObject("DamageNumber");
        go.transform.position = worldPosition + Vector3.up * 0.55f;

        TextMesh tm = go.AddComponent<TextMesh>();
        tm.text = "-" + damage;
        tm.fontSize = 28;
        tm.characterSize = 0.12f;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.alignment = TextAlignment.Center;
        tm.color = new Color(1f, 0.35f, 0.35f, 1f);

        MeshRenderer mr = go.GetComponent<MeshRenderer>();
        if (mr != null)
            mr.sortingOrder = 200;

        FloatingDamageNumber floater = go.AddComponent<FloatingDamageNumber>();
        floater.textMesh = tm;
    }

    void Update()
    {
        transform.position += Vector3.up * riseSpeed * Time.deltaTime;
        timer += Time.deltaTime;

        if (textMesh != null)
        {
            float a = 1f - (timer / lifetime);
            Color c = color;
            c.a = Mathf.Clamp01(a);
            textMesh.color = c;
        }

        if (timer >= lifetime)
            Destroy(gameObject);
    }
}
