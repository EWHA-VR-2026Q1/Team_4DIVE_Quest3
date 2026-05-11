using UnityEngine;
using UnityEngine.Events;

// 매개변수를 전달하는 UnityEvent 클래스를 명시적으로 선언 (유니티 버전에 따라 필요)
[System.Serializable]
public class TransformEvent : UnityEvent<Transform> { }

public class Event_OnHand_Least : MonoBehaviour, IHandInteractable_Least
{
    [Header("Hand Interaction Events")]
    public UnityEvent OnEnter;
    public UnityEvent OnStay;
    public UnityEvent OnExit;
    public UnityEvent OnPinch;
    public UnityEvent OnPoke;

    // 이 부분을 일반 UnityEvent에서 Transform을 받는 이벤트로 변경
    public TransformEvent OnGrab;
    public UnityEvent OnRelease;

    public void OnHandEnter() => OnEnter?.Invoke();
    public void OnHandStay() => OnStay?.Invoke();
    public void OnHandExit() => OnExit?.Invoke();
    public void OnHandPinch() => OnPinch?.Invoke();
    public void OnHandPoke() => OnPoke?.Invoke();

    // 이제 인자로 받은 handAnchor를 이벤트에 실어서 보냅니다.
    public void OnHandGrab(Transform handAnchor) => OnGrab?.Invoke(handAnchor);
    public void OnHandRelease() => OnRelease?.Invoke();
}