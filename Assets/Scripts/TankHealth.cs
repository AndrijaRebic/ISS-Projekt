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
    public GameObject lightPrefab;

    [Header("Death Settings")]
    public float deathDelay = 2f; 

    [Header("UI")]
    public GameObject tankUI; 

    private bool isDead = false; 

    public bool IsDead { get { return isDead; } }

    void Start()
    {
        currentHealth = maxHealth;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.K) && !isDead)
        {
            currentHealth = 0f;
            Die();
        }
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth -= damage;

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        Vector3 spawnPos = transform.position + transform.forward * 2f + Vector3.up * 1.2f;

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
