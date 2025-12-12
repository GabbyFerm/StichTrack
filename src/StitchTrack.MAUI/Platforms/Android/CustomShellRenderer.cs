// This file is intentionally excluded from the build via preprocessor.
// It preserves the previous custom renderer code so you can re-enable or reference it later.
// To re-enable, remove the #if false / #endif block.
#if false


using Android.Content;
using Android.Graphics.Drawables;
using Google.Android.Material.BottomNavigation;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Controls.Platform.Compatibility;
using System.Diagnostics;

namespace StitchTrack.MAUI.Platforms.Android
{
    /// <summary>
    /// Theme-aware BottomNavigationView appearance tracker.
    /// Uses only fully-qualified global:: types and string-based theme checks to avoid SDK/version symbol issues.
    /// Reads PageBackgroundHex / PageBackgroundDarkHex from Application resources and reapplies on RequestedThemeChanged.
    /// </summary>
    public class CustomShellRenderer : ShellRenderer
    {
        public CustomShellRenderer(Context context) : base(context) { }

        protected override IShellBottomNavViewAppearanceTracker CreateBottomNavViewAppearanceTracker(Microsoft.Maui.Controls.ShellItem shellItem)
            => new ThemeAwareBottomNavAppearanceTracker(this, shellItem);
    }

    public class ThemeAwareBottomNavAppearanceTracker : ShellBottomNavViewAppearanceTracker
    {
        private static readonly List<BottomNavigationView> _instances = new();
        private static bool _subscribedThemeChanged = false;

        public ThemeAwareBottomNavAppearanceTracker(IShellContext shellContext, Microsoft.Maui.Controls.ShellItem shellItem)
            : base(shellContext, shellItem)
        {
            // Subscribe once to RequestedThemeChanged so we can reapply background color on theme toggle
            if (!_subscribedThemeChanged)
            {
                _subscribedThemeChanged = true;
                try
                {
                    var app = global::Microsoft.Maui.Controls.Application.Current;
                    if (app != null)
                    {
                        app.RequestedThemeChanged += (s, e) =>
                        {
                            try
                            {
                                foreach (var bv in _instances.ToArray())
                                {
                                    if (bv != null)
                                        ApplyBackgroundFromResources(bv);
                                }
                            }
                            catch { }
                        };
                    }
                }
                catch { }
            }
        }

        public override void SetAppearance(BottomNavigationView bottomView, IShellAppearanceElement appearance)
        {
            if (bottomView == null) throw new ArgumentNullException(nameof(bottomView));
            base.SetAppearance(bottomView, appearance);

            // Track instances so we can update on theme change
            if (!_instances.Contains(bottomView))
                _instances.Add(bottomView);

            // Aggressive clearing (many fallbacks) to avoid white backgrounds
            Try(() => bottomView.SetBackgroundColor(global::Android.Graphics.Color.Transparent));
            Try(() => bottomView.Background = null);
            Try(() => bottomView.BackgroundTintList = null);
            Try(() => bottomView.SetBackgroundResource(0));
            Try(() => bottomView.ItemBackground = new global::Android.Graphics.Drawables.ColorDrawable(global::Android.Graphics.Color.Transparent));
            Try(() => bottomView.ItemBackground = null);

            // zero elevation so content shadows can overlap if card elevation is raised
            Try(() => bottomView.Elevation = 0f);
            Try(() => bottomView.TranslationZ = 0f);
            Try(() => bottomView.SetZ(0f));
            Try(() => bottomView.LabelVisibilityMode = 1); // labeled

            // Reapply text/icon tint so visuals remain expected
            Try(() =>
            {
                int unselectedColor = global::Android.Graphics.Color.White.ToArgb();
                int selectedColor = global::Android.Graphics.Color.ParseColor("#3D4A54");
                var states = new int[][]
                {
                    new int[] { global::Android.Resource.Attribute.StateChecked },
                    Array.Empty<int>()
                };
                var colors = new int[] { selectedColor, unselectedColor };
                var colorStateList = new global::Android.Content.Res.ColorStateList(states, colors);
                bottomView.ItemTextColor = colorStateList;
                bottomView.ItemIconTintList = colorStateList;
            });

            // Apply nav background from app resources (theme-aware)
            ApplyBackgroundFromResources(bottomView);

            // Post a second pass to handle drawables/framework-applied drawables that run after layout
            Try(() =>
            {
                bottomView.Post(() =>
                {
                    try
                    {
                        Try(() => bottomView.Background = null);
                        ApplyBackgroundFromResources(bottomView);
                    }
                    catch { }
                });
            });
        }

        private static void ApplyBackgroundFromResources(BottomNavigationView bottomView)
        {
            try
            {
                var app = global::Microsoft.Maui.Controls.Application.Current;
                string hex = null;

                // Avoid referencing AppTheme enums — use string representation of RequestedTheme.
                // RequestedTheme may be an enum on some SDKs; calling ToString() and checking for "Dark" is robust.
                string themeStr = app.RequestedTheme.ToString().ToLowerInvariant();

                if (!string.IsNullOrEmpty(themeStr) && themeStr.Contains("dark"))
                {
                    if (app?.Resources?.ContainsKey("PageBackgroundDark") == true)
                        hex = app.Resources["PageBackgroundDark"]?.ToString();
                }

                // Fallback to light key or PageBackgroundHex
                if (string.IsNullOrWhiteSpace(hex) && app?.Resources?.ContainsKey("PageBackground") == true)
                    hex = app.Resources["PageBackground"]?.ToString();

                // Final fallback value
                if (string.IsNullOrWhiteSpace(hex))
                    hex = "#D4A847";

                // Parse and apply color
                var androidColor = global::Android.Graphics.Color.ParseColor(hex);
                Try(() => bottomView.SetBackgroundColor(androidColor));
                Try(() => bottomView.Background = new ColorDrawable(androidColor));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ApplyBackgroundFromResources ERROR: {ex.Message}");
            }
        }

        private static void Try(Action a) { try { a(); } catch { } }
    }
}
#endif
