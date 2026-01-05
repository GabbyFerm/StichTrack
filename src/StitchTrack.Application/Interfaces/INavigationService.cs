namespace StitchTrack.Application.Interfaces;

/// <summary>
/// Service for handling navigation between pages. 
/// </summary>
public interface INavigationService
{
    Task NavigateToAsync(string route);
    Task NavigateToAsync(string route, IDictionary<string, object> parameters);
    Task GoBackAsync();
}
