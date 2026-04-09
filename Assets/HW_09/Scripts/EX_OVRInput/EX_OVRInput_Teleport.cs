using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class EX_OVRInput_Teleport : MonoBehaviour
{
    CharacterController Character;

    public Transform RightController;

    public LineRenderer Arc;

    public GameObject Marker;

    public LayerMask TeleportLayer;

    public int resolution = 30;

    public float velocity = 8f;

    public float timestep = 0.08f;

    Vector3[] LineFragments;

    bool active;

    Vector3 Target;

    void Start()
    {
        Character = GetComponent<CharacterController>();

        LineFragments = new Vector3[resolution];

        Arc.enabled = false;
    }

    void Update()
    {
        bool trigger = OVRInput.Get(OVRInput.RawButton.RIndexTrigger);

        if (trigger)
        {
            active = true;
            Arc.enabled = true;
            Preview();
        }
        else
        {
            if (active)
                Teleport();

            active = false;

            Arc.enabled = false;

            Marker.SetActive(false);
        }
    }

    void Preview()
    {
        Vector3 start = RightController.position + RightController.forward * 0.04f;

        Vector3 vel = RightController.forward * velocity;

        for (int i = 0; i < resolution; i++)
        {
            float t = i * timestep;

            Vector3 p = start + vel * t + 0.5f * Physics.gravity * t * t;

            LineFragments[i] = p;

            if (i > 0)
            {
                if (Physics.Linecast(LineFragments[i - 1], p, out RaycastHit hit, TeleportLayer))
                {
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
        Character.enabled = false;

        transform.position = Target + Vector3.up * 0.5f;

        Character.enabled = true;
    }
}