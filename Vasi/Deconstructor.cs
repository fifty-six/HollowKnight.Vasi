using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace Vasi
{
    [PublicAPI]
    public static class Deconstructor
    {
        public static void Deconstruct(this Vector3 v, out float x, out float y, out float z) => (x, y, z) = (v.x, v.y, v.z);

        public static void Desconstruct(this Vector2 v, out float x, out float y) => (x, y) = (v.x, v.y);
        
        public static void Deconstruct<TKey, TValue>(this KeyValuePair<TKey, TValue> self, out TKey key, out TValue value)
        {
            (key, value) = (self.Key, self.Value);
        }
    }
}