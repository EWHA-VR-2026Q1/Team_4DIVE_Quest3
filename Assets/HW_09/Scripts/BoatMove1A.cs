using UnityEngine;

public class BoatMove_gurinwager : MonoBehaviour
{
    [Header("파도 설정 (셰이더랑 동일하게)")]
    public float waveHeight = 0.5f;
    public float waveSpeed = 1.0f;
    public float waveFreq = 1.0f;

    [Header("흔들림 설정")]
    public float rotationSpeed = 2f;
    public float maxTiltAngle = 15f;
    public float sensitivity = 30f;

    private float initialY;
    private Quaternion initialRotation;

    void Start()
    {
        initialY = transform.position.y;
        initialRotation = transform.rotation;
    }

    float GetWaveHeight(float x, float z)
    {
        float wave1 = Mathf.Sin(x * waveFreq + Time.time * waveSpeed) * waveHeight;
        float wave2 = Mathf.Sin(z * waveFreq * 0.8f + Time.time * waveSpeed * 1.2f) * waveHeight * 0.6f;
        float wave3 = Mathf.Sin((x + z) * waveFreq * 0.5f + Time.time * waveSpeed * 0.8f) * waveHeight * 0.4f;
        return wave1 + wave2 + wave3;
    }

    void FixedUpdate()
    {
        Vector3 pos = transform.position;
        float range = 2f;

        // 파도 높이 샘플링
        float front = GetWaveHeight(pos.x + transform.forward.x * range, pos.z + transform.forward.z * range);
        float back  = GetWaveHeight(pos.x - transform.forward.x * range, pos.z - transform.forward.z * range);
        float right = GetWaveHeight(pos.x + transform.right.x  * range, pos.z + transform.right.z  * range);
        float left  = GetWaveHeight(pos.x - transform.right.x  * range, pos.z - transform.right.z  * range);
        float center = GetWaveHeight(pos.x, pos.z);

        // 배 Y위치를 파도에 맞게
        Vector3 newPos = pos;
        newPos.y = Mathf.Lerp(pos.y, initialY + center, Time.fixedDeltaTime * rotationSpeed);
        transform.position = newPos;

        // pitch/roll 계산
        float pitch = Mathf.Clamp((back - front) * sensitivity, -maxTiltAngle, maxTiltAngle);
        float roll  = Mathf.Clamp((right - left)  * sensitivity, -maxTiltAngle, maxTiltAngle);

        Quaternion tilt = Quaternion.Euler(pitch, 0f, roll);
        Quaternion targetRot = initialRotation * tilt;
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.fixedDeltaTime * rotationSpeed);
    }
}