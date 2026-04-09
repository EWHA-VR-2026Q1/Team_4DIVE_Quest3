using UnityEngine;

namespace HW09
{
    /// <summary>
    /// 위아래 바운스 + Y축 회전 — 유도 화살표에 부착
    /// </summary>
    public class GuideArrow2A : MonoBehaviour
    {
        public float bounceHeight = 0.18f;
        public float bounceSpeed  = 2.2f;
        public float rotateSpeed  = 60f;   // 0이면 회전 안 함

        private Vector3 _origin;

        void Start()  => _origin = transform.localPosition;

        void Update()
        {
            float y = Mathf.Sin(Time.time * bounceSpeed) * bounceHeight;
            transform.localPosition = _origin + Vector3.up * y;
            transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime, Space.World);
        }
    }
}
