# Profiling Tools

The Profiling Tools package allows you to setup Unity project for profiling with native platform tools.

The Profiling Tools support two instrumentation modes - for development and release builds
In development build, integration provides you Unity debug markers. The same markers which you can see in Unity Editor Profiler.
In release builds, integration can provide you limited set of markers on general Unity subsystem activities. You can enchant you release build with custom markers using provided simple API.

## Android
### Google Android Studio
Android Studio allows to profile with sampling profiler and using Unity instrumentation markers.
More details on setup and profiling in this [Google Document - Unity Profiling With - Android Studio](https://docs.google.com/document/d/17WJQZyT4PSSumEZvyvDlpAfC0qZER_vRqmkhrelU6k4/edit?usp=sharing)

### Qualcomm Snapdragon Profiler
Snapdragon Profiler allows you to perform CPU and GPU profiling on Adreno powered devices.
You can download Snapdragon Profiler on [Qualcomm web site](https://developer.qualcomm.com/software/snapdragon-profiler)

### Arm Mobile Studio
Arm Mobile Studio consist of multiple tools, which allow you to do CPU and GPU profiling and GPU frame analyzis.
* Arm Streamline Analyzer for GPU and CPU sampling profiling.
* Arm Graphics Analyzer for GPU frame analyzis

Arm Mobile Studiocan be downloaded on [Arm web site](https://www.arm.com/products/development-tools/graphics/arm-mobile-studio)

## Windows and Linux
### Intel VTune Amplifier
Intel VTune Amplifier allows you to do sampling and instrumentation profiling at the same time.
You can download Intel VTune Amplifier on [Intel web site](https://software.intel.com/en-us/vtune)

[!] Be aware of limitations of sampling profiler! While you can have dense Unity markers, always check how dense your sampling markers are before selecting any small perioud of time for analyzis.

