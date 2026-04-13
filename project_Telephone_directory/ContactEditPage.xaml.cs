using Microsoft.Extensions.DependencyInjection;
using project_Telephone_directory.Controls;
using project_Telephone_directory.ViewModels;

namespace project_Telephone_directory;

[QueryProperty(nameof(QueryContactId), "ContactId")]
public partial class ContactEditPage : ContentPage
{
    private readonly SemaphoreSlim _attachLock = new(1, 1);

    private string? _queryContactId;
    private ContactEditViewModel? _vm;
    private int _serviceResolveAttempts;

    public ContactEditPage()
    {
        InitializeComponent();
        ShaderBg.Profile = ShaderProfile.AddFormDynamic;
        ShaderBg.ValidationStrength = 0;

        BtnPickAvatar.Clicked += async (_, _) =>
        {
            await EnsureViewModelAttachedAsync().ConfigureAwait(true);
            if (_vm != null)
                await _vm.PickAvatarAsync().ConfigureAwait(true);
        };
        BtnClearAvatar.Clicked += async (_, _) =>
        {
            await EnsureViewModelAttachedAsync().ConfigureAwait(true);
            _vm?.ClearAvatar();
        };
        BtnSave.Clicked += async (_, _) =>
        {
            await EnsureViewModelAttachedAsync().ConfigureAwait(true);
            await Task.Yield();
            CommitPendingInputToVm();
            if (_vm != null)
                await _vm.SaveAsync().ConfigureAwait(true);
        };

        EntryName.TextChanged += (_, e) =>
        {
            var wasEmpty = string.IsNullOrWhiteSpace(e.OldTextValue);
            var nowEmpty = string.IsNullOrWhiteSpace(e.NewTextValue);
            if (wasEmpty != nowEmpty)
                _vm?.NotifyValidation(!nowEmpty);
        };
    }

    public string? QueryContactId
    {
        get => _queryContactId;
        set
        {
            _queryContactId = value;
            if (_vm != null)
                ApplyRouteToViewModel();
        }
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _serviceResolveAttempts = 0;
        _ = AttachViewModelForPageAsync();
    }

    /// <summary>Не отписываемся в OnDisappearing: при выборе фото страница может «пропасть»,
    /// иначе обработчики срываются и UI не обновляется. Полный сброс — в OnAppearing.</summary>

    private IServiceProvider? ResolveServices()
    {
        if (Handler?.MauiContext?.Services is { } pageSp)
            return pageSp;
        if (Shell.Current?.Handler?.MauiContext?.Services is { } shellSp)
            return shellSp;
        if (Application.Current?.Handler?.MauiContext?.Services is { } appSp)
            return appSp;
        return null;
    }

    private async Task EnsureViewModelAttachedAsync()
    {
        if (_vm != null)
            return;
        await AttachViewModelForPageAsync().ConfigureAwait(true);
    }

    private async Task AttachViewModelForPageAsync()
    {
        await _attachLock.WaitAsync().ConfigureAwait(true);
        try
        {
            IServiceProvider? sp = null;
            while (sp == null && _serviceResolveAttempts < 60)
            {
                _serviceResolveAttempts++;
                sp = ResolveServices();
                if (sp == null)
                    await Task.Delay(16).ConfigureAwait(true);
            }

            if (sp == null)
                return;

            DetachViewModel();

            _vm = sp.GetRequiredService<ContactEditViewModel>();
            _vm.ValidationChanged += OnValidationChanged;
            _vm.AvatarChanged += OnAvatarChanged;
            ApplyRouteToViewModel();
            SyncUiFromVm();
        }
        finally
        {
            _attachLock.Release();
        }
    }

    private void DetachViewModel()
    {
        if (_vm == null)
            return;
        _vm.ValidationChanged -= OnValidationChanged;
        _vm.AvatarChanged -= OnAvatarChanged;
        _vm = null;
    }

    private void OnValidationChanged(bool isValid)
    {
        ShaderBg.ValidationStrength = isValid ? 1f : 0f;
    }

    private void OnAvatarChanged()
    {
        AvatarImage.Source = _vm?.AvatarPreviewSource;
        AvatarPlaceholder.IsVisible = _vm?.AvatarPreviewSource is null;
    }

    private void SyncUiFromVm()
    {
        if (_vm == null) return;
        EntryName.Text = _vm.Name;
        EntryPhone.Text = _vm.Phone;
        EntryEmail.Text = _vm.Email;
        EditorNotes.Text = _vm.Notes;
        OnAvatarChanged();
        OnValidationChanged(!string.IsNullOrWhiteSpace(_vm.Name));
    }

    private void ApplyRouteToViewModel()
    {
        if (_vm == null)
            return;

        var fromUri = ParseQueryFromShellLocation();
        if (fromUri.TryGetValue("ContactId", out var idObj) && idObj is string s && !string.IsNullOrWhiteSpace(s))
        {
            _vm.ApplyQueryAttributes(fromUri, SyncUiFromVm);
            return;
        }

        if (!string.IsNullOrWhiteSpace(_queryContactId))
        {
            _vm.ApplyQueryAttributes(
                new Dictionary<string, object> { ["ContactId"] = _queryContactId },
                SyncUiFromVm);
            return;
        }

        _vm.ApplyQueryAttributes(new Dictionary<string, object>(), SyncUiFromVm);
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

    private void CommitPendingInputToVm()
    {
        if (_vm == null) return;
        _vm.Name = EntryName.Text ?? string.Empty;
        _vm.Phone = EntryPhone.Text ?? string.Empty;
        _vm.Email = EntryEmail.Text ?? string.Empty;
        _vm.Notes = EditorNotes.Text;
    }
}
