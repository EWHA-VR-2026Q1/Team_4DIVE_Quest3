using System.Collections;
using UnityEngine;

namespace HW09.Heejin
{
    public class NPC1Behavior4B : MonoBehaviour
    {
        private Transform   _playerTarget;
        private Transform   _rightArmBone;
        private AudioSource _audioAI;

        private bool _audioStarted  = false;
        private bool _hasRaisedHand = false;

        private const float SlerpSpeed = 2.0f;

        void Start()
        {
            GameObject audioManager = GameObject.Find("AudioManager");
            if (audioManager != null)
            {
                foreach (AudioSource src in audioManager.GetComponentsInChildren<AudioSource>())
                {
                    if (src.gameObject.name == "AudioAI_Phase0") { _audioAI = src; break; }
                }
            }

            if (Camera.main != null) _playerTarget = Camera.main.transform;
            _rightArmBone = FindDeepChild(transform, "upper_arm.R");
        }

        void Update()
        {
            if (_playerTarget != null)
            {
                Vector3 dir = _playerTarget.position - transform.position;
                dir.y = 0f;
                if (dir.sqrMagnitude > 0.001f)
                    transform.rotation = Quaternion.Slerp(transform.rotation,
                        Quaternion.LookRotation(dir), SlerpSpeed * Time.deltaTime);
            }

            if (_audioAI == null) return;
            if (_audioAI.isPlaying) _audioStarted = true;
            if (_audioStarted && !_audioAI.isPlaying && !_hasRaisedHand)
            {
                _hasRaisedHand = true;
                StartCoroutine(RaiseHandCoroutine());
            }
        }

        IEnumerator RaiseHandCoroutine()
        {
            if (_rightArmBone == null) yield break;
            Quaternion startRot  = _rightArmBone.localRotation;
            Quaternion targetRot = Quaternion.Euler(-22f, -120f, 8f);
            float elapsed = 0f;
            while (elapsed < 1f)
            {
                elapsed += Time.deltaTime;
                _rightArmBone.localRotation = Quaternion.Slerp(startRot, targetRot, Mathf.Clamp01(elapsed));
                yield return null;
            }
            _rightArmBone.localRotation = targetRot;
        }

        private static Transform FindDeepChild(Transform parent, string childName)
        {
            foreach (Transform child in parent)
            {
                if (child.name == childName) return child;
                Transform found = FindDeepChild(child, childName);
                if (found != null) return found;
            }
            return null;
        }
    }
}
