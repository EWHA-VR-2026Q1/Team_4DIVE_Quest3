using UnityEngine;

public class Controller_SceneChanger : MonoBehaviour
{
    [SerializeField] private string targetSceneName; // 이동할 씬 이름

    // OnRay_Event의 OnClick에서 이 함수를 호출하게 설정하세요.
    public void TriggerChange()
    {
        if (EX_SceneChanger.Instance != null)
        {
            EX_SceneChanger.Instance.ChangeScene(targetSceneName);
        }
        else
        {
            Debug.LogError("SceneChanger Instance를 찾을 수 없습니다!");
        }
    }
}