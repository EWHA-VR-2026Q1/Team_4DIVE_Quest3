using UnityEngine;

/// <summary>
/// DoorTrigger — door 오브젝트에 붙이는 씬 전환 트리거
///
/// ■ 설치 방법
///   1. door 오브젝트(또는 자식)에 BoxCollider 추가, IsTrigger = true
///   2. 이 컴포넌트를 door 오브젝트에 추가
///   3. Inspector의 Manager 필드에 Scene3AManager 오브젝트 연결
///   4. Meta Quest: XR Origin 루트 오브젝트에 "Player" 태그 설정
///
/// ■ PC 테스트
///   Scene3AManager.Update()의 카메라 근접 판정이 동작하므로
///   이 스크립트가 없어도 씬 전환됩니다.
///
/// ■ VR 동작
///   XR Origin(또는 자식) Collider가 Trigger와 겹치면 OnPlayerEnterDoor() 호출
/// </summary>
[RequireComponent(typeof(Collider))]
public class DoorTrigger3A : MonoBehaviour
{
    [Header("── 연결 필요 ──────────────────────────")]
    [Tooltip("SceneManager3A가 붙어 있는 오브젝트")]
    public SceneManager3A manager;

    [Header("── 감지 태그 ──────────────────────────")]
    [Tooltip("Player_OVRInput_Parts 루트 오브젝트에 설정할 태그 (기본: Player)")]
    public string playerTag  = "Player";
    [Tooltip("OVR Hand Anchor 태그 (손이 문에 닿아도 감지)")]
    public string ovrHandTag = "OVRHand";

    void Start()
    {
        if (manager == null)
            manager = FindObjectOfType<SceneManager3A>();

        if (manager == null)
            Debug.LogError("[DoorTrigger] SceneManager3A를 찾을 수 없습니다. Inspector에서 연결해주세요.");

        Collider col = GetComponent<Collider>();
        if (!col.isTrigger)
        {
            Debug.LogWarning("[DoorTrigger] Is Trigger가 꺼져 있어 자동으로 켭니다.");
            col.isTrigger = true;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (manager == null) return;

        if (IsPlayer(other))
        {
            Debug.Log($"[DoorTrigger] 플레이어 감지: {other.gameObject.name} (tag={other.tag})");
            manager.OnPlayerEnterDoor();
        }
    }

    bool IsPlayer(Collider other)
    {
        // 1) 태그로 판별 (Player_OVRInput_Parts 루트에 "Player" 태그 설정 권장)
        if (other.CompareTag(playerTag))  return true;
        if (other.CompareTag(ovrHandTag)) return true;

        // 2) OVR / XR 계층 이름으로 Fallback
        string n = other.gameObject.name;
        if (n.Contains("OVRCameraRig")    || n.Contains("CenterEyeAnchor") ||
            n.Contains("Player_OVRInput") || n.Contains("CameraRig")       ||
            n.Contains("XR Origin")       || n.Contains("XRRig"))
            return true;

        // 3) 부모 계층 5단계까지 "Player" 태그 탐색
        Transform t = other.transform.parent;
        int depth = 0;
        while (t != null && depth < 5)
        {
            if (t.CompareTag(playerTag)) return true;
            if (t.gameObject.name.Contains("Player_OVRInput") ||
                t.gameObject.name.Contains("OVRCameraRig"))
                return true;
            t = t.parent;
            depth++;
        }

        return false;
    }
}
