package com.digitalstokvel.android

import android.app.Application
import dagger.hilt.android.HiltAndroidApp

/**
 * Application class for Digital Stokvel Android app.
 * Annotated with @HiltAndroidApp to enable Hilt dependency injection.
 */
@HiltAndroidApp
class DigitalStokvelApplication : Application() {
    
    override fun onCreate() {
        super.onCreate()
        
        // TODO: Initialize Firebase
        // TODO: Initialize Crashlytics
        // TODO: Initialize Analytics
        // TODO: Set up WorkManager for background sync
    }
}
