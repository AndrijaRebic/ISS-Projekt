using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class MissileDynamicEuler : MonoBehaviour
{
    public enum MotionMode { EulerForces, UnityRigidbody }

    [Header("Mode")]
    public MotionMode motionMode = MotionMode.EulerForces;

    [Header("Mass/Forces")]
    public float mass = 5f;
    public float thrustForce = 200f;      // “pogonska sila” naprijed
    public bool thrustOn = true;
    public KeyCode toggleThrustKey = KeyCode.M;

    [Header("Aero/Gravity")]
    public bool useGravity = true;
    public float gravity = 9.81f;
    public float linearDrag = 0.1f;       // u Euler modu: Fdrag = -k*v

    [Header("Guidance (same as a)")]
    public bool useMouse = false;
    public float turnRate = 80f;

    [Header("Lifetime")]
    public float lifeTime = 15f;

    [Header("Safety")]
    public float armDelay = 0.05f;

    [Header("Collision (Euler sweep)")]
    public LayerMask collisionMask = ~0;  // sve layere
    public float sphereRadius = 0.15f;    // ako je 0 -> auto iz collidera
    public float skin = 0.02f;            // mali razmak prije kontakta

    [Header("Effects")]
    public GameObject hitEffectPrefab;
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
    private Collider myCol;

    private Vector3 flyDir;

    // Euler state
    private Vector3 vel;
    private Vector3 pos;

    private bool armed = false;
    private bool isLaunched = false;
    private bool hasExploded = false;

    private GameObject smokeTrail;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        myCol = GetComponent<Collider>();

        rb.useGravity = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        rb.linearDamping = 0f;
        rb.angularDamping = 10f;

        // auto radius iz collider bounds ako nije ručno
        if (sphereRadius <= 0f && myCol != null)
        {
            var b = myCol.bounds;
            sphereRadius = Mathf.Max(0.05f, Mathf.Min(b.extents.x, b.extents.y, b.extents.z) * 0.5f);
        }

        Destroy(gameObject, lifeTime);

        flyDir = transform.forward;
        pos = transform.position;
        vel = Vector3.zero;
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

    // Launcher će ovo zvati kad je “d” mode uključen
    public void LaunchEuler(Vector3 direction, float initialSpeed, Collider ownerCollider = null)
    {
        if (isLaunched) return;

        flyDir = direction.normalized;

        // ignore owner collision
        if (ownerCollider != null)
        {
            var cols = ownerCollider.GetComponentsInParent<Collider>();
            foreach (var c in cols)
                Physics.IgnoreCollision(myCol, c, true);
        }

        // init state
        pos = transform.position;
        vel = flyDir * initialSpeed;

        // init orientation
        transform.rotation = Quaternion.LookRotation(flyDir);

        // physics mode setup
        if (motionMode == MotionMode.EulerForces)
        {
            rb.isKinematic = true; // Unity NE smije računati gibanje
        }
        else
        {
            rb.isKinematic = false;
            rb.mass = mass;
            rb.useGravity = useGravity;
            rb.linearVelocity = vel;
        }

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
        if (!isLaunched || hasExploded) return;

        if (Input.GetKeyDown(toggleThrustKey))
            thrustOn = !thrustOn;

        // update smoke pose
        if (smokeTrail != null && smokeTrail.activeSelf)
        {
            smokeTrail.transform.position = transform.position - transform.forward * 0.5f;
            smokeTrail.transform.rotation = transform.rotation;
        }
    }

    void FixedUpdate()
    {
        if (!isLaunched || hasExploded) return;

        // upravljanje orijentacijom (bez momenata) - samo yaw
        float yaw = 0f;

        if (useMouse)
        {
            yaw = Input.GetAxis("Mouse X");
        }
        else
        {
            if (Input.GetKey(KeyCode.A)) yaw = -1f;
            if (Input.GetKey(KeyCode.D)) yaw = 1f;
        }

        float dt = Time.fixedDeltaTime;

        Quaternion delta = Quaternion.Euler(
            0f,
            yaw * turnRate * dt,
            0f
        );

        flyDir = (delta * flyDir).normalized;

        // When not controlling, return to initial forward direction for natural behavior
        if (yaw == 0f)
        {
            flyDir = transform.forward.normalized;
        }

        // Update velocity direction for path change without rotating the missile, preserving y component for gravity
        if (vel.magnitude > 0.01f)
        {
            float horizontalSpeed = new Vector3(vel.x, 0, vel.z).magnitude;
            vel = new Vector3(flyDir.x * horizontalSpeed, vel.y, flyDir.z * horizontalSpeed);
        }

        // Set rotation to face the direction, like MissileManualControl, with smoothing
        Quaternion targetRotation = Quaternion.LookRotation(flyDir);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * 5f);

        // Ako koristiš UnityRigidbody mod, OnCollisionEnter radi normalno
        if (motionMode == MotionMode.UnityRigidbody)
        {
            rb.isKinematic = false;
            rb.mass = mass;
            rb.useGravity = useGravity;

            if (thrustOn)
                rb.AddForce(transform.forward * thrustForce, ForceMode.Force);

            rb.linearDamping = linearDrag;

            // Update velocity direction for path change without rotating the missile, preserving y component for gravity
            if (rb.linearVelocity.magnitude > 0.01f)
            {
                float horizontalSpeed = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z).magnitude;
                rb.linearVelocity = new Vector3(flyDir.x * horizontalSpeed, rb.linearVelocity.y, flyDir.z * horizontalSpeed);
            }

            return;
        }

        // EulerForces: F = thrust + gravity + drag
        rb.isKinematic = true;

        Vector3 force = Vector3.zero;

        if (thrustOn)
            force += flyDir * thrustForce;

        if (useGravity)
            force += Vector3.down * (mass * gravity);

        // linear drag
        force += -linearDrag * vel;

        Vector3 acc = force / Mathf.Max(0.0001f, mass);

        // predikcija
        Vector3 newVel = vel + acc * dt;
        Vector3 newPos = pos + newVel * dt;

        // ---- SWEEP COLLISION: SphereCast od stare do nove pozicije ----
        Vector3 travel = newPos - pos;
        float dist = travel.magnitude;

        if (dist > 0.0001f)
        {
            Vector3 dir = travel / dist;

            if (Physics.SphereCast(pos, sphereRadius, dir, out RaycastHit hit, dist + skin, collisionMask, QueryTriggerInteraction.Ignore))
            {
                // Postavi na mjesto udara (malo prije) i eksplodiraj
                pos = hit.point - dir * skin;
                transform.position = pos;

                bool hitTarget = false;

                TankHealth tank = hit.collider.GetComponentInParent<TankHealth>();
                if (tank != null)
                {
                    hitTarget = true;
                    tank.TakeDamage(25f);

                    if (hitEffectPrefab != null)
                        Instantiate(hitEffectPrefab, hit.point, Quaternion.identity);
                }

                Explode(hitTarget);
                return;
            }
        }

        // nema sudara → normalno updateaj state
        vel = newVel;
        pos = newPos;
        transform.position = pos;
    }

    // Ovo može ostati (radi za UnityRigidbody mod)
    void OnCollisionEnter(Collision collision)
    {
        if (!armed || hasExploded) return;

        Debug.Log($"MISSILE HIT (Euler/UnityRB): {collision.gameObject.name}");

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
        if (hasExploded) return; // zaštita da se ne okine 2 puta
        hasExploded = true;

        // stop motion odmah
        vel = Vector3.zero;
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        if (explosionPrefab != null)
        {
            GameObject explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);

            ExplosionController controller = explosion.GetComponent<ExplosionController>();
            if (controller != null)
                controller.TriggerExplosion(transform.position, hitTarget);
            else
                Destroy(explosion, explosionLife);
        }

        // Play explosion sound
        AudioClip clip = explosionSound;
        if (clip == null && !string.IsNullOrEmpty(explosionSoundResourcesPath))
            clip = Resources.Load<AudioClip>(explosionSoundResourcesPath);
        if (clip != null)
            AudioSource.PlayClipAtPoint(clip, transform.position, explosionAudioVolume);

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
        foreach (var renderer in GetComponentsInChildren<MeshRenderer>())
            renderer.enabled = false;

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;
    }

    void OnDestroy()
    {
        if (launcher != null)
            launcher.OnMissileDestroyed();
    }

    // Helper - create temporary AudioSource at position with 3D settings
    void Play3DClipAtPosition(AudioClip clip, Vector3 position, float volume, float minDistance, float maxDistance)
    {
        GameObject go = new GameObject("OneShotAudio_Explosion");
        go.transform.position = position;
        AudioSource src = go.AddComponent<AudioSource>();
        src.clip = clip;
        src.spatialBlend = 1f; // fully 3D
        src.volume = Mathf.Clamp01(volume);
        src.minDistance = Mathf.Max(0.01f, minDistance);
        src.maxDistance = Mathf.Max(src.minDistance + 0.01f, maxDistance);
        src.rolloffMode = AudioRolloffMode.Logarithmic;
        src.Play();
        Destroy(go, clip.length + 0.1f);
    }
}
