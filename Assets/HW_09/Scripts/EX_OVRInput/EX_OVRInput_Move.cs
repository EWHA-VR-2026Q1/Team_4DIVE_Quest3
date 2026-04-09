using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class EX_OVRInput_Move : MonoBehaviour
{
    CharacterController Character;

    public Transform CenterEye;

    [Header("Move")]
    public float walkSpeed = 1.5f;
    public float runSpeed = 3.5f;

    [Header("Jump")]
    public float jumpHeight = 0.5f;
    public float gravity = -9.81f;

    [Header("Climb")]
    public float climbSpeed = 1.2f;

    enum ClimbType
    {
        None,
        Ladder,
        Cliff
    }

    ClimbType climbType = ClimbType.None;

    bool isClimbing = false;

    Vector3 velocity;

    void Start()
    {
        Character = GetComponent<CharacterController>();
    }

    void Update()
    {
        if (isClimbing)
        {
            ClimbMove();
        }
        else
        {
            WalkMove();
        }

        Character.Move(velocity * Time.deltaTime);
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

        velocity.x = dir.x * speed;
        velocity.z = dir.z * speed;

        if (Character.isGrounded)
        {
            if (velocity.y < 0)
                velocity.y = -2f;

            if (jump)
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        velocity.y += gravity * Time.deltaTime;
    }

    void ClimbMove()
    {
        Vector2 move = OVRInput.Get(OVRInput.RawAxis2D.LThumbstick);

        bool jump = OVRInput.GetDown(OVRInput.RawButton.A);

        velocity.x = 0;
        velocity.z = 0;

        if (climbType == ClimbType.Ladder)
        {
            velocity.y = move.y * climbSpeed;
        }
        else if (climbType == ClimbType.Cliff)
        {
            velocity =
                transform.right * move.x * climbSpeed +
                Vector3.up * move.y * climbSpeed;

            velocity += -transform.forward * 0.01f;
        }

        if (jump)
            ClimbJump();
    }

    void ClimbJump()
    {
        isClimbing = false;

        Vector3 jumpOutDir = -transform.forward + Vector3.up;

        float jumpForce = Mathf.Sqrt(jumpHeight * -2f * gravity);

        velocity = jumpOutDir.normalized * jumpForce;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("ClimbableLadder"))
        {
            climbType = ClimbType.Ladder;
            isClimbing = true;
            velocity = Vector3.zero;
        }

        if (other.CompareTag("ClimbableCliff"))
        {
            climbType = ClimbType.Cliff;
            isClimbing = true;
            velocity = Vector3.zero;
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