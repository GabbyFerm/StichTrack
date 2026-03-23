using StitchTrack.Infrastructure.Data;

namespace StitchTrack.MAUI;

public partial class App : Microsoft.Maui.Controls.Application
{
    public App(AppDbContext dbContext)
    {
        InitializeComponent();

        // Force light mode during development
        UserAppTheme = AppTheme.Light;

        MainPage = new AppShell();
    }
}
