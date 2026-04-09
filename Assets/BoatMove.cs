using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoatMove : MonoBehaviour
{
    [Header("흔들림 설정")]
    public float rotationSpeed = 2f;
    public float maxTiltAngle = 15f;
    public float sensitivity = 2f;
    public float waterHeight = 18.5f; // 수면 Y 높이 직접 지정

    private Component module;
    private System.Reflection.MethodInfo heightMethod;
    private Quaternion initialRotation;

    void Start()
    {
        initialRotation = transform.rotation; // 원래 rotation 저장
        
        GameObject obj = GameObject.Find("SUIMONO_Module");
        if (obj == null) { Debug.LogError("SUIMONO_Module 못 찾음!"); return; }
        module = obj.GetComponent("SuimonoModule");
        heightMethod = module.GetType().GetMethod("SuimonoGetHeight");
    }

    void FixedUpdate()
    {
        if (module == null || heightMethod == null) return;

        float range = 2f;
        Vector3 pos = transform.position;

        float front = (float)heightMethod.Invoke(module, new object[] { pos + transform.forward * range, "height" });
        float back  = (float)heightMethod.Invoke(module, new object[] { pos - transform.forward * range, "height" });
        float right = (float)heightMethod.Invoke(module, new object[] { pos + transform.right   * range, "height" });
        float left  = (float)heightMethod.Invoke(module, new object[] { pos - transform.right   * range, "height" });

        float pitch = Mathf.Clamp((back - front) * sensitivity, -maxTiltAngle, maxTiltAngle);
        float roll  = Mathf.Clamp((right - left)  * sensitivity, -maxTiltAngle, maxTiltAngle);

        // 원래 rotation에서 pitch/roll만 추가
        Quaternion tilt = Quaternion.Euler(pitch, 0f, roll);
        Quaternion targetRot = initialRotation * tilt;
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.fixedDeltaTime * rotationSpeed);

        // 수면 높이에 배 위치 고정
        Vector3 p = transform.position;
        p.y = Mathf.Lerp(p.y, waterHeight, Time.fixedDeltaTime * rotationSpeed);
        transform.position = p;
    }
}