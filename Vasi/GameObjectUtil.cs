using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

namespace Vasi
{
    [PublicAPI]
    public static class GameObjectUtil
    {
        public static GameObject Child(this GameObject go, string name)
        {
            return go.transform.Find(name).gameObject;
        }

        public static GameObject FindInChildren(this GameObject go, string name)
        {
            return go.GetComponentsInChildren<Transform>().First(x => x.name == name).gameObject;
        }
    }
}