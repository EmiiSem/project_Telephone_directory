namespace project_Telephone_directory;

/// <summary>
/// Навигация к экранам контактов на главном потоке и через строковый URI
/// (ShellNavigationQueryParameters на части устройств даёт сбой в нативном слое).
/// </summary>
public static class ShellContactNavigation
{
    public static Task GoToNewContactAsync() =>
        MainThread.InvokeOnMainThreadAsync(() => Shell.Current.GoToAsync($"{nameof(ContactEditPage)}"));

    public static Task GoToEditContactAsync(int contactId) =>
        MainThread.InvokeOnMainThreadAsync(() =>
            Shell.Current.GoToAsync($"{nameof(ContactEditPage)}?ContactId={contactId}"));

    public static Task GoToContactDetailAsync(int contactId) =>
        MainThread.InvokeOnMainThreadAsync(() =>
            Shell.Current.GoToAsync($"{nameof(ContactDetailPage)}?ContactId={contactId}"));
}
