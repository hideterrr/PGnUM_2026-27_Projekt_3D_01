using UnityEngine;

/// <summary>
/// Spawns a floating score indicator in 3D world space at the click position.
/// </summary>
public class ScorePopup : MonoBehaviour
{
    private TextMesh textMesh;
    private Color textColor;
    private float floatSpeed = 2.5f;
    private float duration = 0.7f;
    private float elapsed = 0f;

    public static void Create(Vector3 worldPosition, int scoreDelta)
    {
        GameObject go = new GameObject("ScorePopup");
        go.transform.position = worldPosition + Vector3.up * 0.3f;

        ScorePopup popup = go.AddComponent<ScorePopup>();
        popup.Setup(scoreDelta);
    }

    private void Setup(int scoreDelta)
    {
        textMesh = gameObject.AddComponent<TextMesh>();
        textMesh.characterSize = 0.12f;
        textMesh.fontSize = 48;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;

        if (scoreDelta > 0)
        {
            textMesh.text = "+" + scoreDelta;
            textColor = (scoreDelta >= 3) ? new Color(1f, 0.85f, 0.1f) : new Color(0.2f, 1f, 0.4f);
        }
        else
        {
            textMesh.text = scoreDelta.ToString();
            textColor = new Color(1f, 0.2f, 0.2f);
        }

        textMesh.color = textColor;
    }

    private void Update()
    {
        // Float upward
        transform.position += Vector3.up * (floatSpeed * Time.deltaTime);

        // Face the camera
        if (Camera.main != null)
        {
            transform.rotation = Camera.main.transform.rotation;
        }

        // Fade out alpha
        elapsed += Time.deltaTime;
        float alpha = Mathf.Clamp01(1f - (elapsed / duration));
        if (textMesh != null)
        {
            Color c = textColor;
            c.a = alpha;
            textMesh.color = c;
        }

        if (elapsed >= duration)
        {
            Destroy(gameObject);
        }
    }
}
