using UnityEngine;

public class TurretController : MonoBehaviour
{
    [Header("Rotation Settings")]
    public float rotationSpeed = 60f;

    [Header("Optional Cannon")]
    public Transform cannon;     
    public float cannonSpeed = 30f;
    public float minPitch = -10f;
    public float maxPitch = 20f;

    private float currentPitch = 0f;

    [Header("Engine State")]
    public TankController tank; 

    void Update()
    {
        if (tank != null && !tank.isEngineOn) return;

        RotateTurret();
        RotateCannon();
    }

    void RotateTurret()
    {
        float horizontal = 0f;

        if (Input.GetKey(KeyCode.LeftArrow))
            horizontal = -1f;
        else if (Input.GetKey(KeyCode.RightArrow))
            horizontal = 1f;

        transform.Rotate(Vector3.up, horizontal * rotationSpeed * Time.deltaTime);
    }

    void RotateCannon()
    {
        if (cannon == null) return;

        float vertical = 0f;

        if (Input.GetKey(KeyCode.UpArrow))
            vertical = 1f;
        else if (Input.GetKey(KeyCode.DownArrow))
            vertical = -1f;

        currentPitch += vertical * cannonSpeed * Time.deltaTime;
        currentPitch = Mathf.Clamp(currentPitch, minPitch, maxPitch);

        cannon.localRotation = Quaternion.Euler(currentPitch, 0f, 0f);
    }
}
