using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using HW09;

public static class MissingScriptFixer
{
    [MenuItem("EDEN/Fix: Remove All Missing Scripts")]
    public static void RemoveAllMissingScripts()
    {
        string[] scenePaths = {
            "Assets/HW_09/Scenes/MainScene.unity",
            "Assets/HW_09/Scenes/Scene1_A.unity",
            "Assets/HW_09/Scenes/Scene2_A.unity",
            "Assets/HW_09/Scenes/Scene2_B.unity",
            "Assets/HW_09/Scenes/Scene3_A.unity",
        };

        int total = 0;
        foreach (var path in scenePaths)
        {
            var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            int removed = 0;
            foreach (var go in Object.FindObjectsOfType<GameObject>(true))
            {
                int count = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
                if (count > 0)
                {
                    removed += count;
                    EditorUtility.SetDirty(go);
                }
            }
            if (removed > 0)
            {
                EditorSceneManager.SaveScene(scene);
                Debug.Log($"[{scene.name}] Missing Script {removed}개 제거 후 저장");
            }
            else
                Debug.Log($"[{scene.name}] ✅ Missing Script 없음");
            total += removed;
        }
        Debug.Log($"총 {total}개 Missing Script 제거 완료");
    }

    [MenuItem("EDEN/Fix: Reattach MainScene Scripts")]
    public static void ReattachMainScene()
    {
        EditorSceneManager.OpenScene("Assets/HW_09/Scenes/MainScene.unity", OpenSceneMode.Single);

        // GameManager → ManagerMain
        var gm = GameObject.Find("GameManager");
        if (gm != null && gm.GetComponent<ManagerMain>() == null)
        {
            gm.AddComponent<ManagerMain>();
            Debug.Log("[MainScene] GameManager → ManagerMain 추가");
        }

        // Door_01~04 / Trigger → DoorMain
        string[] doorNames = { "Door_01", "Door_02", "Door_03", "Door_04" };
        string[] targetScenes = { "Scene1_A", "Scene2_A", "Scene3_A", "Scene4_A" };
        var hub = GameObject.Find("=== SF HUB ===");
        if (hub != null)
        {
            for (int i = 0; i < doorNames.Length; i++)
            {
                var doorTf = hub.transform.Find(doorNames[i] + "/Trigger");
                if (doorTf != null && doorTf.GetComponent<DoorMain>() == null)
                {
                    var dm = doorTf.gameObject.AddComponent<DoorMain>();
                    dm.targetScene = targetScenes[i];
                    dm.triggerRadius = 2.5f;
                    EditorUtility.SetDirty(doorTf.gameObject);
                    Debug.Log($"[MainScene] {doorNames[i]}/Trigger → DoorMain (target={targetScenes[i]}) 추가");
                }
            }
        }

        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        Debug.Log("[MainScene] 저장 완료");
    }

    [MenuItem("EDEN/Fix: Reattach Scene1_A Scripts")]
    public static void ReattachScene1A()
    {
        EditorSceneManager.OpenScene("Assets/HW_09/Scenes/Scene1_A.unity", OpenSceneMode.Single);

        // Ocean → OceanMesh
        var ocean = GameObject.Find("Ocean");
        if (ocean != null && ocean.GetComponent<HW09.Gayoung.OceanMesh>() == null)
        {
            ocean.AddComponent<HW09.Gayoung.OceanMesh>();
            EditorUtility.SetDirty(ocean);
            Debug.Log("[Scene1_A] Ocean → OceanMesh 추가");
        }

        // WaterAudio → WaterAudio
        var waterAudio = GameObject.Find("WaterAudio");
        if (waterAudio != null && waterAudio.GetComponent<HW09.Gayoung.WaterAudio>() == null)
        {
            waterAudio.AddComponent<HW09.Gayoung.WaterAudio>();
            EditorUtility.SetDirty(waterAudio);
            Debug.Log("[Scene1_A] WaterAudio → WaterAudio 추가");
        }

        // Audio → Audios
        var audio = GameObject.Find("Audio");
        if (audio != null && audio.GetComponent<HW09.Gayoung.Audios>() == null)
        {
            audio.AddComponent<HW09.Gayoung.Audios>();
            EditorUtility.SetDirty(audio);
            Debug.Log("[Scene1_A] Audio → Audios 추가");
        }

        // SceneChangeObject → AutoSceneChange
        var sceneChange = GameObject.Find("SceneChangeObject");
        if (sceneChange != null && sceneChange.GetComponent<HW09.Gayoung.AutoSceneChange>() == null)
        {
            sceneChange.AddComponent<HW09.Gayoung.AutoSceneChange>();
            EditorUtility.SetDirty(sceneChange);
            Debug.Log("[Scene1_A] SceneChangeObject → AutoSceneChange 추가");
        }

        // boat_1 → BoatMove_gurinwager
        var boat = GameObject.Find("boat_1");
        if (boat != null && boat.GetComponent<HW09.Gayoung.BoatMove_gurinwager>() == null)
        {
            boat.AddComponent<HW09.Gayoung.BoatMove_gurinwager>();
            EditorUtility.SetDirty(boat);
            Debug.Log("[Scene1_A] boat_1 → BoatMove_gurinwager 추가");
        }

        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        Debug.Log("[Scene1_A] 저장 완료");
    }

    [MenuItem("EDEN/Fix: Reattach Scene3_A Scripts")]
    public static void ReattachScene3A()
    {
        EditorSceneManager.OpenScene("Assets/HW_09/Scenes/Scene3_A.unity", OpenSceneMode.Single);

        // Scene3AManager → SceneManager3A
        var mgr = GameObject.Find("Scene3AManager");
        if (mgr != null && mgr.GetComponent<HW09.heejo.SceneManager3A>() == null)
        {
            mgr.AddComponent<HW09.heejo.SceneManager3A>();
            EditorUtility.SetDirty(mgr);
            Debug.Log("[Scene3_A] Scene3AManager → SceneManager3A 추가");
        }

        // candle → CandleGrabTrigger3A
        var candle = GameObject.Find("candle");
        if (candle != null && candle.GetComponent<HW09.heejo.CandleGrabTrigger3A>() == null)
        {
            candle.AddComponent<HW09.heejo.CandleGrabTrigger3A>();
            EditorUtility.SetDirty(candle);
            Debug.Log("[Scene3_A] candle → CandleGrabTrigger3A 추가");
        }

        // candle (1) → CandleGrabTrigger3A
        var candle1 = GameObject.Find("candle (1)");
        if (candle1 != null && candle1.GetComponent<HW09.heejo.CandleGrabTrigger3A>() == null)
        {
            candle1.AddComponent<HW09.heejo.CandleGrabTrigger3A>();
            EditorUtility.SetDirty(candle1);
            Debug.Log("[Scene3_A] candle (1) → CandleGrabTrigger3A 추가");
        }

        // door → DoorTrigger3A
        var door = GameObject.Find("door");
        if (door != null && door.GetComponent<HW09.heejo.DoorTrigger3A>() == null)
        {
            door.AddComponent<HW09.heejo.DoorTrigger3A>();
            EditorUtility.SetDirty(door);
            Debug.Log("[Scene3_A] door → DoorTrigger3A 추가");
        }

        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        Debug.Log("[Scene3_A] 저장 완료");
    }

    // ─────────────────────────────────────────────────────────
    // Scene1_A 완전 복구: SceneChangeObject 생성 + WaterAudio 클립 + BGM
    // ─────────────────────────────────────────────────────────
    [MenuItem("EDEN/Fix: Full Fix Scene1_A")]
    public static void FullFixScene1A()
    {
        EditorSceneManager.OpenScene("Assets/HW_09/Scenes/Scene1_A.unity", OpenSceneMode.Single);

        // 1. WaterAudio 클립 연결
        var waterAudioGo = GameObject.Find("WaterAudio");
        if (waterAudioGo != null)
        {
            var wa = waterAudioGo.GetComponent<HW09.Gayoung.WaterAudio>();
            if (wa == null) wa = waterAudioGo.AddComponent<HW09.Gayoung.WaterAudio>();

            if (wa.clips == null || wa.clips.Length == 0)
            {
                var clipPaths = new[] {
                    "Assets/HW_09/Gayoung/SUIMONO - WATER SYSTEM 2/SOUNDS/water_splash01.wav",
                    "Assets/HW_09/Gayoung/SUIMONO - WATER SYSTEM 2/SOUNDS/water_splash02.wav",
                    "Assets/HW_09/Gayoung/SUIMONO - WATER SYSTEM 2/SOUNDS/water_splash03.wav",
                };
                var list = new System.Collections.Generic.List<AudioClip>();
                foreach (var p in clipPaths)
                {
                    var c = AssetDatabase.LoadAssetAtPath<AudioClip>(p);
                    if (c != null) list.Add(c);
                    else Debug.LogWarning($"[Scene1_A] 클립 없음: {p}");
                }
                wa.clips = list.ToArray();
                EditorUtility.SetDirty(waterAudioGo);
                Debug.Log($"[Scene1_A] WaterAudio 클립 {list.Count}개 연결");
            }
        }

        // 2. SceneChangeObject 생성 (없으면 새로 만듦) + AutoSceneChange
        var sceneChangeGo = GameObject.Find("SceneChangeObject");
        if (sceneChangeGo == null)
        {
            sceneChangeGo = new GameObject("SceneChangeObject");
            Debug.Log("[Scene1_A] SceneChangeObject 새로 생성");
        }
        var asc = sceneChangeGo.GetComponent<HW09.Gayoung.AutoSceneChange>();
        if (asc == null) asc = sceneChangeGo.AddComponent<HW09.Gayoung.AutoSceneChange>();
        // Scene1_B가 없으므로 일단 MainScene으로 복귀 (Scene1_B 완성 시 변경)
        if (asc.nextScene == "gurin_Scene01_B" || string.IsNullOrEmpty(asc.nextScene))
        {
            asc.nextScene = "MainScene";
            Debug.Log("[Scene1_A] AutoSceneChange.nextScene → MainScene (Scene1_B 완성 시 변경 필요)");
        }
        asc.delay = 90f; // 90초 후 자동 복귀
        EditorUtility.SetDirty(sceneChangeGo);

        // 3. BGAudio (배경음악) — scenea.mp3
        var bgAudioGo = GameObject.Find("BGAudio");
        if (bgAudioGo == null) bgAudioGo = new GameObject("BGAudio");
        var audios = bgAudioGo.GetComponent<HW09.Gayoung.Audios>();
        if (audios == null) audios = bgAudioGo.AddComponent<HW09.Gayoung.Audios>();
        if (audios.audioClip == null)
        {
            string[] guids = AssetDatabase.FindAssets("scenea t:AudioClip", new[] { "Assets/HW_09/Gayoung" });
            if (guids.Length > 0)
            {
                audios.audioClip = AssetDatabase.LoadAssetAtPath<AudioClip>(AssetDatabase.GUIDToAssetPath(guids[0]));
                EditorUtility.SetDirty(bgAudioGo);
                Debug.Log("[Scene1_A] BGAudio scenea.mp3 연결");
            }
            else Debug.LogWarning("[Scene1_A] scenea.mp3 를 찾을 수 없음");
        }

        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        Debug.Log("[Scene1_A] Full Fix 완료 — 저장됨");
    }

    // ─────────────────────────────────────────────────────────
    // Scene3_A 참조 연결: SceneManager3A ← door/candle/light/clips
    // ─────────────────────────────────────────────────────────
    [MenuItem("EDEN/Fix: Wire Scene3_A References")]
    public static void WireScene3AReferences()
    {
        EditorSceneManager.OpenScene("Assets/HW_09/Scenes/Scene3_A.unity", OpenSceneMode.Single);

        var mgrGo = GameObject.Find("Scene3AManager");
        if (mgrGo == null) { Debug.LogError("[Scene3_A] Scene3AManager 없음"); return; }

        var mgr = mgrGo.GetComponent<HW09.heejo.SceneManager3A>();
        if (mgr == null) { Debug.LogError("[Scene3_A] SceneManager3A 컴포넌트 없음"); return; }

        // door / candle / candle(1) 연결
        var doorGo   = GameObject.Find("door");
        var candleGo = GameObject.Find("candle");
        var candle1Go = GameObject.Find("candle (1)");

        if (mgr.door   == null && doorGo   != null) { mgr.door   = doorGo;   Debug.Log("[Scene3_A] door 연결"); }
        if (mgr.candle == null && candleGo != null) { mgr.candle = candleGo; Debug.Log("[Scene3_A] candle 연결"); }
        if (mgr.candle1 == null && candle1Go != null) { mgr.candle1 = candle1Go; Debug.Log("[Scene3_A] candle(1) 연결"); }

        // candleLight: candle/Point 에 있는 Light
        if (mgr.candleLight == null && candleGo != null)
        {
            var lt = candleGo.GetComponentInChildren<Light>();
            if (lt != null) { mgr.candleLight = lt; Debug.Log("[Scene3_A] candleLight 연결"); }
        }
        if (mgr.candleLight1 == null && candle1Go != null)
        {
            var lt = candle1Go.GetComponentInChildren<Light>();
            if (lt != null) { mgr.candleLight1 = lt; Debug.Log("[Scene3_A] candleLight1 연결"); }
        }

        // AudioSource: Scene3AManager 에 이미 있는 AudioSource 연결
        if (mgr.audioSource == null)
        {
            var src = mgrGo.GetComponent<AudioSource>();
            if (src == null) src = mgrGo.AddComponent<AudioSource>();
            mgr.audioSource = src;
            Debug.Log("[Scene3_A] AudioSource 연결");
        }

        // 오디오 클립 연결 (heejo/Audio 폴더에서 탐색)
        string[] audioGuids = AssetDatabase.FindAssets("t:AudioClip", new[] { "Assets/HW_09/heejo/Audio" });
        var audioClips = new System.Collections.Generic.List<AudioClip>();
        foreach (var g in audioGuids)
            audioClips.Add(AssetDatabase.LoadAssetAtPath<AudioClip>(AssetDatabase.GUIDToAssetPath(g)));

        if (audioClips.Count > 0 && mgr.clip1 == null) { mgr.clip1 = audioClips[0]; Debug.Log($"[Scene3_A] clip1 = {audioClips[0].name}"); }
        if (audioClips.Count > 1 && mgr.clip2 == null) { mgr.clip2 = audioClips[1]; Debug.Log($"[Scene3_A] clip2 = {audioClips[1].name}"); }
        // clip3~5는 현재 오디오 파일 없음 — 혜조가 추가 시 직접 연결

        // DoorTrigger3A.manager 연결
        if (doorGo != null)
        {
            var dt = doorGo.GetComponent<HW09.heejo.DoorTrigger3A>();
            if (dt != null && dt.manager == null) { dt.manager = mgr; EditorUtility.SetDirty(doorGo); Debug.Log("[Scene3_A] DoorTrigger3A.manager 연결"); }
        }

        // CandleGrabTrigger3A.manager 연결
        foreach (var go in new[] { candleGo, candle1Go })
        {
            if (go == null) continue;
            var ct = go.GetComponent<HW09.heejo.CandleGrabTrigger3A>();
            if (ct != null && ct.manager == null) { ct.manager = mgr; EditorUtility.SetDirty(go); Debug.Log($"[Scene3_A] {go.name} CandleGrabTrigger3A.manager 연결"); }
        }

        EditorUtility.SetDirty(mgrGo);
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        Debug.Log("[Scene3_A] Wire References 완료 — 저장됨");
    }

    // ─────────────────────────────────────────────────────────
    // Scene4_B: ExitDoor에 ExitDoorTrigger4B 추가 + 저장
    // ─────────────────────────────────────────────────────────
    [MenuItem("EDEN/Fix: Wire Scene4_B ExitDoor")]
    public static void WireScene4BExitDoor()
    {
        EditorSceneManager.OpenScene("Assets/HW_09/Scenes/Scene4_B.unity", OpenSceneMode.Single);

        var exitDoor = GameObject.Find("ExitDoor");
        if (exitDoor == null)
        {
            Debug.LogError("[Scene4_B] ExitDoor 오브젝트를 찾을 수 없습니다.");
            return;
        }

        // ExitDoorTrigger4B 추가 (없으면)
        var trigger = exitDoor.GetComponent<HW09.Heejin.ExitDoorTrigger4B>();
        if (trigger == null)
        {
            trigger = exitDoor.AddComponent<HW09.Heejin.ExitDoorTrigger4B>();
            trigger.targetScene = "MainScene";
            trigger.playerTag   = "Player";
            EditorUtility.SetDirty(exitDoor);
            Debug.Log("[Scene4_B] ExitDoor → ExitDoorTrigger4B 추가 (targetScene=MainScene)");
        }
        else
        {
            Debug.Log("[Scene4_B] ExitDoorTrigger4B 이미 존재");
        }

        // BoxCollider isTrigger 확인
        var col = exitDoor.GetComponent<BoxCollider>();
        if (col != null && !col.isTrigger)
        {
            col.isTrigger = true;
            EditorUtility.SetDirty(exitDoor);
            Debug.Log("[Scene4_B] ExitDoor BoxCollider.isTrigger = true");
        }

        // SceneIntroManager4B 확인 (없으면 추가)
        var introMgr = Object.FindObjectOfType<HW09.Heejin.SceneIntroManager4B>();
        if (introMgr == null)
        {
            var mgrGo = GameObject.Find("SceneIntroManager") ?? new GameObject("SceneIntroManager");
            mgrGo.AddComponent<HW09.Heejin.SceneIntroManager4B>();
            EditorUtility.SetDirty(mgrGo);
            Debug.Log("[Scene4_B] SceneIntroManager4B 추가");
        }

        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        Debug.Log("[Scene4_B] Wire ExitDoor 완료 — 저장됨");
    }

    [MenuItem("EDEN/Fix: Run All Fixes")]
    public static void RunAllFixes()
    {
        RemoveAllMissingScripts();
        ReattachMainScene();
        ReattachScene1A();
        ReattachScene3A();
        FullFixScene1A();
        WireScene3AReferences();
        WireScene4BExitDoor();
        Debug.Log("=== 전체 Missing Script 수정 완료 ===");
    }
}
