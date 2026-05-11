using UnityEngine;

public interface IHandInteractable_Least
{
    void OnHandEnter();
    void OnHandStay();
    void OnHandExit();
    void OnHandPinch();   // 4-1: 원거리 레이 핀치
    void OnHandPoke();    // 4-3: 근거리 검지 찌르기
    void OnHandGrab(Transform handAnchor); // 누구에게 잡혔는지 인자 추가
    void OnHandRelease();
}