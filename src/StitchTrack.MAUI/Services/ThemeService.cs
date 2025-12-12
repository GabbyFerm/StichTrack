using System;
using Microsoft.Maui.Controls;
using StitchTrack.MAUI.Resources.Styles;

namespace StitchTrack.MAUI.Services
{
    public static class ThemeService
    {
        // Apply light theme (call on startup)
        public static void ApplyLightTheme()
        {
            ApplyTheme(new LightColors());
        }

        // Apply dark theme
        public static void ApplyDarkTheme()
        {
            ApplyTheme(new DarkColors());
        }

        private static void ApplyTheme(ResourceDictionary themeDictionary)
        {
            if (Microsoft.Maui.Controls.Application.Current == null) return;

            // Replace merged dictionaries (simple swap)
            Microsoft.Maui.Controls.Application.Current.Resources.MergedDictionaries.Clear();
            Microsoft.Maui.Controls.Application.Current.Resources.MergedDictionaries.Add(themeDictionary);

            // OPTIONAL: if you want to also update Android system bars, call platform logic here
            // e.g., call a platform service to set Window.SetStatusBarColor/NavigationBarColor
        }
    }
}
