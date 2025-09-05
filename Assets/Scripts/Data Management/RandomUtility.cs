using System;
using System.Collections.Generic;

public static class RandomUtility
{
    // Fisher Yates shuffle
    // https://stackoverflow.com/questions/108819/best-way-to-randomize-an-array-with-net/
    
    public static int RandInt()
    {
        return new Random().Next();
    }
    public static Random GenerateRandom()
    {
        return new Random();
    }
    public static Random GenerateRandomBySeed(int seed)
    {
        return new Random(seed);
    }
    public static void Shuffle<T>(Random rng, T[] array)
    {
        int n = array.Length;
        while (n > 1)
        {
            int k = rng.Next(n--);
            T temp = array[n];
            array[n] = array[k];
            array[k] = temp;
        }
    }
    public static void Shuffle<T>(Random rng, IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            int k = rng.Next(n--);
            T temp = list[n];
            list[n] = list[k];
            list[k] = temp;
        }
    }
}
