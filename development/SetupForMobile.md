# Setup Mobile Development

This document describes the requirements and steps to build and run the mobile apps (Android & iOS) locally using Capacitor.

## Requirements

- **Node.js** (already required for frontend)
- **Java JDK 21**
  - Windows: if you encounter Gradle errors like `Unsupported class file major version 69`, install a JDK 21 distribution and set `org.gradle.java.home` in `./frontend/android/gradle.properties`.
  - Linux: install a matching JDK distribution (e.g. Temurin 21)
- **Android Studio**
  - Install [Android Studio](https://developer.android.com/studio)
  - Includes:
    - Android SDK Platform (API 34 or latest stable)
    - Build Tools
    - Command-line Tools
    - Android Emulator (optional, for testing without a device)
    - Google USB Driver (Windows, for physical devices)
- **Capacitor CLI** (already in project dependencies)

## Preparing environment

1. Install Android Studio and run it once to accept SDK licenses.
2. In **SDK Manager**, install:
   - Android SDK Platform (API 34 or latest)
   - Android SDK Build-Tools
   - Android SDK Command-line Tools
3. In **AVD Manager** (optional, for emulator testing):
   - Create a virtual device (Pixel recommended)
   - Choose a system image (e.g. Android 14 x86_64)

## Building for Android

From `frontend/`:

```bash
# build web assets, copy into android, update icons + versions
npm run mobile:sync

# build debug apk (for testing)
npm run mobile:debug:android
```

### Outputs

- Debug APK: `android/app/build/outputs/apk/debug/app-debug.apk`
- Release APK: `android/app/build/outputs/apk/release/app-release.apk`
- Release AAB: `android/app/build/outputs/bundle/release/app-release.aab`

### Install debug build on device

With adb in PATH:

```bash
adb install -r ./android/app/build/outputs/apk/debug/app-debug.apk
```

Or drag & drop `app-debug.apk` into a running emulator.

### Testing on a physical device

1. Enable Developer Options and USB Debugging on your phone.
2. Connect via USB (File Transfer mode).
3. Verify connection:

```bash
adb devices
```

4. Install:

```bash
adb install -r android/app/build/outputs/apk/debug/app-debug.apk
```

## Notes

- Use npm run mobile:sync before every Android/iOS build to update assets, icons, and version numbers.
- On Linux/macOS, make sure `JAVA_HOME` points to a compatible JDK (17 or 21). If you encounter errors about unsupported class file versions, follow the troubleshooting steps below.

### Troubleshooting: "Unsupported class file major version 69"

If your Android build fails with:

> BUG! exception in phase 'semantic analysis' ... Unsupported class file major version 69

This means Gradle (or the JVM running Gradle) is older than the class files produced by a plugin or dependency (major version 69 = Java 21). To fix:

1. Check your Java and Gradle JVM:

```powershell
java -version
.\android\gradlew.bat -version
```

2. If the JVM reported by Gradle is older than 21, install a JDK 21 distribution (e.g. Temurin 21) and either:

- Set `JAVA_HOME` (Windows PowerShell example):

```powershell
setx JAVA_HOME "C:\\Program Files\\Java\\jdk-25" -m
# restart terminal
```

- Or pin Gradle to a local JDK by adding to `frontend/android/gradle.properties`:

```properties
# Example - set to your JDK 25 installation path
# org.gradle.java.home=C:\\Program Files\\Java\\jdk-25
```

3. Re-open your terminal (so env vars take effect) and re-run the build:

```bash
npm run mobile:debug:android
```

If problems persist, share the `java -version` and the output of `.\android\gradlew.bat -version` so we can diagnose further.
