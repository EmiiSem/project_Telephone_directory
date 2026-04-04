using Microsoft.Extensions.DependencyInjection;
using project_Telephone_directory.Controls;
using project_Telephone_directory.ViewModels;

namespace project_Telephone_directory;

[QueryProperty(nameof(QueryContactId), "ContactId")]
public partial class ContactEditPage : ContentPage
{
    private bool _wired;
    private string? _queryContactId;

    public ContactEditPage()
    {
        InitializeComponent();
    }

    /// <summary>Заполняется Shell при маршруте ...?ContactId= (может сработать до появления BindingContext).</summary>
    public string? QueryContactId
    {
        get => _queryContactId;
        set
        {
            _queryContactId = value;
            ApplyRouteToViewModel();
        }
    }

    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();
        if (_wired || Handler?.MauiContext?.Services is not { } sp)
            return;

        _wired = true;
        BindingContext = sp.GetRequiredService<ContactEditViewModel>();
        ShaderBg.Profile = ShaderProfile.AddFormDynamic;
        ApplyRouteToViewModel();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        ApplyRouteToViewModel();
    }

    private void ApplyRouteToViewModel()
    {
        if (BindingContext is not ContactEditViewModel vm)
            return;

        var fromUri = ParseQueryFromShellLocation();
        if (fromUri.TryGetValue("ContactId", out var idObj) && idObj is string s && !string.IsNullOrWhiteSpace(s))
        {
            vm.ApplyQueryAttributes(fromUri);
            return;
        }

        if (!string.IsNullOrWhiteSpace(_queryContactId))
        {
            vm.ApplyQueryAttributes(new Dictionary<string, object> { ["ContactId"] = _queryContactId });
            return;
        }

        vm.ApplyQueryAttributes(new Dictionary<string, object>());
    }

    /// <summary>
    /// На Windows у <see cref="Uri.Query"/> часто пусто при Shell-навигации; параметры остаются в OriginalString.
    /// </summary>
    private static Dictionary<string, object> ParseQueryFromShellLocation()
    {
        var dict = new Dictionary<string, object>();
        var loc = Shell.Current?.CurrentState?.Location;
        if (loc == null)
            return dict;

        var query = loc.Query;
        if (string.IsNullOrEmpty(query))
        {
            var full = loc.OriginalString ?? string.Empty;
            var q = full.IndexOf('?', StringComparison.Ordinal);
            if (q >= 0 && q < full.Length - 1)
                query = full[(q + 1)..];
        }

        if (string.IsNullOrEmpty(query))
            return dict;

        query = query.TrimStart('?');
        foreach (var pair in query.Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var kv = pair.Split('=', 2);
            if (kv.Length != 2)
                continue;
            dict[Uri.UnescapeDataString(kv[0])] = Uri.UnescapeDataString(kv[1]);
        }

        return dict;
    }
}
