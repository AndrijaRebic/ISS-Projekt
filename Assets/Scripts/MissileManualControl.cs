using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class MissileManualControl : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 40f;
    public float turnRate = 120f;
    public bool useMouse = false;
    public float lifeTime = 15f;

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
    private Vector3 flyDir;
    private bool armed = false;
    private bool isLaunched = false;
    private bool hasExploded = false;
    private GameObject smokeTrail;

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

        rb.rotation = Quaternion.LookRotation(flyDir);

        if (ownerCollider != null)
        {
            var myCol = GetComponent<Collider>();
            var cols = ownerCollider.GetComponentsInParent<Collider>();
            foreach (var c in cols)
                Physics.IgnoreCollision(myCol, c, true);
        }

        rb.linearVelocity = flyDir * speed;

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

    void FixedUpdate()
    {
        float yaw = 0f;
        float pitch = 0f;

        if (useMouse)
        {
            yaw = Input.GetAxis("Mouse X");
            pitch = -Input.GetAxis("Mouse Y");
        }
        else
        {
            if (Input.GetKey(KeyCode.A)) yaw = -1f;
            if (Input.GetKey(KeyCode.D)) yaw = 1f;
            if (Input.GetKey(KeyCode.W)) pitch = 1f;
            if (Input.GetKey(KeyCode.S)) pitch = -1f;
        }

        Quaternion delta = Quaternion.Euler(
            pitch * turnRate * Time.fixedDeltaTime,
            yaw * turnRate * Time.fixedDeltaTime,
            0f
        );

        flyDir = (delta * flyDir).normalized;

        rb.isKinematic = false;
        rb.linearVelocity = flyDir * speed;
        rb.MoveRotation(Quaternion.LookRotation(flyDir));
    }

    void Update()
    {
        if (smokeTrail != null && smokeTrail.activeSelf && !hasExploded)
        {
            smokeTrail.transform.position = transform.position;
            smokeTrail.transform.rotation = transform.rotation;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!armed || hasExploded) return;

        Debug.Log($"MISSILE HIT: {collision.gameObject.name}");

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
        foreach (var renderer in GetComponentsInChildren<MeshRenderer>())
            renderer.enabled = false;

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.isKinematic = true;
        }
    }

    void OnDestroy()
    {
        if (launcher != null)
            launcher.OnMissileDestroyed();
    }
}
