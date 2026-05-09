using UnityEngine;
using UnityEngine.SceneManagement;

namespace HW09.Gayoung
{
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
}
