using Microsoft.UI.Xaml;
using Microsoft.UI.Dispatching;

namespace FluentGit;

public partial class App : Application
{
    private Window? _window;
    public static MainWindow? MainWindow { get; private set; }

    public App()
    {
        InitializeComponent();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        _window = new MainWindow();
        MainWindow = _window as MainWindow;
        _window.Activate();
    }
}