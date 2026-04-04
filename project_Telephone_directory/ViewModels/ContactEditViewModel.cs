using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Media;
using project_Telephone_directory.Models;
using project_Telephone_directory.Services;

namespace project_Telephone_directory.ViewModels;

public partial class ContactEditViewModel : ObservableObject
{
    private readonly IContactRepository _contacts;
    private int _editingId;
    private string? _pendingAvatarTempPath;
    private bool _clearAvatarRequested;

    [ObservableProperty]
    private string name = string.Empty;

    [ObservableProperty]
    private string phone = string.Empty;

    [ObservableProperty]
    private string email = string.Empty;

    [ObservableProperty]
    private string? notes;

    [ObservableProperty]
    private double validationStrength = 1;

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private ImageSource? avatarPreview;

    public ContactEditViewModel(IContactRepository contacts)
    {
        _contacts = contacts;
    }

    public bool ShowAvatarPlaceholder => AvatarPreview is null;

    partial void OnAvatarPreviewChanged(ImageSource? value) => OnPropertyChanged(nameof(ShowAvatarPlaceholder));

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("ContactId", out var raw) && raw is not null)
        {
            var s = raw.ToString();
            if (!string.IsNullOrEmpty(s) && int.TryParse(s, out var id))
                _ = LoadAsync(id);
            else
                Clear();
        }
        else
        {
            Clear();
        }
    }

    partial void OnNameChanged(string value) => UpdateValidation();

    partial void OnPhoneChanged(string value) => UpdateValidation();

    partial void OnEmailChanged(string value) => UpdateValidation();

    private void Clear()
    {
        _editingId = 0;
        Name = string.Empty;
        Phone = string.Empty;
        Email = string.Empty;
        Notes = null;
        ResetAvatarState();
        UpdateValidation();
    }

    private void ResetAvatarState()
    {
        _pendingAvatarTempPath = null;
        _clearAvatarRequested = false;
        AvatarPreview = null;
    }

    private void UpdateValidation()
    {
        var ok = !string.IsNullOrWhiteSpace(Name);
        ValidationStrength = ok ? 1 : 0;
    }

    private async Task LoadAsync(int id)
    {
        if (IsBusy)
            return;
        try
        {
            IsBusy = true;
            await _contacts.EnsureReadyAsync().ConfigureAwait(true);
            var c = await _contacts.GetByIdAsync(id).ConfigureAwait(true);
            if (c == null)
            {
                Clear();
                return;
            }

            _editingId = c.Id;
            Name = c.Name;
            Phone = c.Phone;
            Email = c.Email;
            Notes = c.Notes;
            _pendingAvatarTempPath = null;
            _clearAvatarRequested = false;
            AvatarPreview = string.IsNullOrEmpty(c.AvatarLocalPath) ? null : ImageSource.FromFile(c.AvatarLocalPath);
            UpdateValidation();
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task PickAvatarAsync()
    {
        try
        {
            var photos = await MediaPicker.Default.PickPhotosAsync(new MediaPickerOptions { Title = "Фото контакта" }).ConfigureAwait(true);
            var photo = photos?.FirstOrDefault();
            if (photo == null)
                return;

            _clearAvatarRequested = false;
            var ext = Path.GetExtension(photo.FileName);
            if (string.IsNullOrEmpty(ext))
                ext = ".jpg";
            var cachePath = Path.Combine(FileSystem.CacheDirectory, "avatar_pick_" + Guid.NewGuid().ToString("N") + ext);
            await using (var read = await photo.OpenReadAsync().ConfigureAwait(true))
            await using (var write = File.Create(cachePath))
                await read.CopyToAsync(write).ConfigureAwait(true);

            TryDeleteFile(_pendingAvatarTempPath);
            _pendingAvatarTempPath = cachePath;
            AvatarPreview = ImageSource.FromFile(cachePath);
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Фото", "Не удалось выбрать изображение: " + ex.Message, "OK").ConfigureAwait(true);
        }
    }

    [RelayCommand]
    private void ClearAvatar()
    {
        TryDeleteFile(_pendingAvatarTempPath);
        _pendingAvatarTempPath = null;
        _clearAvatarRequested = true;
        AvatarPreview = null;
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            await Shell.Current.DisplayAlertAsync("Проверка", "Укажите имя контакта.", "OK").ConfigureAwait(true);
            return;
        }

        if (IsBusy)
            return;
        try
        {
            IsBusy = true;
            await _contacts.EnsureReadyAsync().ConfigureAwait(true);
            var model = new ContactDisplayModel
            {
                Id = _editingId,
                Name = Name.Trim(),
                Phone = Phone.Trim(),
                Email = Email.Trim(),
                Notes = Notes,
                Social = new Dictionary<string, string>(),
                FromSystem = false
            };

            int contactId;
            if (_editingId == 0)
                contactId = await _contacts.InsertAsync(model).ConfigureAwait(true);
            else
            {
                contactId = _editingId;
                await _contacts.UpdateAsync(model).ConfigureAwait(true);
            }

            if (_clearAvatarRequested)
                await _contacts.ClearAvatarAsync(contactId).ConfigureAwait(true);
            else if (!string.IsNullOrEmpty(_pendingAvatarTempPath))
                await _contacts.SetAvatarFromFileAsync(contactId, _pendingAvatarTempPath).ConfigureAwait(true);

            TryDeleteFile(_pendingAvatarTempPath);
            _pendingAvatarTempPath = null;
            _clearAvatarRequested = false;

            await Shell.Current.GoToAsync("..").ConfigureAwait(true);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private static void TryDeleteFile(string? path)
    {
        if (string.IsNullOrEmpty(path))
            return;
        try
        {
            if (File.Exists(path))
                File.Delete(path);
        }
        catch
        {
            // ignore
        }
    }
}
