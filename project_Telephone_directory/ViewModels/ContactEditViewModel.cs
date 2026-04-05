using Microsoft.Maui.Storage;
using project_Telephone_directory.Models;
using project_Telephone_directory.Services;

namespace project_Telephone_directory.ViewModels;

/// <summary>
/// VM формы добавления/редактирования контакта.
/// Не использует INotifyPropertyChanged/ObservableProperty для текстовых полей —
/// значения читаются напрямую из Entry/Editor перед сохранением (FlushFieldsToVm),
/// чтобы исключить цепочку Binding → PropertyChanged → layout pass на каждый символ.
/// </summary>
public sealed class ContactEditViewModel
{
    private readonly IContactRepository _contacts;
    private int _editingId;
    private string? _pendingAvatarTempPath;
    private bool _clearAvatarRequested;

    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public ImageSource? AvatarPreviewSource { get; private set; }

    /// <summary>Срабатывает при смене валидации (имя пустое / непустое). bool = isValid.</summary>
    public event Action<bool>? ValidationChanged;

    public void NotifyValidation(bool isValid) => ValidationChanged?.Invoke(isValid);

    /// <summary>Срабатывает при смене превью аватара.</summary>
    public event Action? AvatarChanged;

    public ContactEditViewModel(IContactRepository contacts)
    {
        _contacts = contacts;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query, Action? onLoaded = null)
    {
        if (query.TryGetValue("ContactId", out var raw) && raw is not null)
        {
            var s = raw.ToString();
            if (!string.IsNullOrEmpty(s) && int.TryParse(s, out var id))
            {
                _ = LoadAsync(id, onLoaded);
                return;
            }
        }

        Clear();
        onLoaded?.Invoke();
    }

    private void Clear()
    {
        _editingId = 0;
        Name = string.Empty;
        Phone = string.Empty;
        Email = string.Empty;
        Notes = null;
        _pendingAvatarTempPath = null;
        _clearAvatarRequested = false;
        AvatarPreviewSource = null;
        AvatarChanged?.Invoke();
        ValidationChanged?.Invoke(false);
    }

    private async Task LoadAsync(int id, Action? onLoaded)
    {
        try
        {
            await _contacts.EnsureReadyAsync().ConfigureAwait(true);
            var c = await _contacts.GetByIdAsync(id).ConfigureAwait(true);
            if (c == null)
            {
                Clear();
                onLoaded?.Invoke();
                return;
            }

            _editingId = c.Id;
            Name = c.Name;
            Phone = c.Phone;
            Email = c.Email;
            Notes = c.Notes;
            _pendingAvatarTempPath = null;
            _clearAvatarRequested = false;
            AvatarPreviewSource = TryLoadAvatarPreview(c.AvatarLocalPath);

            onLoaded?.Invoke();
            AvatarChanged?.Invoke();
            ValidationChanged?.Invoke(!string.IsNullOrWhiteSpace(Name));
        }
        catch
        {
            Clear();
            onLoaded?.Invoke();
        }
    }

    public async Task PickAvatarAsync()
    {
        try
        {
            // Единый путь: FilePicker стабильно работает на Windows/Android/iOS; MediaPicker.PickPhotosAsync на Windows часто пустой.
            var file = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "Фото контакта",
                FileTypes = FilePickerFileType.Images
            }).ConfigureAwait(true);

            if (file == null)
                return;

            _clearAvatarRequested = false;
            var ext = Path.GetExtension(file.FileName);
            if (string.IsNullOrEmpty(ext))
                ext = ".jpg";
            var cachePath = Path.Combine(
                FileSystem.CacheDirectory,
                "avatar_pick_" + Guid.NewGuid().ToString("N") + ext);

            await using (var read = await file.OpenReadAsync().ConfigureAwait(true))
            await using (var write = File.Create(cachePath))
                await read.CopyToAsync(write).ConfigureAwait(true);

            TryDeleteFile(_pendingAvatarTempPath);
            _pendingAvatarTempPath = cachePath;
            AvatarPreviewSource = ImageSource.FromFile(cachePath);
            AvatarChanged?.Invoke();
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync(
                "Фото", "Не удалось выбрать изображение: " + ex.Message, "OK").ConfigureAwait(true);
        }
    }

    public void ClearAvatar()
    {
        TryDeleteFile(_pendingAvatarTempPath);
        _pendingAvatarTempPath = null;
        _clearAvatarRequested = true;
        AvatarPreviewSource = null;
        AvatarChanged?.Invoke();
    }

    /// <summary>
    /// Перед вызовом страница должна вызвать FlushFieldsToVm(),
    /// чтобы записать текущие значения Entry/Editor в свойства VM.
    /// </summary>
    public async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            await Shell.Current.DisplayAlertAsync("Проверка", "Укажите имя контакта.", "OK").ConfigureAwait(true);
            return;
        }

        try
        {
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
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync(
                "Ошибка", "Не удалось сохранить: " + ex.Message, "OK").ConfigureAwait(true);
        }
    }

    private static ImageSource? TryLoadAvatarPreview(string? path)
    {
        if (string.IsNullOrEmpty(path)) return null;
        try { return File.Exists(path) ? ImageSource.FromFile(path) : null; }
        catch { return null; }
    }

    private static void TryDeleteFile(string? path)
    {
        if (string.IsNullOrEmpty(path)) return;
        try { if (File.Exists(path)) File.Delete(path); }
        catch { /* ignore */ }
    }
}
