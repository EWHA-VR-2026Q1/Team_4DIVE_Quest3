using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class Controller_Video_Control : MonoBehaviour
{
    public VideoPlayer Player;
    Renderer Renderer;
    Color color;
    bool hasMat = false;

    void Start()
    {
        Renderer = Player.gameObject.GetComponent<Renderer>();
        if (Renderer != null)
        {
            hasMat = true;
            color = Renderer.material.GetColor("_Color");
            print("has renderer");
        }
        else
        {
            print("NO senderer");
        }
        Stop();
        //Play();
    }

    public void Play()
    {
        if (hasMat)
        {
            Renderer.material.SetColor("_Color", Color.white);
        }
        Player.Play();
    }

    public void Stop()
    {
        if (hasMat)
        {
            Renderer.material.SetColor("_Color", color);
        }
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
