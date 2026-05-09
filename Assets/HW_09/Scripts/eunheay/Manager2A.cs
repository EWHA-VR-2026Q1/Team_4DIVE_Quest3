using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HW09
{
    public class Manager2A : MonoBehaviour
    {
        [Header("오디오")]
        public AudioSource ambientSound;
        public AudioSource aiVoice;
        public AudioClip aiClip_Enter;
        public AudioClip aiClip_Explore;

        [Header("탈출 트리거 (AI 음성 완료 후 활성화)")]
        [Tooltip("비워두면 'ExitTrigger' 이름으로 자동 탐색")]
        public Collider exitTrigger;

        [Header("유도선 + 출구 문 (AI 음성 완료 후 표시)")]
        [Tooltip("'=== EXIT GUIDE ===' 오브젝트를 연결하세요")]
        public GameObject exitGuide;

        [Header("앰비언트 페이드 (해커 개입 연출)")]
        public float ambientFadeDuration = 4f;

        void Start()
        {
            // ExitTrigger 자동 탐색
            if (exitTrigger == null)
            {
                var go = GameObject.Find("ExitTrigger");
                if (go != null) exitTrigger = go.GetComponent<Collider>();
            }
            if (exitTrigger != null) exitTrigger.enabled = false;

            // EXIT GUIDE 자동 탐색 — 비활성 오브젝트도 찾을 수 있도록 씬 루트 순회
            if (exitGuide == null)
            {
                foreach (var root in gameObject.scene.GetRootGameObjects())
                {
                    if (root.name == "=== EXIT GUIDE ===")
                    {
                        exitGuide = root;
                        break;
                    }
                }
            }
            if (exitGuide != null) exitGuide.SetActive(false);

            StartCoroutine(SceneFlow());
        }

        IEnumerator SceneFlow()
        {
            if (ambientSound != null) ambientSound.Play();

            // 씬 전환 직후 오디오 짤림 방지 — 3초 대기
            yield return new WaitForSeconds(3f);

            if (aiClip_Enter != null)
            {
                aiVoice.clip = aiClip_Enter;
                aiVoice.Play();
                yield return new WaitForSeconds(aiClip_Enter.length + 2f);
            }

            if (aiClip_Explore != null)
            {
                aiVoice.clip = aiClip_Explore;
                aiVoice.Play();
                yield return new WaitForSeconds(aiClip_Explore.length + 1f);
            }
            else
            {
                yield return new WaitForSeconds(3f);
            }

            yield return StartCoroutine(FadeOutAmbient());

            // ── 경험 완료: 유도선 + 출구 문 표시 ──
            if (exitGuide != null)  exitGuide.SetActive(true);
            if (exitTrigger != null) exitTrigger.enabled = true;

            Debug.Log("[Scene2_A] 출구 가이드 활성화!");
        }

        IEnumerator FadeOutAmbient()
        {
            if (ambientSound == null || !ambientSound.isPlaying) yield break;
            float startVolume = ambientSound.volume;
            float elapsed = 0f;
            while (elapsed < ambientFadeDuration)
            {
                elapsed += Time.deltaTime;
                ambientSound.volume = Mathf.Lerp(startVolume, 0f, elapsed / ambientFadeDuration);
                yield return null;
            }
            ambientSound.Stop();
            ambientSound.volume = startVolume;
        }
    }
}
