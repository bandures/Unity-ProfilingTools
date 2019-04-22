using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Scripting;

namespace Unity.NativeProfiling
{
    /// <summary>
    /// Static class implementing API for starting and stopping markers, which can be used in release mode
    /// </summary>
    public static class NativeProfiler
    {
        [DllImport("ProfilerPluginX")]
        public static extern void BeginMarker([MarshalAs(UnmanagedType.LPStr)]string str);

        [DllImport("ProfilerPluginX")]
        public static extern void EndMarker();
    }
}