using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class D05_LoadScene : MonoBehaviour
{
    public string SceneName;

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.name == "OVRPlayerController" || other.gameObject.tag == "Player")
        {
            SceneManager.LoadScene(SceneName);
        }
    }

}
