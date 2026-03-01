using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Ambient ses ve SFX'i yöneten Singleton.
/// Kullanım: AudioManager.Instance.PlaySFX("footstep");
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Ambient")]
    [SerializeField] private AudioSource ambientSource;
    [SerializeField] private AudioClip ambientNormal;
    [SerializeField] private AudioClip ambientParanormal;

    [Header("SFX Library")]
    [SerializeField] private SoundEntry[] sounds;

    private AudioSource _sfxSource;
    private Dictionary<string, AudioClip> _soundMap;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        _sfxSource = gameObject.AddComponent<AudioSource>();
        _sfxSource.playOnAwake = false;

        BuildSoundMap();
    }

    void Start()
    {
        PlayAmbient(ambientNormal);
    }

    // ── Ambient ──────────────────────────────────────────────

    public void PlayAmbient(AudioClip clip)
    {
        if (ambientSource == null || clip == null) return;
        if (ambientSource.clip == clip && ambientSource.isPlaying) return;

        ambientSource.clip = clip;
        ambientSource.loop = true;
        ambientSource.Play();
    }

    public void SwitchToParanormalAmbient() => PlayAmbient(ambientParanormal);
    public void SwitchToNormalAmbient()     => PlayAmbient(ambientNormal);

    public void FadeAmbientVolume(float targetVolume, float duration)
    {
        StartCoroutine(FadeCoroutine(ambientSource, targetVolume, duration));
    }

    // ── SFX ──────────────────────────────────────────────────

    public void PlaySFX(string soundName, float volumeScale = 1f)
    {
        if (_soundMap.TryGetValue(soundName, out AudioClip clip))
            _sfxSource.PlayOneShot(clip, volumeScale);
        else
            Debug.LogWarning($"[AudioManager] '{soundName}' sesi bulunamadı.");
    }

    public void PlaySFXAt(string soundName, Vector3 position, float volumeScale = 1f)
    {
        if (_soundMap.TryGetValue(soundName, out AudioClip clip))
            AudioSource.PlayClipAtPoint(clip, position, volumeScale);
        else
            Debug.LogWarning($"[AudioManager] '{soundName}' sesi bulunamadı.");
    }

    // ── Internal ─────────────────────────────────────────────

    private void BuildSoundMap()
    {
        _soundMap = new Dictionary<string, AudioClip>();
        if (sounds == null) return;
        foreach (var entry in sounds)
        {
            if (entry.clip != null && !string.IsNullOrEmpty(entry.name))
                _soundMap[entry.name] = entry.clip;
        }
    }

    private System.Collections.IEnumerator FadeCoroutine(AudioSource source, float target, float duration)
    {
        float start = source.volume;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            source.volume = Mathf.Lerp(start, target, elapsed / duration);
            yield return null;
        }
        source.volume = target;
    }
}

[System.Serializable]
public struct SoundEntry
{
    public string name;
    public AudioClip clip;
}
