using Microsoft.Extensions.DependencyInjection;
using project_Telephone_directory.Controls;
using project_Telephone_directory.ViewModels;

namespace project_Telephone_directory;

public partial class HomePage : ContentPage
{
    private bool _wired;

    public HomePage()
    {
        InitializeComponent();
    }

    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();
        if (_wired || Handler?.MauiContext?.Services is not { } sp)
            return;

        _wired = true;
        BindingContext = sp.GetRequiredService<HomeViewModel>();
        ShaderBg.Profile = ShaderProfile.MainGradient;
        SketchfabWebViewHelper.LoadModel(SketchfabView, SketchfabConstants.PhoneOrBook, 200);
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is HomeViewModel vm)
            vm.LoadCommand.Execute(null);
    }
}
