using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public static class MissingScriptFinder
{
    [MenuItem("EDEN/Debug: Find Missing Scripts in All Scenes")]
    public static void FindAll()
    {
        string[] scenePaths = {
            "Assets/HW_09/Scenes/MainScene.unity",
            "Assets/HW_09/Scenes/Scene1_A.unity",
            "Assets/HW_09/Scenes/Scene2_A.unity",
            "Assets/HW_09/Scenes/Scene2_B.unity",
            "Assets/HW_09/Scenes/Scene3_A.unity",
            "Assets/HW_09/Scenes/Set4_B.unity"
        };

        int totalMissing = 0;

        foreach (var path in scenePaths)
        {
            var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            var allObjects = Object.FindObjectsOfType<GameObject>(true);

            foreach (var go in allObjects)
            {
                int missing = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(go);
                if (missing > 0)
                {
                    Debug.LogWarning($"[{scene.name}] ⚠️ Missing Script x{missing}: {GetPath(go)}", go);
                    totalMissing += missing;
                }
            }
        }

        if (totalMissing == 0)
            Debug.Log("✅ 모든 씬에 Missing Script 없음!");
        else
            Debug.LogWarning($"총 {totalMissing}개 Missing Script 발견. 위 로그에서 오브젝트 클릭하면 씬에서 선택됩니다.");
    }

    static string GetPath(GameObject go)
    {
        string path = go.name;
        var t = go.transform.parent;
        while (t != null) { path = t.name + "/" + path; t = t.parent; }
        return path;
    }
}
