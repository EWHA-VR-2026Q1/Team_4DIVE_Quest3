using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.XR; // XR 장치 직접 접근을 위해 필수

[RequireComponent(typeof(CharacterController))]
public class EX_InputSystem_XR_DirectAccess : MonoBehaviour
{
    CharacterController Character;

    [Header("References")]
    public Transform CenterEye;
    public Transform LeftController;
    public Transform RightController;

    [Header("Line Renderers")]
    public LineRenderer LeftControllerRay;
    public LineRenderer Arc;
    public GameObject Marker;

    [Header("Move Settings")]
    public float walkSpeed = 1.5f;
    public float runSpeed = 3.5f;
    public float gravity = -9.81f;
    public float jumpHeight = 0.5f;

    [Header("Climb")]
    public float climbSpeed = 1.2f;
    bool isClimbing = false;

    [Header("Turn")]
    public float snapAngle = 45f;
    bool turnReady = true;

    [Header("Teleport")]
    public LayerMask TeleportLayer;
    public int resolution = 30;
    public float teleportVelocity = 8f;
    public float timestep = 0.08f;
    bool hasValidTarget = false;
    bool teleportActive = false;

    [Header("Grab")]
    public float grabRadius = 0.2f;
    public LayerMask GrabLayer;

    [Header("Ray Interface")]
    public float rayDistance = 10f;
    IRayInteractable CurrentTarget;

    // 내부 물리 상태
    Vector3 Velocity;
    Vector3[] LineFragments;
    Vector3 TeleportTarget;

    // 그랩 관련
    Rigidbody GrabbedRB;
    Collider PlayerCollider;
    Collider ObjectCollider;
    Vector3 PosOffset;
    Quaternion RotOffset;

    enum ClimbType { None, Ladder, Cliff }
    ClimbType climbType = ClimbType.None;

    // 입력 데이터 캐싱 (매 프레임 갱신)
    Vector2 moveInput;
    Vector2 turnInput;
    bool sprintInput;
    bool jumpInput;
    bool lTriggerInput;
    bool lTriggerDown;
    bool rTriggerInput;
    bool lGripInput;

    void Start()
    {
        Character = GetComponent<CharacterController>();
        PlayerCollider = GetComponent<Collider>();
        LineFragments = new Vector3[resolution];

        LeftControllerRay.enabled = false;
        Arc.enabled = false;
    }

    void Update()
    {
        // 1. 하드웨어 입력 수집
        GatherInput();

        // 2. 물리 및 인터랙션 로직
        Move();
        Turn();
        RayInteraction();
        TeleportControl();
        GrabInteraction();

        // 3. 최종 이동 적용
        Character.Move(Velocity * Time.deltaTime);
    }

    void GatherInput()
    {
        // 왼손 컨트롤러 (이동, 스프린트, 트리거, 그립)
        var leftHand = XRController.leftHand;
        if (leftHand != null)
        {
            moveInput = leftHand.GetChildControl<Vector2Control>("thumbstick").ReadValue();
            sprintInput = leftHand.GetChildControl<ButtonControl>("thumbstickClicked").isPressed;
            lTriggerInput = leftHand.GetChildControl<ButtonControl>("triggerButton").isPressed;
            lTriggerDown = leftHand.GetChildControl<ButtonControl>("triggerButton").wasPressedThisFrame;
            lGripInput = leftHand.GetChildControl<ButtonControl>("gripButton").isPressed;
        }

        // 오른손 컨트롤러 (회전, 점프, 트리거)
        var rightHand = XRController.rightHand;
        if (rightHand != null)
        {
            turnInput = rightHand.GetChildControl<Vector2Control>("thumbstick").ReadValue();
            jumpInput = rightHand.GetChildControl<ButtonControl>("primaryButton").wasPressedThisFrame; // A 버튼
            rTriggerInput = rightHand.GetChildControl<ButtonControl>("triggerButton").isPressed;
        }
    }

    // =========================
    // MOVE & CLIMB
    // =========================
    void Move()
    {
        if (isClimbing) ClimbMove();
        else WalkMove();
    }

    void WalkMove()
    {
        Vector3 forward = Vector3.ProjectOnPlane(CenterEye.forward, Vector3.up).normalized;
        Vector3 right = Vector3.ProjectOnPlane(CenterEye.right, Vector3.up).normalized;
        Vector3 dir = forward * moveInput.y + right * moveInput.x;

        float speed = sprintInput ? runSpeed : walkSpeed;
        Velocity.x = dir.x * speed;
        Velocity.z = dir.z * speed;

        if (Character.isGrounded)
        {
            if (Velocity.y < 0) Velocity.y = -2f;
            if (jumpInput) Velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
        Velocity.y += gravity * Time.deltaTime;
    }

    void ClimbMove()
    {
        Velocity = Vector3.zero;
        if (climbType == ClimbType.Ladder) Velocity.y = moveInput.y * climbSpeed;
        else if (climbType == ClimbType.Cliff)
        {
            Velocity = transform.right * moveInput.x * climbSpeed + Vector3.up * moveInput.y * climbSpeed;
            Velocity += -transform.forward * 0.01f;
        }

        if (jumpInput)
        {
            isClimbing = false;
            Vector3 jumpDir = (-transform.forward + Vector3.up).normalized;
            Velocity = jumpDir * Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    }

    // =========================
    // TURN (Snap Turn)
    // =========================
    void Turn()
    {
        if (turnReady)
        {
            if (turnInput.x > 0.7f)
            {
                transform.Rotate(Vector3.up * snapAngle);
                turnReady = false;
            }
            else if (turnInput.x < -0.7f)
            {
                transform.Rotate(Vector3.up * -snapAngle);
                turnReady = false;
            }
        }

        if (Mathf.Abs(turnInput.x) < 0.2f) turnReady = true;
    }

    // =========================
    // RAY (L-Index Trigger)
    // =========================
    void RayInteraction()
    {
        if (!lTriggerInput)
        {
            LeftControllerRay.enabled = false;
            if (CurrentTarget != null) { CurrentTarget.OnRayExit(); CurrentTarget = null; }
            return;
        }

        LeftControllerRay.enabled = true;
        Vector3 start = LeftController.position + LeftController.forward * 0.04f;
        LeftControllerRay.SetPosition(0, start);

        if (Physics.Raycast(start, LeftController.forward, out RaycastHit hit, rayDistance))
        {
            LeftControllerRay.SetPosition(1, hit.point);
            var interactable = hit.collider.GetComponent<IRayInteractable>();

            if (interactable != null)
            {
                if (CurrentTarget != interactable)
                {
                    CurrentTarget?.OnRayExit();
                    CurrentTarget = interactable;
                    CurrentTarget.OnRayEnter();
                }
                CurrentTarget.OnRayStay();
                if (lTriggerDown) CurrentTarget.OnRayClick();
            }
            else if (CurrentTarget != null) { CurrentTarget.OnRayExit(); CurrentTarget = null; }
        }
        else
        {
            LeftControllerRay.SetPosition(1, start + LeftController.forward * rayDistance);
            if (CurrentTarget != null) { CurrentTarget.OnRayExit(); CurrentTarget = null; }
        }
    }

    // =========================
    // TELEPORT (R-Index Trigger)
    // =========================
    void TeleportControl()
    {
        if (rTriggerInput)
        {
            teleportActive = true;
            Arc.enabled = true;
            TeleportPreview();
        }
        else
        {
            if (teleportActive && hasValidTarget)
            {
                Character.enabled = false;
                transform.position = TeleportTarget + Vector3.up * 0.5f;
                Character.enabled = true;
            }
            teleportActive = false;
            Arc.enabled = false;
            Marker.SetActive(false);
        }
    }

    void TeleportPreview()
    {
        hasValidTarget = false;
        Vector3 start = RightController.position + RightController.forward * 0.04f;
        Vector3 vel = RightController.forward * teleportVelocity;

        for (int i = 0; i < resolution; i++)
        {
            float t = i * timestep;
            Vector3 p = start + vel * t + 0.5f * Physics.gravity * t * t;
            LineFragments[i] = p;

            if (i > 0)
            {
                if (Physics.Linecast(LineFragments[i - 1], p, out RaycastHit hit, TeleportLayer))
                {
                    hasValidTarget = true;
                    TeleportTarget = hit.point;
                    Marker.SetActive(true);
                    Marker.transform.position = TeleportTarget + Vector3.up * 0.02f;
                    Arc.positionCount = i + 1;
                    Arc.SetPositions(LineFragments);
                    return;
                }
            }
        }
        Arc.positionCount = resolution;
        Arc.SetPositions(LineFragments);
    }

    // =========================
    // GRAB (L-Hand Grip)
    // =========================
    void GrabInteraction()
    {
        if (lGripInput)
        {
            if (GrabbedRB == null) TryGrab();
            else UpdateGrab();
        }
        else if (GrabbedRB != null) Release();
    }

    void TryGrab()
    {
        Collider[] hits = Physics.OverlapSphere(LeftController.position, grabRadius, GrabLayer);
        Rigidbody closest = null;
        float minDist = float.MaxValue;

        foreach (var c in hits)
        {
            Rigidbody rb = c.attachedRigidbody;
            if (rb == null) continue;
            float d = Vector3.Distance(LeftController.position, rb.position);
            if (d < minDist) { minDist = d; closest = rb; }
        }

        if (closest == null) return;
        GrabbedRB = closest;
        ObjectCollider = GrabbedRB.GetComponent<Collider>();
        GrabbedRB.isKinematic = true;
        Physics.IgnoreCollision(ObjectCollider, PlayerCollider, true);
        PosOffset = Quaternion.Inverse(LeftController.rotation) * (GrabbedRB.position - LeftController.position);
        RotOffset = Quaternion.Inverse(LeftController.rotation) * GrabbedRB.rotation;
    }

    void UpdateGrab()
    {
        GrabbedRB.MovePosition(LeftController.position + LeftController.rotation * PosOffset);
        GrabbedRB.MoveRotation(LeftController.rotation * RotOffset);
    }

    void Release()
    {
        if (GrabbedRB == null) return;

        GrabbedRB.isKinematic = false;

        // 에러 해결: GetChildControl을 사용하여 하드웨어의 속도 값을 직접 읽어옵니다.
        var leftHand = XRController.leftHand;
        if (leftHand != null)
        {
            // 'deviceVelocity' 대신 'devicePosition/delta'나 'velocity' 컨트롤 이름을 확인합니다.
            // 대부분의 XR 장치에서 "deviceVelocity"라는 이름의 Vector3Control을 제공합니다.
            var velControl = leftHand.GetChildControl<Vector3Control>("deviceVelocity");
            var angVelControl = leftHand.GetChildControl<Vector3Control>("deviceAngularVelocity");

            if (velControl != null)
                GrabbedRB.velocity = velControl.ReadValue();

            if (angVelControl != null)
                GrabbedRB.angularVelocity = angVelControl.ReadValue();
        }

        Physics.IgnoreCollision(ObjectCollider, PlayerCollider, false);

        GrabbedRB = null;
        ObjectCollider = null;
    }

    // =========================
    // TRIGGER
    // =========================
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("ClimbableLadder")) { climbType = ClimbType.Ladder; isClimbing = true; Velocity = Vector3.zero; }
        if (other.CompareTag("ClimbableCliff")) { climbType = ClimbType.Cliff; isClimbing = true; Velocity = Vector3.zero; }
    }
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("ClimbableLadder") || other.CompareTag("ClimbableCliff")) { isClimbing = false; climbType = ClimbType.None; }
    }
}