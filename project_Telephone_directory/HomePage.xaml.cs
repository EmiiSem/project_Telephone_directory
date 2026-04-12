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
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        SimpleThreeJsWebViewHelper.LoadPrimitive(SketchfabView, SimpleThreeJsWebViewHelper.PrimitiveKind.Cube, 200);
        if (BindingContext is HomeViewModel vm)
            vm.LoadCommand.Execute(null);
    }

    protected override void OnDisappearing()
    {
        SimpleThreeJsWebViewHelper.Clear(SketchfabView);
        base.OnDisappearing();
    }
}
