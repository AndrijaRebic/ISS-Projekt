using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MissileManualControl : MonoBehaviour
{
    public float speed = 120f;          // stalna brzina
    public float turnRate = 90f;
    public bool useMouse = false;

    Rigidbody rb;

    // smjer leta (world space), jednom zadamo pri launchu
    private Vector3 flyDir = Vector3.up;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
    }

    // Pozovi ovo odmah nakon Instantiate
    public void Launch(Vector3 direction, float launchSpeed)
    {
        flyDir = direction.normalized;
        speed = launchSpeed;

        // uskladi vizual da "gleda" u smjer leta
        transform.rotation = Quaternion.LookRotation(flyDir, Vector3.up);

        rb.linearVelocity = flyDir * speed;
    }

    void FixedUpdate()
    {
        float yaw = Input.GetAxis("Horizontal");
        float pitch = -Input.GetAxis("Vertical");

        if (useMouse)
        {
            yaw = Input.GetAxis("Mouse X");
            pitch = -Input.GetAxis("Mouse Y");
        }

        // rotiraj smjer leta (a ne nužno transform.forward)
        Quaternion delta = Quaternion.Euler(
            pitch * turnRate * Time.fixedDeltaTime,
            yaw   * turnRate * Time.fixedDeltaTime,
            0f
        );

        flyDir = (delta * flyDir).normalized;

        // drži brzinu u tom smjeru
        rb.linearVelocity = flyDir * speed;

        // i okreni model da prati smjer
        transform.rotation = Quaternion.LookRotation(flyDir, Vector3.up);
    }
}
