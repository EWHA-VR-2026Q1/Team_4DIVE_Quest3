using System.Collections.Generic;
using UnityEngine;

public class EX_OVR_Hand_Least : MonoBehaviour
{
    [Header("Hand Settings")]
    public OVRHand hand;
    private OVRSkeleton skeleton;

    [Header("Interaction Settings")]
    public LayerMask interactableLayer;
    public float pokeRadius = 0.02f;

    // 상태 관리용
    private IHandInteractable_Least currentTarget;
    private Transform fixedIndexTip; // 검지 끝을 저장할 변수
    private bool isGrabbingState = false;
    bool wasPinchingLastFrame = false;

    private void Start()
    {
        if (hand != null) skeleton = hand.GetComponent<OVRSkeleton>();
    }

    void Update()
    {
        if (hand == null || !hand.IsTracked || !skeleton.IsDataValid)
        {
            ClearCurrentTarget();
            return;
        }

        HandleInteraction();
    }

    private void HandleInteraction()
    {
        // 1. 진짜 검지 끝(IndexTip) 찾기 (최초 1회 혹은 누락 시)
        if (fixedIndexTip == null)
        {
            foreach (var bone in skeleton.Bones)
            {
                // 이름에 "Index"와 "Tip"이 포함된 뼈를 찾습니다. 
                // ID가 꼬여도 이름은 모델링 데이터에 따라 'IndexTip'으로 되어 있을 확률이 매우 높습니다.
                string bName = bone.Transform.name.ToLower();
                if (bName.Contains("index") && bName.Contains("tip"))
                {
                    fixedIndexTip = bone.Transform;
                    break;
                }
            }

            // 만약 이름으로도 못 찾는다면 이전에 확인한 10번 인덱스를 강제 할당합니다.
            if (fixedIndexTip == null && skeleton.Bones.Count > 10)
            {
                fixedIndexTip = skeleton.Bones[10].Transform;
            }
        }

        if (fixedIndexTip == null) return;

        // 2. 물리 감지 (OverlapSphere)
        Debug.DrawRay(fixedIndexTip.position, Vector3.up * 0.05f, Color.red);
        Collider[] hits = Physics.OverlapSphere(fixedIndexTip.position, pokeRadius, interactableLayer);
        IHandInteractable_Least newTarget = (hits.Length > 0) ? hits[0].GetComponent<IHandInteractable_Least>() : null;

        // 3. Enter / Exit 처리
        //if (newTarget != currentTarget)
        //{
        //    if (newTarget == null)
        //    {
        //        currentTarget?.OnHandExit();
        //        Debug.Log("<color=gray>[Target 분리]</color> currentTarget = NULL");
        //    }
        //    else
        //    {
        //        currentTarget?.OnHandExit(); // 기존 타겟이 있었다면 Exit
        //        currentTarget = newTarget;
        //        currentTarget?.OnHandEnter();

        //        var objName = (currentTarget as MonoBehaviour).name;
        //        Debug.Log($"<color=green>[Target 발견]</color> 현재 타겟: {objName}");
        //    }

        //    currentTarget = newTarget;
        //}
        if (newTarget != currentTarget)
        {
            currentTarget?.OnHandExit();
            currentTarget = newTarget;

            if (currentTarget != null)
            {
                currentTarget.OnHandEnter();
                // 여기서 색상을 바꾸면 무한 루프 위험 없이 한 번만 실행됩니다.
                Debug.Log("<color=green>새로운 물체 터치 시작!</color>");
            }
        }

        // 4. 타겟이 있을 때 제스처 실행
        if (currentTarget != null)
        {
            currentTarget.OnHandStay();

            bool grabbing = IsGrabbing();
            if (grabbing && !isGrabbingState)
            {
                isGrabbingState = true;

                //currentTarget.OnHandGrab(this.transform);
                currentTarget.OnHandGrab(fixedIndexTip);

                // 콘솔 출력: 어떤 물체를 잡았는지 표시
                var objName = (currentTarget as MonoBehaviour).name;
                Debug.Log($"<color=red>[Grab 시작]</color> 대상: {objName}");
            }
            else if (!grabbing && isGrabbingState)
            {
                isGrabbingState = false;
                currentTarget.OnHandRelease();
                Debug.Log("<color=red>[Grab 해제]</color>");
            }

            // [2] Pinch (검지-엄지 집기) 체크
            // 시스템에서 제공하는 Pinch 판정은 강도가 1.0일 때 true가 됩니다.
            bool isPinching = hand.GetFingerIsPinching(OVRHand.HandFinger.Index);
            float pinchStrength = hand.GetFingerPinchStrength(OVRHand.HandFinger.Index);

            if (isPinching)
            {
                currentTarget.OnHandPinch();
                // 강도와 함께 출력 (F2는 소수점 2자리)
                Debug.Log($"<color=yellow>[Pinch 검출]</color> 강도: {pinchStrength:F2}");
            }

            // [3] Poke (찌르기) - 핀치나 그랩이 아닐 때만 작동
            else if (!isGrabbingState)
            {
                currentTarget.OnHandPoke();
                // Poke는 매 프레임 찍히면 너무 많으므로 필요 시에만 활성화하세요.
            }

            //bool isPinching = hand.GetFingerIsPinching(OVRHand.HandFinger.Index);

            //if (isPinching)
            //{
            //    // 이미 핀치 중이라면 다시 이벤트를 발생시키지 않도록 로직 추가 가능
            //    if (!wasPinchingLastFrame)
            //    {
            //        currentTarget?.OnHandPinch();
            //        wasPinchingLastFrame = true;
            //    }
            //}
            //else
            //{
            //    wasPinchingLastFrame = false;
            //}
        }
    }

    private void ClearCurrentTarget()
    {
        if (currentTarget != null)
        {
            currentTarget.OnHandExit();
            currentTarget = null;
        }
        isGrabbingState = false;
    }

    private bool IsGrabbing()
    {
        float i = hand.GetFingerPinchStrength(OVRHand.HandFinger.Index);
        float m = hand.GetFingerPinchStrength(OVRHand.HandFinger.Middle);
        float r = hand.GetFingerPinchStrength(OVRHand.HandFinger.Ring);
        float score = i + m + r;

        if (!isGrabbingState && score > 0.8f) return true;
        if (isGrabbingState && score < 0.3f) return false;
        return isGrabbingState;
    }

    private void OnDrawGizmos()
    {
        // 씬 뷰에서 실제 감지 위치 확인
        if (fixedIndexTip != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(fixedIndexTip.position, pokeRadius);
        }
    }
}

//---------------------------------------------------------------------------------------------------------------------------------------

//using System.Collections.Generic;
//using UnityEngine;

//public class EX_OVR_Hand_Least : MonoBehaviour
//{
//    [Header("Hand Settings (Use OVRHandPrefab")]
//    // Inspector에서 OVRHand가 부착된 오브젝트(좌/우)를 연결합니다.
//    public OVRHand hand;
//    //public OVRSkeleton skeleton;
//    OVRSkeleton skeleton;

//    [Header("Poke Settings")]
//    public LayerMask HandLayer;
//    public float pokeRadius = 0.015f; // 약 1.5cm 감지 범위

//    bool isGrabbingState = false;

//    private void Start()
//    {
//        skeleton = hand.GetComponent<OVRSkeleton>();
//    }
//    void Update()
//    {
//        // 1. hand 변수가 비어있는지 확인 (방어 코드)
//        if (hand == null || !hand.IsTracked || !skeleton.IsDataValid) return;

//        // 1. Poke 체크 (최우선순위)
//        HandlePoke();

//        if (IsGrabbing())
//        {
//            Debug.Log($"{hand.GetHand()}: Hand Grab / Fist");
//        }
//        else if (hand.GetFingerIsPinching(OVRHand.HandFinger.Index))
//        {
//            float pinchStrength = hand.GetFingerPinchStrength(OVRHand.HandFinger.Index);
//            Debug.Log($"{hand.GetHand()}: Pinch (Strength: {pinchStrength:F2})");
//        }
//    }

//    // Grab 상태를 판별하는 보조 함수
//    private bool IsGrabbing()
//    {
//        bool isIndexPinching = hand.GetFingerIsPinching(OVRHand.HandFinger.Index);
//        float i = hand.GetFingerPinchStrength(OVRHand.HandFinger.Index);
//        float m = hand.GetFingerPinchStrength(OVRHand.HandFinger.Middle);
//        float r = hand.GetFingerPinchStrength(OVRHand.HandFinger.Ring);
//        //float p = hand.GetFingerPinchStrength(OVRHand.HandFinger.Pinky);
//        //print($"i = {i} m={m} r={r} sum={m + r}");
//        float grabScore = (i + m + r);

//        if (!isGrabbingState && grabScore > 0.8f) // 꽉 쥐어야 잡기 시작
//        {
//            if (i > 0f && i < 0.8f && m > 0f && r > 0f)
//            {
//                isGrabbingState = true;
//            }
//        }
//        else if (isGrabbingState && grabScore < 0.3f) // 0.3 미만으로 확 펴야 놓기
//        {
//            isGrabbingState = false;
//        }
//        return isGrabbingState;
//    }

//    private void HandlePoke()
//    {
//        // 뼈 목록에서 검지 끝(Index Tip)을 찾습니다.
//        IList<OVRBone> bones = skeleton.Bones;
//        foreach (var bone in bones)
//        {
//            if (bone.Id == OVRSkeleton.BoneId.Hand_IndexTip)
//            {
//                Debug.DrawRay(bone.Transform.position, Vector3.up * 0.1f, Color.red);

//                // 검지 끝 위치(bone.Transform.position) 기준 주변 탐색
//                Collider[] hitColliders = Physics.OverlapSphere(bone.Transform.position, pokeRadius, HandLayer);

//                foreach (var hit in hitColliders)
//                {
//                    // 검지를 펴고 있을 때만 Poke로 인정 (Pinch 중이면 제외)
//                    if (!hand.GetFingerIsPinching(OVRHand.HandFinger.Index))
//                    {
//                        Debug.Log($"{hand.GetHand()} Poked: {hit.name}");
//                        // 여기에 대상 물체의 'OnPoke' 함수를 호출하는 로직을 넣으면 됩니다.
//                    }
//                }
//                break;
//            }
//        }
//    }
//}