using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AutoSceneChange : MonoBehaviour
{
    public string nextScene = "gurin_Scene01_B";
    public float delay = 10f;

    void Start()
    {
        Invoke("LoadNext", delay);
    }

    void LoadNext()
    {
        // 카메라 위치 저장
        GameObject cam = GameObject.Find("OVRCameraRig");
        if (cam != null)
        {
            PlayerPrefs.SetFloat("camX", cam.transform.position.x);
            PlayerPrefs.SetFloat("camY", cam.transform.position.y);
            PlayerPrefs.SetFloat("camZ", cam.transform.position.z);
            PlayerPrefs.SetFloat("camRotY", cam.transform.eulerAngles.y);
        }
        SceneManager.LoadScene(nextScene);
    }
}