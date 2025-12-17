using UnityEngine;

public class TankHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    public float currentHealth;

    public HealthBar healthBar; 
 
    void Start()
    {
        currentHealth = maxHealth;
        if (healthBar != null)
            healthBar.UpdateNeedle(currentHealth);
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        if (healthBar != null)
            healthBar.UpdateNeedle(currentHealth);
    }

    public void Heal(float amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        if (healthBar != null)
            healthBar.UpdateNeedle(currentHealth);
    }

    void Update()
{
    if (healthBar != null)
        healthBar.UpdateNeedle(currentHealth);
}

}


