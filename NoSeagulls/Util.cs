using System.Reflection;
using UnityEngine;

namespace NoSeagulls
{
    public static class Util
    {
        internal static void DebugPrint(params string[] args)
        {
            string s = string.Join(" ", args);
            Debug.Log("[NoSeagulls] " + s);
        }

        internal static object Get(object instance, string field) =>
            instance.GetType().GetField(field, BindingFlags.NonPublic | BindingFlags.Instance).GetValue(instance);

        internal static void Set(object instance, string field, object value) =>
            instance.GetType().GetField(field, BindingFlags.NonPublic | BindingFlags.Instance).SetValue(instance, value);
    }
}
