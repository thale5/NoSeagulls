using ColossalFramework;
using ColossalFramework.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using UnityEngine;

namespace NoSeagulls
{
    public class Util
    {
        public static void DebugPrint(params string[] args)
        {
            // DateTime now = System.DateTime.Now;
            // string s = String.Join(" ", args) + " (" + Thread.CurrentThread.ManagedThreadId + ", " + now.Ticks + ", " + now + ")";
            string s = String.Join(" ", args);
            Debug.Log("[NoSeagulls] " + s);
        }
    }
}
