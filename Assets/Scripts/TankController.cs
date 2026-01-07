using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class TankController : MonoBehaviour
{
    public float moveSpeed = 10f;
    public float turnSpeed = 150f;

    float moveInput;
    float turnInput;

    Rigidbody rb;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    
    [HideInInspector]
    public bool isEngineOn = false;
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {

        if (!isEngineOn)
        {
            moveInput = 0f;
            turnInput = 0f;
            return;
        }


        moveInput = 0f;
        turnInput = 0f;

        if (Input.GetKey(KeyCode.W)) moveInput = 1f;
        if (Input.GetKey(KeyCode.S)) moveInput = -1f;

        if (Input.GetKey(KeyCode.A)) turnInput = -1f;
        if (Input.GetKey(KeyCode.D)) turnInput = 1f;

    }

    private void FixedUpdate()
    {
        if (!isEngineOn) return;
        rb.AddForce(transform.forward * moveInput * moveSpeed, ForceMode.Force);
        rb.AddTorque(Vector3.up * turnInput * turnSpeed, ForceMode.Force);
    }
}
