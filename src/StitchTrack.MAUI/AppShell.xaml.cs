using StitchTrack.MAUI.Views;

namespace StitchTrack.MAUI;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Register child/modal pages only
        Routing.RegisterRoute("SingleProjectPage", typeof(SingleProjectPage));

        // Handle tab navigation
        Navigating += OnShellNavigating;
    }

    private async void OnShellNavigating(object? sender, ShellNavigatingEventArgs e)
    {
        var target = e.Target.Location.OriginalString;

        // Prevent Shell from restoring cached child pages when switching tabs
        if (target.Contains("//projects", StringComparison.OrdinalIgnoreCase) &&
            target.Contains("SingleProjectPage", StringComparison.OrdinalIgnoreCase))
        {
            System.Diagnostics.Debug.WriteLine("⛔ Blocked navigation to cached child page");
            e.Cancel();
            await Shell.Current.GoToAsync("//projects", false).ConfigureAwait(true);
        }
    }
}
