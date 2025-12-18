using UnityEngine;

public class LauncherFire : MonoBehaviour
{
    public Transform firePoint;        // Firepoint (child od Launchera)
    public GameObject missilePrefab;   // Missile.prefab
    public float missileSpeed = 10f;  // demo brzina
    public KeyCode fireKey = KeyCode.Space;

    public Camera missileCam;



    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("SPACE pressed -> firing");
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

        if (missileCam != null) missileCam.enabled = true;


        GameObject missile = Instantiate(missilePrefab, firePoint.position, firePoint.rotation);

        Debug.Log("Fired missile, trying to set camera target...");
        
        Rigidbody rb = missile.GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("Missile prefab nema Rigidbody!");
            return;
        }

        rb.linearVelocity = firePoint.forward * missileSpeed;

        var camFollow = FindFirstObjectByType<CameraFollow>();
        if (camFollow != null)
            camFollow.target = missile.transform;
    }
}
