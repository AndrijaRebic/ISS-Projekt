using UnityEngine;

public class TankProjectileDamage : MonoBehaviour
{
    [Header("Damage")]
    public float damage = 20f;

    void OnCollisionEnter(Collision collision)
    {
        // traži LauncherHealth na pogođenom objektu ili parentu
        LauncherHealth health =
            collision.gameObject.GetComponentInParent<LauncherHealth>();

        if (health != null && !health.IsDead)
        {
            Debug.Log($"[TankProjectileDamage] Hit launcher for {damage}");
            health.TakeDamage(damage);
        }
    }
}
