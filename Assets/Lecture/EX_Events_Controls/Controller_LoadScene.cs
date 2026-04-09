using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Controller_LoadScene : MonoBehaviour
{
    public string TargetScene;

    public void LoadScene()
    {
        SceneManager.LoadScene(TargetScene);
    }
}
