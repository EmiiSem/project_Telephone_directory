using Microsoft.Extensions.DependencyInjection;
using project_Telephone_directory.Controls;
using project_Telephone_directory.ViewModels;

namespace project_Telephone_directory;

public partial class ContactsListPage : ContentPage
{
    private bool _wired;

    public ContactsListPage()
    {
        InitializeComponent();
    }

    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();
        if (_wired || Handler?.MauiContext?.Services is not { } sp)
            return;

        _wired = true;
        BindingContext = sp.GetRequiredService<ContactsListViewModel>();
        ShaderBg.Profile = ShaderProfile.ContactSoft;
        SketchfabHeader.Source = new HtmlWebViewSource
        {
            Html = SketchfabConstants.BuildEmbedHtml(SketchfabConstants.Magnifier, 130)
        };
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is ContactsListViewModel vm)
            vm.LoadCommand.Execute(null);
    }
}
