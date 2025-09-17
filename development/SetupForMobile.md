# Setup Mobile Development

This document describes the requirements and steps to build and run the mobile apps (Android & iOS) locally using Capacitor.

## Requirements

- **Node.js** (already required for frontend)
- **Java JDK 17**
  - Windows: comes bundled with Android Studio (`C:\Program Files\Android\Android Studio\jbr`)
  - Linux: install `openjdk-17-jdk` or Temurin
- **Android Studio**
  - Install [Android Studio](https://developer.android.com/studio)
  - Includes:
    - Android SDK Platform (API 34 or latest stable)
    - Build Tools
    - Command-line Tools
    - Android Emulator (optional, for testing without a device)
    - Google USB Driver (Windows, for physical devices)
- **Capacitor CLI** (already in project dependencies)
- **CocoaPods** (for iOS, macOS only): `sudo gem install cocoapods`

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

## Building for iOS (macOS only)

1. Install Xcode and CocoaPods.
2. From `frontend/`:

```bash
npm run mobile:sync
npx cap open ios
```

-> opens the iOS project in Xcode. 3. Build & run in Xcode on simulator or device. 4. To publish on App Store: configure signing in Xcode with your Apple Developer account.

## Notes

- Use npm run mobile:sync before every Android/iOS build to update assets, icons, and version numbers.
- On Linux/macOS, make sure JAVA_HOME points to JDK 17 if Gradle errors about unsupported versions occurr.
