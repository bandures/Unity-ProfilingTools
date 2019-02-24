using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.Android.Profiling
{
    public class AndroidDeviceInfo
    {
#if UNITY_2019_1_OR_NEWER
        public AndroidDeviceInfo(ADB adb, string deviceId)
        {
            var deviceInfo = new AndroidDevice(adb, deviceId);

            Id = deviceInfo.Id;
            Model = deviceInfo.Model;
            Features = deviceInfo.Features;
            GLVersion = deviceInfo.GLVersion;

            GetProperty = (string id) =>
            {
                return deviceInfo.Properties[id];
            };
        }
#else
        public AndroidDeviceInfo(object adb, string deviceId)
        {
            var asm = AndroidUtils.GetUnityEditorAndroidAssembly();
            var refType = asm.GetType("UnityEditor.Android.AndroidDevice");
            var deviceInfo = Activator.CreateInstance(refType, new object[] { adb, deviceId });

            Id = PropertyAccessor<string>(deviceInfo, "Id");
            Model = PropertyAccessor<string>(deviceInfo, "Model");
            Features = PropertyAccessor<List<string>>(deviceInfo, "Features");
            GLVersion = PropertyAccessor<int>(deviceInfo, "GLVersion");

            GetProperty = (string id) =>
            {
                var table = deviceInfo.GetType().GetProperty("Properties").GetValue(deviceInfo, null);
                return (string)table.GetType().GetProperty("Item").GetValue(table, new object[] { id });
            };
        }

        private T PropertyAccessor<T>(object deviceInfo, string id)
        {
            return (T)deviceInfo.GetType().GetProperty(id).GetValue(deviceInfo, null);
        }
#endif

        public readonly string Id;
        public readonly string Model;
        public readonly List<string> Features;
        public readonly int GLVersion;
        public readonly Func<string, string> GetProperty;
    }
}
