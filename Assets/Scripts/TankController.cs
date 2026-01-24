using UnityEngine;

public class TankController : MonoBehaviour
{
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



        rb.AddForce(transform.forward * moveInput * moveSpeed * forceMultiplier,
                    ForceMode.Force);                                  // [web:38][web:96]

        rb.AddTorque(Vector3.up * turnInput * turnSpeed * torqueMultiplier,
                     ForceMode.Force);                                 // [web:84][web:96]
    }
}
