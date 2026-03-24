# Digital Stokvel - Android Application

**Version:** 1.0  
**Technology Stack:** Kotlin + Jetpack Compose  
**Minimum SDK:** API 24 (Android 7.0)  
**Target SDK:** API 34 (Android 14)  
**Design System:** Material Design 3

---

## Overview

This is the Android mobile application for the Digital Stokvel Banking platform. It provides members with access to group management, contribution payments, payout tracking, voting, and dispute resolution features.

### Key Features

- **Authentication:** JWT-based authentication with biometric support
- **Group Management:** Create groups, invite members, manage roles
- **Contributions:** Make payments via linked bank accounts or debit orders
- **Payouts:** Track payout schedules and receive notifications
- **Voting:** Participate in group votes with real-time updates
- **Disputes:** Submit and track dispute resolutions
- **Offline Support:** Graceful degradation for low-connectivity scenarios
- **Multilingual:** Support for 5 South African languages (English, Afrikaans, Zulu, Xhosa, Sotho)

---

## Architecture

### Technology Stack

| Component | Technology | Version |
|-----------|------------|---------|
| **Language** | Kotlin | 2.1.0 |
| **UI Framework** | Jetpack Compose | 1.7.5 |
| **Navigation** | Compose Navigation | 2.8.5 |
| **Networking** | Retrofit + OkHttp | 2.11.0 |
| **JSON Parsing** | Kotlinx Serialization | 1.7.3 |
| **Dependency Injection** | Hilt | 2.54 |
| **Async** | Kotlin Coroutines | 1.9.0 |
| **State Management** | ViewModel + StateFlow | Lifecycle 2.8.7 |
| **Local Storage** | Room Database | 2.6.1 |
| **Image Loading** | Coil | 3.0.4 |
| **Security** | Jetpack Security | 1.1.0-alpha06 |

### Design Patterns

- **MVVM (Model-View-ViewModel):** Clean separation of concerns
- **Repository Pattern:** Abstract data sources (API + local database)
- **Single Source of Truth:** Room database for offline-first architecture
- **Unidirectional Data Flow:** StateFlow for reactive UI updates

### Project Structure

```
app/src/main/java/com/digitalstokvel/android/
├── DigitalStokvelApplication.kt       # Application class with Hilt setup
├── MainActivity.kt                     # Single activity host
├── data/
│   ├── api/                           # Retrofit API interfaces
│   │   ├── AuthApi.kt
│   │   ├── GroupApi.kt
│   │   ├── ContributionApi.kt
│   │   └── PayoutApi.kt
│   ├── local/                         # Room database
│   │   ├── DigitalStokvelDatabase.kt
│   │   ├── dao/
│   │   └── entities/
│   ├── repository/                    # Repository implementations
│   └── models/                        # Data models and DTOs
├── domain/
│   ├── usecase/                       # Business logic use cases
│   └── models/                        # Domain models
├── ui/
│   ├── theme/                         # Material Design 3 theme
│   │   ├── Color.kt
│   │   ├── Theme.kt
│   │   ├── Type.kt
│   │   └── Shape.kt
│   ├── screens/                       # Composable screens
│   │   ├── auth/
│   │   │   ├── LoginScreen.kt
│   │   │   └── RegisterScreen.kt
│   │   ├── groups/
│   │   │   ├── GroupListScreen.kt
│   │   │   ├── GroupDetailScreen.kt
│   │   │   └── CreateGroupScreen.kt
│   │   ├── contributions/
│   │   │   ├── ContributionListScreen.kt
│   │   │   └── MakeContributionScreen.kt
│   │   ├── payouts/
│   │   │   └── PayoutListScreen.kt
│   │   └── voting/
│   │       └── VotingScreen.kt
│   ├── components/                    # Reusable UI components
│   └── navigation/
│       └── NavigationGraph.kt         # App navigation
├── di/                                # Dependency injection modules
│   ├── NetworkModule.kt
│   ├── DatabaseModule.kt
│   └── RepositoryModule.kt
└── util/                              # Utility classes
    ├── NetworkMonitor.kt
    ├── Extensions.kt
    └── Constants.kt
```

---

## Prerequisites

### Development Environment

- **Android Studio:** Ladybug | 2024.2.1 or later
- **JDK:** 17 or later
- **Kotlin Plugin:** 2.1.0 or later
- **Android SDK:**
  - SDK Platform: API 34
  - SDK Build-Tools: 34.0.0
  - Android Emulator or physical device

### API Configuration

The app connects to the Digital Stokvel backend API. Configure the base URL in `gradle.properties`:

```properties
API_BASE_URL=https://api.digitalstokvel.co.za/v1
```

---

## Setup Instructions

### 1. Clone the Repository

```bash
git clone https://github.com/your-org/digital-stokvel.git
cd digital-stokvel/src/mobile/DigitalStokvel.Android
```

### 2. Open in Android Studio

1. Launch Android Studio
2. Select "Open an Existing Project"
3. Navigate to `src/mobile/DigitalStokvel.Android`
4. Click "OK"

### 3. Configure API Endpoint

Edit `gradle.properties` or use local.properties for development:

```properties
# local.properties (not committed to version control)
API_BASE_URL=http://10.0.2.2:5000/v1  # For Android Emulator
```

### 4. Sync Gradle

Android Studio should automatically sync Gradle. If not:
- Click **File > Sync Project with Gradle Files**

### 5. Run the App

- Select a device or emulator
- Click **Run > Run 'app'** or press `Shift+F10`

---

## Building

### Debug Build

```bash
./gradlew assembleDebug
```

Output: `app/build/outputs/apk/debug/app-debug.apk`

### Release Build (Signed)

```bash
./gradlew assembleRelease
```

Requires signing configuration in `app/build.gradle.kts`.

---

## Testing

### Unit Tests

```bash
./gradlew test
```

### Instrumentation Tests

```bash
./gradlew connectedAndroidTest
```

### Run Tests in Android Studio

- Right-click on `app/src/test` or `app/src/androidTest`
- Select **Run Tests**

---

## Code Quality

### Ktlint (Code Formatting)

```bash
./gradlew ktlintCheck
./gradlew ktlintFormat
```

### Detekt (Static Analysis)

```bash
./gradlew detekt
```

---

## Offline Support

The app uses an **offline-first architecture**:

1. **Local Database:** Room database stores critical data
2. **Network Monitor:** Detects connectivity changes
3. **Sync Queue:** Queues operations when offline
4. **Background Sync:** WorkManager syncs when connection restored

### Handling Low Connectivity

- Read operations work offline using cached data
- Write operations are queued and synced later
- User sees clear indicators for offline state
- Critical actions (payments) require connectivity

---

## Multilingual Support

The app supports 5 languages:

| Language | Code | Resource Folder |
|----------|------|-----------------|
| English | en | `values` (default) |
| Afrikaans | af | `values-af` |
| Zulu | zu | `values-zu` |
| Xhosa | xh | `values-xh` |
| Sotho | st | `values-st` |

Add translations in `app/src/main/res/values-XX/strings.xml`.

---

## Security

### Authentication

- JWT tokens stored in EncryptedSharedPreferences
- Biometric authentication (fingerprint/face unlock)
- Token refresh with automatic retry

### Data Protection

- HTTPS only (enforced by Network Security Config)
- Certificate pinning for API calls
- ProGuard/R8 obfuscation for release builds
- No sensitive data in logs (production)

### Permissions

Minimal permissions requested:
- `INTERNET` - API communication
- `USE_BIOMETRIC` - Biometric authentication (optional)
- `POST_NOTIFICATIONS` - Push notifications (optional)

---

## Push Notifications

The app uses **Firebase Cloud Messaging (FCM)** for push notifications:

- **Payment reminders:** 3 days before due date
- **Payout notifications:** When disbursement is approved
- **Voting alerts:** When new vote is created
- **Dispute updates:** Status changes

### FCM Configuration

Add `google-services.json` to `app/` directory (not committed).

---

## CI/CD Pipeline

### GitHub Actions Workflow

```yaml
name: Android CI

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main ]

jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - name: Set up JDK 17
        uses: actions/setup-java@v4
        with:
          java-version: '17'
          distribution: 'temurin'
      - name: Build with Gradle
        run: ./gradlew assembleDebug
      - name: Run tests
        run: ./gradlew test
      - name: Run linting
        run: ./gradlew ktlintCheck
```

---

## Performance Optimization

### Best Practices

- **Lazy Loading:** Load data on-demand
- **Pagination:** Cursor-based pagination for large lists
- **Image Caching:** Coil with disk cache
- **Network Optimization:** Compress JSON, minimize payload size
- **Battery Efficiency:** Use WorkManager for background tasks

### Monitoring

- **Firebase Performance Monitoring:** Track app start time, screen rendering
- **Crashlytics:** Automatic crash reporting

---

## Troubleshooting

### Common Issues

#### 1. Build Fails with "SDK not found"

**Solution:** Configure SDK location in `local.properties`:

```properties
sdk.dir=/Users/yourname/Library/Android/sdk
```

#### 2. App crashes on startup

**Solution:** Check API base URL configuration in `gradle.properties`.

#### 3. Emulator can't reach localhost

**Solution:** Use `10.0.2.2` instead of `localhost` for Android Emulator.

---

## Contributing

### Code Style

- Follow [Kotlin Coding Conventions](https://kotlinlang.org/docs/coding-conventions.html)
- Use Ktlint for automatic formatting
- Write unit tests for ViewModels and repositories
- Write instrumentation tests for critical flows

### Pull Request Process

1. Create feature branch: `git checkout -b feature/your-feature`
2. Run tests: `./gradlew test`
3. Run linting: `./gradlew ktlintCheck`
4. Commit with conventional commits: `feat: add group creation screen`
5. Push and create PR

---

## License

Copyright © 2026 Digital Stokvel Banking. All rights reserved.

---

## Contact

- **Technical Lead:** [Your Name]
- **Email:** tech@digitalstokvel.co.za
- **Slack:** #android-team
