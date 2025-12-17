using UnityEngine;
using UnityEngine.UI;
using TMPro;



public class HealthBar : MonoBehaviour
{
    public Image needle;
    public TMP_Text healthText;
    public float maxHealth = 100f;
    public float minAngle = -90f; 
    public float maxAngle = 90f;

    public void UpdateNeedle(float currentHealth)
    {
        float t = currentHealth / maxHealth;  
        float angle = Mathf.Lerp(minAngle, maxAngle, t);
        needle.rectTransform.localEulerAngles = new Vector3(0, 0, angle);

        if (healthText != null)
        healthText.text = Mathf.RoundToInt(currentHealth).ToString();
    }
}
