using UnityEngine;
using UnityEngine.InputSystem;

public class TankShooting : MonoBehaviour
{
    [Header("Cannon")]
    public Transform cannonMuzzle;
    public GameObject cannonShellPrefab;
    public GameObject cannonMuzzleFlashPrefab; // NEW
    public float cannonForce = 1500f;
    public float cannonCooldown = 1.5f;

    [Header("Machine Gun")]
    public Transform machineGunMuzzle;
    public GameObject machineGunBulletPrefab;
    public GameObject lmgMuzzleFlashPrefab;    // NEW
    public float machineGunForce = 800f;
    public float machineGunFireRate = 0.1f;

    float cannonTimer;
    float machineGunTimer;

    void Update()
    {
        cannonTimer += Time.deltaTime;
        machineGunTimer += Time.deltaTime;

        HandleCannon();
        HandleMachineGun();
    }

    void HandleCannon()
    {
        if (Input.GetKeyDown(KeyCode.Space) && cannonTimer >= cannonCooldown)
        {
            cannonTimer = 0f;

            GameObject shell = Instantiate(
                cannonShellPrefab,
                cannonMuzzle.position,
                cannonMuzzle.rotation
            );

            Rigidbody rb = shell.GetComponent<Rigidbody>();
            rb.AddForce(cannonMuzzle.forward * cannonForce);

            // Muzzle flash
            if (cannonMuzzleFlashPrefab != null)
            {
                Instantiate(
                    cannonMuzzleFlashPrefab,
                    cannonMuzzle.position,
                    cannonMuzzle.rotation
                );
            }
        }
    }

    void HandleMachineGun()
    {
        if (Input.GetKey(KeyCode.M) && machineGunTimer >= machineGunFireRate)
        {
            machineGunTimer = 0f;

            GameObject bullet = Instantiate(
                machineGunBulletPrefab,
                machineGunMuzzle.position,
                machineGunMuzzle.rotation
            );

            Rigidbody rb = bullet.GetComponent<Rigidbody>();
            rb.AddForce(machineGunMuzzle.forward * machineGunForce);

            // Muzzle flash
            Quaternion flashRotation = machineGunMuzzle.rotation * Quaternion.Euler(180f, 0f, 0f);

            if (lmgMuzzleFlashPrefab != null)
            {
                Instantiate(
                    lmgMuzzleFlashPrefab,
                    machineGunMuzzle.position,
                    flashRotation
                );
            }
        }
    }
}

