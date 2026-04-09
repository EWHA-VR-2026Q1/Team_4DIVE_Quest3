using UnityEngine;

// For Left Controller
public class EX_OVRInput_RayForInteraction : MonoBehaviour
{
    public Transform LeftController;

    public LineRenderer LeftControllerRay;

    public float rayDistance = 10f;

    IRayInteractable CurrentTarget; // Declare a Variable for Ray for Interaction

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
}