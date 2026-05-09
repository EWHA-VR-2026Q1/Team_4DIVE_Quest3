using UnityEngine;
using UnityEngine.SceneManagement;

namespace HW09
{
    public class ExitTrigger2B : MonoBehaviour
    {
        private bool triggered = false;

        private void OnTriggerEnter(Collider other)
        {
            if (triggered) return;
            if (!other.CompareTag("Player")) return;

            triggered = true;
            SceneManager.LoadScene("MainScene");
        }
    }
}
