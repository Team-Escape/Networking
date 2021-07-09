using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MonoExtension
{
    public static void AbleToDo<T>(this T tweener, float sec, System.Action callback) where T : MonoBehaviour
    {
        tweener.StartCoroutine(DelaySec(sec, callback));
    }
    public static IEnumerator DelaySec(float sec, System.Action callback)
    {
        yield return new WaitForSeconds(sec);
        callback();
    }
}
