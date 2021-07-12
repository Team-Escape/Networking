using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MonoExtension
{
    public static T Random<T>(this List<T> source)
    {
        return source[UnityEngine.Random.Range(0, source.Count)];
    }
    public static int RandomInt<T>(this List<T> source)
    {
        return UnityEngine.Random.Range(0, source.Count);
    }
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
