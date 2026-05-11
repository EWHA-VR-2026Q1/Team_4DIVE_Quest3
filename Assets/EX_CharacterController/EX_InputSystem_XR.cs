using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

[RequireComponent(typeof(CharacterController))]
public class EX_InputSystem_XR : MonoBehaviour
{
    CharacterController Character;

    [Header("XR Rig")]
    public Transform CenterEye;

    [Header("Controllers")]
    public Transform leftController;
    public Transform rightController;

    [Header("Controller Rays")]
    public LineRenderer leftRay;
    public LineRenderer rightRay;
    public float rayDistance = 10f;

    [Header("Input Actions")]
    public InputActionReference moveAction;
    public InputActionReference turnAction;
    public InputActionReference jumpAction;
    public InputActionReference sprintAction;
    public InputActionReference teleportAction;

    [Header("Movement")]
    public float walkSpeed = 1.5f;
    public float runSpeed = 3.5f;
    public float friction = 0.9f;

    Vector3 Velocity;
    Vector2 MoveInput;
    bool isSprinting;

    [Header("Jump")]
    public float jumpHeight = 0.5f;
    public float gravity = -9.81f;
    bool jumpPressed;

    [Header("Snap Turn")]
    public float snapTurnAngle = 45f;
    public float turnThreshold = 0.7f;
    bool turnReady = true;

    [Header("Climb")]
    public float climbSpeed = 1.2f;
    bool isClimbing = false;

    enum ClimbType { None, Ladder, Cliff }
    ClimbType climbType = ClimbType.None;

    [Header("Teleport")]
    public LayerMask teleportLayer;
    public GameObject teleportMarker;

    bool teleportValid;
    Vector3 teleportPoint;

    [Header("Physics Teleport Arc")]
    public LineRenderer teleportRay;

    public int arcResolution = 30;
    public float arcVelocity = 8f;
    public float arcTimeStep = 0.1f;

    void Start()
    {
        Character = GetComponent<CharacterController>();
    }

    void Update()
    {
        ReadInput();

        if (teleportAction.action.WasPressedThisFrame())
        {
            TryTeleport();
        }

        Turn();

        if (isClimbing)
        {
            Climb();
            CheckClimbJump();
        }
        else
        {
            Move();
            ApplyGravity();
        }

        UpdateControllerRay(leftRay, leftController);
        UpdateControllerRay(rightRay, rightController);

        UpdateTeleportPreview();

        Character.Move(Velocity * Time.deltaTime);
    }

    void ReadInput()
    {
        MoveInput = moveAction.action.ReadValue<Vector2>();
        jumpPressed = jumpAction.action.WasPressedThisFrame();
        isSprinting = sprintAction.action.IsPressed();
    }

    void Turn()
    {
        Vector2 turnInput = turnAction.action.ReadValue<Vector2>();

        if (turnReady)
        {
            if (turnInput.x > turnThreshold)
            {
                transform.Rotate(Vector3.up * snapTurnAngle);
                turnReady = false;
            }
            else if (turnInput.x < -turnThreshold)
            {
                transform.Rotate(Vector3.up * -snapTurnAngle);
                turnReady = false;
            }
        }

        if (Mathf.Abs(turnInput.x) < 0.2f)
            turnReady = true;
    }

    void Move()
    {
        Vector3 forward = CenterEye.forward;
        Vector3 right = CenterEye.right;

        forward.y = 0;
        right.y = 0;

        forward.Normalize();
        right.Normalize();

        Vector3 inputDir = forward * MoveInput.y + right * MoveInput.x;

        if (inputDir.sqrMagnitude > 1)
            inputDir.Normalize();

        float speed = isSprinting ? runSpeed : walkSpeed;

        if (inputDir.sqrMagnitude > 0)
        {
            Velocity.x = inputDir.x * speed;
            Velocity.z = inputDir.z * speed;
        }
        else
        {
            Velocity.x *= friction;
            Velocity.z *= friction;
        }

        if (Character.isGrounded)
        {
            if (Velocity.y < 0)
                Velocity.y = -2f;

            if (jumpPressed)
            {
                Velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }
        }
    }

    void UpdateTeleportPreview()
    {
        Vector3 start = rightController.position;
        Vector3 startVelocity = rightController.forward * arcVelocity;

        List<Vector3> points = new List<Vector3>();

        teleportValid = false;

        for (int i = 0; i < arcResolution; i++)
        {
            float t = i * arcTimeStep;

            Vector3 point =
                start +
                startVelocity * t +
                0.5f * Vector3.up * gravity * t * t;

            points.Add(point);

            if (i > 0)
            {
                Vector3 prev = points[i - 1];

                if (Physics.Linecast(prev, point, out RaycastHit hit, teleportLayer))
                {
                    teleportValid = true;
                    teleportPoint = hit.point;

                    points.Add(hit.point);

                    teleportMarker.SetActive(true);
                    teleportMarker.transform.position =
                        hit.point + Vector3.up * 0.02f;

                    break;
                }
            }
        }

        teleportRay.positionCount = points.Count;
        teleportRay.SetPositions(points.ToArray());

        if (!teleportValid)
        {
            teleportMarker.SetActive(false);
        }
    }

    void TryTeleport()
    {
        if (!teleportValid) return;

        Character.enabled = false;

        transform.position = teleportPoint + Vector3.up * 0.05f;

        Character.enabled = true;

        Velocity = Vector3.zero;
    }

    void UpdateControllerRay(LineRenderer ray, Transform controller)
    {
        Vector3 start = controller.position + controller.forward * 0.035f;
        Vector3 dir = controller.forward;

        ray.SetPosition(0, start);

        if (Physics.Raycast(start, dir, out RaycastHit hit, rayDistance))
        {
            ray.SetPosition(1, hit.point);
        }
        else
        {
            ray.SetPosition(1, start + dir * rayDistance);
        }
    }

    void ApplyGravity()
    {
        Velocity.y += gravity * Time.deltaTime;
    }

    void Climb()
    {
        Velocity.x = 0;
        Velocity.z = 0;

        if (climbType == ClimbType.Ladder)
        {
            Velocity.y = MoveInput.y * climbSpeed;
        }
        else if (climbType == ClimbType.Cliff)
        {
            Velocity = transform.right * MoveInput.x * climbSpeed +
                       Vector3.up * MoveInput.y * climbSpeed;

            Velocity += -transform.forward * 0.1f;
        }
    }

    void CheckClimbJump()
    {
        if (jumpPressed)
        {
            isClimbing = false;

            Vector3 jumpOutDir = -transform.forward + Vector3.up;
            float jumpForce = Mathf.Sqrt(jumpHeight * -2f * gravity);

            Velocity = jumpOutDir.normalized * jumpForce;
        }
    }

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
        if (other.CompareTag("ClimbableLadder") || other.CompareTag("ClimbableCliff"))
            isClimbing = false;
    }
}