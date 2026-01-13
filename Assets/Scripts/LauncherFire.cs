// LauncherFire.cs
using UnityEngine;
using System.Collections;

public class LauncherFire : MonoBehaviour
{
    public Transform firePoint;
    public GameObject missilePrefab;

    //NOVO
    public Camera missileCamera;
    public Transform missileCarrier; // launcher / nosač
    private GameObject activeMissile;


    public float missileSpeed = 10f;

    // skala klona (kao tvoj launcher 0.15)
    public float cloneScale = 0.10f;

    // pomak da ne zapne u launcheru
    public float spawnForwardOffset = 0.0f;
    public float spawnUpOffset = 0.05f;

    public KeyCode fireKey = KeyCode.Space;

    void Update()
    {
        if (Input.GetKeyDown(fireKey) && activeMissile == null)
        {
            Fire();
        }
    }


    void Fire()
    {
        if (firePoint == null || missilePrefab == null)
        {
            Debug.LogError("Postavi FirePoint i Missile Prefab u Inspectoru!");
            return;
        }

        // Spawn pozicija: točno iz firePointa + mali offset da ne kolidira odmah
        Vector3 spawnPos = firePoint.position
                         + firePoint.forward * spawnForwardOffset
                         + Vector3.up * spawnUpOffset;

        // Rotacija: da raketa gleda gore
        Quaternion spawnRot = Quaternion.LookRotation(Vector3.up, Vector3.up);

        // Ako želiš da gleda smjer cijevi umjesto gore, koristi ovo:
        // Quaternion spawnRot = firePoint.rotation;

        GameObject missile = Instantiate(missilePrefab, spawnPos, spawnRot);

        //NOVO
        var manual = missile.GetComponent<MissileManualControl>();
        if (manual != null)
        {
            manual.launcher = this;
        }

        activeMissile = missile;

        var camFollow = missileCamera.GetComponent<MissileCameraFollow>();
        if (camFollow != null)
        {
            camFollow.target = missile.transform;
        }


        // Odvoji od parenta
        missile.transform.SetParent(null, true);

        // Scale odmah + opet idući frame (da prepiše druge skripte ako resetiraju)
        ForceScale(missile.transform);
        StartCoroutine(ForceScaleNextFrame(missile.transform));

        // Smjer leta:
        Vector3 dir = Vector3.up;               // uvijek gore ✅
        // Ako želiš smjer cijevi, prebaci na:
        // Vector3 dir = firePoint.forward;

        // Ako ima manual control, njemu predaj launch parametre
        //var manual = missile.GetComponent<MissileManualControl>();
        if (manual != null)
        {
            manual.Launch(dir, missileSpeed);
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
        rb.linearVelocity = dir.normalized * missileSpeed;
    }

    //NOVO
    public void OnMissileDestroyed()
    {
        activeMissile = null;

        var camFollow = missileCamera.GetComponent<MissileCameraFollow>();
        if (camFollow != null)
        {
            camFollow.target = missileCarrier;
        }
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
