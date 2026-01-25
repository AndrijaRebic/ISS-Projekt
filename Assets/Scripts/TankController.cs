using UnityEngine;

public class TankController : MonoBehaviour
{
    public enum DriveModel
    {
        PhysX,
        Differencial
    }

    [Header("PhysX")]
    public float moveSpeed = 10f;
    public float turnSpeed = 150f;

    public float forceMultiplier = 200f;
    public float torqueMultiplier = 200f;

    public bool controlsEnabled = true;

    [HideInInspector]
    public bool isEngineOn = false;

    float moveInput;
    float turnInput;

    Rigidbody rb;


    [Header("Drive Model")]
    public DriveModel model = DriveModel.PhysX;
    public KeyCode toggleKey = KeyCode.L;


    [Header("Differencial: forces, no moments")]
    [Tooltip("Maksimalna pogonska sila prije trenja.")]
    public float maxDriveForce = 8000f;

    [Tooltip("Zamjena za moment.")]
    public float turnRateDeg = 90f;

    [Tooltip("Bočni grip: sila koja guši bočnu brzinu.")]
    public float lateralGrip = 2000f;

    [Tooltip("Dodatni otpor.")]
    public float extraForwardDamping = 0f;

    [Header("Differencial: stability")]
    [Tooltip("Koliko brzo gas raste (1/s).")]
    public float throttleRise = 2.5f;

    [Tooltip("Koliko brzo gas pada (1/s).")]
    public float throttleFall = 4.0f;

    [Tooltip("Koeficijent prijanjanja za limit pogonske sile.")]
    public float tractionMu = 2.06f;

    [Tooltip("Smanjenje pogona prilikom skretanja.")]
    [Range(0f, 0.8f)]
    public float turnDriveReduction = 0.25f;

    float throttle;


    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (!isEngineOn || !controlsEnabled)
        {
            moveInput = 0f;
            turnInput = 0f;
            return;
        }

        if (Input.GetKeyDown(toggleKey))
        {
            model = (model == DriveModel.PhysX) ? DriveModel.Differencial : DriveModel.PhysX;
            rb.angularVelocity = Vector3.zero;
            throttle = 0f;

            Debug.Log("Tank model switched to: " + model);
        }

        moveInput = 0f;
        turnInput = 0f;

        // Arrow key controls
        if (Input.GetKey(KeyCode.UpArrow)) moveInput = 1f;   // forward
        if (Input.GetKey(KeyCode.DownArrow)) moveInput = -1f;  // backward

        if (Input.GetKey(KeyCode.LeftArrow)) turnInput = -1f;  // turn left
        if (Input.GetKey(KeyCode.RightArrow)) turnInput = 1f;   // turn right
    }

    void FixedUpdate()
    {
        if (!isEngineOn) return;

        if (model == DriveModel.PhysX)
        {
            rb.AddForce(transform.forward * moveInput * moveSpeed * forceMultiplier,
                    ForceMode.Force);                                  // [web:38][web:96]

            rb.AddTorque(Vector3.up * turnInput * turnSpeed * torqueMultiplier,
                         ForceMode.Force);                                 // [web:84][web:96
            return;
        }

        // Differencial
        float dt = Time.fixedDeltaTime;

        // (1) yaw direktno (bez momenta)
        float yawDelta = turnInput * turnRateDeg * dt;
        Quaternion yawRot = Quaternion.Euler(0f, yawDelta, 0f);
        rb.MoveRotation(rb.rotation * yawRot);

        Vector3 forward = rb.rotation * Vector3.forward;
        Vector3 right = rb.rotation * Vector3.right;

        Vector3 v = rb.linearVelocity;

        // (2) Smooth throttle
        float targetThrottle = moveInput;
        float rate = (Mathf.Abs(targetThrottle) > Mathf.Abs(throttle)) ? throttleRise : throttleFall;
        throttle = Mathf.MoveTowards(throttle, targetThrottle, rate * dt);

        // (3) Smanji pogon dok skreće
        float turnFactor = 1f - (Mathf.Abs(turnInput) * turnDriveReduction);
        float commandedForce = throttle * maxDriveForce * turnFactor;

        // (4) Traction limit: |F| <= mu*m*g
        float tractionLimit = tractionMu * rb.mass * Physics.gravity.magnitude;
        float driveForce = Mathf.Clamp(commandedForce, -tractionLimit, tractionLimit);
        Vector3 F_drive = forward * driveForce;

        // (5) Lateral grip: guši bočno klizanje
        float vRight = Vector3.Dot(v, right);
        Vector3 F_lat = -right * (vRight * lateralGrip);

        // (6) Opcionalni dodatni forward damping (ako treba)
        float vForward = Vector3.Dot(v, forward);
        Vector3 F_fwdDamp = (extraForwardDamping <= 0f) ? Vector3.zero : (-forward * (vForward * extraForwardDamping));

        Vector3 F = F_drive + F_lat + F_fwdDamp;

        // Primjena sila
        rb.AddForce(F, ForceMode.Force);
    }
}
