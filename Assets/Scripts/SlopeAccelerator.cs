using UnityEngine;

public class SlopeAccelerator : MonoBehaviour
{
    public float accelerationForce = 20f;
    public float  maxSpeed = 50f;
    private string playerTag = "Player";
    public float slopeChordCooldown = 0.6f;

    private float nextSlopeChordTime;

    private void OnCollisionStay(Collision collision)
    {
        if (!collision.gameObject.CompareTag(playerTag))
        {
            return;
        }

        Rigidbody playerRb = collision.rigidbody;
        if (playerRb == null)
        {
            return;
        }

        ContactPoint contact = collision.GetContact(0);
        Vector3 slopeNormal = contact.normal.normalized;

        // Downhill direction along slope plane
        Vector3 downhill = Vector3.ProjectOnPlane(Vector3.down, slopeNormal);
        if (downhill.sqrMagnitude < 0.0001f)
        {
            return;
        }

        // Uphill
        Vector3 uphill = -downhill.normalized;

        Vector3 planarVelocity = new Vector3(playerRb.velocity.x, 0f, playerRb.velocity.z);
        if (planarVelocity.magnitude >=  maxSpeed)
        {
            return;
        }

        playerRb.AddForce(uphill * accelerationForce, ForceMode.Acceleration);

        if (Time.time >= nextSlopeChordTime)
        {
            ProceduralChordSfx.PlayAction(GameSfxAction.SlopeHit);
            nextSlopeChordTime = Time.time + slopeChordCooldown;
        }
    }
}
