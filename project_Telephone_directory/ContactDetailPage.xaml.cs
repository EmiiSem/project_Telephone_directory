using Microsoft.Extensions.DependencyInjection;
using project_Telephone_directory.Controls;
using project_Telephone_directory.ViewModels;

namespace project_Telephone_directory;

/// <summary>
/// Shell передаёт query до того, как у страницы есть BindingContext (OnHandlerChanged).
/// IQueryAttributable на VM тогда не срабатывает для этой VM — дублируем ContactId через QueryProperty
/// и повторно применяем маршрут после привязки VM (как на ContactEditPage).
/// </summary>
[QueryProperty(nameof(QueryContactId), "ContactId")]
public partial class ContactDetailPage : ContentPage
{
    private bool _wired;
    private string? _queryContactId;

    public ContactDetailPage()
    {
        InitializeComponent();
    }

    public string? QueryContactId
    {
        get => _queryContactId;
        set
        {
            _queryContactId = value;
            ApplyContactRouteToViewModel();
        }
    }

    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();
        if (_wired || Handler?.MauiContext?.Services is not { } sp)
            return;

        _wired = true;
        BindingContext = sp.GetRequiredService<ContactDetailViewModel>();
        ApplyContactRouteToViewModel();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        ShaderBg.Profile = ShaderProfile.ContactDetailAmbient;
        ApplyContactRouteToViewModel();
        SketchfabWebViewHelper.LoadModel(SketchHeader, SketchfabConstants.Envelope, 160);
        if (BindingContext is ContactDetailViewModel vm && vm.Contact != null)
            vm.RefreshContact();
    }

    protected override void OnDisappearing()
    {
        SketchfabWebViewHelper.Clear(SketchHeader);
        base.OnDisappearing();
    }

    private void ApplyContactRouteToViewModel()
    {
        if (BindingContext is not ContactDetailViewModel vm)
            return;

        var dict = new Dictionary<string, object>();
        if (!string.IsNullOrWhiteSpace(_queryContactId))
            dict["ContactId"] = _queryContactId;
        else
        {
            var fromUri = ParseQueryFromShellLocation();
            if (fromUri.TryGetValue("ContactId", out var idObj) && idObj is string s && !string.IsNullOrWhiteSpace(s))
                dict["ContactId"] = s;
        }

        if (dict.Count > 0)
            vm.ApplyQueryAttributes(dict);
    }

    private static Dictionary<string, object> ParseQueryFromShellLocation()
    {
        var dict = new Dictionary<string, object>();
        var loc = Shell.Current?.CurrentState?.Location;
        if (loc == null)
            return dict;

        string? query = null;
        if (loc.IsAbsoluteUri)
        {
            query = loc.Query;
            if (string.IsNullOrEmpty(query))
            {
                var full = loc.OriginalString ?? string.Empty;
                var q = full.IndexOf('?', StringComparison.Ordinal);
                if (q >= 0 && q < full.Length - 1)
                    query = full[(q + 1)..];
            }
        }
        else
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
