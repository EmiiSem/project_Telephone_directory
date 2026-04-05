using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using project_Telephone_directory.Models;
using project_Telephone_directory.Services;

namespace project_Telephone_directory.ViewModels;

public partial class SearchViewModel : ObservableObject
{
    private readonly IContactRepository _contacts;

    [ObservableProperty]
    private string query = string.Empty;

    [ObservableProperty]
    private double typingIntensity;

    [ObservableProperty]
    private bool isBusy;

    public ObservableCollection<ContactDisplayModel> Results { get; } = new();

    public SearchViewModel(IContactRepository contacts)
    {
        _contacts = contacts;
    }

    partial void OnQueryChanged(string value)
    {
        var len = value?.Length ?? 0;
        TypingIntensity = Math.Min(1, len / 20.0);
        _ = SearchAsync();
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        await _contacts.EnsureReadyAsync().ConfigureAwait(true);
        await SearchAsync().ConfigureAwait(true);
    }

    [RelayCommand]
    private async Task SearchAsync()
    {
        try
        {
            IsBusy = true;
            var list = await _contacts.SearchAsync(Query).ConfigureAwait(true);
            Results.Clear();
            foreach (var c in list)
                Results.Add(c);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task OpenDetailAsync(ContactDisplayModel? model)
    {
        if (model == null)
            return;
        await ShellContactNavigation.GoToContactDetailAsync(model.Id).ConfigureAwait(true);
    }

    [RelayCommand]
    private async Task AddContactAsync() =>
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
        await SearchAsync().ConfigureAwait(true);
    }
}
