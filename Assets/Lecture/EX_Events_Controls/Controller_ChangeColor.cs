using UnityEngine;

public class Controller_ChangeColor : MonoBehaviour
{
    [Header("Color Settings")]
    [SerializeField] private Color hoverColor = Color.yellow;
    [SerializeField] private Color clickColor = Color.red;

    private Renderer rend;
    private Color originalColor;

    void Start()
    {
        rend = GetComponent<Renderer>();
        if (rend != null)
        {
            // URP 권장: "_BaseColor", 일반: ".color"
            // 여기서는 호환성을 위해 공유 머티리얼이 아닌 인스턴스 머티리얼의 색상을 저장합니다.
            originalColor = rend.material.color;
        }
    }

    // 1. 호버 색상으로 변경
    public void SetHoverColor()
    {
        if (rend != null) rend.material.color = hoverColor;
    }

    // 2. 클릭 색상으로 변경
    public void SetClickColor()
    {
        if (rend != null) rend.material.color = clickColor;
    }

    // 3. 원래 색상으로 복구
    public void ResetColor()
    {
        if (rend != null) rend.material.color = originalColor;
    }
}