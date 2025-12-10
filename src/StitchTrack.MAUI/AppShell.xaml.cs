namespace StitchTrack.MAUI;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Register routes for navigation
        Routing.RegisterRoute(nameof(Views.QuickCounterPage), typeof(Views.QuickCounterPage));
        Routing.RegisterRoute(nameof(Views.ProjectsPage), typeof(Views.ProjectsPage));
        Routing.RegisterRoute(nameof(Views.SessionsPage), typeof(Views.SessionsPage));
        Routing.RegisterRoute(nameof(Views.ExportPage), typeof(Views.ExportPage));
        Routing.RegisterRoute(nameof(Views.SettingsPage), typeof(Views.SettingsPage));
    }
}
