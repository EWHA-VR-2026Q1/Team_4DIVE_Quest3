using System.Collections;
using UnityEngine;

/// <summary>
/// Scene2_B — 전체 오디오 Mute / 라디오 인터랙션
/// </summary>
public class Audio2B : MonoBehaviour
{
    [Header("Scene 시작 시 오디오 뮤트 여부")]
    public bool muteOnStart = false;

    [Header("라디오 오브젝트 (Scene2_B)")]
    public GameObject radioObject;
    public AudioSource radioAudio;
    public float interactDistance = 2.5f;

    private Camera playerCam;

    void Start()
    {
        playerCam = Camera.main;

        if (muteOnStart)
            AudioListener.volume = 0f;
        else
            AudioListener.volume = 1f;
    }

    void Update()
    {
        // 라디오 Ray 인터랙션
        if (radioObject == null || playerCam == null) return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            Ray ray = playerCam.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
            if (Physics.Raycast(ray, out RaycastHit hit, interactDistance))
            {
                if (hit.collider.gameObject == radioObject)
                {
                    ActivateRadio();
                }
            }
        }
    }

    public void MuteAll()
    {
        AudioListener.volume = 0f;
        Debug.Log("[EDEN] 오디오 전체 뮤트");
    }

    public void UnmuteAll()
    {
        AudioListener.volume = 1f;
        Debug.Log("[EDEN] 오디오 복원");
    }

    void ActivateRadio()
    {
        if (radioAudio != null && !radioAudio.isPlaying)
        {
            StartCoroutine(RadioSequence());
        }
    }

    IEnumerator RadioSequence()
    {
        Debug.Log("[EDEN] 라디오 켜짐 — 해커: '소리가 없으면 공간도 죽는다.'");
        radioAudio.Play();
        // 잠깐 볼륨 복원 후 다시 뮤트
        AudioListener.volume = 0.4f;
        yield return new WaitForSeconds(radioAudio.clip != null ? radioAudio.clip.length : 3f);
        AudioListener.volume = 0f;
    }
}
