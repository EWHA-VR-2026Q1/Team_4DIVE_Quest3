using UnityEngine;
using UnityEngine.Events;

public class Event_OnRay : MonoBehaviour, IRayInteractable
{
    [Header("Ray Events")]
    public UnityEvent OnEnter;
    public UnityEvent OnStay;
    public UnityEvent OnExit;
    public UnityEvent OnClick;

    public void OnRayEnter() => OnEnter?.Invoke();
    public void OnRayStay() => OnStay?.Invoke();
    public void OnRayExit() => OnExit?.Invoke();
    public void OnRayClick() => OnClick?.Invoke();
}