using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ListExtentions {
   
    private static System.Random rng = new System.Random();
    // FISHER YATES SHUFFLE
    /// <summary>
    /// Shuffles the List using the Fisher-Yates shuffling method
    /// </summary>
    public static void Shuffle<T>(this IList<T> list)
    {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
    }
    /// <summary>
    /// Returns a Random Value from the List, make sure the List isn't Empty
    /// </summary>
    public static T GetRandomValue<T>(this IList<T> list)
    {
        
        if(list.Count > 0)
        {
            return list[Random.Range(0, list.Count)];
        }
        else
        {
            return default(T);
        }
    } 
}
