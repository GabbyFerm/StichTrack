namespace StitchTrack.App;

public partial class MainPage : ContentPage
{
    private int count;

    public MainPage()
    {
        InitializeComponent();
    }

    private void OnCounterClicked(object sender, EventArgs e)
    {
        count++;

        if (count == 1)
        {
            CounterBtn.Text = $"Clicked {count} time";
        }
        else
        {
            CounterBtn.Text = $"Clicked {count} times";
        }

        SemanticScreenReader.Announce(CounterBtn.Text);

<<<<<<< TODO: Unmerged change from project 'StitchTrack.App(net9.0-ios)', Before:
    }
=======
    }
>>>>>>> After
}
}
