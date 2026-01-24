using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class SoftHomingMissile : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 60f;
    public float turnRate = 60f;           // degrees per second – lower = less homing
    public float lifeTime = 12f;

    [Header("Homing")]
    public Transform aimReference;         // e.g. tank turret or cannon
    [Range(0f, 1f)]
    public float homingStrength = 0.5f;    // 0 = straight, 1 = full homing

    [Header("FX")]
    public GameObject hitEffectPrefab;

    Rigidbody rb;
    Vector3 flyDir;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        rb.useGravity = false;
        rb.isKinematic = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        Destroy(gameObject, lifeTime);

        flyDir = transform.forward;
    }

    public void Launch(Vector3 direction, float launchSpeed)
    {
        flyDir = direction.normalized;
        speed = launchSpeed;

        rb.rotation = Quaternion.LookRotation(flyDir);
        rb.linearVelocity = flyDir * speed;     // Unity 6 style [web:71][web:93]
    }

    void FixedUpdate()
    {
        // Base forward direction stays mostly straight
        Vector3 desiredDir = flyDir;

        if (aimReference != null)
        {
            // Direction tank is aiming
            Vector3 aimDir = aimReference.forward.normalized;

            // Blend between current direction and aim direction
            desiredDir = Vector3.Slerp(
                flyDir,
                aimDir,
                homingStrength * Time.fixedDeltaTime
            );
        }

        // Limit how fast the missile can turn
        Quaternion currentRot = Quaternion.LookRotation(flyDir);
        Quaternion targetRot = Quaternion.LookRotation(desiredDir);

        // Rotate towards targetRot by at most turnRate per second [web:131][web:123]
        Quaternion newRot = Quaternion.RotateTowards(
            currentRot,
            targetRot,
            turnRate * Time.fixedDeltaTime
        );

        flyDir = newRot * Vector3.forward;

        rb.linearVelocity = flyDir * speed;
        rb.MoveRotation(newRot);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (hitEffectPrefab != null)
        {
            Instantiate(hitEffectPrefab, collision.contacts[0].point, Quaternion.identity);
        }

        // Optionally damage targets here, similar to your other missile logic
        Destroy(gameObject);
    }
}
