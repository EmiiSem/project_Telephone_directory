using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using project_Telephone_directory.Models;
using project_Telephone_directory.Services;

namespace project_Telephone_directory.ViewModels;

public partial class ContactDetailViewModel : ObservableObject, IQueryAttributable
{
    private readonly IContactRepository _contacts;

    [ObservableProperty]
    private ContactDisplayModel? contact;

    [ObservableProperty]
    private bool isBusy;

    public ContactDetailViewModel(IContactRepository contacts)
    {
        _contacts = contacts;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (!query.TryGetValue("ContactId", out var raw) || raw is null)
            return;
        var s = raw.ToString();
        if (int.TryParse(s, out var id))
            _ = LoadAsync(id);
    }

    /// <summary>Перезагрузка после возврата с экрана редактирования.</summary>
    public void RefreshContact()
    {
        if (Contact != null)
            _ = LoadAsync(Contact.Id);
    }

    private async Task LoadAsync(int id)
    {
        try
        {
            IsBusy = true;
            await _contacts.EnsureReadyAsync().ConfigureAwait(true);
            Contact = await _contacts.GetByIdAsync(id).ConfigureAwait(true);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task DeleteAsync()
    {
        if (Contact == null)
            return;
        var ok = await Shell.Current.DisplayAlertAsync(
            "Удалить контакт?",
            $"«{Contact.Name}» будет удалён из локального справочника.",
            "Удалить",
            "Отмена").ConfigureAwait(true);
        if (!ok)
            return;
        await _contacts.DeleteAsync(Contact.Id).ConfigureAwait(true);
        await Shell.Current.GoToAsync("..").ConfigureAwait(true);
    }

    [RelayCommand]
    private async Task EditAsync()
    {
        if (Contact == null)
            return;
        await ShellContactNavigation.GoToEditContactAsync(Contact.Id).ConfigureAwait(true);
    }

    [RelayCommand]
    private async Task AddNewContactAsync() =>
        await ShellContactNavigation.GoToNewContactAsync().ConfigureAwait(true);
}
