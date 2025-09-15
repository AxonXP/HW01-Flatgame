using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(AudioSource))] // Ensures an AudioSource is available
public class SoundEffectManager : MonoBehaviour
{
    [System.Serializable]
    public class SoundClipEntry
    {
        public string name;
        public AudioClip audioClip;
        [Range(0f, 5f)] public float defaultVolume = 1.0f;
        [Range(0.1f, 3f)] public float defaultPitch = 1.0f;
        public bool loopByDefault = false; // If true, PlaySound/StartPlaySound will loop this clip
    }

    public List<SoundClipEntry> soundClips = new List<SoundClipEntry>();

    private AudioSource c_audioSource;
    private Dictionary<string, SoundClipEntry> c_soundClipMap;

    void Awake()
    {
        c_audioSource = GetComponent<AudioSource>();
        c_soundClipMap = new Dictionary<string, SoundClipEntry>();
        foreach (var entry in soundClips)
        {
            c_soundClipMap[entry.name] = entry;
        }
    }
    private SoundClipEntry GetSoundClipEntry(string soundName)
    {
        if (c_soundClipMap.TryGetValue(soundName, out SoundClipEntry entry))
        {
            return entry;
        }
        return null;
    }

    public void PlaySound(string soundName)
    {
        SoundClipEntry entry = GetSoundClipEntry(soundName);
        if (entry != null && entry.audioClip != null)
        {
            c_audioSource.Stop(); // Stop current sound before playing new one
            c_audioSource.clip = entry.audioClip;
            c_audioSource.volume = entry.defaultVolume;
            c_audioSource.pitch = entry.defaultPitch;
            c_audioSource.loop = entry.loopByDefault;
            c_audioSource.Play();
        }
    }
    public void StartPlaySound(string soundName)
    {
        if (!c_audioSource.isPlaying)
        {
            PlaySound(soundName);
        }
    }

    public void PlayRandShot(string soundName)
    {
        PlaySoundOneShot(soundName, 1, Random.Range(0.25f, 1.25f));
    }

    public void PlaySoundOneShot(string soundName, float volumeScale = 1.0f, float? pitch = null)
    {
        SoundClipEntry entry = GetSoundClipEntry(soundName);
        if (entry != null && entry.audioClip != null)
        {
            float finalPitch = pitch.HasValue ? pitch.Value : entry.defaultPitch;
            // Unity's PlayOneShot takes the final volume, not just a scale of AudioSource.volume
            c_audioSource.pitch = finalPitch; // Set pitch for this one-shot
            c_audioSource.PlayOneShot(entry.audioClip, entry.defaultVolume * volumeScale);
            c_audioSource.pitch = entry.defaultPitch; // Reset pitch if it was changed for the main audio source
        }
    }

    public void StopSound(string soundName)
    {
        SoundClipEntry entry = GetSoundClipEntry(soundName);
        if (entry != null && c_audioSource.isPlaying && c_audioSource.clip == entry.audioClip)
        {
            c_audioSource.Stop();
        }
    }

    public void StopAllSounds()
    {
        c_audioSource.Stop();
    }

    public bool IsPlaying(string soundName)
    {
        SoundClipEntry entry = GetSoundClipEntry(soundName);
        if (entry != null && entry.audioClip != null)
        {
            return c_audioSource.isPlaying && c_audioSource.clip == entry.audioClip;
        }
        return false;
    }

    public void SetMasterVolume(float volume)
    {
        c_audioSource.volume = Mathf.Clamp01(volume);
    }

    public AudioSource GetPrimaryAudioSource()
    {
        return c_audioSource;
    }
}