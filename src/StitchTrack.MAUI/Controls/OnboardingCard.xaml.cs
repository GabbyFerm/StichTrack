namespace StitchTrack.MAUI.Controls;

/// <summary>
/// Onboarding card shown on first app launch.
/// Commands are bound to the parent page's ViewModel (QuickCounterViewModel).
/// </summary>
public partial class OnboardingCard : ContentView
{
    public OnboardingCard()
    {
        InitializeComponent();

        // When the control is loaded, inherit the parent page's BindingContext
        Loaded += OnLoaded;
    }

    private void OnLoaded(object? sender, EventArgs e)
    {
        // Find the parent ContentPage and use its BindingContext
        // This gives us access to the QuickCounterViewModel's commands
        var parentPage = this.GetParentOfType<ContentPage>();
        if (parentPage != null)
        {
            BindingContext = parentPage.BindingContext;
            System.Diagnostics.Debug.WriteLine("✅ OnboardingCard: BindingContext set to parent page's ViewModel");
        }
        else
        {
            System.Diagnostics.Debug.WriteLine("⚠️ OnboardingCard: Could not find parent ContentPage");
        }
    }
}

/// <summary>
/// Extension method to walk up the visual tree and find a parent of a specific type.
/// </summary>
public static class VisualTreeExtensions
{
    public static T? GetParentOfType<T>(this Element element) where T : Element
    {
        ArgumentNullException.ThrowIfNull(element);

        var parent = element.Parent;
        while (parent != null)
        {
            if (parent is T typedParent)
            {
                return typedParent;
            }
            parent = parent.Parent;
        }
        return null;
    }
}
