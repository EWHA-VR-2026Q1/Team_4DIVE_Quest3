using UnityEngine;
using UnityEngine.SceneManagement;

namespace HW09
{
    public class ExitTrigger2A : MonoBehaviour
    {
        private bool triggered = false;

        void OnTriggerEnter(Collider other)
        {
            if (triggered) return;
            if (!other.CompareTag("Player")) return;

            triggered = true;
            Debug.Log("[Scene2_A] 플레이어 출구 진입 -> MainScene 로드");
            SceneManager.LoadScene("MainScene");
        }
    }
}
