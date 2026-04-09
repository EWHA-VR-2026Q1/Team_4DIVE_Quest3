using UnityEngine;
using UnityEngine.SceneManagement;

public class ManagerMain : MonoBehaviour
{
    public static ManagerMain Instance;

    [Header("클리어 상태")]
    public bool[] setClear = new bool[4];

    [Header("허브 환경 오브젝트")]
    public GameObject lightOff;
    public GameObject wallCrack;
    public GameObject waterPuddle;
    public GameObject structureTwist;

    [Header("Player 프리팹 (없으면 자동 생성)")]
    public GameObject playerPrefab;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void OnEnable()  => SceneManager.sceneLoaded += OnSceneLoaded;
    void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // MainScene 으로 돌아오면 허브 환경 업데이트
        if (scene.name == "MainScene")
        {
            UpdateHubEnvironment();
            return;
        }

        // 팀원 씬 : Player 태그 오브젝트 존재 확인
        var player = GameObject.FindWithTag("Player");
        if (player == null)
        {
            if (playerPrefab != null)
            {
                Instantiate(playerPrefab, Vector3.up * 2f, Quaternion.identity);
                Debug.Log($"[GameManager] '{scene.name}' — Player 없어서 프리팹 생성");
            }
            else
            {
                Debug.LogWarning($"[GameManager] ⚠️ 씬 '{scene.name}'에 'Player' 태그 오브젝트가 없습니다!\n" +
                                  "EDEN > Setup Player In Scene 메뉴로 Player를 추가하거나\n" +
                                  "GameManager의 PlayerPrefab 슬롯에 프리팹을 연결하세요.");
            }
        }
    }

    public void ClearSet(int index)
    {
        if (index < 0 || index >= setClear.Length) return;
        setClear[index] = true;
        Debug.Log($"[EDEN] Set {index + 1} 클리어!");
        UpdateHubEnvironment();
    }

    void UpdateHubEnvironment()
    {
        if (setClear[0] && lightOff != null)       lightOff.SetActive(true);
        if (setClear[1] && wallCrack != null)       wallCrack.SetActive(true);
        if (setClear[2] && waterPuddle != null)     waterPuddle.SetActive(true);
        if (setClear[3] && structureTwist != null)  structureTwist.SetActive(true);
    }

    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void ReturnToHub()
    {
        SceneManager.LoadScene("MainScene");
    }

    public int GetClearCount()
    {
        int count = 0;
        foreach (bool b in setClear) if (b) count++;
        return count;
    }
}
