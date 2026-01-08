using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace GunController
{
    public class GunShooter : MonoBehaviour
    {
        [Header("References")]
        public Camera playerCamera;
        public Transform muzzlePoint;
        public ParticleSystem muzzleFlash;
        public GameObject bulletTrailPrefab;
        public GameObject hitEffectPrefab;
        public TextMeshProUGUI ammoText;
        public AudioSource fireAudio;

        [Header("Gun Settings")]
        public float fireRate = 0.2f;
        public float maxRange = 100f;
        public float bulletSpeed = 200f;
        public LayerMask hitLayers;
        public int magazineSize = 30;
        public float reloadTime = 2f;

        private int currentBullets;
        private float nextFireTime = 0f;
        private bool isHoldingFire = false;
        private bool isReloading = false;

        public bool IsFiring { get; private set; }

        private void Start()
        {
            currentBullets = magazineSize;
            UpdateAmmoUI();
        }

        private void Update()
        {
            IsFiring = false; // reset each frame

            // Fire continuously if holding the button
            if (isHoldingFire && Time.time >= nextFireTime && !isReloading)
            {
                Fire();
            }
        }

        // Hook these directly in the EventTrigger component
        public void OnPointerDown() => isHoldingFire = true;
        public void OnPointerUp() => isHoldingFire = false;

        private void Fire()
        {
            if (currentBullets <= 0)
            {
                StartCoroutine(Reload());
                return;
            }

            currentBullets--;
            UpdateAmmoUI();
            nextFireTime = Time.time + fireRate;
            IsFiring = true;

            // Determine target point
            Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
            Vector3 targetPoint = Physics.Raycast(ray, out RaycastHit hit, maxRange, hitLayers)
                ? hit.point
                : ray.GetPoint(maxRange);

            // Muzzle flash & sound
            muzzleFlash?.Play();
            if (fireAudio != null && fireAudio.clip != null)
                fireAudio.PlayOneShot(fireAudio.clip);

            // Spawn bullet trail
            if (bulletTrailPrefab != null && muzzlePoint != null)
            {
                GameObject bulletTrail = Instantiate(bulletTrailPrefab, muzzlePoint.position, Quaternion.identity);
                StartCoroutine(MoveTrail(bulletTrail.GetComponent<TrailRenderer>(), muzzlePoint.position, targetPoint));
            }
        }

        private IEnumerator MoveTrail(TrailRenderer trail, Vector3 start, Vector3 target)
        {
            float distance = Vector3.Distance(start, target);
            float time = 0f;
            float duration = distance / bulletSpeed;
            Vector3 lastPos = start;

            while (time < duration)
            {
                time += Time.deltaTime;
                Vector3 currentPos = Vector3.Lerp(start, target, time / duration);

                if (trail != null)
                    trail.transform.position = currentPos;

                if (Physics.Linecast(lastPos, currentPos, out RaycastHit hit, hitLayers))
                {
                    hit.collider.GetComponent<EnemyHealth>()?.TakeDamage(20f);

                    if (hitEffectPrefab != null)
                    {
                        GameObject fx = Instantiate(hitEffectPrefab, hit.point, Quaternion.LookRotation(hit.normal));
                        Destroy(fx, 2f);
                    }

                    currentPos = hit.point;
                    if (trail != null) trail.transform.position = currentPos;
                    break;
                }

                lastPos = currentPos;
                yield return null;
            }

            if (trail != null)
                Destroy(trail.gameObject, trail.time);
        }

        private IEnumerator Reload()
        {
            if (isReloading) yield break;
            isReloading = true;

            yield return new WaitForSeconds(reloadTime);

            currentBullets = magazineSize;
            UpdateAmmoUI();
            isReloading = false;
        }

        private void UpdateAmmoUI()
        {
            if (ammoText != null)
                ammoText.text = $"Ammo: {currentBullets}/{magazineSize}";
        }
    }
}
