using Microsoft.Extensions.DependencyInjection;
using project_Telephone_directory.Controls;
using project_Telephone_directory.ViewModels;

namespace project_Telephone_directory;

public partial class SettingsPage : ContentPage
{
    private bool _wired;

    public SettingsPage()
    {
        InitializeComponent();
    }

    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();
        if (_wired || Handler?.MauiContext?.Services is not { } sp)
            return;

        _wired = true;
        BindingContext = sp.GetRequiredService<SettingsViewModel>();
        ShaderBg.Profile = ShaderProfile.SettingsAmbient;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        SketchfabWebViewHelper.LoadModel(Gear3d, SketchfabConstants.Gear, 170);
    }

    protected override void OnDisappearing()
    {
        SketchfabWebViewHelper.Clear(Gear3d);
        base.OnDisappearing();
    }
}
