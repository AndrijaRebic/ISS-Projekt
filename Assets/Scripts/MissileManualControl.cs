using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class MissileManualControl : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 40f;
    public float turnRate = 120f;
    public bool useMouse = false;
    public float lifeTime = 15f;

    [Header("Stabilization")]
    [Tooltip("Max pitch angle up/down relative to world up, to prevent flipping/spirals.")]
    public float maxPitchAngle = 80f;

    [Tooltip("How strongly we keep the missile 'upright' (remove roll). 0 = no roll correction.")]
    public float rollUprightStrength = 8f;

    [Header("Collision (stable flight fix)")]
    [Tooltip("Multiplier for spherecast radius based on collider bounds. 0.6-1.0 is typical.")]
    public float castRadiusMultiplier = 0.75f;

    [Tooltip("Extra distance to avoid missing hits at high speed.")]
    public float castSkin = 0.05f;

    [Header("FX")]
    public GameObject hitEffectPrefab;

    [Header("Safety")]
    public float armDelay = 0.05f;

    [Header("Effects")]
    public GameObject explosionPrefab;
    public GameObject smokeTrailPrefab;
    public float explosionLife = 2f;

    [Header("Audio")]
    public AudioClip explosionSound;
    public string explosionSoundResourcesPath = "Free Pack/Explosion 1";
    public float explosionAudioVolume = 1f;
    public float explosionMinDistance = 1f;
    public float explosionMaxDistance = 500f;

    [Header("Callbacks")]
    public LauncherFire launcher;

    private Rigidbody rb;

    private Collider[] allCols;
    private Vector3 flyDir;
    private bool armed = false;
    private bool isLaunched = false;
    private bool hasExploded = false;
    private GameObject smokeTrail;

    private float inputYaw;
    private float inputPitch;

    private float castRadius;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        // Kinematic flight (we drive movement), physics won't mess with it
        rb.useGravity = false;
        rb.isKinematic = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        allCols = GetComponentsInChildren<Collider>(true);

        Destroy(gameObject, lifeTime);

        flyDir = transform.forward.normalized;

        // Combined bounds from ALL colliders (capsule + box)
        if (allCols != null && allCols.Length > 0)
        {
            Bounds b = allCols[0].bounds;
            for (int i = 1; i < allCols.Length; i++)
                b.Encapsulate(allCols[i].bounds);

            // Use an XY-ish radius: extents magnitude can be too big on long missiles
            float approxRadius = Mathf.Max(b.extents.x, b.extents.y) * castRadiusMultiplier;
            castRadius = Mathf.Max(0.03f, approxRadius);
        }
        else
        {
            castRadius = 0.05f;
        }
    }

    void Start()
    {
        if (smokeTrailPrefab != null)
        {
            smokeTrail = Instantiate(smokeTrailPrefab, transform.position, transform.rotation);
            smokeTrail.transform.parent = transform;

            ParticleSystem ps = smokeTrail.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                var renderer = ps.GetComponent<Renderer>();
                if (renderer != null) renderer.enabled = false;
            }

            smokeTrail.SetActive(false);
        }
    }

    public void Launch(Vector3 direction, float launchSpeed, Collider ownerCollider = null)
    {
        if (isLaunched) return;

        flyDir = direction.normalized;
        speed = launchSpeed;

        // Ignore collisions with owner for ALL missile colliders
        if (ownerCollider != null && allCols != null)
        {
            var ownerCols = ownerCollider.GetComponentsInParent<Collider>();
            foreach (var oc in ownerCols)
            {
                foreach (var mc in allCols)
                    Physics.IgnoreCollision(mc, oc, true);
            }
        }

        rb.position = transform.position;
        rb.rotation = StableLookRotation(flyDir);

        Invoke(nameof(Arm), armDelay);
        isLaunched = true;

        StartCoroutine(StartSmokeTrail());
    }

    void Arm() => armed = true;

    IEnumerator StartSmokeTrail()
    {
        yield return null;

        if (smokeTrail != null && !hasExploded)
        {
            smokeTrail.SetActive(true);
            smokeTrail.transform.position = transform.position - transform.forward * 0.5f;

            ParticleSystem ps = smokeTrail.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                ps.Play();
            }
        }
    }

    void Update()
    {
        inputYaw = 0f;
        inputPitch = 0f;

        if (useMouse)
        {
            inputYaw = Input.GetAxis("Mouse X");
            inputPitch = -Input.GetAxis("Mouse Y");
        }
        else
        {
            if (Input.GetKey(KeyCode.A)) inputYaw = -1f;
            if (Input.GetKey(KeyCode.D)) inputYaw = 1f;
            if (Input.GetKey(KeyCode.W)) inputPitch = 1f;
            if (Input.GetKey(KeyCode.S)) inputPitch = -1f;
        }

        if (smokeTrail != null && smokeTrail.activeSelf && !hasExploded)
        {
            smokeTrail.transform.position = transform.position;
            smokeTrail.transform.rotation = transform.rotation;
        }
    }

    void FixedUpdate()
    {
        if (!isLaunched || hasExploded) return;

        float dt = Time.fixedDeltaTime;

        // 1) YAW around WORLD UP (so left/right is consistent)
        Quaternion yawQ = Quaternion.AngleAxis(inputYaw * turnRate * dt, Vector3.up);

        // 2) PITCH around missile's "right" axis, computed from current direction and WORLD UP
        Vector3 rightAxis = Vector3.Cross(Vector3.up, flyDir);
        if (rightAxis.sqrMagnitude < 1e-6f)
            rightAxis = transform.right;
        else
            rightAxis.Normalize();

        Quaternion pitchQ = Quaternion.AngleAxis(inputPitch * turnRate * dt, rightAxis);

        Vector3 newDir = (yawQ * pitchQ * flyDir).normalized;

        // 3) Clamp pitch so it can't flip and start spiraling
        newDir = ClampPitchToWorldUp(newDir, maxPitchAngle);

        flyDir = newDir;

        // Move step
        Vector3 currentPos = rb.position;
        Vector3 step = flyDir * speed * dt;
        float dist = step.magnitude;

        // SphereCast for stable hits
        if (dist > 0.0001f)
        {
            if (Physics.SphereCast(currentPos, castRadius, flyDir, out RaycastHit hit, dist + castSkin, ~0, QueryTriggerInteraction.Ignore))
            {
                Vector3 hitPos = hit.point - flyDir * castSkin;
                rb.MovePosition(hitPos);

                // Keep upright (remove roll drift)
                Quaternion targetRot = StableLookRotation(flyDir);
                rb.MoveRotation(ApplyUpright(targetRot, rollUprightStrength, dt));

                OnHit(hit);
                return;
            }
        }

        rb.MovePosition(currentPos + step);

        Quaternion rot = StableLookRotation(flyDir);
        rb.MoveRotation(ApplyUpright(rot, rollUprightStrength, dt));
    }

    Quaternion StableLookRotation(Vector3 dir)
    {
        // Use world up so roll doesn't get introduced automatically
        if (dir.sqrMagnitude < 1e-6f) dir = transform.forward;
        return Quaternion.LookRotation(dir, Vector3.up);
    }

    Quaternion ApplyUpright(Quaternion target, float strength, float dt)
    {
        if (strength <= 0f) return target;

        // Gently blend towards an upright rotation (same forward, world-up as up)
        Quaternion upright = Quaternion.LookRotation(target * Vector3.forward, Vector3.up);
        float t = 1f - Mathf.Exp(-strength * dt);
        return Quaternion.Slerp(target, upright, t);
    }

    Vector3 ClampPitchToWorldUp(Vector3 dir, float maxAngle)
    {
        // Clamp angle between dir and horizontal plane (relative to world up)
        // maxAngle=80 means can go 80 degrees up or down, but not 90+ (flip)
        float upDot = Vector3.Dot(dir, Vector3.up); // -1..1
        float maxUpDot = Mathf.Sin(maxAngle * Mathf.Deg2Rad); // sin(80deg) ~ 0.985

        upDot = Mathf.Clamp(upDot, -maxUpDot, maxUpDot);

        // Rebuild direction with same yaw projection but clamped vertical
        Vector3 horiz = Vector3.ProjectOnPlane(dir, Vector3.up);
        if (horiz.sqrMagnitude < 1e-6f)
            horiz = Vector3.ProjectOnPlane(transform.forward, Vector3.up);

        horiz.Normalize();

        // dir = horiz * cos + up * sin
        float sin = upDot;
        float cos = Mathf.Sqrt(Mathf.Max(0f, 1f - sin * sin));
        return (horiz * cos + Vector3.up * sin).normalized;
    }

    private void OnHit(RaycastHit hit)
    {
        if (!armed || hasExploded) return;

        Debug.Log($"MISSILE HIT: {hit.collider.gameObject.name}");

        bool hitTarget = false;

        TankHealth tank = hit.collider.gameObject.GetComponentInParent<TankHealth>();
        if (tank != null)
        {
            hitTarget = true;
            tank.TakeDamage(25f);

            if (hitEffectPrefab != null)
                Instantiate(hitEffectPrefab, hit.point, Quaternion.identity);
        }

        Explode(hitTarget);
    }

    void OnCollisionEnter(Collision collision)
    {
        // fallback only (should rarely happen with kinematic + casts)
        if (!armed || hasExploded) return;

        bool hitTarget = false;
        TankHealth tank = collision.gameObject.GetComponentInParent<TankHealth>();
        if (tank != null)
        {
            hitTarget = true;
            tank.TakeDamage(25f);

            if (hitEffectPrefab != null)
                Instantiate(hitEffectPrefab, collision.contacts[0].point, Quaternion.identity);
        }

        Explode(hitTarget);
    }

    void Explode(bool hitTarget)
    {
        if (hasExploded) return;
        hasExploded = true;

        if (explosionPrefab != null)
        {
            GameObject explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);

            ExplosionController controller = explosion.GetComponent<ExplosionController>();
            if (controller != null)
                controller.TriggerExplosion(transform.position, hitTarget);
            else
                Destroy(explosion, explosionLife);
        }

        AudioClip clip = explosionSound;
        if (clip == null && !string.IsNullOrEmpty(explosionSoundResourcesPath))
            clip = Resources.Load<AudioClip>(explosionSoundResourcesPath);

        if (clip != null)
            AudioUtil.Play3DClipAtPosition(clip, transform.position, explosionAudioVolume, explosionMinDistance, explosionMaxDistance);

        if (smokeTrail != null)
        {
            smokeTrail.transform.parent = null;
            ParticleSystem ps = smokeTrail.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                var emission = ps.emission;
                emission.enabled = false;
            }
            Destroy(smokeTrail, 5f);
        }

        HideMissile();
        Destroy(gameObject, 0.5f);

        if (launcher != null)
            launcher.OnMissileDestroyed();
    }

    void HideMissile()
    {
        foreach (var r in GetComponentsInChildren<MeshRenderer>())
            r.enabled = false;

        // disable ALL colliders
        if (allCols != null)
            foreach (var c in allCols)
                if (c != null) c.enabled = false;

        if (rb != null)
            rb.isKinematic = true;
    }

    void OnDestroy()
    {
        if (launcher != null)
            launcher.OnMissileDestroyed();
    }
}
