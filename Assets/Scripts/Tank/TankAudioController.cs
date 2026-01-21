using System.Collections.Generic;
using UnityEngine;

public class TankAudioController : MonoBehaviour
{
    public enum SoundType
    {
        Shoot,
        Explode,
        Motor,
        Hit
        //Add more sounds as needed
    }

    [System.Serializable]       //omogućava Unity Inspectoru da prikaže ovu klasu
    public class Sound
    {
        public SoundType Type;
        public AudioClip Clip;  //audioclip koji će se reproducirati
        [Range (0f, 1f)] public float Volume = 1f;      //0-tiho 1-maksimalno
        [HideInInspector] public AudioSource Source;        //neće prikazivati ovaj field u Inspectoru - AudioSource koji će reproducirati zvuk
    }

    //Singleton - jedna instanca ove klase kojoj možemo lako pristupiti bilogdje
    public static TankAudioController Instance;

    //All sounds and their associated type - Set these in the inspector
    //niz svih zvukova koje ćemo koristiti - Sound se sastoji od tipa, clipa i volumena
    public Sound[] AllSounds;

    //Runtime Collections
    //dictionary omogućava brzo dohvaćanje zvuka po tipu, umjesto iteriranja kroz niz svaki put
    private Dictionary<SoundType, Sound> soundDictionary = new Dictionary<SoundType, Sound>();

    private AudioSource motorSource;

    //inicijalizira dictionary tako da svaki SoundType odgovara svom Sound objektu
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
    //tip zvuka koji se reproducira svaki put kada pritisneš Space
    public SoundType SelectedSound;

    //Call this method to play a sound 
    public void Play(SoundType type)
    {
        if (type == SoundType.Motor)
            return;

        //provjerava da li postoji zvuk za dani tip
        if (!soundDictionary.TryGetValue(type, out Sound s))
        {
            Debug.LogWarning($"Sound type {type} not found.");
            return;
        }

        //ako postoji, kreira novi objekt za zvuk 
        var soundObj = new GameObject($"Sound_{type}");
        var audioSrc = soundObj.AddComponent<AudioSource>();    //dodaje na njega AudioSource komponentu

        //Assigns your sound properties
        audioSrc.clip = s.Clip;
        audioSrc.volume = s.Volume;

        //Play the sound
        audioSrc.Play();

        //uništava objekt nakon što zvuk završi
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

    private bool engineOn = false;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.E))  //provjerava je li space tipka puštena
        {
            engineOn = !engineOn;

            if (engineOn)
                StartMotor();
            else
                StopMotor();
        }

        if (Input.GetKeyDown(KeyCode.Space) && engineOn)
        {
            Play(SoundType.Shoot);
        }
    }
}
