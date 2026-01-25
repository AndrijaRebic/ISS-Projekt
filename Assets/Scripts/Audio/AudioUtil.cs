using UnityEngine;

/// <summary>
/// Small static helper for playing audio.
/// - PlayGlobal plays a non-spatial (2D) clip so it sounds the same everywhere.
/// - PlayGlobalFromResources loads an AudioClip from Resources and plays it globally.
/// - Play3DAtPosition is a thin wrapper around AudioSource.PlayClipAtPoint if you need 3D later.
/// </summary>
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
        // Make this source 2D (non-spatial) so the clip sounds the same regardless of position
        _globalSource.spatialBlend = 0f;
    }

    /// <summary>
    /// Play a non-spatial (2D) clip so it is heard equally everywhere.
    /// </summary>
    public static void PlayGlobal(AudioClip clip, float volume = 1f)
    {
        if (clip == null) return;
        EnsureGlobalSource();
        _globalSource.PlayOneShot(clip, Mathf.Clamp01(volume));
    }

    /// <summary>
    /// Load an AudioClip from Resources and play it globally.
    /// Path example: "Free Pack/Explosion 1" for Assets/Resources/Free Pack/Explosion 1.wav
    /// </summary>
    public static void PlayGlobalFromResources(string resourcePath, float volume = 1f)
    {
        if (string.IsNullOrEmpty(resourcePath)) return;
        var clip = Resources.Load<AudioClip>(resourcePath);
        if (clip == null) return;
        PlayGlobal(clip, volume);
    }

    /// <summary>
    /// Convenience wrapper for 3D playback at a world position (uses Unity's temporary AudioSource).
    /// Left here so scripts can opt-in if they need spatialized playback later.
    /// </summary>
    public static void Play3DAtPosition(AudioClip clip, Vector3 position, float volume = 1f)
    {
        if (clip == null) return;
        AudioSource.PlayClipAtPoint(clip, position, Mathf.Clamp01(volume));
    }

    /// <summary>
    /// Backwards-compatible API used by older scripts:
    /// Play a clip at a position with explicit min/max distance and volume.
    /// This creates a temporary GameObject with a 3D AudioSource and destroys it when done.
    /// </summary>
    public static void Play3DClipAtPosition(AudioClip clip, Vector3 position, float volume, float minDistance, float maxDistance)
    {
        if (clip == null) return;

        GameObject go = new GameObject("AudioUtil_Temp3D");
        go.transform.position = position;
        var src = go.AddComponent<AudioSource>();
        src.clip = clip;
        src.spatialBlend = 1f; // fully 3D
        src.minDistance = Mathf.Max(0.01f, minDistance);
        src.maxDistance = Mathf.Max(src.minDistance, maxDistance);
        src.rolloffMode = AudioRolloffMode.Linear;
        src.playOnAwake = false;
        src.volume = Mathf.Clamp01(volume);
        src.Play();

        // Destroy when finished
        Object.Destroy(go, clip.length + 0.1f);
    }
}
