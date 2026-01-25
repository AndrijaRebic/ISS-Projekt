// LauncherFire.cs
using UnityEngine;
using System.Collections;

public class LauncherFire : MonoBehaviour
{
    public enum GuidanceMode { ManualConstantVelocity, DynamicEulerForces }

    [Header("Mode")]
    public GuidanceMode guidanceMode = GuidanceMode.ManualConstantVelocity;
    public KeyCode toggleModeKey = KeyCode.T;

    [Header("Setup")]
    public Transform firePoint;
    public GameObject missilePrefab;

    public Camera missileCamera;
    public Transform missileCarrier; // launcher / nosač
    private GameObject activeMissile;

    [Header("Missile params")]
    public float missileSpeed = 44f;

    [Header("Spawn offsets")]
    public float cloneScale = 0.10f;
    public float spawnForwardOffset = 0.2f;
    public float spawnUpOffset = 0.00f;

    [Header("Input")]
    public KeyCode fireKey = KeyCode.Space;

    [Header("Audio")]
    public AudioClip fireSound;
    public string fireSoundResourcesPath = "Free Pack/MissileShot";
    public float fireAudioVolume = 1f;
    public float fireMinDistance = 1f;
    public float fireMaxDistance = 50f;

    public bool HasActiveMissile => activeMissile != null;

    void Update()
    {
        if (Input.GetKeyDown(toggleModeKey))
        {
            guidanceMode = (guidanceMode == GuidanceMode.ManualConstantVelocity)
                ? GuidanceMode.DynamicEulerForces
                : GuidanceMode.ManualConstantVelocity;

            Debug.Log($"[LauncherFire] Mode switched to: {guidanceMode}");
        }

        if (Input.GetKeyDown(fireKey))
        {
            Debug.Log($"[LauncherFire] Space on GO='{gameObject.name}' instanceID={gameObject.GetInstanceID()} compID={GetInstanceID()} activeMissile={(activeMissile ? activeMissile.name : "null")}", this);
        }

        if (Input.GetKeyDown(fireKey) && activeMissile == null)
            Fire();
    }

    void Fire()
    {
        Debug.Log($"Firing missile! missileSpeed={missileSpeed} mode={guidanceMode}");

        if (firePoint == null || missilePrefab == null)
        {
            Debug.LogError("Postavi FirePoint i Missile Prefab u Inspectoru!");
            return;
        }

        Vector3 spawnPos = firePoint.position
                         + firePoint.forward * spawnForwardOffset
                         + firePoint.up * spawnUpOffset;

        Quaternion spawnRot = firePoint.rotation;

        GameObject missile = Instantiate(missilePrefab, spawnPos, spawnRot);
        activeMissile = missile;

        // Play 3D launch sound at spawn position
        AudioClip clipToPlay = fireSound;
        if (clipToPlay == null && !string.IsNullOrEmpty(fireSoundResourcesPath))
        {
            clipToPlay = Resources.Load<AudioClip>(fireSoundResourcesPath);
        }
        if (clipToPlay != null)
        {
            Play3DClipAtPosition(clipToPlay, spawnPos, fireAudioVolume, fireMinDistance, fireMaxDistance);
        }

        // Kamera prati raketu
        var camFollow = missileCamera != null ? missileCamera.GetComponent<MissileCameraFollow>() : null;
        if (camFollow != null)
            camFollow.target = missile.transform;

        missile.transform.SetParent(null, true);

        // Scale
        ForceScale(missile.transform);
        StartCoroutine(ForceScaleNextFrame(missile.transform));

        // Smjer leta = smjer cijevi
        Vector3 dir = firePoint.forward;

        // Owner collider (za ignore)
        Collider ownerCol = GetComponentInParent<Collider>();

        // Prefer: prepoznaj koji skript je prisutan i pokreni pravi launch
        var manual = missile.GetComponent<MissileManualControl>();
        var euler  = missile.GetComponent<MissileDynamicEuler>();

        if (guidanceMode == GuidanceMode.DynamicEulerForces && euler != null)
        {
            euler.launcher = this;
            euler.motionMode = MissileDynamicEuler.MotionMode.EulerForces; // obavezno Euler
            euler.LaunchEuler(dir, missileSpeed, ownerCol);
            return;
        }

        if (guidanceMode == GuidanceMode.ManualConstantVelocity && manual != null)
        {
            manual.launcher = this;
            manual.Launch(dir, missileSpeed, ownerCol);
            return;
        }

        // Fallback: ako nema očekivanih skripti, bar pošalji rigidbody naprijed
        Rigidbody rb = missile.GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("Missile prefab nema Rigidbody!");
            return;
        }

        rb.useGravity = false;
        rb.isKinematic = false;
        rb.linearVelocity = dir.normalized * missileSpeed;
    }

    public void OnMissileDestroyed()
    {
        activeMissile = null;

        var camFollow = missileCamera != null ? missileCamera.GetComponent<MissileCameraFollow>() : null;
        if (camFollow != null)
            camFollow.target = missileCarrier;
    }

    void ForceScale(Transform t) => t.localScale = Vector3.one * cloneScale;

    IEnumerator ForceScaleNextFrame(Transform t)
    {
        yield return null;
        if (t != null) ForceScale(t);
    }

    // Helper - create temporary AudioSource at position with 3D settings
    void Play3DClipAtPosition(AudioClip clip, Vector3 position, float volume, float minDistance, float maxDistance)
    {
        GameObject go = new GameObject("OneShotAudio_Fire");
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
