using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller_Audio_Control : MonoBehaviour
{
    // VideoPlayer 대신 AudioSource를 사용합니다.
    public AudioSource Player;


    void Start()
    {
        if(Player == null) Player = GetComponent<AudioSource>();
        // 시작할 때 정지 상태로 설정
        Stop();
    }

    public void Play()
    {
        Player.Play();
    }

    public void Stop()
    {
        Player.Stop();
    }

    public void Pause()
    {
        Player.Pause();
    }

    public void PlayStop()
    {
        if (Player.isPlaying)
        {
            Stop();
        }
        else
        {
            Play();
        }
    }
}