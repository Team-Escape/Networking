using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MonoExtension
{
    /// <param name="RandomSeed">
    /// Random a list of seed. Length of n, and choose object in prefab pool randomly.
    ///</param>
    public static List<T> RandomSeed<T>(this List<T> source, int n)
    {
        List<int> seed = new List<int>();
        List<T> container = new List<T>();
        for (int i = 0; i < n; i++)
        {
            // int rnd = UnityEngine.Random.Range(0, max);
            int r = UnityEngine.Random.Range(0, source.Count);
            while (true)
            {
                if (seed.Contains(r) == false) break;
                r = UnityEngine.Random.Range(0, source.Count);
            }
            seed.Add(r);
            container.Add(source[r]);
        }
        return container;
    }
    public static List<int> RandomSeedInt<T>(this List<T> source, int n)
    {
        List<int> seed = new List<int>();
        for (int i = 0; i < n; i++)
        {
            // int rnd = UnityEngine.Random.Range(0, max);
            int r = UnityEngine.Random.Range(0, source.Count);
            while (true)
            {
                if (seed.Contains(r) == false) break;
                r = UnityEngine.Random.Range(0, source.Count);
            }
            seed.Add(r);
        }
        return seed;
    }
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
