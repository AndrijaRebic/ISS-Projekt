using UnityEngine;

public class RocketTurretRotate : MonoBehaviour
{
    [Header("Refs")]
    public LauncherFire launcherFire;   
    public Transform pitchPivot;

    [Header("Rotation speeds")]
    public float yawSpeed = 80f;
    public float pitchSpeed = 50f;

    [Header("Pitch limits")]
    public float minPitch = -10f;
    public float maxPitch = 25f;

    [Header("Input")]
    public bool useMouse = false;

    [Header("Lock while missile active")]
    public bool lockWhileMissileActive = true;

    float currentPitch;

    void Awake()
    {
        
        useMouse = false;

        
        if (launcherFire == null)
            launcherFire = GetComponentInParent<LauncherFire>();

        
        if (pitchPivot == null)
        {
            var all = GetComponentsInChildren<Transform>(true);
            foreach (var t in all)
            {
                if (t.name.ToLower().Contains("pitch"))
                {
                    pitchPivot = t;
                    break;
                }
            }
        }
    }

    void Start()
    {
        if (pitchPivot != null)
            currentPitch = NormalizeAngle(pitchPivot.localEulerAngles.x);

        Debug.Log($"[RocketTurretRotate] launcherFire={(launcherFire ? launcherFire.name : "NULL")} pitchPivot={(pitchPivot ? pitchPivot.name : "NULL")}");
    }

    void Update()
    {
        
        if (lockWhileMissileActive && launcherFire != null && launcherFire.HasActiveMissile)
            return;

        float yawInput = 0f;
        float pitchInput = 0f;

        if (useMouse)
        {
            yawInput = Input.GetAxis("Mouse X");
            pitchInput = -Input.GetAxis("Mouse Y");
        }

        else
        {
        
            if (Input.GetKey(KeyCode.A)) yawInput = -1f;
            if (Input.GetKey(KeyCode.D)) yawInput = 1f;

            if (Input.GetKey(KeyCode.W)) pitchInput = 1f;
            if (Input.GetKey(KeyCode.S)) pitchInput = -1f;
        }

        transform.Rotate(0f, yawInput * yawSpeed * Time.deltaTime, 0f, Space.Self);

        if (pitchPivot != null)
        {
            currentPitch += pitchInput * pitchSpeed * Time.deltaTime;
            currentPitch = Mathf.Clamp(currentPitch, minPitch, maxPitch);
            pitchPivot.localRotation = Quaternion.Euler(currentPitch, 0f, 0f);
        }
    }

    float NormalizeAngle(float a)
    {
        a %= 360f;
        if (a > 180f) a -= 360f;
        return a;
    }
}
