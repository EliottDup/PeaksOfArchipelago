using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PeaksOfArchipelago.Extensions
{
    internal static class DeepFind
    {
        public static Transform FindDeep(this Transform aParent, string name)
        {
            Queue<Transform> queue = new Queue<Transform>();
            queue.Enqueue(aParent);
            while (queue.Count > 0) {
                Transform t = queue.Dequeue();
                if (t.name == name)
                {
                    return t;
                }
                foreach (Transform child in t)
                {
                    queue.Enqueue(child);
                }
            }
            return null;
        }
    }
}
