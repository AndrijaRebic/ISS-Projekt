using UnityEngine;

public class SimpleLever : MonoBehaviour
{

    public float upAngle = 0f;
    public float downAngle = -45f;

    public float rotationSpeed = 5f;

    private bool isDown = false;
    private float targetAngle;
    public KeyCode toggleKey = KeyCode.O;

    [Header("Tank Reference")]
    public TankController tank;

    void Start()
    {
        targetAngle = upAngle;
    }

    void Update()
    {
        Vector3 currentRotation = transform.localEulerAngles;
        float angle = Mathf.LerpAngle(currentRotation.z, targetAngle, Time.deltaTime * rotationSpeed);
        transform.localEulerAngles = new Vector3(currentRotation.x, currentRotation.y, angle);

        if (Input.GetKeyDown(toggleKey))
        {
            ToggleLever();
        }
    }

    void OnMouseDown()
    {
        ToggleLever();
    }

    private void ToggleLever()
    {
        InputBlocker.BlockFireForOneFrame();

        isDown = !isDown;
        targetAngle = isDown ? downAngle : upAngle;

        if (tank != null)
        {
            tank.isEngineOn = isDown;
        }

        //zvuk
        if (TankAudioController.Instance != null)
        {
            if (isDown)
            {
                TankAudioController.Instance.StartMotor();
            }
            else
            {
                TankAudioController.Instance.StopMotor();
            }
        }
    }
}
