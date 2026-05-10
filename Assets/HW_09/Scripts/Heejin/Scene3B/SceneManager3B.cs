using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HW09.Heejin
{
    /// <summary>
    /// Scene3_B 전체 흐름 제어.
    /// 1) Audio1,2,3 인트로 → 2) Table+IceBlock 글로우 ON → 3) 손 가까이 시 글로우 OFF + Audio4,5
    /// → 4) IceBlock 잡으면 Audio6,7 → 5) Panel 상호작용 시 즉시 MainScene 로드.
    /// </summary>
    public class SceneManager3B : MonoBehaviour
    {
        public enum Step
        {
            IntroAudio,        // Audio1,2,3 재생 중
            WaitingApproach,   // 글로우 ON, 손 접근 대기
            ApproachAudio,     // Audio4,5 재생 중 (글로우 OFF)
            WaitingGrab,       // 잡기 대기
            GrabAudio,         // Audio6,7 재생 중
            WaitingPanel,      // 패널 상호작용 대기
            Done
        }

        [Header("── 오디오 ──")]
        [Tooltip("모든 클립을 이 한 AudioSource로 순차 재생")]
        public AudioSource audioSource;
        public AudioClip audio1;
        public AudioClip audio2;
        public AudioClip audio3;
        public AudioClip audio4;
        public AudioClip audio5;
        public AudioClip audio6;
        public AudioClip audio7;

        [Header("── 글로우 대상 ──")]
        public OutlineGlow3B tableGlow;
        public OutlineGlow3B iceBlockGlow;

        [Header("── 씬 전환 ──")]
        public string targetScene = "MainScene";

        [Header("── 디버그 (런타임 표시) ──")]
        public Step currentStep = Step.IntroAudio;
        public bool hasApproached = false;
        public bool hasGrabbed = false;

        private Coroutine _currentSequence;

        void Start()
        {
            if (audioSource == null) audioSource = GetComponent<AudioSource>();
            if (tableGlow != null) tableGlow.SetGlow(false);
            if (iceBlockGlow != null) iceBlockGlow.SetGlow(false);

            _currentSequence = StartCoroutine(IntroSequence());
        }

        // ── 1단계: 인트로 ─────────────────────────────
        IEnumerator IntroSequence()
        {
            currentStep = Step.IntroAudio;
            yield return PlayClip(audio1);
            yield return PlayClip(audio2);
            yield return PlayClip(audio3);

            // 인트로 끝 → 글로우 켜고 접근 대기
            currentStep = Step.WaitingApproach;
            if (tableGlow != null) tableGlow.SetGlow(true);
            if (iceBlockGlow != null) iceBlockGlow.SetGlow(true);
            _currentSequence = null;
        }

        // ── 2단계: 손 접근 (IceProximityDetector3B에서 호출) ──
        public void OnIceApproached()
        {
            if (hasApproached) return;
            if (currentStep != Step.WaitingApproach) return;
            hasApproached = true;
            _currentSequence = StartCoroutine(ApproachSequence());
        }

        IEnumerator ApproachSequence()
        {
            currentStep = Step.ApproachAudio;

            // ★ Audio4 시작과 동시에 글로우 OFF
            if (tableGlow != null) tableGlow.SetGlow(false);
            if (iceBlockGlow != null) iceBlockGlow.SetGlow(false);

            yield return PlayClip(audio4);
            yield return PlayClip(audio5);
            currentStep = Step.WaitingGrab;
            _currentSequence = null;
        }

        // ── 3단계: 잡기 (IceBlockGrabber3B에서 호출) ──
        public void OnIceGrabbed()
        {
            if (hasGrabbed) return;
            hasGrabbed = true;

            // Approach 시퀀스가 진행 중이면 중단
            if (_currentSequence != null)
            {
                StopCoroutine(_currentSequence);
                if (audioSource != null && audioSource.isPlaying) audioSource.Stop();
            }

            _currentSequence = StartCoroutine(GrabSequence());
        }

        IEnumerator GrabSequence()
        {
            currentStep = Step.GrabAudio;
            yield return PlayClip(audio6);
            yield return PlayClip(audio7);
            currentStep = Step.WaitingPanel;
            _currentSequence = null;
        }

        // ── 4단계: 패널 상호작용 → 즉시 씬 전환 ──
        public void OnPanelInteracted()
        {
            if (currentStep != Step.WaitingPanel) return;
            currentStep = Step.Done;
            SceneManager.LoadScene(targetScene);
        }

        // ── 헬퍼: 클립 재생 후 끝날 때까지 대기 ──
        IEnumerator PlayClip(AudioClip clip)
        {
            if (clip == null || audioSource == null) yield break;
            audioSource.clip = clip;
            audioSource.Play();
            yield return new WaitForSeconds(clip.length);
        }
    }
}
