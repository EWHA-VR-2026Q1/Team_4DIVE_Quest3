using UnityEngine;
using UnityEngine.Events;

public class Event_OnCollision : MonoBehaviour, ICollidable
{
    [Header("Collision Events")]
    public UnityEvent OnEnter;
    public UnityEvent OnExit;

    // 인터페이스 구현: 신호를 받으면 연결된 유니티 이벤트를 실행(Invoke)함
    public void OnCollisionStart() => OnEnter?.Invoke();
    public void OnCollisionEnd() => OnExit?.Invoke();
}