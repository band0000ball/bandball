using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Commons
{
    public interface IHaveWeight
    {
        float Weight { get; }
    }

    public static class Tools
    {
        public static float RoundValue(float value, int digits = 0)
        {
            return (float)Math.Round(value, digits, MidpointRounding.AwayFromZero);
        }
        
        public static (T min, int arg) MinAndArg<T>(params T[] nums) where T : IComparable
        {
            if(nums.Length == 0) return (default(T), 0);

            T min = nums[0];
            int minArg = 0;
            for(int i = 1; i < nums.Length; i++)
            {
                if (min.CompareTo(nums[i]) > 0)
                {
                    min = nums[i];
                    minArg = i;
                }
            }
            return (min, minArg);
        }
        
        public static (T max, int arg) MaxAndArg<T>(params T[] nums) where T : IComparable
        {
            if(nums.Length == 0) return (default(T), 0);

            T max = nums[0];
            int maxArg = 0;
            for(int i = 1; i < nums.Length; i++)
            {
                if (max.CompareTo(nums[i]) < 0)
                {
                    max = nums[i];
                    maxArg = i; 
                }
            }
            return (max, maxArg);
        }
        
        public static T Min<T>(params T[] nums) where T : IComparable
        {
            if(nums.Length == 0) return default(T);

            T min = nums[0];
            for(int i = 1; i < nums.Length; i++)
            {
                min = min.CompareTo(nums[i]) < 0 ? min : nums[i];
            }
            return min;
        }
        
        public static T Max<T>(params T[] nums) where T : IComparable
        {
            if(nums.Length == 0) return default(T);

            T max = nums[0];
            for(int i = 1; i < nums.Length; i++)
            {
                max = max.CompareTo(nums[i]) > 0 ? max : nums[i];
            }
            return max;
        }
        
        /// <summary>
        /// 重み付き抽選を行う
        /// </summary>
        /// <param name="itemWeightPairs"></param>
        /// <param name="addNum"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns> 
        // interface利用
        public static T Lotto<T>(IEnumerable<T> itemWeightPairs, float addNum = 0f) where T : IHaveWeight
        {
            // Weight順でソート
            var sortedPairs = itemWeightPairs.OrderByDescending(x => x.Weight).ToArray();

            // ドロップアイテムの抽選
            float total = sortedPairs.Select(x => x.Weight).Sum();

            float randomPoint = Random.Range(0, total + addNum);

            // randomPointの位置に該当するキーを返す
            foreach (T elem in sortedPairs)
            {
                if (randomPoint < elem.Weight)
                {
                    return elem;
                }

                randomPoint -= elem.Weight;
            }

            return sortedPairs[^1];
        }

        public static T Lotto<T>(IEnumerable<KeyValuePair<T, float>> itemWeightPairs, float addNum = 0f)
        {
            // Weight降順でソート
            var sortedPairs = itemWeightPairs.OrderByDescending(x => x.Value).ToArray();

            // ドロップアイテムの抽選
            float total = sortedPairs.Select(x => x.Value).Sum();

            float randomPoint = Random.Range(0, total + addNum);

            // randomPointの位置に該当するキーを返す
            foreach (KeyValuePair<T, float> elem in sortedPairs)
            {
                if (randomPoint < elem.Value)
                {
                    return elem.Key;
                }

                randomPoint -= elem.Value;
            }

            return sortedPairs[^1].Key;
        }
        
        public static void Invoke(this MonoBehaviour mb, Action f, float delay)
        {
            mb.StartCoroutine(InvokeRoutine(f, delay));
        }

        private static IEnumerator InvokeRoutine(Action f, float delay)
        {
            yield return new WaitForSeconds(delay);
            f();
        }
    }
}