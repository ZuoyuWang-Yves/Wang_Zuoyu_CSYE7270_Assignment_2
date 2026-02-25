using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAbilities : MonoBehaviour
{
    // Keyboard Input
    public KeyCode flashKey = KeyCode.Q;
    public KeyCode explosionKey = KeyCode.E;

    // Skill: Flash
    public float flashDistance = 6f;
    public float flashCooldown = 2f;
    public float flashObstaclePadding = 0.5f;
    public LayerMask flashBlockingLayers = ~0;
    public TrailRenderer flashTrail;
    public float flashTrailDuration = 0.2f;

    // Skill: Explosion
    public float explosionRadius = 4f;
    public float explosionCooldown = 4f;
    public GameObject explosionVfxPrefab;
    public float explosionVfxLifetime = 1f;

    private Rigidbody rb;
    private float nextFlashTime;
    private float nextExplosionTime;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (flashTrail == null)
        {
            flashTrail = GetComponent<TrailRenderer>();
        }

        if (flashTrail != null)
        {
            // Keep trail off by default. We only show it during flash.
            flashTrail.emitting = false;
        }
    }

    private void TryFlash()
    {
        if (Time.time < nextFlashTime)
        {
            return;
        }

        Vector3 flashDirection = GetFlashDirection();
        if (flashDirection.sqrMagnitude < 0.0001f)
        {
            return;
        }

        Vector3 origin = transform.position + Vector3.up * 0.1f;
        float travelDistance = flashDistance;

        if (Physics.Raycast(origin, flashDirection, out RaycastHit hit, flashDistance, flashBlockingLayers, QueryTriggerInteraction.Ignore))
        {
            travelDistance = Mathf.Max(0f, hit.distance - flashObstaclePadding);
        }

        if (travelDistance <= 0.01f)
        {
            return;
        }

        Vector3 destination = transform.position + flashDirection * travelDistance;

        if (flashTrail != null)
        {
            // Briefly enable the trail so the flash movement is visible.
            StartCoroutine(FlashTrailBurst());
        }

        rb.position = destination;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        nextFlashTime = Time.time + flashCooldown;
        ProceduralChordSfx.PlayAction(GameSfxAction.Flash);
    }

    private Vector3 GetFlashDirection()
    {
        Vector3 inputDirection = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));
        if (inputDirection.sqrMagnitude > 0.0001f)
        {
            return inputDirection.normalized;
        }

        Vector3 planarVelocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        if (planarVelocity.sqrMagnitude > 0.0001f)
        {
            return planarVelocity.normalized;
        }

        Vector3 forwardFlat = new Vector3(transform.forward.x, 0f, transform.forward.z);
        return forwardFlat.normalized;
    }

    private void TryExplosion()
    {
        if (Time.time < nextExplosionTime)
        {
            return;
        }

        Collider[] hits = Physics.OverlapSphere(transform.position, explosionRadius, ~0, QueryTriggerInteraction.Collide);
        HashSet<DestructibleObstacle> destroyed = new HashSet<DestructibleObstacle>();

        for (int i = 0; i < hits.Length; i++)
        {
            DestructibleObstacle destructible = hits[i].GetComponentInParent<DestructibleObstacle>();
            if (destructible == null || destroyed.Contains(destructible))
            {
                continue;
            }

            destroyed.Add(destructible);
            Destroy(destructible.gameObject);
        }

        PlayExplosionVfx();
        nextExplosionTime = Time.time + explosionCooldown;
        ProceduralChordSfx.PlayAction(GameSfxAction.Explosion);
    }

    private void PlayExplosionVfx()
    {
        if (explosionVfxPrefab == null)
        {
            return;
        }

        GameObject vfx = Instantiate(explosionVfxPrefab, transform.position, Quaternion.identity);
        Destroy(vfx, explosionVfxLifetime);
    }

    private IEnumerator FlashTrailBurst()
    {
        flashTrail.emitting = true;
        yield return new WaitForSeconds(flashTrailDuration);
        flashTrail.emitting = false;
    }

    private void Update()
    {
        if (Input.GetKeyDown(flashKey))
        {
            TryFlash();
        }

        if (Input.GetKeyDown(explosionKey))
        {
            TryExplosion();
        }
    }
}
