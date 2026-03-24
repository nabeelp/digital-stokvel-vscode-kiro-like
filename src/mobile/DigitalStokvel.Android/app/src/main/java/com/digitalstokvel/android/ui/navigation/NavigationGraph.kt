package com.digitalstokvel.android.ui.navigation

import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.padding
import androidx.compose.material3.Scaffold
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import androidx.navigation.compose.NavHost
import androidx.navigation.compose.composable
import androidx.navigation.compose.rememberNavController

/**
 * Navigation routes for the Digital Stokvel app.
 */
sealed class Screen(val route: String) {
    data object Splash : Screen("splash")
    data object Login : Screen("login")
    data object Register : Screen("register")
    data object Home : Screen("home")
    data object GroupList : Screen("groups")
    data object GroupDetail : Screen("groups/{groupId}") {
        fun createRoute(groupId: String) = "groups/$groupId"
    }
    data object CreateGroup : Screen("groups/create")
    data object ContributionList : Screen("contributions")
    data object MakeContribution : Screen("contributions/make")
    data object PayoutList : Screen("payouts")
    data object Voting : Screen("voting")
    data object Profile : Screen("profile")
}

/**
 * Main navigation graph for the Digital Stokvel app.
 * Uses Jetpack Compose Navigation for screen transitions.
 */
@Composable
fun NavigationGraph() {
    val navController = rememberNavController()
    
    NavHost(
        navController = navController,
        startDestination = Screen.Splash.route
    ) {
        composable(Screen.Splash.route) {
            SplashScreen(
                onNavigateToLogin = { navController.navigate(Screen.Login.route) },
                onNavigateToHome = { navController.navigate(Screen.Home.route) }
            )
        }
        
        composable(Screen.Login.route) {
            LoginScreen(
                onNavigateToRegister = { navController.navigate(Screen.Register.route) },
                onLoginSuccess = { 
                    navController.navigate(Screen.Home.route) {
                        popUpTo(Screen.Login.route) { inclusive = true }
                    }
                }
            )
        }
        
        composable(Screen.Register.route) {
            RegisterScreen(
                onNavigateBack = { navController.popBackStack() },
                onRegisterSuccess = { 
                    navController.navigate(Screen.Home.route) {
                        popUpTo(Screen.Register.route) { inclusive = true }
                    }
                }
            )
        }
        
        composable(Screen.Home.route) {
            HomeScreen(
                onNavigateToGroups = { navController.navigate(Screen.GroupList.route) },
                onNavigateToContributions = { navController.navigate(Screen.ContributionList.route) },
                onNavigateToPayouts = { navController.navigate(Screen.PayoutList.route) },
                onNavigateToProfile = { navController.navigate(Screen.Profile.route) }
            )
        }
        
        // TODO: Add remaining screen destinations
        // - GroupList
        // - GroupDetail
        // - CreateGroup
        // - ContributionList
        // - MakeContribution
        // - PayoutList
        // - Voting
        // - Profile
    }
}

/**
 * Temporary splash screen placeholder.
 */
@Composable
fun SplashScreen(
    onNavigateToLogin: () -> Unit,
    onNavigateToHome: () -> Unit
) {
    Scaffold(modifier = Modifier.fillMaxSize()) { innerPadding ->
        Text(
            text = "Digital Stokvel - Splash Screen",
            modifier = Modifier.padding(innerPadding)
        )
    }
    
    // TODO: Check authentication status
    // TODO: Navigate to Login or Home based on auth state
    // For now, navigate to login after delay
    onNavigateToLogin()
}

/**
 * Temporary login screen placeholder.
 */
@Composable
fun LoginScreen(
    onNavigateToRegister: () -> Unit,
    onLoginSuccess: () -> Unit
) {
    Scaffold(modifier = Modifier.fillMaxSize()) { innerPadding ->
        Text(
            text = "Digital Stokvel - Login Screen",
            modifier = Modifier.padding(innerPadding)
        )
    }
    // TODO: Implement login UI
}

/**
 * Temporary register screen placeholder.
 */
@Composable
fun RegisterScreen(
    onNavigateBack: () -> Unit,
    onRegisterSuccess: () -> Unit
) {
    Scaffold(modifier = Modifier.fillMaxSize()) { innerPadding ->
        Text(
            text = "Digital Stokvel - Register Screen",
            modifier = Modifier.padding(innerPadding)
        )
    }
    // TODO: Implement register UI
}

/**
 * Temporary home screen placeholder.
 */
@Composable
fun HomeScreen(
    onNavigateToGroups: () -> Unit,
    onNavigateToContributions: () -> Unit,
    onNavigateToPayouts: () -> Unit,
    onNavigateToProfile: () -> Unit
) {
    Scaffold(modifier = Modifier.fillMaxSize()) { innerPadding ->
        Text(
            text = "Digital Stokvel - Home Screen",
            modifier = Modifier.padding(innerPadding)
        )
    }
    // TODO: Implement home UI with bottom navigation
}
