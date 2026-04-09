using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class EX_OVRInput_Combined_V2 : MonoBehaviour
{
    CharacterController Character;

    [Header("References")]
    public Transform CenterEye;
    public Transform LeftController;
    public Transform RightController;

    [Header("Line")]
    public LineRenderer LeftControllerRay;
    public LineRenderer Arc;
    public GameObject Marker;

    [Header("Move")]
    public float walkSpeed = 1.5f;
    public float runSpeed = 3.5f;
    public float gravity = -9.81f;

    [Header("Jump")]
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
    IRayInteractable CurrentTarget; // Declare a Variable for Ray for Interaction

    // ===== 내부 상태 =====
    Vector3 Velocity;     
    

    Vector3[] LineFragments;
    Vector3 TeleportTarget;

    Rigidbody GrabbedRB;
    Collider PlayerCollider;
    Collider ObjectCollider;

    Vector3 PosOffset;
    Quaternion RotOffset;

    enum ClimbType { None, Ladder, Cliff }
    ClimbType climbType = ClimbType.None;

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
        Move();
        Turn();
        Ray(); // Ray for Interaction
        TeleportControl();
        Grab();

        Character.Move(Velocity * Time.deltaTime);
    }

    // =========================
    // MOVE
    // =========================
    void Move()
    {
        if (isClimbing)
            ClimbMove();
        else
            WalkMove();
    }

    void WalkMove()
    {
        Vector2 move = OVRInput.Get(OVRInput.RawAxis2D.LThumbstick);
        bool sprint = OVRInput.Get(OVRInput.RawButton.LThumbstick);
        bool jump = OVRInput.GetDown(OVRInput.RawButton.A);

        Vector3 forward = CenterEye.forward;
        Vector3 right = CenterEye.right;

        forward.y = 0;
        right.y = 0;

        forward.Normalize();
        right.Normalize();

        Vector3 dir = forward * move.y + right * move.x;

        float speed = sprint ? runSpeed : walkSpeed;

        Velocity.x = dir.x * speed;
        Velocity.z = dir.z * speed;

        if (Character.isGrounded)
        {
            if (Velocity.y < 0)
                Velocity.y = -2f;

            if (jump)
                Velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        Velocity.y += gravity * Time.deltaTime;
    }

    void ClimbMove()
    {
        Vector2 move = OVRInput.Get(OVRInput.RawAxis2D.LThumbstick);
        bool jump = OVRInput.GetDown(OVRInput.RawButton.A);

        Velocity = Vector3.zero;

        if (climbType == ClimbType.Ladder)
        {
            Velocity.y = move.y * climbSpeed;
        }
        else if (climbType == ClimbType.Cliff)
        {
            Velocity =
                transform.right * move.x * climbSpeed +
                Vector3.up * move.y * climbSpeed;

            Velocity += -transform.forward * 0.01f;
        }

        if (jump)
            ClimbJump();
    }

    void ClimbJump()
    {
        isClimbing = false;

        Vector3 dir = (-transform.forward + Vector3.up).normalized;
        float force = Mathf.Sqrt(jumpHeight * -2f * gravity);

        Velocity = dir * force;
    }

    // =========================
    // TURN
    // =========================
    void Turn()
    {
        float turn = OVRInput.Get(OVRInput.RawAxis2D.RThumbstick).x;

        if (turnReady)
        {
            if (turn > 0.7f)
            {
                transform.Rotate(Vector3.up * snapAngle);
                turnReady = false;
            }
            else if (turn < -0.7f)
            {
                transform.Rotate(Vector3.up * -snapAngle);
                turnReady = false;
            }
        }

        if (Mathf.Abs(turn) < 0.2f)
            turnReady = true;
    }

    // =========================
    // RAY
    // =========================
    void Ray()
    {
        bool trigger = OVRInput.Get(OVRInput.RawButton.LIndexTrigger);

        if (!trigger)
        {
            LeftControllerRay.enabled = false;

            // Process Ray Exit
            if (CurrentTarget != null)
            {
                CurrentTarget.OnRayExit();
                CurrentTarget = null;
            }
            return;
        }

        LeftControllerRay.enabled = true;

        Vector3 start = LeftController.position + LeftController.forward * 0.04f;
        Vector3 dir = LeftController.forward;

        LeftControllerRay.SetPosition(0, start);

        if (Physics.Raycast(start, dir, out RaycastHit hit, rayDistance))
        {
            LeftControllerRay.SetPosition(1, hit.point);

            // Process Ray Interaction
            var Interactable = hit.collider.GetComponent<IRayInteractable>();

            if (Interactable != null)
            {
                // Assign Interacable to CurrentTarget
                if (CurrentTarget != Interactable)
                {
                    CurrentTarget?.OnRayExit();

                    CurrentTarget = Interactable;
                    CurrentTarget.OnRayEnter();
                }

                CurrentTarget.OnRayStay();

                if (OVRInput.GetDown(OVRInput.RawButton.LIndexTrigger))
                {
                    CurrentTarget.OnRayClick();
                }
            }
            else
            {
                // If not the interactable target, exit
                if (CurrentTarget != null)
                {
                    CurrentTarget.OnRayExit();
                    CurrentTarget = null;
                }
            }
        }
        else
        {
            LeftControllerRay.SetPosition(1, start + dir * rayDistance);

            // Exit
            if (CurrentTarget != null)
            {
                CurrentTarget.OnRayExit();
                CurrentTarget = null;
            }
        }
    }

    // =========================
    // TELEPORT
    // =========================
    void TeleportControl()
    {
        bool trigger = OVRInput.Get(OVRInput.RawButton.RIndexTrigger);

        if (trigger)
        {
            teleportActive = true;
            Arc.enabled = true;
            TeleportPreview();
        }
        else
        {
            if (teleportActive)
                Teleport();

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

    void Teleport()
    {
        if (!hasValidTarget) return;
        Character.enabled = false;
        transform.position = TeleportTarget + Vector3.up * 0.5f;
        Character.enabled = true;
    }

    // =========================
    // GRAB
    // =========================
    void Grab()
    {
        bool grab = OVRInput.Get(OVRInput.RawButton.LHandTrigger);

        if (grab)
        {
            if (GrabbedRB == null)
                TryGrab();
            else
                UpdateGrab();
        }
        else
        {
            if (GrabbedRB != null)
                Release();
        }
    }

    void TryGrab()
    {
        Collider[] hits = Physics.OverlapSphere(LeftController.position, grabRadius, GrabLayer);

        float minDist = float.MaxValue;
        Rigidbody closest = null;

        foreach (Collider c in hits)
        {
            Rigidbody rb = c.attachedRigidbody;
            if (rb == null) continue;

            float d = Vector3.Distance(LeftController.position, rb.position);

            if (d < minDist)
            {
                minDist = d;
                closest = rb;
            }
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
        Vector3 targetPos = LeftController.position + LeftController.rotation * PosOffset;
        Quaternion targetRot = LeftController.rotation * RotOffset;

        GrabbedRB.MovePosition(targetPos);
        GrabbedRB.MoveRotation(targetRot);
    }

    void Release()
    {
        GrabbedRB.isKinematic = false;

        GrabbedRB.velocity = OVRInput.GetLocalControllerVelocity(OVRInput.Controller.LTouch);
        GrabbedRB.angularVelocity = OVRInput.GetLocalControllerAngularVelocity(OVRInput.Controller.LTouch);

        Physics.IgnoreCollision(ObjectCollider, PlayerCollider, false);

        GrabbedRB = null;
        ObjectCollider = null;
    }

    // =========================
    // CLIMB TRIGGER
    // =========================
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("ClimbableLadder"))
        {
            climbType = ClimbType.Ladder;
            isClimbing = true;
            Velocity = Vector3.zero;
        }

        if (other.CompareTag("ClimbableCliff"))
        {
            climbType = ClimbType.Cliff;
            isClimbing = true;
            Velocity = Vector3.zero;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("ClimbableLadder") ||
            other.CompareTag("ClimbableCliff"))
        {
            isClimbing = false;
            climbType = ClimbType.None;
        }
    }
}