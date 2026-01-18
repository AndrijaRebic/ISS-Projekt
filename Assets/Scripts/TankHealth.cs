using UnityEngine;
using System.Collections;

public class TankHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    private float currentHealth;

    [Header("Explosion Prefabs")]
    public GameObject explosionPrefab;
    public GameObject smokePrefab;

    [Header("Death Settings")]
    public float deathDelay = 2f;

    [Header("UI")]
    public GameObject tankUI;

    private bool isDead = false;
    public bool IsDead => isDead;

    void Start()
    {
        currentHealth = maxHealth;
    }

    //samo za testiranje
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            TakeDamage(999f);
        }
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        Debug.Log("Tank hit! Health: " + currentHealth);

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        Vector3 spawnPos = transform.position + Vector3.up * 1.2f;

        if (explosionPrefab != null)
            Instantiate(explosionPrefab, spawnPos, Quaternion.identity);

        if (smokePrefab != null)
            Instantiate(smokePrefab, spawnPos, Quaternion.identity);

        if (tankUI != null)
            Destroy(tankUI);

        StartCoroutine(DestroyAfterDelay());
    }

    IEnumerator DestroyAfterDelay()
    {
        yield return new WaitForSeconds(deathDelay);
        Destroy(gameObject);
    }
}
