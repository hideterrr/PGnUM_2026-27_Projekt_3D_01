using UnityEngine;

public class Capsule : MonoBehaviour
{
    [Header("Patrol Limits")]
    public Vector3 startPos = new Vector3(-2.8f, 0.8f, 0f);
    public Vector3 endPos = new Vector3(2.8f, 0.8f, 0f);

    [Header("Timing")]
    public float duration = 2.0f;
    public float rotationSpeed = 30.0f;

    private float time = 0f;

    void Update()
    {
        if (duration > 0f)
        {
            time += Time.deltaTime;
            float t = Mathf.PingPong(time / duration, 1.0f);
            float smoothT = Mathf.SmoothStep(0f, 1f, t);
            transform.position = Vector3.Lerp(startPos, endPos, smoothT);
        }

        if (rotationSpeed > 0f)
        {
            transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime, Space.Self);
        }
    }
}
