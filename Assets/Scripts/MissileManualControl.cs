using UnityEngine;
using System.Collections;

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
        // Create but don't start smoke trail
        if (smokeTrailPrefab != null)
        {
            smokeTrail = Instantiate(smokeTrailPrefab, transform.position,transform.rotation);
            smokeTrail.transform.parent = transform;
            ParticleSystem ps = smokeTrail.GetComponent<ParticleSystem>();
            if (ps != null) { ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
             var renderer = ps.GetComponent<Renderer>();
            if (renderer != null)
                renderer.enabled = false;
        }
        
        // Hide the entire smoke trail GameObject
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

        // ✅ UNITY 6
        rb.linearVelocity = flyDir * speed;
        
        Invoke(nameof(Arm), armDelay);

        isLaunched = true;

         // Start smoke trail AFTER a tiny delay
        StartCoroutine(StartSmokeTrail());
    }

    void Arm() => armed = true;

     
    

     IEnumerator StartSmokeTrail()
    {
        // Wait for next frame - missile will have moved
        yield return null;

        if (smokeTrail != null  && !hasExploded)
        {
             smokeTrail.SetActive(true);
             smokeTrail.transform.position = transform.position - transform.forward * 0.5f;
            ParticleSystem ps = smokeTrail.GetComponent<ParticleSystem>();
            if (ps != null) {
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

        rb.linearVelocity = flyDir * speed;
        rb.MoveRotation(Quaternion.LookRotation(flyDir));
    }

    void Update() {
         // Update smoke trail position
         if (smokeTrail != null && smokeTrail.activeSelf && !hasExploded)
        {
            smokeTrail.transform.position = transform.position - transform.forward * 0.5f;
            smokeTrail.transform.rotation = transform.rotation;
        }
    }


    void OnCollisionEnter(Collision collision)
    {
        if (!armed) return;

        Debug.Log($"MISSILE HIT: {collision.gameObject.name}");

        bool hitTarget = false;

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

        Explode(hitTarget);
    }

     void Explode(bool hitTarget)
    {
        hasExploded = true;
        
        // Explosion effect
        if (explosionPrefab != null)
        {
            GameObject explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            
            ExplosionController controller = explosion.GetComponent<ExplosionController>();
            if (controller != null)
            {
                controller.TriggerExplosion(transform.position, hitTarget);
            }
            else
            {
                Destroy(explosion, explosionLife);
            }
        }

        // Play explosion sound using helper (Inspector clip preferred, otherwise Resources fallback)
        AudioClip clip = explosionSound;
        if (clip == null && !string.IsNullOrEmpty(explosionSoundResourcesPath))
            clip = Resources.Load<AudioClip>(explosionSoundResourcesPath);
        if (clip != null)
            AudioUtil.Play3DClipAtPosition(clip, transform.position, explosionAudioVolume, explosionMinDistance, explosionMaxDistance);

        // Smoke trail cleanup
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
        
        // Hide missile
        HideMissile();
        
        // Destroy
        Destroy(gameObject, 0.5f);
        
        if (launcher != null)
            launcher.OnMissileDestroyed();
    }


    void HideMissile()
    {
        MeshRenderer[] renderers = GetComponentsInChildren<MeshRenderer>();
        foreach (var renderer in renderers)
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
