using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using project_Telephone_directory.Models;
using project_Telephone_directory.Services;

namespace project_Telephone_directory.ViewModels;

public partial class ContactsListViewModel : ObservableObject
{
    private readonly IContactRepository _contacts;

    [ObservableProperty]
    private bool isBusy;

    public ObservableCollection<ContactDisplayModel> Items { get; } = new();

    public ContactsListViewModel(IContactRepository contacts)
    {
        _contacts = contacts;
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        if (IsBusy)
            return;
        try
        {
            IsBusy = true;
            await _contacts.EnsureReadyAsync().ConfigureAwait(true);
            await RefreshAsync().ConfigureAwait(true);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        var list = await _contacts.GetAllAsync().ConfigureAwait(true);
        Items.Clear();
        foreach (var c in list)
            Items.Add(c);
    }

    [RelayCommand]
    private async Task OpenDetailAsync(ContactDisplayModel? model)
    {
        if (model == null)
            return;
        await ShellContactNavigation.GoToContactDetailAsync(model.Id).ConfigureAwait(true);
    }

    [RelayCommand]
    private async Task AddAsync() =>
        await ShellContactNavigation.GoToNewContactAsync().ConfigureAwait(true);

    [RelayCommand]
    private async Task EditContactAsync(ContactDisplayModel? model)
    {
        if (model == null)
            return;
        await ShellContactNavigation.GoToEditContactAsync(model.Id).ConfigureAwait(true);
    }

    [RelayCommand]
    private async Task DeleteContactAsync(ContactDisplayModel? model)
    {
        if (model == null)
            return;
        var ok = await Shell.Current.DisplayAlertAsync(
            "Удалить контакт?",
            $"«{model.Name}» будет удалён из справочника.",
            "Удалить",
            "Отмена").ConfigureAwait(true);
        if (!ok)
            return;
        await _contacts.DeleteAsync(model.Id).ConfigureAwait(true);
        await RefreshAsync().ConfigureAwait(true);
    }
}
