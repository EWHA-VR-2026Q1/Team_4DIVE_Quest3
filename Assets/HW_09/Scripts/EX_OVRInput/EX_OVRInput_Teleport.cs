using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class EX_OVRInput_Teleport : MonoBehaviour
{
    CharacterController Character;
    public Transform RightController;
    public LayerMask TeleportLayer;

    public int resolution = 30;
    public float teleportVelocity = 8f;
    public float timestep = 0.08f;
    bool hasValidTarget = false;
    bool teleportActive = false;
    
    public LineRenderer Arc;
    public GameObject Marker;
    Vector3[] LineFragments;
    Vector3 Target;

    void Start()
    {
        Character = GetComponent<CharacterController>();

        LineFragments = new Vector3[resolution];

        Arc.enabled = false;
    }

    void Update()
    {
        TeleportControl();
    }

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

                    Target = hit.point;

                    Marker.SetActive(true);
                    Marker.transform.position = Target + Vector3.up * 0.02f;

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
        transform.position = Target + Vector3.up * 0.5f;
        Character.enabled = true;
    }
}