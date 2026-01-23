using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class MissileManualControl : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 40f;          // fallback, launcher ga pregazi
    public float turnRate = 120f;
    public bool useMouse = false;
    public float lifeTime = 15f;

    [Header("FX")]
    public GameObject hitEffectPrefab;

    [Header("Safety")]
    public float armDelay = 0.05f;

    [Header("Callbacks")]
    public LauncherFire launcher;

    private Rigidbody rb;
    private Vector3 flyDir;
    private bool armed = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        rb.useGravity = false;
        rb.isKinematic = false;

        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        rb.linearDamping = 0f;
        rb.angularDamping = 10f;

        Destroy(gameObject, lifeTime);

        flyDir = transform.forward;
    }

    public void Launch(Vector3 direction, float launchSpeed, Collider ownerCollider = null)
    {
        flyDir = direction.normalized;
        speed = launchSpeed;

        rb.rotation = Quaternion.LookRotation(flyDir);

        if (ownerCollider != null)
        {
            var myCol = GetComponent<Collider>();
            var cols = ownerCollider.GetComponentsInParent<Collider>();
            foreach (var c in cols)
                Physics.IgnoreCollision(myCol, c, true);
        }

        // ✅ UNITY 6
        rb.linearVelocity = flyDir * speed;

        Invoke(nameof(Arm), armDelay);
    }

    void Arm() => armed = true;

    void FixedUpdate()
    {
        float yaw = Input.GetAxis("Horizontal");
        float pitch = Input.GetAxis("Vertical");

        if (useMouse)
        {
            yaw = Input.GetAxis("Mouse X");
            pitch = -Input.GetAxis("Mouse Y");
        }

        Quaternion delta = Quaternion.Euler(
            pitch * turnRate * Time.fixedDeltaTime,
            yaw * turnRate * Time.fixedDeltaTime,
            0f
        );

        flyDir = (delta * flyDir).normalized;

        // ✅ UNITY 6
        rb.linearVelocity = flyDir * speed;
        rb.MoveRotation(Quaternion.LookRotation(flyDir));
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!armed) return;

        Debug.Log($"MISSILE HIT: {collision.gameObject.name}");

        TankHealth tank = collision.gameObject.GetComponentInParent<TankHealth>();
        if (tank != null)
        {
            tank.TakeDamage(25f); 
            Debug.Log("Tank health sada: " + tank.IsDead);

             if (hitEffectPrefab != null)
            {
                Instantiate(hitEffectPrefab, collision.contacts[0].point, Quaternion.identity);
            }
        }

        Destroy(gameObject);
    }

    void OnDestroy()
    {
        if (launcher != null)
            launcher.OnMissileDestroyed();
    }
}
