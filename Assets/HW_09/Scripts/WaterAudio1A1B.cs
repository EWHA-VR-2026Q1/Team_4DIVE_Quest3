using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterAudio : MonoBehaviour
{
    public AudioClip[] clips;
    public float minInterval = 0.5f;
    public float maxInterval = 2f;
    public float volume = 0.5f;

    private AudioSource source;

    void Start()
    {
        source = gameObject.AddComponent<AudioSource>();
        source.spatialBlend = 0f;
        source.volume = volume;
        PlayNext();
    }

    void PlayNext()
    {
        if (clips.Length == 0) return;

        int idx = Random.Range(0, clips.Length);
        source.clip = clips[idx];
        source.Play();

        float interval = source.clip.length + Random.Range(minInterval, maxInterval);
        Invoke("PlayNext", interval);
    }
}