using UnityEngine;
using UnityEngine.SceneManagement;

public class DoorMain : MonoBehaviour
{
    [Header("이동할 씬 이름")]
    public string targetScene;

    [Header("감지 반경 (수평, m)")]
    public float triggerRadius = 2.5f;

    [Header("텍스트 UI (선택)")]
    public GameObject hintUI;

    private bool _triggered = false;
    private Transform _player;

    void Start()
    {
        var go = GameObject.FindWithTag("Player");
        if (go != null) _player = go.transform;
        Debug.Log($"[DoorInteraction] {gameObject.name} Start. targetScene={targetScene}, radius={triggerRadius}, player={(_player != null ? _player.name : "NULL")}");
    }

    void Update()
    {
        if (_triggered) return;
        if (string.IsNullOrEmpty(targetScene)) return;

        if (_player == null)
        {
            var go = GameObject.FindWithTag("Player");
            if (go != null) _player = go.transform;
            else return;
        }

        // 수평 거리만 체크 (Y 무시)
        Vector3 myPos  = new Vector3(transform.position.x, 0, transform.position.z);
        Vector3 plrPos = new Vector3(_player.position.x,   0, _player.position.z);
        float dist = Vector3.Distance(myPos, plrPos);

        if (dist <= triggerRadius)
        {
            _triggered = true;
            Debug.Log($"[DoorInteraction] 씬 전환 → {targetScene}  (dist={dist:F2})");
            if (hintUI != null) hintUI.SetActive(false);
            SceneManager.LoadScene(targetScene);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(new Vector3(transform.position.x, 0, transform.position.z), triggerRadius);
    }
}
