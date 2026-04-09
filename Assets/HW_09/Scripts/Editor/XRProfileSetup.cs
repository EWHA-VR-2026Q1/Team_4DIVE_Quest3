// XRProfileSetup.cs — 에디터 전용 유틸리티
// 상단 메뉴: Tools > Setup XR Profiles (Quest 3S) 실행
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEditor.XR.OpenXR.Features;

namespace HW09.Editor
{
    public static class XRProfileSetup
    {
        [MenuItem("Tools/Setup XR Profiles (Quest 3S)")]
        public static void SetupProfiles()
        {
            var group = BuildTargetGroup.Android;

            // 활성화할 피처 ID 목록
            string[] featureIds = new[]
            {
                "com.unity.openxr.feature.input.metaquestplus",  // Meta Quest Touch Plus (Quest 3S!)
                "com.unity.openxr.feature.input.oculustouch",    // Oculus Touch Controller
                "com.meta.openxr.feature.metaxr",                // Meta XR Feature (필수)
                "com.unity.openxr.feature.metaquest",            // Meta Quest Support
            };

            int enabledCount = 0;

            foreach (var id in featureIds)
            {
                var feature = FeatureHelpers.GetFeatureWithIdForBuildTarget(group, id);
                if (feature == null)
                {
                    Debug.LogWarning($"[XRSetup] 피처 없음 (정상일 수 있음): {id}");
                    continue;
                }

                if (!feature.enabled)
                {
                    feature.enabled = true;
                    enabledCount++;
                    Debug.Log($"[XRSetup] ✅ 활성화: {feature.GetType().Name}  ({id})");
                }
                else
                {
                    Debug.Log($"[XRSetup] (이미 ON) {feature.GetType().Name}");
                }

                EditorUtility.SetDirty(feature);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            string msg = enabledCount > 0
                ? $"✅ {enabledCount}개 피처 활성화!\n\nAPK 빌드: File > Build Settings > Build"
                : "이미 모두 활성화 상태입니다.\nConsole 로그를 확인하세요.";

            EditorUtility.DisplayDialog("Quest 3S XR 설정", msg, "확인");
            Debug.Log($"[XRSetup] 완료! {enabledCount}개 활성화됨.");
        }
    }
}
#endif
