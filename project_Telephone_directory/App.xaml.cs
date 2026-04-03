namespace project_Telephone_directory;

public partial class App : Application
{
    public static IServiceProvider Services { get; internal set; } = null!;

    public App()
    {
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new AppShell());
    }
}
