using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unity.Android.Profiling
{
    class AndroidUtils
    {
        public static Assembly GetUnityEditorAndroidAssembly()
        {
            Assembly[] assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
            foreach (var asm in assemblies)
            {
                if (asm.GetName().Name == "UnityEditor.Android.Extensions")
                    return asm;
            }
            return null;
        }

        public static object GetJavaToolsInstance(Assembly asm)
        {
            var refType = asm.GetType("UnityEditor.Android.AndroidJavaTools");
            var refMethod = refType.GetMethod("GetInstance", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
            return refMethod.Invoke(null, new object[] { false });
        }

        public static object GetAndroidSDKToolsInstance(Assembly asm, object javaTools)
        {
            var refType = asm.GetType("UnityEditor.Android.AndroidSDKTools");
            var refMethod = refType.GetMethod("GetInstance", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
            return refMethod.Invoke(null, new object[] { javaTools, false });
        }

        public static object InstantiateADB(Assembly asm, object sdkTools)
        {
            var refType = asm.GetType("UnityEditor.Android.ADB");
            return System.Activator.CreateInstance(refType, new object[] { sdkTools });
        }
    }
}
