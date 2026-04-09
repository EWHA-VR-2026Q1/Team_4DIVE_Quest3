using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller_Animator : MonoBehaviour
{
    public Animator animator;
    public string Parameter;

    public void SetInteger(int value)
    {
        animator.SetInteger(Parameter, value);
    }
}
