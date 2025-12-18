using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class TankController : MonoBehaviour
{
    public float moveSpeed = 10f;
    public float turnSpeed = 150f;

    public bool controlsEnabled = true;

    float moveInput;
    float turnInput;

    Rigidbody rb;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {

        if (!controlsEnabled){
            moveInput = 0f;
            turnInput = 0f;
            return;
        }

        moveInput = Input.GetAxis("Vertical");
        turnInput = Input.GetAxis("Horizontal");
    }

    private void FixedUpdate()
    {
        if (!controlsEnabled) return;

        rb.AddForce(transform.forward * moveInput * moveSpeed, ForceMode.Force);
        rb.AddTorque(Vector3.up * turnInput * turnSpeed, ForceMode.Force);
    }
}
