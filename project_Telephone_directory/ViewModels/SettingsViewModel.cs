using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.ApplicationModel.DataTransfer;
using Microsoft.Maui.Storage;
using project_Telephone_directory.Models;
using project_Telephone_directory.Services;

namespace project_Telephone_directory.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly IContactRepository _contacts;
    private readonly ISystemContactsService _system;
    private readonly IContactImportExportService _importExport;

    [ObservableProperty]
    private string statusMessage = string.Empty;

    [ObservableProperty]
    private bool isBusy;

    public SettingsViewModel(
        IContactRepository contacts,
        ISystemContactsService system,
        IContactImportExportService importExport)
    {
        _contacts = contacts;
        _system = system;
        _importExport = importExport;
    }

    [RelayCommand]
    private async Task RequestContactsPermissionAsync()
    {
        var granted = await _system.EnsureReadPermissionAsync().ConfigureAwait(true);
        StatusMessage = granted
            ? "Доступ к контактам разрешён."
            : "Доступ к контактам не получен.";
    }

    [RelayCommand]
    private async Task ImportFromDeviceAsync()
    {
        if (IsBusy)
            return;
        try
        {
            IsBusy = true;
            await _contacts.EnsureReadyAsync().ConfigureAwait(true);
            var ok = await _system.EnsureReadPermissionAsync().ConfigureAwait(true);
            if (!ok)
            {
                StatusMessage = "Нужно разрешение на чтение контактов.";
                return;
            }

            var rows = await _system.FetchAsync().ConfigureAwait(true);
            var models = rows
                .Where(r => !string.IsNullOrWhiteSpace(r.Name))
                .Select(r => new ContactDisplayModel
                {
                    Id = 0,
                    Name = r.Name.Trim(),
                    Phone = r.Phone ?? string.Empty,
                    Email = r.Email ?? string.Empty,
                    Notes = null,
                    Social = new Dictionary<string, string>(),
                    FromSystem = true
                })
                .ToList();

            var n = await _contacts.UpsertManyAsync(models).ConfigureAwait(true);
            StatusMessage = $"Импортировано контактов: {n}.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task ExportJsonAsync()
    {
        if (IsBusy)
            return;
        try
        {
            IsBusy = true;
            await _contacts.EnsureReadyAsync().ConfigureAwait(true);
            var all = await _contacts.GetAllAsync().ConfigureAwait(true);
            var json = await _importExport.ExportToJsonAsync(all).ConfigureAwait(true);
            var path = Path.Combine(FileSystem.CacheDirectory, "contacts_export.json");
            await File.WriteAllTextAsync(path, json).ConfigureAwait(false);
            await Share.Default.RequestAsync(new ShareFileRequest
            {
                Title = "Экспорт контактов",
                File = new ShareFile(path)
            }).ConfigureAwait(true);
            StatusMessage = "Файл экспорта подготовлен.";
        }
        catch (Exception ex)
        {
            StatusMessage = "Ошибка экспорта: " + ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task ImportJsonAsync()
    {
        if (IsBusy)
            return;
        try
        {
            IsBusy = true;
            var result = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "Выберите JSON"
            }).ConfigureAwait(true);

            if (result == null)
            {
                StatusMessage = "Импорт отменён.";
                return;
            }

            await using var stream = await result.OpenReadAsync().ConfigureAwait(true);
            using var reader = new StreamReader(stream);
            var json = await reader.ReadToEndAsync().ConfigureAwait(true);
            var imported = await _importExport.ImportFromJsonAsync(json).ConfigureAwait(true);
            await _contacts.EnsureReadyAsync().ConfigureAwait(true);
            var n = await _contacts.UpsertManyAsync(imported).ConfigureAwait(true);
            StatusMessage = $"Импортировано из файла: {n}.";
        }
        catch (Exception ex)
        {
            StatusMessage = "Ошибка импорта: " + ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }
}
