using UnityEngine;

public class FuelSystem : MonoBehaviour
{
    public float maxFuel = 100f;
    public float currentFuel = 100f;
    public float fuelDrainRate = 5f;

    public FuelGauge fuelGauge;
    public TankController tank;

    void Start()
    {
        fuelGauge.UpdateNeedle(currentFuel);
    }

    void Update()
    {

        if (tank.isEngineOn && IsMoving())
        {
            currentFuel -= Time.deltaTime * fuelDrainRate;
            currentFuel = Mathf.Clamp(currentFuel, 10f, maxFuel); 

            fuelGauge.UpdateNeedle(currentFuel);
        }

    }

    bool IsMoving()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        return Mathf.Abs(h) > 0.01f || Mathf.Abs(v) > 0.01f;
    }
}
