using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AnimationExtension
{
    public static void DoAnimation(this Animator animator, string name)
    {
        animator.SetTrigger(name);
    }
    public static void DoAnimation(this Animator animator, string name, float value)
    {
        animator.SetFloat(name, value);
    }
}
