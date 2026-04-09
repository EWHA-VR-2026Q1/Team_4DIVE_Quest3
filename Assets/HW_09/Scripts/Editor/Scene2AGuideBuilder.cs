using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using HW09;

public static class Scene2AGuideBuilder
{
    static readonly Vector3 ExitPos = new Vector3(20f, 0f, 35f);

    [MenuItem("EDEN/Setup Scene2_A Exit Guide")]
    public static void Build()
    {
        var toDelete = new List<GameObject>();
        foreach (var go in GameObject.FindObjectsOfType<GameObject>(true))
            if (go.transform.parent == null && go.name == "=== EXIT GUIDE ===")
                toDelete.Add(go);
        foreach (var go in toDelete)
            if (go != null) Undo.DestroyObjectImmediate(go);

        var root = new GameObject("=== EXIT GUIDE ===");
        Undo.RegisterCreatedObjectUndo(root, "ExitGuide");
        root.SetActive(false);

        BuildExitDoor(root);
        BuildGuideArrows(root);

        var mgr = Object.FindObjectOfType<Manager2A>();
        if (mgr != null)
        {
            var so = new SerializedObject(mgr);
            so.FindProperty("exitGuide").objectReferenceValue = root;
            so.ApplyModifiedProperties();
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("[EDEN] Exit Guide 빌드 완료! 출구: 플레이어 바로 앞 (20, 0, 35)");
    }

    static void BuildExitDoor(GameObject root)
    {
        var door = new GameObject("ExitDoor");
        Undo.RegisterCreatedObjectUndo(door, "ExitDoor");
        door.transform.SetParent(root.transform);
        door.transform.position = ExitPos;

        var mat = MakeEmitMat(new Color(0.1f, 0.4f, 0.9f), new Color(0f, 0.8f, 2.5f));
        float W = 2.2f, H = 3.2f, T = 0.15f;
        Box(door, "PillarL", new Vector3(-W*0.5f, H*0.5f, 0), new Vector3(T, H, T), mat);
        Box(door, "PillarR", new Vector3( W*0.5f, H*0.5f, 0), new Vector3(T, H, T), mat);
        Box(door, "Lintel",  new Vector3(0, H, 0), new Vector3(W+T, T, T), mat);

        var pm = new Material(Shader.Find("Standard"));
        pm.color = new Color(0.1f, 0.5f, 1f, 0.3f);
        pm.SetInt("_Mode", 3);
        pm.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        pm.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        pm.SetInt("_ZWrite", 0); pm.renderQueue = 3000;
        pm.EnableKeyword("_EMISSION"); pm.SetColor("_EmissionColor", new Color(0f, 0.3f, 0.8f));
        var portal = GameObject.CreatePrimitive(PrimitiveType.Quad);
        Undo.RegisterCreatedObjectUndo(portal, "Portal"); portal.name = "PortalFill";
        portal.transform.SetParent(door.transform);
        portal.transform.localPosition = new Vector3(0, H*0.5f, 0.01f);
        portal.transform.localScale    = new Vector3(W, H, 1);
        Object.DestroyImmediate(portal.GetComponent<MeshCollider>());
        portal.GetComponent<MeshRenderer>().material = pm;

        var lg = new GameObject("DoorLight"); Undo.RegisterCreatedObjectUndo(lg, "DL");
        lg.transform.SetParent(door.transform); lg.transform.localPosition = new Vector3(0, H+0.5f, 0);
        var lt = lg.AddComponent<Light>(); lt.type = LightType.Point;
        lt.color = new Color(0.4f, 0.7f, 1f); lt.intensity = 4f; lt.range = 8f;
    }

    static void BuildGuideArrows(GameObject root)
    {
        Vector3[] pos = { new Vector3(20.8f, 3f, 30f), new Vector3(20.5f, 3f, 32.5f) };
        var am = MakeEmitMat(new Color(1f, 0.85f, 0f), new Color(3f, 2f, 0f));

        for (int i = 0; i < pos.Length; i++)
        {
            var ar = new GameObject($"GuideArrow_{i+1}");
            Undo.RegisterCreatedObjectUndo(ar, "GA"); ar.transform.SetParent(root.transform);
            ar.transform.position = pos[i];
            Vector3 d = new Vector3(ExitPos.x - pos[i].x, 0, ExitPos.z - pos[i].z);
            if (d != Vector3.zero) ar.transform.rotation = Quaternion.LookRotation(d.normalized, Vector3.up);

            var sh = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Undo.RegisterCreatedObjectUndo(sh, "Sh"); sh.name = "Shaft"; sh.transform.SetParent(ar.transform);
            sh.transform.localPosition = new Vector3(0,0,0.5f); sh.transform.localScale = new Vector3(0.25f,0.25f,1f);
            Object.DestroyImmediate(sh.GetComponent<BoxCollider>()); sh.GetComponent<MeshRenderer>().material = am;

            for (int s = -1; s <= 1; s += 2)
            {
                var f = GameObject.CreatePrimitive(PrimitiveType.Cube);
                Undo.RegisterCreatedObjectUndo(f, "F"); f.name = "Fin"; f.transform.SetParent(ar.transform);
                f.transform.localPosition = new Vector3(s*0.28f,0,1.1f);
                f.transform.localEulerAngles = new Vector3(0,s*-40f,0);
                f.transform.localScale = new Vector3(0.22f,0.22f,0.55f);
                Object.DestroyImmediate(f.GetComponent<BoxCollider>()); f.GetComponent<MeshRenderer>().material = am;
            }

            var lg = new GameObject("AL"); Undo.RegisterCreatedObjectUndo(lg, "AL");
            lg.transform.SetParent(ar.transform); lg.transform.localPosition = Vector3.zero;
            var lt = lg.AddComponent<Light>(); lt.type = LightType.Point;
            lt.color = new Color(1f,0.9f,0.2f); lt.intensity = 2.5f; lt.range = 5f;

            var b = ar.AddComponent<GuideArrow2A>();
            b.rotateSpeed = 0f; b.bounceHeight = 0.25f; b.bounceSpeed = 1.8f;
        }
    }

    static Material MakeEmitMat(Color base_, Color emit)
    {
        var m = new Material(Shader.Find("Standard"));
        m.color = base_; m.SetFloat("_Metallic", 0.05f); m.SetFloat("_Glossiness", 0.6f);
        m.EnableKeyword("_EMISSION"); m.SetColor("_EmissionColor", emit); return m;
    }

    static void Box(GameObject parent, string name, Vector3 lp, Vector3 ls, Material mat)
    {
        var g = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Undo.RegisterCreatedObjectUndo(g, name); g.name = name; g.transform.SetParent(parent.transform);
        g.transform.localPosition = lp; g.transform.localScale = ls;
        Object.DestroyImmediate(g.GetComponent<BoxCollider>()); g.GetComponent<MeshRenderer>().material = mat;
    }
}
