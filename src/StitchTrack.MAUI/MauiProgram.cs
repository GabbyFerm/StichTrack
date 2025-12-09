using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StitchTrack.Application.ViewModels;
using StitchTrack.Domain.Interfaces;
using StitchTrack.Infrastructure.Data;
using StitchTrack.Infrastructure.Repositories;
using StitchTrack.MAUI.Data;

namespace StitchTrack.MAUI;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");

                // Montserrat
                fonts.AddFont("Montserrat-Medium.ttf", "MontserratMedium");
                fonts.AddFont("Montserrat-Regular.ttf", "MontserratRegular");
                fonts.AddFont("Montserrat-SemiBold.ttf", "MontserratSemiBold");
                fonts.AddFont("Montserrat-ExtraBold.ttf", "MontserratExtraBold");
                fonts.AddFont("Montserrat-Bold.ttf", "MontserratBold");
            });

        // Register database context
        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(DatabaseConfig.ConnectionString));

        // Register repositories
        builder.Services.AddScoped<IAppSettingsRepository, AppSettingsRepository>();

        // Register ViewModels
        builder.Services.AddTransient<QuickCounterViewModel>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
