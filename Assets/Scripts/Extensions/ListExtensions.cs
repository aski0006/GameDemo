using System.Collections.Generic;

namespace Extensions
{
    public static class ListExtensions
    {
        public static T DrawRandomElement<T>(this System.Collections.Generic.List<T> list)
        {
            if (list == null || list.Count == 0)
            {
                return default;
            }
            int randomIndex = UnityEngine.Random.Range(0, list.Count);
            T t = list[randomIndex];
            list.RemoveAt(randomIndex);
            return t;
        }
        public static void Shuffle<T>(this System.Collections.Generic.List<T> list)
        {
            int n = list.Count;
            for (int i = 0; i < n; i++)
            {
                int r = UnityEngine.Random.Range(i, n);
                (list[i], list[r]) = (list[r], list[i]);
            }
        }
        
    }
}
