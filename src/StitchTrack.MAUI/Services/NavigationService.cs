using StitchTrack.Application.Interfaces;

namespace StitchTrack.MAUI.Services;

public class NavigationService : INavigationService
{
    public Task NavigateToAsync(string route)
    {
        return Shell.Current.GoToAsync(route);
    }

    public Task NavigateToAsync(string route, IDictionary<string, object> parameters)
    {
        return Shell.Current.GoToAsync(route, parameters);
    }

    public async Task GoBackAsync()
    {
        await MainThread.InvokeOnMainThreadAsync(async () =>
            await Shell.Current.GoToAsync("..")
        );
    }
}
