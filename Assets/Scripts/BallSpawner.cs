using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Spawns balls at random positions, random sizes, and with randomized physical trajectories.
/// Spawns Normal (+1), Bonus (+3), and Hazard/Red (-2) balls.
/// </summary>
public class BallSpawner : MonoBehaviour
{
    [Header("Spawn Interval")]
    public float minSpawnInterval = 0.5f;
    public float maxSpawnInterval = 1.0f;

    [Header("Spawn Area")]
    public float spawnY = 4.5f;
    public float minX = -2.8f;
    public float maxX = 2.8f;
    public float minZ = -1.6f;
    public float maxZ = 1.6f;

    [Header("Ball Scale Range")]
    public float minScale = 0.65f;
    public float maxScale = 1.25f;

    [Header("Chances (%)")]
    [Range(0, 100)] public int normalChance = 60;
    [Range(0, 100)] public int hazardChance = 25; // Red penalty balls
    // Remaining % is Bonus (Gold)

    [Header("Materials & Physics")]
    public Material normalMaterial;
    public Material bonusMaterial;
    public Material hazardMaterial;
#if UNITY_6000_0_OR_NEWER
    public PhysicsMaterial bouncyPhysicsMaterial;
#else
    public PhysicMaterial bouncyPhysicsMaterial;
#endif

    [Header("Spawn Limits")]
    public int maxActiveBalls = 16;
    public GameObject ballPrefab;

    private Coroutine spawnRoutine;

    private void Start()
    {
        // Try auto-loading materials if not assigned in Inspector
        if (normalMaterial == null) normalMaterial = Resources.Load<Material>("Materials/Material_Ball_Green");
        if (bonusMaterial == null) bonusMaterial = Resources.Load<Material>("Materials/Material_Ball_Gold");
        if (hazardMaterial == null) hazardMaterial = Resources.Load<Material>("Materials/Material_Ball_Red");
#if UNITY_6000_0_OR_NEWER
        if (bouncyPhysicsMaterial == null) bouncyPhysicsMaterial = Resources.Load<PhysicsMaterial>("Materials/Physics_Material_Ball");
#else
        if (bouncyPhysicsMaterial == null) bouncyPhysicsMaterial = Resources.Load<PhysicMaterial>("Materials/Physics_Material_Ball");
#endif

        StartSpawning();
    }

    public void StartSpawning()
    {
        if (spawnRoutine != null) StopCoroutine(spawnRoutine);
        spawnRoutine = StartCoroutine(SpawnLoop());
    }

    public void StopSpawning()
    {
        if (spawnRoutine != null)
        {
            StopCoroutine(spawnRoutine);
            spawnRoutine = null;
        }
    }

    private IEnumerator SpawnLoop()
    {
        // Initial delay before first spawn
        yield return new WaitForSeconds(0.5f);

        while (true)
        {
            if (GameManager.Instance == null || GameManager.Instance.IsGameActive)
            {
#if UNITY_6000_0_OR_NEWER
                int activeCount = FindObjectsByType<Ball>().Length;
#elif UNITY_2023_1_OR_NEWER
                int activeCount = FindObjectsByType<Ball>(FindObjectsSortMode.None).Length;
#else
                int activeCount = FindObjectsOfType<Ball>().Length;
#endif
                if (activeCount < maxActiveBalls)
                {
                    SpawnSingleBall();
                }
            }

            float waitTime = Random.Range(minSpawnInterval, maxSpawnInterval);
            yield return new WaitForSeconds(waitTime);
        }
    }

    public void SpawnSingleBall()
    {
        // Random position at top of arena
        Vector3 spawnPos = new Vector3(
            Random.Range(minX, maxX),
            spawnY,
            Random.Range(minZ, maxZ)
        );

        // Random diameter / scale
        float scale = Random.Range(minScale, maxScale);

        // Determine ball type based on weights
        int roll = Random.Range(0, 100);
        BallType type;
        Material mat;

        if (roll < normalChance)
        {
            type = BallType.Normal;
            mat = normalMaterial;
        }
        else if (roll < normalChance + hazardChance)
        {
            type = BallType.Hazard;
            mat = hazardMaterial;
        }
        else
        {
            type = BallType.Bonus;
            mat = bonusMaterial;
            // Bonus balls are slightly smaller and more dynamic
            scale *= 0.85f;
        }

        // Random slight horizontal velocity/kick with guaranteed downward trajectory
        Vector3 initialVelocity = new Vector3(
            Random.Range(-1.2f, 1.2f),
            Random.Range(-1.2f, -0.3f),
            Random.Range(-1.0f, 1.0f)
        );

        GameObject ballObj;
        if (ballPrefab != null)
        {
            ballObj = Instantiate(ballPrefab, spawnPos, Quaternion.identity);
        }
        else
        {
            // Dynamically instantiate primitive Sphere
            ballObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            ballObj.name = $"Ball_{type}";
            ballObj.transform.position = spawnPos;

            Rigidbody rb = ballObj.AddComponent<Rigidbody>();
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

            SphereCollider sc = ballObj.GetComponent<SphereCollider>();
            if (sc != null && bouncyPhysicsMaterial != null)
            {
                sc.material = bouncyPhysicsMaterial;
            }

            ballObj.AddComponent<Ball>();
        }

        Ball ballComp = ballObj.GetComponent<Ball>();
        if (ballComp != null)
        {
            ballComp.Init(type, mat, scale, initialVelocity);
        }
    }
}
