using UnityEngine.Audio;
using UnityEngine;
using System;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [SerializeField] private Sound[] sounds = null;

    [Header("Rising Pitch Settings")]
    [SerializeField] private float pitchStep = 0.05f;
    [SerializeField] private float maxRisingPitch = 2.0f;
    [SerializeField] private float pitchResetTime = 1.5f;

    private float currentComboPitch = 1.0f;
    private Coroutine pitchResetCoroutine;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        foreach (Sound sound in sounds)
        {
            sound.source = gameObject.AddComponent<AudioSource>();
            sound.source.clip = sound.clip;
            sound.source.pitch = sound.pitch;
            sound.source.loop = sound.loop;
            sound.source.volume = sound.volume;
        }
    }

    public void Play(string soundName)
    {
        Sound s = GetSound(soundName);
        if (s == null || s.source == null) return;

        if (s.randomPitch)
        {
            s.source.pitch = s.pitch + UnityEngine.Random.Range(-0.1f, 0.1f);
        }
        else
        {
            s.source.pitch = s.pitch;
        }

        s.source.Play();
    }

    public void PlayRising(string soundName)
    {
        Sound s = GetSound(soundName);
        if (s == null || s.source == null) return;

        s.source.pitch = currentComboPitch;
        s.source.Play();

        currentComboPitch = Mathf.Min(currentComboPitch + pitchStep, maxRisingPitch);

        if (pitchResetCoroutine != null) StopCoroutine(pitchResetCoroutine);
        pitchResetCoroutine = StartCoroutine(ResetPitchRoutine());
    }

    private IEnumerator ResetPitchRoutine()
    {
        yield return new WaitForSeconds(pitchResetTime);
        currentComboPitch = 1.0f;
    }

    public void ShiftMusicPitch(string soundName, float targetPitch, float duration)
    {
        Sound s = GetSound(soundName);
        if (s == null || s.source == null) return;

        StopCoroutine("PitchShiftRoutine");
        StartCoroutine(PitchShiftRoutine(s.source, targetPitch, duration));
    }

    private IEnumerator PitchShiftRoutine(AudioSource source, float target, float duration)
    {
        float startPitch = source.pitch;
        float elapsed = 0;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            source.pitch = Mathf.Lerp(startPitch, target, elapsed / duration);
            yield return null;
        }
        source.pitch = target;
    }

    private Sound GetSound(string name)
    {
        Sound s = Array.Find(sounds, soundClip => soundClip.name == name);
        if (s == null) Debug.LogWarning("Sound: " + name + " not found.");
        return s;
    }

    public void Stop(string soundName)
    {
        Sound s = GetSound(soundName);
        if (s != null && s.source.isPlaying) s.source.Stop();
    }

    public void SetSFXVolume(float value)
    {
        foreach (Sound sound in sounds)
        {
            sound.source.volume = sound.volume * value;
        }
    }
    public void PlayRisingManual(string soundName)
    {
        Sound s = GetSound(soundName);
        if (s == null || s.source == null) return;

        s.source.pitch = s.lastRisingPitch;
        s.source.Play();

        s.lastRisingPitch = Mathf.Min(s.lastRisingPitch + pitchStep, maxRisingPitch);
    }

    public void ResetRisingPitch(string soundName)
    {
        Sound s = Array.Find(sounds, item => item.name == soundName);

        if (s != null)
        {
            s.lastRisingPitch = 1.0f;
        }
        else
        {
            Debug.LogWarning($"Could not find sound '{soundName}' to reset pitch.");
        }
    }
    public void ResetAllRisingPitches()
    {
        foreach (Sound s in sounds)
        {
            s.lastRisingPitch = 1.0f;
        }
    }

    public void PlayOneShot(string soundName)
    {
        Sound s = GetSound(soundName);
        if (s == null || s.source == null) return;

        if (s.randomPitch)
        {
            s.source.pitch = s.pitch + UnityEngine.Random.Range(-0.1f, 0.1f);
        }
        else
        {
            s.source.pitch = s.pitch;
        }

        s.source.PlayOneShot(s.clip, s.volume);
    }

    public void PlayRisingOneShot(string soundName)
    {
        Sound s = GetSound(soundName);
        if (s == null || s.source == null) return;
        s.source.pitch = s.lastRisingPitch;
        s.source.PlayOneShot(s.clip, s.volume);
        s.lastRisingPitch = Mathf.Min(s.lastRisingPitch + pitchStep, maxRisingPitch);
        StopCoroutine(nameof(ResetPitchAfterDelay));
        StartCoroutine(ResetPitchAfterDelay(s));
    }

    private IEnumerator ResetPitchAfterDelay(Sound s)
    {
        yield return new WaitForSeconds(pitchResetTime);
        s.lastRisingPitch = 1.0f;
    }
}