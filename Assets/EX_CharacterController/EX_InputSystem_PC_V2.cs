using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class EX_InputSystem_PC_V2 : MonoBehaviour
{
    CharacterController Character;

    [Header("Camera")]
    public Transform CameraPivot;
    public float mouseSensitivity = 0.1f;
    float yaw;
    float pitch;

    [Header("Move/Run/Jump/Climb")]
    public float walkSpeed = 1.5f;
    public float runSpeed = 3.5f;
    public float friction = 0.9f;
    public float climbSpeed = 1.2f;
    public float jumpHeight = 0.5f;
    public float gravity = -9.81f;

    // State Variables
    Vector3 Velocity;
    Vector2 MoveInput;
    Vector2 LookInput;

    bool isSprinting;
    bool jumpPressed;


    enum PlayerState
    {
        Ground,
        Air,
        Climb
    }

    PlayerState state = PlayerState.Ground;

    enum ClimbType
    {
        None,
        Ladder,
        Cliff
    }

    ClimbType climbType = ClimbType.None;

    void Start()
    {
        Character = GetComponent<CharacterController>();

        if (CameraPivot == null)
            CameraPivot = transform.Find("CameraRig/CameraPivot");

        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        Look();

        StateMachine();

        Character.Move(Velocity * Time.deltaTime);

        jumpPressed = false;
    }

    // -------- Input System 메시지 --------

    void OnMove(InputValue value)
    {
        MoveInput = value.Get<Vector2>();
    }

    void OnLook(InputValue value)
    {
        LookInput = value.Get<Vector2>();
    }

    void OnJump(InputValue value)
    {
        if (value.isPressed)
            jumpPressed = true;
    }

    void OnSprint(InputValue value)
    {
        isSprinting = value.isPressed;
    }

    // -------- Camera --------

    void Look()
    {
        yaw += LookInput.x * mouseSensitivity;
        pitch -= LookInput.y * mouseSensitivity;

        pitch = Mathf.Clamp(pitch, -90f, 90f);

        transform.rotation = Quaternion.Euler(0, yaw, 0);
        CameraPivot.localRotation = Quaternion.Euler(pitch, 0, 0);
    }

    // -------- State Machine --------

    void StateMachine()
    {
        switch (state)
        {
            case PlayerState.Ground:
                GroundMove();
                break;

            case PlayerState.Air:
                AirMove();
                break;

            case PlayerState.Climb:
                ClimbMove();
                break;
        }
    }

    // -------- Ground --------

    void GroundMove()
    {
        Vector3 inputDir =
            transform.forward * MoveInput.y +
            transform.right * MoveInput.x;

        if (inputDir.sqrMagnitude > 1)
            inputDir.Normalize();

        float speed = isSprinting ? runSpeed : walkSpeed;

        if (inputDir.magnitude > 0)
        {
            Velocity.x = inputDir.x * speed;
            Velocity.z = inputDir.z * speed;
        }
        else
        {
            Velocity.x *= friction;
            Velocity.z *= friction;
        }

        Velocity.y = -2f;

        if (jumpPressed)
        {
            Velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            state = PlayerState.Air;
            return;
        }

        if (!Character.isGrounded)
        {
            state = PlayerState.Air;
        }
    }

    // -------- Air --------

    void AirMove()
    {
        Vector3 inputDir =
            transform.forward * MoveInput.y +
            transform.right * MoveInput.x;

        float speed = walkSpeed;

        Velocity.x = inputDir.x * speed;
        Velocity.z = inputDir.z * speed;

        Velocity.y += gravity * Time.deltaTime;

        if (Character.isGrounded)
        {
            state = PlayerState.Ground;
        }
    }

    // -------- Climb --------

    void ClimbMove()
    {
        Velocity = Vector3.zero;

        if (climbType == ClimbType.Ladder)
        {
            Velocity.y = MoveInput.y * climbSpeed;
        }
        else if (climbType == ClimbType.Cliff)
        {
            Velocity =
                transform.right * MoveInput.x * climbSpeed +
                Vector3.up * MoveInput.y * climbSpeed;

            Velocity += -transform.forward * 0.1f;
        }

        if (jumpPressed)
        {
            Vector3 jumpDir = -transform.forward + Vector3.up;
            float jumpForce = Mathf.Sqrt(jumpHeight * -2f * gravity);

            Velocity = jumpDir.normalized * jumpForce;

            state = PlayerState.Air;
        }
    }

    // -------- Trigger --------

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("ClimbableLadder"))
        {
            climbType = ClimbType.Ladder;
            state = PlayerState.Climb;
            Velocity = Vector3.zero;
        }

        if (other.CompareTag("ClimbableCliff"))
        {
            climbType = ClimbType.Cliff;
            state = PlayerState.Climb;
            Velocity = Vector3.zero;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("ClimbableLadder") ||
            other.CompareTag("ClimbableCliff"))
        {
            state = PlayerState.Air;
        }
    }
}