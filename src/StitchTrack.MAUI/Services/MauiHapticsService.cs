using StitchTrack.Application.Interfaces;

namespace StitchTrack.MAUI.Services
{
    public class MauiHapticsService : IHapticsService
    {
        public void Click()
        {
            // 1) Prefer "haptic click"
            try
            {
                HapticFeedback.Default.Perform(HapticFeedbackType.Click);
                return;
            }
            catch (FeatureNotSupportedException)
            {
                // Device/OS doen't support haptic feedback - fallback to vibration
            }
            catch (InvalidOperationException)
            {
                // Sometimes throw if the platform isn't in a valid state for haptics
            }
            catch (OperationCanceledException)
            {
                // Rare, but treat as "no haptics right now" 
            }

            // 2) Fallback to vibration
            try
            {
                Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(10));
            }
            catch (FeatureNotSupportedException)
            {
                // No vibration hardware / not supported
            }
            catch (PermissionException)
            {
                // Missing android.permission.VIBRATE - should be caught by PermissionsService, but just in case
            }
            catch (InvalidOperationException)
            {
                // Platform state doesn't allow vibration right now
            }
        }
    }
}
