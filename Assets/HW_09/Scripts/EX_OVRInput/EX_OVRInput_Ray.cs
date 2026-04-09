using UnityEngine;

// For Left Controller
public class EX_OVRInput_Ray : MonoBehaviour
{
    public Transform LeftController;

    public LineRenderer LeftControllerRay;

    public float distance = 10f;

    void Start()
    {
        LeftControllerRay.enabled = false;
    }

    void Update()
    {
        bool trigger = OVRInput.Get(OVRInput.RawButton.LIndexTrigger);

        if (!trigger)
        {
            LeftControllerRay.enabled = false;
            return;
        }

        LeftControllerRay.enabled = true;

        Vector3 start = LeftController.position + LeftController.forward * 0.04f;
        Vector3 dir = LeftController.forward;

        LeftControllerRay.SetPosition(0, start);

        if (Physics.Raycast(start, dir, out RaycastHit hit, distance))
        {
            LeftControllerRay.SetPosition(1, hit.point);
        }
        else
        {
            LeftControllerRay.SetPosition(1, start + dir * distance);
        }
    }
}