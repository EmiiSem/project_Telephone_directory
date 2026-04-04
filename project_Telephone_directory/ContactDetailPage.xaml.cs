using Microsoft.Extensions.DependencyInjection;
using project_Telephone_directory.Controls;
using project_Telephone_directory.ViewModels;

namespace project_Telephone_directory;

public partial class ContactDetailPage : ContentPage
{
    private bool _wired;

    public ContactDetailPage()
    {
        InitializeComponent();
    }

    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();
        if (_wired || Handler?.MauiContext?.Services is not { } sp)
            return;

        _wired = true;
        BindingContext = sp.GetRequiredService<ContactDetailViewModel>();
        ShaderBg.Profile = ShaderProfile.ContactSoft;
        SketchfabWebViewHelper.LoadModel(Phone3d, SketchfabConstants.ContactPhone, 120);
        SketchfabWebViewHelper.LoadModel(Mail3d, SketchfabConstants.Envelope, 120);
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is ContactDetailViewModel vm)
            vm.RefreshContact();
    }
}
