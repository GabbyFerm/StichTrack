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
        builder.Services.AddScoped<IPatternFileRepository, PatternFileRepository>();
        builder.Services.AddScoped<IAppSettingsRepository, AppSettingsRepository>();
        builder.Services.AddScoped<ISessionRepository, SessionRepository>();

        // SERVICES
        builder.Services.AddSingleton<IDialogService, MauiDialogService>();
        builder.Services.AddSingleton<INavigationService, NavigationService>();
        builder.Services.AddSingleton<IHapticsService, MauiHapticsService>();
        builder.Services.AddSingleton<IExportService, MauiExportService>();

        // VIEWMODELS
        builder.Services.AddTransient<QuickCounterViewModel>();
        builder.Services.AddTransient<ProjectsViewModel>();
        builder.Services.AddTransient<SingleProjectViewModel>();
        builder.Services.AddTransient<ProjectCounterViewModel>();
        builder.Services.AddTransient<SessionsViewModel>();
        builder.Services.AddTransient<SettingsViewModel>();
        builder.Services.AddTransient<ExportViewModel>();

        // PAGES
        builder.Services.AddTransient<QuickCounterPage>();
        builder.Services.AddTransient<ProjectsPage>();
        builder.Services.AddTransient<SingleProjectPage>();
        builder.Services.AddTransient<ProjectCounterPage>();
        builder.Services.AddTransient<SessionsPage>();
        builder.Services.AddTransient<SettingsPage>();
        builder.Services.AddTransient<ExportPage>();

        // POPUPS (Community Toolkit)
        builder.Services.AddTransient<OnboardingPopup>();
        builder.Services.AddTransient<ProjectFormPopup>();
        builder.Services.AddTransient<ProjectMenuPopup>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        var app = builder.Build();

        // Remove Android Material underline from all Entry and Editor controls
        Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping(  // ← add this block
            "NoUnderline", (handler, view) =>
            {
#if ANDROID
                handler.PlatformView.BackgroundTintList =
                    Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.Transparent);
#endif
            });

        Microsoft.Maui.Handlers.EditorHandler.Mapper.AppendToMapping(  // ← already there
            "NoUnderline", (handler, view) =>
            {
#if ANDROID
                handler.PlatformView.BackgroundTintList =
                    Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.Transparent);
#endif
            });

        // Remove Android Material underline from all Entry and Editor controls
        Microsoft.Maui.Handlers.EditorHandler.Mapper.AppendToMapping(
    "NoUnderline", (handler, view) =>
    {
#if ANDROID
        handler.PlatformView.BackgroundTintList =
            Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.Transparent);
#endif
    });
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
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
#pragma warning restore CA1031
            {
                System.Diagnostics.Debug.WriteLine($"❌ Migration error: {ex.Message}");
            }
        });

        return app;
    }
}
