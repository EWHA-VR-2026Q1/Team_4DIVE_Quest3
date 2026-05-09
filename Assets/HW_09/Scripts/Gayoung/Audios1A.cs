using UnityEngine;
using UnityEngine.SceneManagement;

namespace HW09.Gayoung
{
    public class Audios : MonoBehaviour
    {
        public AudioClip audioClip;

        void Start()
        {
            AudioSource source = gameObject.AddComponent<AudioSource>();
            source.clip = audioClip;
            source.spatialBlend = 0f;
            source.Play();
        }
    }
}
