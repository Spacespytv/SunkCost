using UnityEngine;

[System.Serializable]
public class Sound
{
    public string name;
    public AudioClip clip;

    [Range(0f, 1f)] public float volume = 0.7f;
    [Range(0.1f, 3f)] public float pitch = 1.0f;

    public bool loop;
    public bool randomPitch;
    public bool stopOnPause;

    [HideInInspector] public AudioSource source;
    [HideInInspector] public float lastRisingPitch = 1.0f; 
}