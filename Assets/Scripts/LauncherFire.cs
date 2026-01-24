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
        var euler  = missile.GetComponent<MissileDynamicsEuler>();

        if (guidanceMode == GuidanceMode.DynamicEulerForces && euler != null)
        {
            euler.launcher = this;
            euler.motionMode = MissileDynamicsEuler.MotionMode.EulerForces; // obavezno Euler
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
}
