using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.IO;
using System.Collections.Generic;

public class Scene2Builder : Editor
{
    // ─────────────────────────────────────────────
    // 씬 등록
    // ─────────────────────────────────────────────

    [MenuItem("EDEN/Register Scenes (HW_09 only)")]
    static void RegisterHW09Scenes()
    {
        string[] guids = AssetDatabase.FindAssets("t:Scene", new[] { "Assets/HW_09/Scenes" });
        AddScenesToBuildSettings(guids);
        EditorUtility.DisplayDialog("EDEN", "HW_09 씬 등록 완료!", "OK");
    }

    [MenuItem("EDEN/Register All Scenes in Assets")]
    static void RegisterAllScenes()
    {
        string[] guids = AssetDatabase.FindAssets("t:Scene", new[] { "Assets" });
        AddScenesToBuildSettings(guids);
        EditorUtility.DisplayDialog("EDEN", $"Assets 내 모든 씬 등록 완료! ({guids.Length}개)", "OK");
    }

    static void AddScenesToBuildSettings(string[] guids)
    {
        var existing = new HashSet<string>();
        foreach (var s in EditorBuildSettings.scenes)
            existing.Add(s.path);

        var list = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (!existing.Contains(path))
            {
                list.Add(new EditorBuildSettingsScene(path, true));
                Debug.Log($"[EDEN] Build Settings에 추가: {path}");
            }
        }
        EditorBuildSettings.scenes = list.ToArray();
    }

    // ─────────────────────────────────────────────
    // Scene2_A 포레스트 세팅
    // ─────────────────────────────────────────────

    [MenuItem("EDEN/Setup Scene2_A Forest")]
    static void SetupScene2AForest()
    {
        var scene = EditorSceneManager.GetActiveScene();
        if (scene.name != "Scene2_A")
        {
            EditorUtility.DisplayDialog("EDEN", "Scene2_A 씬을 먼저 열어주세요.", "OK");
            return;
        }

        // 나무 프리팹 찾기
        string[] treePaths = AssetDatabase.FindAssets("tree_1 t:Prefab");
        if (treePaths.Length == 0)
        {
            Debug.LogWarning("[EDEN] tree_1 프리팹을 찾을 수 없습니다.");
            return;
        }
        string treePath = AssetDatabase.GUIDToAssetPath(treePaths[0]);
        GameObject treePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(treePath);

        Vector3[] positions = {
            new Vector3(11.2f, -1.15f, 34.62f),
            new Vector3(18.57f, -1.52f, 37.27f),
            new Vector3(31.34f, -1.42f, 25.56f),
            new Vector3(24.65f, -0.91f, 17.64f),
            new Vector3(16.73f, -1.41f, 38.6f)
        };

        foreach (var pos in positions)
        {
            var tree = PrefabUtility.InstantiatePrefab(treePrefab) as GameObject;
            if (tree != null)
            {
                tree.transform.position = pos;
                tree.transform.localScale = Vector3.one * 0.267f;
            }
        }

        EditorSceneManager.MarkSceneDirty(scene);
        Debug.Log("[EDEN] Scene2_A 포레스트 세팅 완료");
        EditorUtility.DisplayDialog("EDEN", "Scene2_A 포레스트 세팅 완료!", "OK");
    }

    // ─────────────────────────────────────────────
    // 현재 씬에 Player 추가
    // ─────────────────────────────────────────────

    [MenuItem("EDEN/Setup Player In Scene")]
    static void SetupPlayerInScene()
    {
        var scene = EditorSceneManager.GetActiveScene();

        // 이미 있으면 스킵
        var existing = GameObject.FindWithTag("Player");
        if (existing != null)
        {
            EditorUtility.DisplayDialog("EDEN", $"이미 Player가 있습니다: {existing.name}", "OK");
            return;
        }

        // Player 생성
        var player = new GameObject("Player");
        player.tag = "Player";

        // CharacterController
        var cc = player.AddComponent<CharacterController>();
        cc.height = 1.8f;
        cc.radius = 0.3f;
        cc.center = new Vector3(0, 0.9f, 0);

        // PlayerController (HW09)
        string playerTypeName = scene.name switch
        {
            "MainScene" => "HW09.PlayerMain, Assembly-CSharp",
            "Scene2_A" => "HW09.Player2A, Assembly-CSharp",
            "Scene2_B" => "HW09.Player2B, Assembly-CSharp",
            _ => "HW09.PlayerMain, Assembly-CSharp"
        };

        var pcType = System.Type.GetType(playerTypeName);
        if (pcType != null)
            player.AddComponent(pcType);
        else
            Debug.LogWarning("[EDEN] HW09.PlayerController를 찾을 수 없습니다. 스크립트를 수동으로 추가해주세요.");

        // Main Camera 자식으로 추가
        var camGo = new GameObject("Main Camera");
        camGo.tag = "MainCamera";
        camGo.transform.SetParent(player.transform);
        camGo.transform.localPosition = new Vector3(0, 1.6f, 0);
        camGo.transform.localRotation = Quaternion.identity;
        camGo.AddComponent<Camera>();
        camGo.AddComponent<AudioListener>();

        // 적당한 높이에 배치
        player.transform.position = new Vector3(0, 1f, 0);

        EditorSceneManager.MarkSceneDirty(scene);
        Selection.activeGameObject = player;

        Debug.Log($"[EDEN] '{scene.name}'에 Player 추가 완료");
        EditorUtility.DisplayDialog("EDEN",
            $"'{scene.name}'에 Player 추가 완료!\n\n" +
            "⚠️ 주의: 씬에 Terrain이 있으면 플레이 시 자동으로 지형 위에 안착합니다.\n" +
            "실내 씬은 Player Y 위치를 바닥에 맞게 조정하세요.", "OK");
    }

    // ─────────────────────────────────────────────
    // Door 타겟 씬 일괄 설정
    // ─────────────────────────────────────────────

    [MenuItem("EDEN/Setup Door Targets...")]
    static void SetupDoorTargets()
    {
        var scene = EditorSceneManager.GetActiveScene();
        if (scene.name != "MainScene")
        {
            EditorUtility.DisplayDialog("EDEN", "MainScene을 먼저 열어주세요.", "OK");
            return;
        }

        // 각 Door Trigger 찾기
        string[] doorNames = { "Door_01", "Door_02", "Door_03", "Door_04" };
        string[] current = new string[4];
        DoorMain[] triggers = new DoorMain[4];

        for (int i = 0; i < doorNames.Length; i++)
        {
            var doorGo = GameObject.Find(doorNames[i]);
            if (doorGo == null) continue;
            var trigger = doorGo.GetComponentInChildren<DoorMain>();
            if (trigger == null) continue;
            triggers[i] = trigger;
            current[i] = trigger.targetScene;
        }

        string msg = "현재 Door 타겟 씬:\n";
        for (int i = 0; i < 4; i++)
            msg += $"  Door_{i+1:D2}: {(string.IsNullOrEmpty(current[i]) ? "(미설정)" : current[i])}\n";
        msg += "\n씬 이름을 변경하려면 각 Door의 DoorInteraction 컴포넌트에서\ntargetScene 필드를 직접 수정하세요.";

        EditorUtility.DisplayDialog("EDEN — Door Targets", msg, "OK");
    }
}
