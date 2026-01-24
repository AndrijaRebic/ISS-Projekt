using UnityEngine;

public class TurretController : MonoBehaviour
{
    [Header("Rotation Settings")]
    public float rotationSpeed = 60f;

    [Header("Cannon")]
    public Transform cannon;
    public float cannonSpeed = 30f;
    public float minPitch = -10f;
    public float maxPitch = 20f;

    float currentPitch = 0f;

    [Header("Engine State")]
    public TankController tank;

    void Update()
    {
        if (tank != null && !tank.isEngineOn) return;

        RotateTurretWithMouse();
        RotateCannonWithMouse();
    }

    void RotateTurretWithMouse()
    {
        float mouseX = Input.GetAxis("Mouse X"); // horizontal mouse delta [web:4]
        transform.Rotate(Vector3.up, mouseX * rotationSpeed * Time.deltaTime);
    }

    void RotateCannonWithMouse()
    {
        if (cannon == null) return;

        float mouseY = Input.GetAxis("Mouse Y"); // vertical mouse delta [web:4]

        // invert so moving mouse up raises barrel
        currentPitch -= mouseY * cannonSpeed * Time.deltaTime;
        currentPitch = Mathf.Clamp(currentPitch, minPitch, maxPitch);

        cannon.localRotation = Quaternion.Euler(currentPitch, 0f, 0f);
    }
}
