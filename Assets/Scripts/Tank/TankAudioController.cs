using System.Collections.Generic;
using UnityEngine;

public class TankAudioController : MonoBehaviour
{
    public enum SoundType
    {
        CannonShoot,
        MachineGunShoot,
        TOWShoot,
        Explode,
        Motor,
        Hit
    }

    [System.Serializable]
    public class Sound
    {
        public SoundType Type;
        public AudioClip Clip;  
        [Range (0f, 1f)] public float Volume = 1f;
        [HideInInspector] public AudioSource Source;
    }

    public static TankAudioController Instance;

    public Sound[] AllSounds;

    private Dictionary<SoundType, Sound> soundDictionary = new Dictionary<SoundType, Sound>();

    private AudioSource motorSource;

    private void Awake()
    {
        Instance = this;
        foreach (var sound in AllSounds)
        {
            soundDictionary[sound.Type] = sound;
        }

        if (soundDictionary.TryGetValue(SoundType.Motor, out Sound motorSound))
        {
            motorSource = gameObject.AddComponent<AudioSource>();
            motorSource.clip = motorSound.Clip;
            motorSource.volume = motorSound.Volume;
            motorSource.loop = true;
            motorSource.playOnAwake = false;
        }
    }
    
    public void Play(SoundType type)
    {
        if (type == SoundType.Motor)
            return;

        if (!soundDictionary.TryGetValue(type, out Sound s))
        {
            Debug.LogWarning($"Sound type {type} not found.");
            return;
        }

        GameObject soundObj = new GameObject($"Sound_{type}");
        var audioSrc = soundObj.AddComponent<AudioSource>();    //dodaje na njega AudioSource komponentu

        audioSrc.clip = s.Clip;
        audioSrc.volume = s.Volume;

        audioSrc.Play();
        Destroy(soundObj, s.Clip.length);
    }

    public void StartMotor()
    {
        if (motorSource != null && !motorSource.isPlaying)
        {
            motorSource.Play();
        }
    }

    public void StopMotor()
    {
        if (motorSource != null && motorSource.isPlaying)
        {
            motorSource.Stop();
        }
    }
    
}
