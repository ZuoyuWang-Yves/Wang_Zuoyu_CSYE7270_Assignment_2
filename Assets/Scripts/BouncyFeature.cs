using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BouncyFeature : MonoBehaviour
{
    //Bounce Settings
    private float bounceForce = 20f;
    private float upwardForce = 10f;

    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Player"))
        {
            return;
        }

        Rigidbody playerRb = collision.rigidbody;
        if (playerRb == null)
        {
            return;
        }

        Vector3 awayFromObstacle = (collision.transform.position - transform.position).normalized;
        Vector3 bounceDirection = (awayFromObstacle + Vector3.up * (upwardForce / Mathf.Max(0.01f, bounceForce))).normalized;

        // Reset velocity so bounce feels consistent
        playerRb.velocity = Vector3.zero;
        playerRb.AddForce(bounceDirection * bounceForce, ForceMode.Impulse);
        ProceduralChordSfx.PlayAction(GameSfxAction.BouncyHit);
    }
}
