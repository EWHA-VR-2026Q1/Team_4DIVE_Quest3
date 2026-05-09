using System.Collections;
using UnityEngine;

namespace HW09.Heejin
{
    public class SceneIntroManager4B : MonoBehaviour
    {
        public float startDelay = 1f;

        void Start()
        {
            StartCoroutine(PlayIntroAudio());
        }

        IEnumerator PlayIntroAudio()
        {
            yield return new WaitForSeconds(startDelay);

            GameObject go = GameObject.Find("AudioAI_Phase0");
            if (go == null) { Debug.LogWarning("[SceneIntroManager4B] 'AudioAI_Phase0' not found."); yield break; }

            AudioSource src = go.GetComponent<AudioSource>();
            if (src == null) { Debug.LogWarning("[SceneIntroManager4B] No AudioSource on 'AudioAI_Phase0'."); yield break; }

            src.Play();
        }
    }
}
