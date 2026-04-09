using UnityEngine;

public class EX_OVRInput_Grab : MonoBehaviour
{
    [Header("Hand")]
    public Transform LeftHand;

    [Header("Grab")]
    public float grabRadius = 0.2f;
    public LayerMask GrabLayer;

    Rigidbody GrabbedRB;
    Collider PlayerCollider;
    Collider ObjectCollider;

    Vector3 PosOffset;
    Quaternion RotOffset;

    void Start()
    {
        PlayerCollider = GetComponent<CharacterController>().GetComponent<Collider>();
    }

    void Update()
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
        Collider[] hits = Physics.OverlapSphere(LeftHand.position, grabRadius, GrabLayer);

        if (hits.Length == 0)
            return;

        float minDist = float.MaxValue;
        Rigidbody closest = null;

        foreach (Collider c in hits)
        {
            Rigidbody rb = c.attachedRigidbody;

            if (rb == null)
                continue;

            float d = Vector3.Distance(LeftHand.position, rb.position);

            if (d < minDist)
            {
                minDist = d;
                closest = rb;
            }
        }

        if (closest == null)
            return;

        GrabbedRB = closest;

        ObjectCollider = GrabbedRB.GetComponent<Collider>();

        GrabbedRB.isKinematic = true;

        // Player와 충돌 제거
        Physics.IgnoreCollision(ObjectCollider, PlayerCollider, true);

        // offset 계산
        PosOffset = Quaternion.Inverse(LeftHand.rotation) * (GrabbedRB.position - LeftHand.position);

        RotOffset = Quaternion.Inverse(LeftHand.rotation) * GrabbedRB.rotation;
    }

    void UpdateGrab()
    {
        Vector3 targetPos = LeftHand.position + LeftHand.rotation * PosOffset;

        Quaternion targetRot = LeftHand.rotation * RotOffset;

        GrabbedRB.MovePosition(targetPos);
        GrabbedRB.MoveRotation(targetRot);
    }

    void Release()
    {
        GrabbedRB.isKinematic = false;

        // 던지기
        GrabbedRB.velocity = OVRInput.GetLocalControllerVelocity(OVRInput.Controller.LTouch);

        GrabbedRB.angularVelocity = OVRInput.GetLocalControllerAngularVelocity(OVRInput.Controller.LTouch);

        Physics.IgnoreCollision(ObjectCollider, PlayerCollider, false);

        GrabbedRB = null;
        ObjectCollider = null;
    }

    void OnDrawGizmos()
    {
        if (LeftHand == null)
            return;

        Gizmos.color = Color.yellow;

        Gizmos.DrawWireSphere(LeftHand.position, grabRadius);
    }
}