using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HW09
{
    public class Manager2B : MonoBehaviour
    {
        [Header("오디오")]
        public AudioSource aiVoice;
        public AudioClip aiClip_Glitch;  // "...환경 모듈 정상 작— ...계속하십—"
        public AudioClip aiClip_Clear;   // "입구로 귀환하십시오"

        [Header("글리치 텍스트")]
        public GameObject[] glitchTexts; // 인스펙터에서 3개 연결

        [Header("입구 콜라이더")]
        public Collider exitTrigger;     // 클리어 전엔 막아둠

        public bool IsCleared { get; private set; } = false;

        void Start()
        {
            foreach (var t in glitchTexts)
                if (t != null) t.SetActive(false);

            if (exitTrigger != null)
                exitTrigger.enabled = false;

            StartCoroutine(SceneFlow());
        }

        IEnumerator SceneFlow()
        {
            yield return new WaitForSeconds(1.5f);

            if (aiClip_Glitch != null)
            {
                aiVoice.clip = aiClip_Glitch;
                aiVoice.Play();
                yield return new WaitForSeconds(aiClip_Glitch.length + 0.5f);
            }

            foreach (var text in glitchTexts)
            {
                if (text != null) text.SetActive(true);
                yield return new WaitForSeconds(3f);
            }
        }

        // RadioInteractable에서 호출
        public void OnRadioActivated(float hackerClipLength)
        {
            if (IsCleared) return;
            IsCleared = true;

            foreach (var t in glitchTexts)
                if (t != null) t.SetActive(false);

            StartCoroutine(ClearSequence(hackerClipLength));
        }

        IEnumerator ClearSequence(float waitTime)
        {
            yield return new WaitForSeconds(waitTime + 0.5f);

            if (aiClip_Clear != null)
            {
                aiVoice.clip = aiClip_Clear;
                aiVoice.Play();
                yield return new WaitForSeconds(aiClip_Clear.length);
            }

            if (exitTrigger != null)
                exitTrigger.enabled = true;

            // GameManager에 Set2 클리어 등록
            if (ManagerMain.Instance != null)
                ManagerMain.Instance.ClearSet(1);
        }
    }
}
