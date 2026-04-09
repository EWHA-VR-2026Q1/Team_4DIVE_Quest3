using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public static class DoorDebugger
{
    [MenuItem("EDEN/Debug: Show All Door Settings")]
    public static void ShowDoorSettings()
    {
        var allDI = Object.FindObjectsOfType<DoorMain>();
        foreach (var di in allDI)
        {
            string parent = di.transform.parent?.name ?? "none";
            Vector3 pos = di.transform.position;
            Debug.Log($"[DOOR:{parent}] pos=({pos.x:F2},{pos.y:F2},{pos.z:F2}) | scene='{di.targetScene}' | radius={di.triggerRadius}");
        }
    }

    [MenuItem("EDEN/Fix: Move All Triggers to Corridor")]
    public static void FixAllTriggerPositions()
    {
        // 각 문의 Trigger를 복도 쪽(X=0)으로 이동
        // SFHubBuilder에서 DZ = { 10f, 22f, 34f, 46f }
        float[] dz = { 10f, 22f, 34f, 46f };
        string[] doorNames = { "Door_01", "Door_02", "Door_03", "Door_04" };

        var allDI = Object.FindObjectsOfType<DoorMain>();
        foreach (var di in allDI)
        {
            string parentName = di.transform.parent?.name ?? "";
            for (int i = 0; i < doorNames.Length; i++)
            {
                if (parentName == doorNames[i])
                {
                    // 복도 입구 위치로 이동 (X=0, Y=1.75, Z=doorZ)
                    Undo.RecordObject(di.transform, "FixTrigger");
                    di.transform.position = new Vector3(0f, 1.75f, dz[i]);
                    Debug.Log($"[EDEN] {doorNames[i]}/Trigger 위치 → (0, 1.75, {dz[i]})");
                    break;
                }
            }
        }
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
    }
}
