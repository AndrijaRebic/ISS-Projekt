using UnityEngine;

public class TankTOWLauncher : MonoBehaviour
{
    [Header("Tank / Engine")]
    public TankController tank;

    [Header("Launcher")]
    public Transform towMuzzle;             // missile spawn point
    public GameObject towMissilePrefab;     // SoftHomingMissile prefab
    public float towLaunchSpeed = 80f;
    public float towCooldown = 6f;

    [Header("Aim")]
    public Transform aimReference;          // usually same as your cannon/turret

    [Header("Input")]
    public KeyCode fireKey = KeyCode.P;     // choose what you like

    float towTimer;

    void Update()
    {
        towTimer += Time.deltaTime;

        Debug.Log("TOW: Update running, engineOn=" +
          (tank != null ? tank.isEngineOn.ToString() : "NO TANK REF"));


        if (tank != null && !tank.isEngineOn)
            return;

        if (Input.GetKeyDown(KeyCode.P))
        {
            Debug.Log("TOW: P pressed");
            TryFireTOW();
        }

    }


    void TryFireTOW()
    {
        Debug.Log("TOW: TryFireTOW called");

        if (towMissilePrefab == null || towMuzzle == null)
        {
            Debug.LogWarning("TOW: Missing prefab or muzzle");
            return;
        }

        if (towTimer < towCooldown)
        {
            Debug.Log("TOW: On cooldown");
            return;
        }

        towTimer = 0f;

        GameObject missileGO = Instantiate(
            towMissilePrefab,
            towMuzzle.position,
            towMuzzle.rotation
        );
        Debug.Log("TOW: Missile spawned " + missileGO.name);

        SoftHomingMissile shm = missileGO.GetComponent<SoftHomingMissile>();
        if (shm == null)
        {
            Debug.LogError("TOW: No SoftHomingMissile on prefab!");
            return;
        }

        shm.aimReference = aimReference;
        shm.Launch(towMuzzle.forward, towLaunchSpeed);
    }

}
