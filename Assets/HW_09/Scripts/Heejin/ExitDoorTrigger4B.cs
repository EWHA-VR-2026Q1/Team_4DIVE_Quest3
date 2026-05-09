using UnityEngine;
using UnityEngine.SceneManagement;

namespace HW09.Heejin
{
    public class ExitDoorTrigger4B : MonoBehaviour
    {
        [Header("Detection")]
        public string playerTag = "Player";

        [Header("Scene")]
        public string targetScene = "MainScene";

        void Awake()
        {
            BoxCollider trigger = gameObject.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.center = new Vector3(0f, 0f, -0.6f);
            trigger.size   = new Vector3(1.0f, 2.0f, 0.8f);
        }

        void Start()
        {
            Renderer r = GetComponent<Renderer>();
            if (r != null)
            {
                Material mat = r.material;
                mat.SetColor("_EmissionColor", Color.black);
                mat.DisableKeyword("_EMISSION");
            }

            Transform existingLight = transform.Find("ExitDoorLight");
            if (existingLight != null)
                existingLight.gameObject.SetActive(false);
        }

        void OnTriggerEnter(Collider other)
        {
            if (!(other.CompareTag("Player") || other.CompareTag("MainCamera"))) return;
            SceneManager.LoadScene(targetScene);
        }
    }
}
