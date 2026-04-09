using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Audios : MonoBehaviour
{
    public AudioClip audioClip;

    void Start()
    {
        AudioSource source = gameObject.AddComponent<AudioSource>();
        source.clip = audioClip;
        source.spatialBlend = 0f; // 2D 사운드
        source.Play();
    }
}