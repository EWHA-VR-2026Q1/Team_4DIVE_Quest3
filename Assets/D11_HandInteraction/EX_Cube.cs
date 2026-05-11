using UnityEngine;

public class EX_Cube : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        ICollidable target = gameObject.GetComponent<ICollidable>();

        if (target != null)
        {
            target.OnCollisionStart();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        ICollidable target = gameObject.GetComponent<ICollidable>();

        if (target != null)
        {
            target.OnCollisionStart();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        ICollidable target = gameObject.GetComponent<ICollidable>();

        if (target != null)
        {
            target.OnCollisionStart();
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        ICollidable target = gameObject.GetComponent<ICollidable>();

        if (target != null)
        {
            target.OnCollisionEnd();
        }
    }
}