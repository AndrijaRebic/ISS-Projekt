using UnityEngine;

public class MissileExplosion : MonoBehaviour
{
    public GameObject explosionPrefab;
    public float explosionLife = 2f;
    [Header("Audio")]
    public AudioClip explosionSound;
    public string explosionSoundResourcesPath = "Free Pack/Explosion 1";
    public float explosionAudioVolume = 1f;
    public float explosionMinDistance = 1f;
    public float explosionMaxDistance = 500f;

     /*void OnCollisionEnter(Collision collision)
    {
        if (exploded) return;
        exploded = true;

        Explode();
    } */

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log("HIT: "+ collision.gameObject.name);
        Explode();
    }


    void Explode()
    {
        if (explosionPrefab != null)
        {
            GameObject fx = Instantiate(
                explosionPrefab,
                transform.position,
                Quaternion.identity
            );

            // Try to play audio from prefab's AudioSource first
            var a = fx.GetComponentInChildren<AudioSource>();
            if (a != null && a.clip != null)
            {
                a.Play();
            }
            else
            {
                // fallback to clip from this component or Resources
                AudioClip clip = explosionSound;
                if (clip == null && !string.IsNullOrEmpty(explosionSoundResourcesPath))
                    clip = Resources.Load<AudioClip>(explosionSoundResourcesPath);
                if (clip != null)
                    AudioUtil.Play3DClipAtPosition(clip, transform.position, explosionAudioVolume, explosionMinDistance, explosionMaxDistance);
            }

            Destroy(fx, explosionLife);
        }

        Destroy(gameObject);
    }
}