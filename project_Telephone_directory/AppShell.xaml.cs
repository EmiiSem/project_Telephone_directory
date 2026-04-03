namespace project_Telephone_directory;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute(nameof(ContactDetailPage), typeof(ContactDetailPage));
        Routing.RegisterRoute(nameof(ContactEditPage), typeof(ContactEditPage));
    }
}
