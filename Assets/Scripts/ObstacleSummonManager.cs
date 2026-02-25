using UnityEngine;

public class ObstacleSummonManager : MonoBehaviour
{

    // References
    public Transform playerOne;
    public GameObject[] obstaclePrefabs;

    // Edit Spawn Range
    public float minSpawnDistance = 3f;
    public float maxSpawnDistance = 8f;
    public float spawnHeightOffset = 0.5f;

    // Mechanics/Restrictions for Spawn
    public float summonCooldown = 0.6f;
    public int maxSpawnAttempts = 12;
    public bool alignToGround = true;
    public bool randomizeYaw = false;
    public LayerMask groundMask = ~0;
    public float raycastStartHeight = 12f;
    public float raycastDistance = 40f;

    private float nextSummonTime;

    public bool TrySpawnObstacle()
    {
        if (Time.time < nextSummonTime)
        {
            return false;
        }

        if (playerOne == null || obstaclePrefabs == null || obstaclePrefabs.Length == 0)
        {
            return false;
        }

        GameObject prefab = obstaclePrefabs[Random.Range(0, obstaclePrefabs.Length)];
        if (prefab == null)
        {
            return false;
        }

        bool spawned = false;
        for (int i = 0; i < maxSpawnAttempts; i++)
        {
            if (!TryBuildSpawnPosition(out Vector3 spawnPos))
            {
                continue;
            }

            Quaternion spawnRot = BuildSpawnRotation(prefab);
            Instantiate(prefab, spawnPos, spawnRot);
            spawned = true;
            break;
        }

        if (spawned)
        {
            nextSummonTime = Time.time + summonCooldown;
        }

        return spawned;
    }

    private bool TryBuildSpawnPosition(out Vector3 spawnPos)
    {
        spawnPos = Vector3.zero;

        float distance = Random.Range(minSpawnDistance, maxSpawnDistance);
        Vector2 planarDir = Random.insideUnitCircle.normalized;
        if (planarDir.sqrMagnitude < 0.001f)
        {
            planarDir = Vector2.right;
        }

        Vector3 candidate = playerOne.position + new Vector3(planarDir.x, 0f, planarDir.y) * distance;

        if (!alignToGround)
        {
            spawnPos = candidate + Vector3.up * spawnHeightOffset;
            return true;
        }

        Vector3 rayStart = candidate + Vector3.up * raycastStartHeight;
        if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, raycastDistance, groundMask, QueryTriggerInteraction.Ignore))
        {
            spawnPos = hit.point + Vector3.up * spawnHeightOffset;
            return true;
        }

        return false;
    }

    private Quaternion BuildSpawnRotation(GameObject prefab)
    {
        Quaternion baseRot = prefab.transform.rotation;
        if (!randomizeYaw)
        {
            return baseRot;
        }

        Quaternion yawOffset = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
        return yawOffset * baseRot;
    }
}
