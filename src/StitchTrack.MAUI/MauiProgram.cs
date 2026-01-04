using CommunityToolkit.Maui;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StitchTrack.Application.Interfaces;
using StitchTrack.Application.ViewModels;
using StitchTrack.Domain.Interfaces;
using StitchTrack.Infrastructure.Data;
using StitchTrack.Infrastructure.Repositories;
using StitchTrack.MAUI.Controls;
using StitchTrack.MAUI.Data;
using StitchTrack.MAUI.Services;
using StitchTrack.MAUI.Views;

namespace StitchTrack.MAUI;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                // Default fonts
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");

                // Montserrat font family
                fonts.AddFont("Montserrat-Regular.ttf", "MontserratRegular");
                fonts.AddFont("Montserrat-Medium.ttf", "MontserratMedium");
                fonts.AddFont("Montserrat-SemiBold.ttf", "MontserratSemiBold");
                fonts.AddFont("Montserrat-Bold.ttf", "MontserratBold");
                fonts.AddFont("Montserrat-ExtraBold.ttf", "MontserratExtraBold");
            });

        // DATABASE
        System.Diagnostics.Debug.WriteLine($"📁 Database path: {DatabaseConfig.DatabasePath}");

        builder.Services.AddDbContext<AppDbContext>(options =>
        {
            options.UseSqlite(DatabaseConfig.ConnectionString);
        });

        // REPOSITORIES
        builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
        builder.Services.AddScoped<IAppSettingsRepository, AppSettingsRepository>();

        // SERVICES
        builder.Services.AddSingleton<IDialogService, MauiDialogService>();

        // VIEWMODELS
        builder.Services.AddTransient<QuickCounterViewModel>();
        builder.Services.AddTransient<ProjectsViewModel>();
        // TODO: Add other ViewModels as we create them
        // builder.Services.AddTransient<SessionViewModel>();
        // builder.Services.AddTransient<SettingsViewModel>();

        // PAGES
        builder.Services.AddTransient<QuickCounterPage>();
        builder.Services.AddTransient<ProjectsPage>();
        // TODO: Add other Pages as we create them
        // builder.Services.AddTransient<SessionPage>();
        // builder.Services.AddTransient<SettingsPage>();

        // POPUPS (Community Toolkit)
        builder.Services.AddTransient<OnboardingPopup>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        var app = builder.Build();

        Task.Run(async () =>
        {
            try
            {
                using var scope = app.Services.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                System.Diagnostics.Debug.WriteLine("🔄 Applying database migrations...");

                await dbContext.Database.MigrateAsync().ConfigureAwait(false);

                System.Diagnostics.Debug.WriteLine("✅ Database migrations applied successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Migration error: {ex.Message}");
            }
        });

        return app;
    }
}
