// 인터페이스는 클래스 밖(위나 아래)에 두는 것이 일반적입니다.
public interface IRayInteractable
{
    void OnRayEnter();
    void OnRayStay();
    void OnRayExit();
    void OnRayClick();
}