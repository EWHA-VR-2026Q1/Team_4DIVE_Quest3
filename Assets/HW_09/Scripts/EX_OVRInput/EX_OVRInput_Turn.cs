using UnityEngine;

public class EX_OVRInput_Turn : MonoBehaviour
{
    public float snapAngle = 45f;

    bool ready = true;

    void Update()
    {
        float turn = OVRInput.Get(OVRInput.RawAxis2D.RThumbstick).x;

        if (ready)
        {
            if (turn > 0.7f)
            {
                transform.Rotate(Vector3.up * snapAngle);
                ready = false;
            }
            else if (turn < -0.7f)
            {
                transform.Rotate(Vector3.up * -snapAngle);
                ready = false;
            }
        }

        if (Mathf.Abs(turn) < 0.2f)
            ready = true;
    }
}