using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace GunController
{
    public class RPGShooter : MonoBehaviour
    {
        [Header("References")]
        public Camera playerCamera;
        public Transform muzzlePoint;
        public GameObject rocketPrefab;         // 🚀 Rocket prefab with smoke + trail already attached
        public GameObject hitEffectPrefab;
        public TextMeshProUGUI rocketsText;
        public AudioSource fireAudio;

        [Header("RPG Settings")]
        public float fireRate = 0.5f;
        public float maxRange = 200f;
        public float rocketSpeed = 100f;
        public LayerMask hitLayers;
        public int rocketMagazineSize = 5;
        public float reloadTime = 3f;
        public float reloadRocketDelay = 1f; // Delay before new rocket spawns at muzzle

        private int currentRockets;
        private float nextFireTime = 0f;
        private bool isHoldingFire = false;
        private bool isReloading = false;

        private GameObject loadedRocket; // 🚀 Rocket currently in RPG chamber
        public bool IsFiring { get; private set; }

        private void Start()
        {
            currentRockets = rocketMagazineSize;
            UpdateAmmoUI();
            LoadRocket(); // Load the first rocket into muzzle
        }

        private void Update()
        {
            IsFiring = false;
            if (isHoldingFire && Time.time >= nextFireTime && !isReloading)
            {
                Fire();
            }
        }

        public void OnPointerDown() => isHoldingFire = true;
        public void OnPointerUp() => isHoldingFire = false;

        private void Fire()
        {
            if (currentRockets <= 0 || loadedRocket == null)
            {
                if (currentRockets <= 0)
                    StartCoroutine(Reload());
                return;
            }

            currentRockets--;
            UpdateAmmoUI();
            nextFireTime = Time.time + fireRate;
            IsFiring = true;

            // Fire sound
            if (fireAudio != null && fireAudio.clip != null)
                fireAudio.PlayOneShot(fireAudio.clip);

            // Fire the loaded rocket
            GameObject rocket = loadedRocket;
            loadedRocket = null; // clear chamber

            // Detach rocket so it moves independently
            rocket.transform.parent = null;

            // Start effects attached to the rocket (smoke, trail, etc.)
            foreach (var ps in rocket.GetComponentsInChildren<ParticleSystem>())
                ps.Play();
            foreach (var tr in rocket.GetComponentsInChildren<TrailRenderer>())
                tr.emitting = true;

            // Move rocket toward target
            Vector3 targetPoint = GetTargetPoint();
            StartCoroutine(MoveRocket(rocket, targetPoint));

            // Spawn next rocket after delay (if ammo left)
            if (currentRockets > 0)
                StartCoroutine(LoadRocketDelayed());
        }

        private Vector3 GetTargetPoint()
        {
            Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
            return Physics.Raycast(ray, out RaycastHit hit, maxRange, hitLayers)
                ? hit.point
                : ray.GetPoint(maxRange);
        }

        private IEnumerator MoveRocket(GameObject rocket, Vector3 target)
        {
            Vector3 start = rocket.transform.position;
            float distance = Vector3.Distance(start, target);
            float duration = distance / rocketSpeed;
            float time = 0f;
            Vector3 lastPos = start;

            while (time < duration)
            {
                if (rocket == null) yield break;

                time += Time.deltaTime;
                Vector3 currentPos = Vector3.Lerp(start, target, time / duration);
                rocket.transform.position = currentPos;

                // Collision detection
                if (Physics.Linecast(lastPos, currentPos, out RaycastHit hit, hitLayers))
                {
                    hit.collider.GetComponent<EnemyHealth>()?.TakeDamage(50f);

                    if (hitEffectPrefab != null)
                    {
                        GameObject fx = Instantiate(hitEffectPrefab, hit.point, Quaternion.LookRotation(hit.normal));
                        Destroy(fx, 2f);
                    }

                    Destroy(rocket); // 🚀 Destroy rocket & its effects
                    yield break;
                }

                lastPos = currentPos;
                yield return null;
            }

            if (rocket != null)
                Destroy(rocket);
        }

        private void LoadRocket()
        {
            if (rocketPrefab != null && muzzlePoint != null)
            {
                loadedRocket = Instantiate(rocketPrefab, muzzlePoint.position, muzzlePoint.rotation, muzzlePoint);
            }
        }

        private IEnumerator LoadRocketDelayed()
        {
            yield return new WaitForSeconds(reloadRocketDelay);
            LoadRocket();
        }

        private IEnumerator Reload()
        {
            if (isReloading) yield break;
            isReloading = true;

            yield return new WaitForSeconds(reloadTime);

            currentRockets = rocketMagazineSize;
            UpdateAmmoUI();
            isReloading = false;

            if (loadedRocket == null) LoadRocket();
        }

        private void UpdateAmmoUI()
        {
            if (rocketsText != null)
                rocketsText.text = $"Rockets: {currentRockets}/{rocketMagazineSize}";
        }
    }
}
