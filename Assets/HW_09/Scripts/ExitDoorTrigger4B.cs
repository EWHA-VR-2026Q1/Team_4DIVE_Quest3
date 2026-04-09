using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitDoorTrigger4B : MonoBehaviour
{
    [Header("Detection")]
    [Tooltip("Tag on the player GameObject")]
    public string playerTag = "Player";

    [Header("Scene")]
    public string targetScene = "MainScene";

    void Awake()
    {
        // Add a separate trigger volume slightly in front of the door (toward the room, -Z local)
        BoxCollider trigger = gameObject.AddComponent<BoxCollider>();
        trigger.isTrigger = true;
        trigger.center = new Vector3(0f, 0f, -0.6f);
        trigger.size   = new Vector3(1.0f, 2.0f, 0.8f);
    }

    void Start()
    {
        // Disable emission at scene start (reset to black per-renderer instance)
        Renderer r = GetComponent<Renderer>();
        if (r != null)
        {
            Material mat = r.material;
            mat.SetColor("_EmissionColor", Color.black);
            mat.DisableKeyword("_EMISSION");
        }

        // Ensure ExitDoorLight is off if it already exists (e.g. after re-entering play mode)
        Transform existingLight = transform.Find("ExitDoorLight");
        if (existingLight != null)
            existingLight.gameObject.SetActive(false);
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Door trigger hit: {other.name}, tag={other.tag}");

        if (!(other.CompareTag("Player") || other.CompareTag("MainCamera")))
            return;

        SceneManager.LoadScene(targetScene);
    }
}
