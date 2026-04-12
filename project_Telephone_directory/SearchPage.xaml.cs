using Microsoft.Extensions.DependencyInjection;
using project_Telephone_directory.Controls;
using project_Telephone_directory.ViewModels;

namespace project_Telephone_directory;

public partial class SearchPage : ContentPage
{
    private bool _wired;

    public SearchPage()
    {
        InitializeComponent();
    }

    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();
        if (_wired || Handler?.MauiContext?.Services is not { } sp)
            return;

        _wired = true;
        BindingContext = sp.GetRequiredService<SearchViewModel>();
        ShaderBg.Profile = ShaderProfile.SearchReactive;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        SimpleThreeJsWebViewHelper.LoadPrimitive(SketchfabSearch, SimpleThreeJsWebViewHelper.PrimitiveKind.Torus, 140);
        if (BindingContext is SearchViewModel vm)
            vm.LoadCommand.Execute(null);
    }

    protected override void OnDisappearing()
    {
        SimpleThreeJsWebViewHelper.Clear(SketchfabSearch);
        base.OnDisappearing();
    }
}
