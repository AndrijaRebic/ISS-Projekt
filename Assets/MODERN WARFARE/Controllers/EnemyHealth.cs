using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace GunController
{
    public class EnemyHealth : MonoBehaviour
    {
        [Header("Enemy Settings")]
        public float maxHealth = 100f;   // Total health
        private float currentHealth;

        [Header("UI")]
        public Slider healthBar;          // Optional health bar UI
        public Transform healthBarPivot;  // Pivot above enemy head
        private Camera mainCamera;

        [Header("Death Settings")]
        public float deathDelay = 2f;     // Delay before destroying enemy

        private bool isDead = false;      // Prevent multiple death triggers

        private void Start()
        {
            currentHealth = maxHealth;

            // Initialize health bar
            if (healthBar != null)
            {
                healthBar.maxValue = maxHealth;
                healthBar.value = currentHealth;
            }

            // Auto-find camera
            if (mainCamera == null)
                mainCamera = Camera.main;
        }

        private void Update()
        {
            // Keep health bar facing camera
            if (healthBarPivot != null && mainCamera != null)
            {
                Vector3 dir = healthBarPivot.position - mainCamera.transform.position;
                healthBarPivot.rotation = Quaternion.LookRotation(dir);
            }
        }

        /// <summary>
        /// Call this method when the enemy takes damage
        /// </summary>
        public void TakeDamage(float amount)
        {
            if (isDead) return;

            currentHealth -= amount;
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

            if (healthBar != null)
                healthBar.value = currentHealth;

            if (currentHealth <= 0f && !isDead)
            {
                isDead = true;
                StartCoroutine(DieWithDelay());
            }
        }

        private IEnumerator DieWithDelay()
        {
            yield return new WaitForSeconds(deathDelay);

            if (healthBar != null)
                Destroy(healthBar.gameObject);

            Destroy(gameObject);
        }
    }
}
