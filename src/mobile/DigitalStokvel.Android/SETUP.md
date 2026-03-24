# Digital Stokvel - Android Setup Guide

This guide will help you set up the Android development environment for the Digital Stokvel project.

## Prerequisites

### 1. Install Android Studio

Download and install Android Studio Ladybug or later:
- **Website:** https://developer.android.com/studio
- **Requirements:**
  - **Windows:** 8 GB RAM minimum, 16 GB recommended
  - **macOS:** 8 GB RAM minimum, 16 GB recommended
  - **Linux:** 8 GB RAM minimum, 16 GB recommended

### 2. Install JDK 17

Android Studio includes JDK, but you may need to install it separately:
- **Windows:** Use Android Studio's embedded JDK or download from https://adoptium.net/
- **macOS:** `brew install openjdk@17`
- **Linux:** `sudo apt install openjdk-17-jdk`

### 3. Configure Android SDK

After installing Android Studio:

1. Open **Android Studio > Settings (Preferences on macOS) > Appearance & Behavior > System Settings > Android SDK**
2. Install the following SDK components:
   - **SDK Platforms:**
     - Android 14.0 (API 34)
     - Android 7.0 (API 24) - minimum supported
   - **SDK Tools:**
     - Android SDK Build-Tools 34.0.0
     - Android Emulator
     - Android SDK Platform-Tools
     - Google Play services

## Project Setup

### 1. Clone the Repository

```bash
git clone https://github.com/your-org/digital-stokvel.git
cd digital-stokvel/src/mobile/DigitalStokvel.Android
```

### 2. Configure Local Properties

Copy the example file and update with your Android SDK path:

```bash
cp local.properties.example local.properties
```

Edit `local.properties`:

```properties
# Windows
sdk.dir=C\:\\Users\\YourName\\AppData\\Local\\Android\\Sdk

# macOS
sdk.dir=/Users/YourName/Library/Android/sdk

# Linux
sdk.dir=/home/YourName/Android/Sdk

# API URL for development (use emulator localhost address)
API_BASE_URL=http://10.0.2.2:5000/v1
```

### 3. Open Project in Android Studio

1. Launch **Android Studio**
2. Select **Open an Existing Project**
3. Navigate to `src/mobile/DigitalStokvel.Android`
4. Click **OK**

Android Studio will:
- Sync Gradle dependencies (this may take a few minutes)
- Index the project files
- Download required dependencies

### 4. Sync Gradle

If Gradle doesn't sync automatically:
- Click **File > Sync Project with Gradle Files**
- Or click the **Sync** button in the notification bar

### 5. Run the App

#### Using Android Emulator:

1. Click **Device Manager** in Android Studio
2. Click **Create Device**
3. Select a device (e.g., Pixel 6)
4. Download a system image (API 34 recommended)
5. Click **Finish**
6. Select the emulator from the device dropdown
7. Click **Run** (green play button) or press `Shift+F10`

#### Using Physical Device:

1. Enable **Developer Options** on your Android device:
   - Go to **Settings > About Phone**
   - Tap **Build Number** 7 times
2. Enable **USB Debugging**:
   - Go to **Settings > System > Developer Options**
   - Enable **USB Debugging**
3. Connect your device via USB
4. Allow USB debugging when prompted
5. Select your device from the device dropdown
6. Click **Run**

## Gradle Commands

You can also build and run the app using Gradle commands:

### Build Debug APK

```bash
./gradlew assembleDebug
```

Output: `app/build/outputs/apk/debug/app-debug.apk`

### Install Debug APK on Connected Device

```bash
./gradlew installDebug
```

### Run Unit Tests

```bash
./gradlew test
```

### Run Instrumentation Tests

```bash
./gradlew connectedAndroidTest
```

### Clean Build

```bash
./gradlew clean
```

### Check Code Style (Ktlint)

```bash
./gradlew ktlintCheck
```

### Format Code (Ktlint)

```bash
./gradlew ktlintFormat
```

## Troubleshooting

### Issue: "SDK location not found"

**Solution:** Ensure `local.properties` exists and contains the correct SDK path.

```bash
cp local.properties.example local.properties
# Edit and update sdk.dir
```

### Issue: "Gradle sync failed"

**Solution:**
1. Click **File > Invalidate Caches / Restart**
2. Delete `.gradle` folder and sync again
3. Ensure you have a stable internet connection for dependency downloads

### Issue: "Unable to resolve dependency"

**Solution:**
1. Check your internet connection
2. Clear Gradle cache: `rm -rf ~/.gradle/caches`
3. Sync Gradle again

### Issue: "Emulator won't start"

**Solution:**
1. Ensure virtualization is enabled in BIOS/UEFI
2. Install Intel HAXM (Windows/Linux) or Apple Virtualization Framework (macOS)
3. Try creating a new emulator with a different API level

### Issue: "App crashes on startup"

**Solution:**
1. Check `adb logcat` for error messages
2. Verify API base URL in `local.properties`
3. Ensure backend services are running

### Issue: "Cannot connect to localhost"

**Solution:**
- Android Emulator: Use `10.0.2.2` instead of `localhost`
- Physical Device: Use your computer's IP address (e.g., `192.168.1.100:5000`)
- Ensure backend API is accessible from your network

## Next Steps

After successful setup:

1. **Read the README:** `README.md` for architecture overview
2. **Check the Design Doc:** `.specs/design.md` for feature requirements
3. **Review Tasks:** `.specs/tasks.md` for implementation plan
4. **Start Development:** Begin with Task 3.1.1 - 3.1.14

## Additional Resources

- **Kotlin Documentation:** https://kotlinlang.org/docs/home.html
- **Jetpack Compose:** https://developer.android.com/jetpack/compose
- **Material Design 3:** https://m3.material.io/
- **Hilt Dependency Injection:** https://dagger.dev/hilt/
- **Android Developers:** https://developer.android.com/

## Support

For technical issues:
- **Slack:** #android-team
- **Email:** tech@digitalstokvel.co.za
- **Documentation:** `docs/` folder

---

**Last Updated:** March 2026
