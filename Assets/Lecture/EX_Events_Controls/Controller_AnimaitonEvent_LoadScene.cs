using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Controller_AnimaitonEvent_LoadScene : MonoBehaviour
{
    //public string SceneName;


    public void LoadScene(string s)
    {
        SceneManager.LoadScene(s);
    }
}
