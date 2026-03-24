package com.digitalstokvel.android.ui.components

import androidx.compose.foundation.layout.PaddingValues
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.size
import androidx.compose.material3.Button
import androidx.compose.material3.ButtonDefaults
import androidx.compose.material3.CircularProgressIndicator
import androidx.compose.material3.FilledTonalButton
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.OutlinedButton
import androidx.compose.material3.Text
import androidx.compose.material3.TextButton
import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import androidx.compose.ui.unit.dp
import com.digitalstokvel.android.ui.theme.Dimensions

/**
 * Primary button for main actions (e.g., "Submit Payment").
 */
@Composable
fun DSPrimaryButton(
    text: String,
    onClick: () -> Unit,
    modifier: Modifier = Modifier,
    enabled: Boolean = true,
    loading: Boolean = false
) {
    Button(
        onClick = onClick,
        modifier = modifier.height(Dimensions.buttonHeightMedium),
        enabled = enabled && !loading,
        contentPadding = PaddingValues(horizontal = Dimensions.paddingButton)
    ) {
        if (loading) {
            CircularProgressIndicator(
                modifier = Modifier.size(Dimensions.iconSizeMedium),
                color = MaterialTheme.colorScheme.onPrimary,
                strokeWidth = 2.dp
            )
        } else {
            Text(text = text)
        }
    }
}

/**
 * Secondary button for less prominent actions (e.g., "View Details").
 */
@Composable
fun DSSecondaryButton(
    text: String,
    onClick: () -> Unit,
    modifier: Modifier = Modifier,
    enabled: Boolean = true
) {
    FilledTonalButton(
        onClick = onClick,
        modifier = modifier.height(Dimensions.buttonHeightMedium),
        enabled = enabled,
        contentPadding = PaddingValues(horizontal = Dimensions.paddingButton)
    ) {
        Text(text = text)
    }
}

/**
 * Outlined button for alternative actions (e.g., "Cancel").
 */
@Composable
fun DSOutlinedButton(
    text: String,
    onClick: () -> Unit,
    modifier: Modifier = Modifier,
    enabled: Boolean = true
) {
    OutlinedButton(
        onClick = onClick,
        modifier = modifier.height(Dimensions.buttonHeightMedium),
        enabled = enabled,
        contentPadding = PaddingValues(horizontal = Dimensions.paddingButton)
    ) {
        Text(text = text)
    }
}

/**
 * Text button for tertiary actions (e.g., "Skip").
 */
@Composable
fun DSTextButton(
    text: String,
    onClick: () -> Unit,
    modifier: Modifier = Modifier,
    enabled: Boolean = true
) {
    TextButton(
        onClick = onClick,
        modifier = modifier,
        enabled = enabled
    ) {
        Text(text = text)
    }
}
