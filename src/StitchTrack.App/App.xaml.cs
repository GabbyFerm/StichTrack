namespace StitchTrack.App;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new AppShell());

<<<<<<< TODO: Unmerged change from project 'StitchTrack.App(net9.0-ios)', Before:
  }
=======
    }
>>>>>>> After
    }
}
