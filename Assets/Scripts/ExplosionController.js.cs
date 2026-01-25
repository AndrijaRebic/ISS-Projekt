using UnityEngine;
using System.Collections;

public class ExplosionController : MonoBehaviour
{
    [Header("Particle Systems")]
    public ParticleSystem explosionParticles;
    public ParticleSystem smokeParticles;
    [Header("Audio")]
    public AudioClip explosionSound;
    public string explosionSoundResourcesPath = "Free Pack/Explosion 1";
    
    [Header("Settings")]
    public float destroyDelay = 3f;
    
    private bool isPlaying = false;
    
    void Awake()
    { 
        
        if (explosionParticles != null)
        {
            explosionParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
        
        if (smokeParticles != null)
        {
            smokeParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
        
        
        Light light = GetComponentInChildren<Light>();
        if (light != null) light.enabled = false;
        
        
        gameObject.SetActive(false);
    }
    
    public void TriggerExplosion(Vector3 position, bool hitTarget)
    {
        if (isPlaying) return;
        isPlaying = true;
        
        gameObject.SetActive(true);
        transform.position = position;
        
        
        AudioClip clipToPlay = explosionSound;
        if (clipToPlay == null && !string.IsNullOrEmpty(explosionSoundResourcesPath))
        {
            clipToPlay = Resources.Load<AudioClip>(explosionSoundResourcesPath);
        }
        if (clipToPlay != null)
        {
            AudioSource.PlayClipAtPoint(clipToPlay, position);
        }

        StartCoroutine(PlayExplosionSequence(hitTarget));
    }
    
    IEnumerator PlayExplosionSequence(bool hitTarget)
    {
        
        if (explosionParticles != null)
        {
            var main = explosionParticles.main;
            main.startColor = hitTarget ? 
                new Color(1f, 0.5f, 0f, 1f) : 
                new Color(0.6f, 0.4f, 0.2f, 1f); 
            
            explosionParticles.Play();
        }
        
        
        if (smokeParticles != null)
        {
            smokeParticles.Play();
        }
        
        
        
        yield return new WaitForSeconds(destroyDelay);
        
        
        Destroy(gameObject);
    }
}
