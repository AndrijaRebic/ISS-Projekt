using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MissileManualControl : MonoBehaviour
{
    public float speed = 120f;          // stalna brzina naprijed
    public float turnRate = 90f;        // stupnjevi u sekundi (koliko brzo skreće)
    public bool useMouse = false;       // opcionalno: miš umjesto tipki

    Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
    }

    void FixedUpdate()
    {
        // Input: strelice ili WASD
        float yaw = Input.GetAxis("Horizontal");   // A/D ili Left/Right
        float pitch = -Input.GetAxis("Vertical");  // W/S ili Up/Down (minus da je prirodnije)

        // Opcija: miš
        if (useMouse)
        {
            yaw = Input.GetAxis("Mouse X");
            pitch = -Input.GetAxis("Mouse Y");
        }

        // Rotacija rakete
        transform.Rotate(pitch * turnRate * Time.fixedDeltaTime,
                         yaw   * turnRate * Time.fixedDeltaTime,
                         0f,
                         Space.Self);

        // Kretanje naprijed
        rb.linearVelocity = transform.forward * speed;
    }
}
