using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Experimental.LowLevel;

namespace Unity.NativeProfiling
{
    /// <summary>
    /// Implementation for release mode profiler markers
    /// It exploits the fact that you can hook to any Unity subsystem using Scriptable Player Loop
    /// </summary>
    public static class SimpleTraceMarkers
    {
        [System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Integrate()
        {
            // Make sure we aren't reporting markers twice
            if (Debug.isDebugBuild)
                return;
            
            var loop = PlayerLoop.GetDefaultPlayerLoop();

            loop = PatchSystem(loop, 0, loop.updateFunction);

            PlayerLoop.SetPlayerLoop(loop);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        delegate uint PlayerLoopDelegate();

        static int m_LastLevel = 0;

        static void TraceMarker(int level, string name)
        {
            for (int i = level; i <= m_LastLevel; i++)
                NativeProfiler.EndMarker();

            NativeProfiler.BeginMarker(name);
            m_LastLevel = level;
        }

        static PlayerLoopSystem PatchSystem(PlayerLoopSystem system, int level, IntPtr nullFnc)
        {
            PlayerLoopDelegate systemDelegate = null;
            if (system.updateFunction.ToInt64() != 0)
            {
                var intPtr = Marshal.ReadIntPtr(system.updateFunction);
                if (intPtr.ToInt64() != 0)
                    systemDelegate = (PlayerLoopDelegate)Marshal.GetDelegateForFunctionPointer(intPtr, typeof(PlayerLoopDelegate));
            }

            var type = system.type;
            system.updateDelegate = () => { TraceMarker(level, type.Name); if (systemDelegate != null) systemDelegate(); };
            system.updateFunction = nullFnc;

            if (system.subSystemList == null)
                return system;

            for (int i = 0; i < system.subSystemList.Length; i++)
                system.subSystemList[i] = PatchSystem(system.subSystemList[i], level + 1, nullFnc);

            return system;
        }
    }
}