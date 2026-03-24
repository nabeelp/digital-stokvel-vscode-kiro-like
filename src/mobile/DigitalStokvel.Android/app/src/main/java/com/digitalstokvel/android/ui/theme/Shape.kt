package com.digitalstokvel.android.ui.theme

import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material3.Shapes
import androidx.compose.ui.unit.dp

/**
 * Material Design 3 shape system for Digital Stokvel.
 * Defines rounded corners for different component sizes.
 */
val Shapes = Shapes(
    // Extra Small - chips, tags
    extraSmall = RoundedCornerShape(4.dp),
    
    // Small - buttons, text fields
    small = RoundedCornerShape(8.dp),
    
    // Medium - cards, dialogs
    medium = RoundedCornerShape(12.dp),
    
    // Large - bottom sheets, modals
    large = RoundedCornerShape(16.dp),
    
    // Extra Large - large cards, image containers
    extraLarge = RoundedCornerShape(28.dp)
)
