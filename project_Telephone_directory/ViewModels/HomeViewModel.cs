using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using project_Telephone_directory.Services;

namespace project_Telephone_directory.ViewModels;

public partial class HomeViewModel : ObservableObject
{
    private readonly IContactRepository _contacts;

    [ObservableProperty]
    private int contactCount;

    [ObservableProperty]
    private bool isBusy;

    public HomeViewModel(IContactRepository contacts)
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
            var all = await _contacts.GetAllAsync().ConfigureAwait(true);
            ContactCount = all.Count;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private static Task OpenContactsAsync() =>
        Shell.Current.GoToAsync($"//{ShellRoutes.Contacts}");

    [RelayCommand]
    private static Task OpenSearchAsync() =>
        Shell.Current.GoToAsync($"//{ShellRoutes.Search}");

    [RelayCommand]
    private static Task AddContactAsync() =>
        Shell.Current.GoToAsync(nameof(ContactEditPage));

    [RelayCommand]
    private static Task OpenSettingsAsync() =>
        Shell.Current.GoToAsync($"//{ShellRoutes.Settings}");
}
