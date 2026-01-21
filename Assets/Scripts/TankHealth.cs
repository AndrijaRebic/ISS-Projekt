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

    [Header("Audio")]
    public AudioClip explodeSound;
    public AudioSource audioSource;

    private bool isDead = false;
    public bool IsDead => isDead;

    void Start()
    {
        currentHealth = maxHealth;

        if (audioSource == null )
            audioSource = gameObject.AddComponent<AudioSource>();

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

        if (TankAudioController.Instance != null)
            TankAudioController.Instance.Play(TankAudioController.SoundType.Hit);

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        if (TankAudioController.Instance != null)
            TankAudioController.Instance.StopMotor();


        Vector3 spawnPos = transform.position + Vector3.up * 1.2f;

        if (explosionPrefab != null)
            Instantiate(explosionPrefab, spawnPos, Quaternion.identity);

        if (smokePrefab != null)
            Instantiate(smokePrefab, spawnPos, Quaternion.identity);

        Camera playerCam = Camera.main; 
        if (playerCam != null && explosionPrefab != null)
        {
            Vector3 playerViewPos = playerCam.transform.position + playerCam.transform.forward * 3f;
            Instantiate(explosionPrefab, playerViewPos, Quaternion.identity);
        }

        if (TankAudioController.Instance != null)
            TankAudioController.Instance.Play(TankAudioController.SoundType.Explode);

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
