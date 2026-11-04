using System.Collections;
using UnityEngine;

/// <summary>
/// Controls an obstacle (capsule/cylinder) that periodically emerges from the ground,
/// deflecting falling balls and obstructing the player's view/clicks, then retreats back.
/// </summary>
public class RisingObstacle : MonoBehaviour
{
    [Header("Position Limits")]
    public float loweredY = -1.5f;
    public float raisedY = 1.5f;

    [Header("Timings & Speeds")]
    public float transitionDuration = 1.2f;
    public float stayUpDuration = 3.0f;
    public float stayDownDuration = 2.0f;
    public float rotationSpeed = 60.0f; // degrees per second when emerged

    private Vector3 initialPos;

    private void Start()
    {
        initialPos = transform.position;
        // Start lowered below ground level
        transform.position = new Vector3(initialPos.x, loweredY, initialPos.z);
        StartCoroutine(ObstacleCycleRoutine());
    }

    private void Update()
    {
        // Continuously rotate around Y axis to deflect balls dynamically
        if (transform.position.y > loweredY + 0.2f)
        {
            transform.Rotate(0f, rotationSpeed * Time.deltaTime, 0f, Space.World);
        }
    }

    private IEnumerator ObstacleCycleRoutine()
    {
        // Initial random offset so multiple obstacles don't move in lockstep
        yield return new WaitForSeconds(Random.Range(0.5f, 2.0f));

        while (true)
        {
            // 1. Rise up
            yield return StartCoroutine(MoveVertical(loweredY, raisedY, transitionDuration));

            // 2. Stay up
            yield return new WaitForSeconds(stayUpDuration);

            // 3. Lower down
            yield return StartCoroutine(MoveVertical(raisedY, loweredY, transitionDuration));

            // 4. Stay down hidden
            yield return new WaitForSeconds(stayDownDuration);
        }
    }

    private IEnumerator MoveVertical(float fromY, float toY, float duration)
    {
        float elapsed = 0f;
        Vector3 pos = transform.position;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            // Smooth ease in / ease out
            float smoothT = Mathf.SmoothStep(0f, 1f, t);
            pos.y = Mathf.Lerp(fromY, toY, smoothT);
            transform.position = pos;
            yield return null;
        }

        pos.y = toY;
        transform.position = pos;
    }
}
