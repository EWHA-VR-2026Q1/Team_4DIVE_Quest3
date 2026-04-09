using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; // 씬 관리를 위한 필수 라이브러리

public class SceneChanger : MonoBehaviour
{
    void Update()
    {
        // 컨트롤러의 'A' 버튼(오른쪽 컨트롤러 하단 버튼)을 누르면 실행
        if (OVRInput.GetDown(OVRInput.Button.One))
        {
            // Build Settings의 1번 인덱스 씬으로 이동
            SceneManager.LoadScene(1); 
        }

        // 만약 'B' 버튼을 누르면 0번(메인) 씬으로 이동하게 하고 싶다면
        if (OVRInput.GetDown(OVRInput.Button.Two))
        {
            SceneManager.LoadScene(0);
        }
    }
}