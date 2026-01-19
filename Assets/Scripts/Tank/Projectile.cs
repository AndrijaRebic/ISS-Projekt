using UnityEngine;

public class Projectile : MonoBehaviour
{
    public GameObject hitEffectPrefab;   // assign explosion/smoke from your asset pack in Inspector
    public float lifeTime = 5f;          // safety auto‑despawn

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    void OnCollisionEnter(Collision collision)
    {
        // Optional: only react to terrain/ground layers or tags
        // if (!collision.gameObject.CompareTag("Ground")) return;

        // Spawn effect at hit point/rotation
        if (hitEffectPrefab != null)
        {
            ContactPoint contact = collision.contacts[0];
            Instantiate(hitEffectPrefab, contact.point, Quaternion.LookRotation(contact.normal));
        }

        Destroy(gameObject);
    }
}
