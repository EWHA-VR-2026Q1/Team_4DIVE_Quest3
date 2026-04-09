using System.Collections;
using UnityEngine;

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
        // AudioAI_Phase0 AudioSource via AudioManager
        GameObject audioManager = GameObject.Find("AudioManager");
        if (audioManager != null)
        {
            foreach (AudioSource src in audioManager.GetComponentsInChildren<AudioSource>())
            {
                if (src.gameObject.name == "AudioAI_Phase0")
                {
                    _audioAI = src;
                    break;
                }
            }
        }

        // Main Camera transform as player target
        if (Camera.main != null)
            _playerTarget = Camera.main.transform;

        // "upper_arm.R" anywhere in NPC_1's child hierarchy
        _rightArmBone = FindDeepChild(transform, "upper_arm.R");

        _audioStarted  = false;
        _hasRaisedHand = false;
    }

    void Update()
    {
        // Rotate toward player on Y axis only
        if (_playerTarget != null)
        {
            Vector3 dir = _playerTarget.position - transform.position;
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(dir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot,
                                                      SlerpSpeed * Time.deltaTime);
            }
        }

        if (_audioAI == null) return;

        if (_audioAI.isPlaying)
            _audioStarted = true;

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
        float      elapsed   = 0f;
        float      duration  = 1.0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            _rightArmBone.localRotation = Quaternion.Slerp(startRot, targetRot, t);
            yield return null;
        }

        _rightArmBone.localRotation = targetRot;
    }

    // Depth-first search through the full child hierarchy
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
