// LauncherFire.cs
using UnityEngine;
using System.Collections;

public class LauncherFire : MonoBehaviour
{
    public Transform firePoint;
    public GameObject missilePrefab;

    public Camera missileCamera;
    public Transform missileCarrier; // launcher / nosač
    private GameObject activeMissile;

    public float missileSpeed = 44f;

    public float cloneScale = 0.10f;

    public float spawnForwardOffset = 0.2f;
    public float spawnUpOffset = 0.00f;

    public KeyCode fireKey = KeyCode.Space;

    public bool HasActiveMissile => activeMissile != null;

    void Update()
    {
        if (Input.GetKeyDown(fireKey))
        {
            Debug.Log($"[LauncherFire] Space on GO='{gameObject.name}' instanceID={gameObject.GetInstanceID()} compID={GetInstanceID()} activeMissile={(activeMissile ? activeMissile.name : "null")}", this);
        }

        if (Input.GetKeyDown(fireKey) && activeMissile == null)
            Fire();
    }


    void Fire()
    {
        Debug.Log($"Firing missile! missileSpeed={missileSpeed}");

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

        // Manual control
        var manual = missile.GetComponent<MissileManualControl>();
        if (manual != null)
        {
            manual.launcher = this;
            manual.Launch(dir, missileSpeed, ownerCol);
            return;
        }

        // Fallback ako nema manual skriptu
        Rigidbody rb = missile.GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("Missile prefab nema Rigidbody!");
            return;
        }

        rb.useGravity = false;

        // ✅ PROMJENA: linearVelocity -> velocity
        rb.linearVelocity = dir.normalized * missileSpeed;
    }

    public void OnMissileDestroyed()
    {
        activeMissile = null;

        var camFollow = missileCamera != null ? missileCamera.GetComponent<MissileCameraFollow>() : null;
        if (camFollow != null)
            camFollow.target = missileCarrier;
    }

    void ForceScale(Transform t)
    {
        t.localScale = Vector3.one * cloneScale;
    }

    IEnumerator ForceScaleNextFrame(Transform t)
    {
        yield return null;
        if (t != null) ForceScale(t);
    }
}
