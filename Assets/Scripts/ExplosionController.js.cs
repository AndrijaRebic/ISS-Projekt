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
        
        // Ensure all effects are OFF at start
        if (explosionParticles != null)
        {
            explosionParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
        
        if (smokeParticles != null)
        {
            smokeParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
        
        // Disable light if present
        Light light = GetComponentInChildren<Light>();
        if (light != null) light.enabled = false;
        
        // Deactivate initially
        gameObject.SetActive(false);
    }
    
    public void TriggerExplosion(Vector3 position, bool hitTarget)
    {
        if (isPlaying) return;
        isPlaying = true;
        
        gameObject.SetActive(true);
        transform.position = position;
        
        // Start coroutine
        // Play sound at position if available (Inspector clip preferred, otherwise Resources fallback)
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
        // 1. Play explosion particles
        if (explosionParticles != null)
        {
            var main = explosionParticles.main;
            main.startColor = hitTarget ? 
                new Color(1f, 0.5f, 0f, 1f) : // Orange for target
                new Color(0.6f, 0.4f, 0.2f, 1f); // Brown for terrain
            
            explosionParticles.Play();
        }
        
        // 2. Play smoke particles
        if (smokeParticles != null)
        {
            smokeParticles.Play();
        }
        
        
        // 3. Wait for particles to finish
        yield return new WaitForSeconds(destroyDelay);
        
        // 4. Cleanup - destroy
        Destroy(gameObject);
    }
}
