﻿using System.Collections.Generic;
using System.Linq;
using System;

namespace NDSB.Models.SparseModels
{
    /// <summary>
    /// Implements various helpers to access data.
    /// </summary>
    public static class SmartIndexes
    {
        /// <summary>
        /// Returns an inverted dictionnary of the keys.
        /// </summary>
        /// <param name="sample"></param>
        public static Dictionary<T, int[]> InverseKeys<T>(Dictionary<T, double>[] sample, double minValue, int preAlloc1 = 1000000, int preAlloc2 = 100)
        {
            Dictionary<T, List<int>> invertedIndexes = new Dictionary<T, List<int>>(preAlloc1);
            for (int i = 0; i < sample.Length; i++)
            {
                Dictionary<T, double> sparsePoint = sample[i];
                foreach (KeyValuePair<T, double> kvp in sparsePoint)
                {
                    T currentKey = kvp.Key;
                    if (kvp.Value < minValue) continue;
                    if (invertedIndexes.ContainsKey(currentKey))
                        invertedIndexes[currentKey].Add(i);
                    else
                    {
                        invertedIndexes.Add(currentKey, new List<int>(preAlloc2));
                        invertedIndexes[currentKey].Add(i);
                    }
                }
            }

            Dictionary<T,int[]> invertedIndexesArray = new Dictionary<T,int[]>(invertedIndexes.Count); // this dictionnary enjoys a better pre-allocation
            for (int i = 0; i < invertedIndexes.Count; i++)
                invertedIndexesArray.Add(invertedIndexes.ElementAt(i).Key, invertedIndexes.ElementAt(i).Value.ToArray());

            return invertedIndexesArray;
        }

        public static Dictionary<T, int[]> InverseKeysAndSort<T>(Dictionary<T, double>[] sample, double minValue = 0, int preAlloc1 = 1000000, int preAlloc2 = 100)
        {
            Dictionary<T, int[]> invertedIndexes = InverseKeys<T>(sample, minValue, preAlloc1, preAlloc2);
            for (int i = 0; i < invertedIndexes.Count; i++)
                Array.Sort(invertedIndexes[invertedIndexes.Keys.ElementAt(i)]);
            return invertedIndexes;
        }

        public static int[] IntersectSortedDichotomy<T>(int[] seq1, int[] seq2)
        {
            List<int> res = new List<int>(seq2.Length);
            for (int i = 0; i < seq2.Length; i++)
            {
                int element = seq2[i];
                int pos = Array.BinarySearch(seq1, element);
                if (pos > -1)
                    res.Add(element);
            }
            return res.ToArray();
        }

        public static IEnumerable<T> IntersectSorted<T>(IEnumerable<T> sequence1, IEnumerable<T> sequence2) where T : IComparable<T>
        {
            using (var cursor1 = sequence1.GetEnumerator())
            using (var cursor2 = sequence2.GetEnumerator())
            {
                if (!cursor1.MoveNext() || !cursor2.MoveNext())
                {
                    yield break;
                }
                var value1 = cursor1.Current;
                var value2 = cursor2.Current;

                while (true)
                {
                    int comparison = value1.CompareTo(value2);
                    if (comparison < 0)
                    {
                        if (!cursor1.MoveNext())
                        {
                            yield break;
                        }
                        value1 = cursor1.Current;
                    }
                    else if (comparison > 0)
                    {
                        if (!cursor2.MoveNext())
                        {
                            yield break;
                        }
                        value2 = cursor2.Current;
                    }
                    else
                    {
                        yield return value1;
                        if (!cursor1.MoveNext() || !cursor2.MoveNext())
                        {
                            yield break;
                        }
                        value1 = cursor1.Current;
                        value2 = cursor2.Current;
                    }
                }
            }
        }

        public static List<T> ExceptSorted<T>(T[] m, T[] n) where T : IComparable<T>
        {
            var result = new List<T>();
            int i = 0, j = 0;
            if (n.Length == 0)
            {
                result.AddRange(m);
                return result;
            }
            while (i < m.Length)
            {
                if (m[i].CompareTo(n[j]) < 0)
                {
                    result.Add(m[i]);
                    i++;
                }
                else if (m[i].CompareTo(n[j]) > 0)
                {
                    j++;
                }
                else
                {
                    i++;
                }
                if (j >= n.Length)
                {
                    for (; i < m.Length; i++)
                    {
                        result.Add(m[i]);
                    }
                    break;
                }
            }
            return result;
        }
    }
}