using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Google.Android.Material.BottomNavigation;
using AndroidView = Android.Views.View;
using AndroidViewGroup = Android.Views.ViewGroup;

namespace StitchTrack.MAUI.Platforms.Android;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        // Wait a bit for the view to be created, then apply customization
        Window?.DecorView?.PostDelayed(() =>
        {
            ApplyTabBarCustomization();
        }, 100);
    }

    private void ApplyTabBarCustomization()
    {
        var rootView = Window?.DecorView?.RootView;
        if (rootView != null)
        {
            var bottomNavView = FindBottomNavigationView(rootView);
            if (bottomNavView != null)
            {
                var density = Resources?.DisplayMetrics?.Density ?? 1;
                int paddingDp = 12;
                int paddingPx = (int)(paddingDp * density);

                // Add padding
                bottomNavView.SetPadding(
                    bottomNavView.PaddingLeft,
                    paddingPx,
                    bottomNavView.PaddingRight,
                    paddingPx
                );

                // Add elevation for shadow
                bottomNavView.Elevation = 16 * density; // Increased from 8 to 16

                // Ensure shadow is visible
                bottomNavView.OutlineProvider = ViewOutlineProvider.Bounds;
            }
        }
    }

    private BottomNavigationView? FindBottomNavigationView(AndroidView view)
    {
        if (view is BottomNavigationView bottomNav)
            return bottomNav;

        if (view is AndroidViewGroup viewGroup)
        {
            for (int i = 0; i < viewGroup.ChildCount; i++)
            {
                var child = viewGroup.GetChildAt(i);
                if (child != null)
                {
                    var result = FindBottomNavigationView(child);
                    if (result != null)
                        return result;
                }
            }
        }

        return null;
    }
}
