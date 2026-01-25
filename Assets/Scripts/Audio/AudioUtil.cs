using UnityEngine;


public static class AudioUtil
{
    private const string GlobalSourceName = "AudioUtil_GlobalSource";
    private static AudioSource _globalSource;

    private static void EnsureGlobalSource()
    {
        if (_globalSource != null) return;

        var go = GameObject.Find(GlobalSourceName);
        if (go == null)
        {
            go = new GameObject(GlobalSourceName);
            Object.DontDestroyOnLoad(go);
        }

        _globalSource = go.GetComponent<AudioSource>();
        if (_globalSource == null)
            _globalSource = go.AddComponent<AudioSource>();

        _globalSource.playOnAwake = false;
        
        _globalSource.spatialBlend = 0f;
    }

    
    public static void PlayGlobal(AudioClip clip, float volume = 1f)
    {
        if (clip == null) return;
        EnsureGlobalSource();
        _globalSource.PlayOneShot(clip, Mathf.Clamp01(volume));
    }

  
    public static void PlayGlobalFromResources(string resourcePath, float volume = 1f)
    {
        if (string.IsNullOrEmpty(resourcePath)) return;
        var clip = Resources.Load<AudioClip>(resourcePath);
        if (clip == null) return;
        PlayGlobal(clip, volume);
    }

    
    public static void Play3DAtPosition(AudioClip clip, Vector3 position, float volume = 1f)
    {
        if (clip == null) return;
        AudioSource.PlayClipAtPoint(clip, position, Mathf.Clamp01(volume));
    }

    
    public static void Play3DClipAtPosition(AudioClip clip, Vector3 position, float volume, float minDistance, float maxDistance)
    {
        if (clip == null) return;

        GameObject go = new GameObject("AudioUtil_Temp3D");
        go.transform.position = position;
        var src = go.AddComponent<AudioSource>();
        src.clip = clip;
        src.spatialBlend = 1f; 
        src.minDistance = Mathf.Max(0.01f, minDistance);
        src.maxDistance = Mathf.Max(src.minDistance, maxDistance);
        src.rolloffMode = AudioRolloffMode.Linear;
        src.playOnAwake = false;
        src.volume = Mathf.Clamp01(volume);
        src.Play();

        
        Object.Destroy(go, clip.length + 0.1f);
    }
}
