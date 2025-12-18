using Foundation;
using UIKit;

namespace StitchTrack.MAUI.Platforms.iOS;

[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
    protected override MauiApp CreateMauiApp()
    {
        // Customize TabBar appearance
        UITabBar.Appearance.ItemSpacing = 0;
        UITabBar.Appearance.ItemPositioning = UITabBarItemPositioning.Fill;

        // Add subtle shadow on top
        var tabBarAppearance = new UITabBarAppearance();
        tabBarAppearance.ConfigureWithDefaultBackground();
        tabBarAppearance.BackgroundColor = UIColor.FromRGB(225, 173, 55);
        tabBarAppearance.ShadowColor = UIColor.Black.ColorWithAlpha(0.1f); // Subtle shadow

        UITabBar.Appearance.StandardAppearance = tabBarAppearance;
        UITabBar.Appearance.ScrollEdgeAppearance = tabBarAppearance;

        return MauiProgram.CreateMauiApp();
    }
}
