using UnityEngine;

// 이 스크립트를 아웃라인이 필요한 오브젝트에 붙입니다.
[RequireComponent(typeof(Outline))]
public class Controller_Outline : MonoBehaviour
{
    private Outline outline;
    Color OutlineColor;

    void Awake()
    {
        outline = GetComponent<Outline>();
        outline.enabled = false; // 시작할 때는 끕니다.
        OutlineColor = outline.OutlineColor;
        //print("Controller_Outline");
    }

    public void OnRayEnter()
    {
        outline.enabled = true;
    }

    public void OnRayStay()
    {
        // 머무는 동안 실행할 로직 (필요 시)
    }

    public void OnRayExit()
    {
        outline.enabled = false;
        outline.OutlineColor = OutlineColor;
    }

    public void OnRayClick()
    {
        Debug.Log($"{gameObject.name}을 클릭했습니다!");
        outline.OutlineColor = Color.red; // 클릭 시 색상 변경 예시
    }
}