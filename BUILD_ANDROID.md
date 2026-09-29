# Building Vector Traffic 3D for Android

## Prerequisites
- Unity 6 Editor (6000.0.32f1 or later)
- Android Build Support module (OpenJDK, Android SDK & NDK)
- Target Android API Level: 34 (Android 14)
- Minimum Android API Level: 24 (Android 7.0 Nougat)
- Scripting Backend: IL2CPP
- Target Architectures: ARM64 (required for Google Play), ARMv7

## CLI Batchmode Build Command
```bash
unity-editor -batchmode -quit \
  -projectPath . \
  -executeMethod VectorTraffic3D.Editor.BuildPipeline.BuildAndroidRelease \
  -logFile build.log
```
