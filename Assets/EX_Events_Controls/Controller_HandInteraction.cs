using UnityEngine;

public class Controller_HandInteraction : MonoBehaviour, IHandInteractable_Least
{
    private Transform originalParent;
    private bool isGrabbed = false;

    // 잡는 순간의 오프셋을 저장할 변수
    private Vector3 positionOffset;
    private Quaternion rotationOffset;

    // 인터페이스 구현 (필요 없는 기능은 비워둡니다)
    public void OnHandEnter() => Debug.Log("Grab 준비 완료");
    public void OnHandStay() { }
    public void OnHandExit() { }
    public void OnHandPinch() {
        //Debug.Log($"<color=red>Controller OnHandPinch:</color>");
    }
    public void OnHandPoke() {
        //Debug.Log($"<color=red>Controller OnHandPoke:</color>");
    }


    public void OnHandGrab(Transform handAnchor)
    {
        if (isGrabbed) return; // 이미 잡고 있다면 무시

        //Debug.Log($"<color=red>Controller OnHandGrab:</color> {handAnchor.name}");
        isGrabbed = true;
        originalParent = transform.parent;

        // 1. 물리 엔진 간섭 방지
        if (TryGetComponent<Rigidbody>(out var rb))
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        // 2. 오프셋 계산 (현재 위치 - 손의 위치)
        // InverseTransformPoint를 사용하면 손 기준의 상대적 위치를 정확히 구할 수 있습니다.
        positionOffset = handAnchor.InverseTransformPoint(transform.position);
        rotationOffset = Quaternion.Inverse(handAnchor.rotation) * transform.rotation;

        // 3. 부모 설정
        transform.SetParent(handAnchor);

        // 4. 오프셋 적용 (순간이동 방지)
        transform.localPosition = positionOffset;
        transform.localRotation = rotationOffset;
    }

    public void OnHandRelease()
    {
        if (!isGrabbed) return;

        //Debug.Log("<color=red>Controller OnHandRelease</color>");
        isGrabbed = false;

        // 부모 관계 해제
        transform.SetParent(originalParent);

        // 물리 엔진 복구
        if (TryGetComponent<Rigidbody>(out var rb))
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }
    }
}