using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FuelGauge : MonoBehaviour
{
    public Image needle;
    public TMP_Text fuelText;

    public float maxFuel = 100f;
    public float minAngle = -90f;
    public float maxAngle = 90f;

    public void UpdateNeedle(float currentFuel)
    {
        float t = Mathf.Clamp01(currentFuel / maxFuel);
        float angle = Mathf.Lerp(minAngle, maxAngle, t);

        needle.rectTransform.localEulerAngles = new Vector3(0, 0, angle);

        if (fuelText != null)
            fuelText.text = Mathf.RoundToInt(currentFuel).ToString();
    }
}
