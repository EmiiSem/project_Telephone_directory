using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.ApplicationModel.Communication;
using Microsoft.Maui.ApplicationModel.DataTransfer;
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
        if (IsBusy)
            return;
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
    private Task EditAsync()
    {
        if (Contact == null)
            return Task.CompletedTask;
        return Shell.Current.GoToAsync($"{nameof(ContactEditPage)}?ContactId={Contact.Id}");
    }

    [RelayCommand]
    private async Task CallAsync()
    {
        if (Contact == null || string.IsNullOrWhiteSpace(Contact.Phone))
            return;
        var uri = new Uri("tel:" + Uri.EscapeDataString(Contact.Phone));
        await Launcher.Default.OpenAsync(uri).ConfigureAwait(true);
    }

    [RelayCommand]
    private async Task MailAsync()
    {
        if (Contact == null)
            return;
        var address = Contact.Email?.Trim() ?? string.Empty;
        if (string.IsNullOrEmpty(address))
        {
            await Shell.Current.DisplayAlertAsync(
                "Нет email",
                "Добавьте адрес электронной почты в режиме «Редактировать» или отправьте SMS, если указан телефон.",
                "Понятно").ConfigureAwait(true);
            return;
        }

        try
        {
            if (Email.Default.IsComposeSupported)
            {
                await Email.Default.ComposeAsync(new EmailMessage
                {
                    To = new List<string> { address },
                    Subject = string.IsNullOrWhiteSpace(Contact.Name)
                        ? string.Empty
                        : Contact.Name
                }).ConfigureAwait(true);
            }
            else
            {
                await Launcher.Default.OpenAsync(new Uri("mailto:" + address)).ConfigureAwait(true);
            }
        }
        catch
        {
            await Clipboard.Default.SetTextAsync(address).ConfigureAwait(true);
            await Shell.Current.DisplayAlertAsync(
                "Почтовый клиент",
                "Не удалось открыть приложение для писем. Адрес скопирован в буфер обмена — вставьте его в браузере или в Outlook.",
                "OK").ConfigureAwait(true);
        }
    }

    [RelayCommand]
    private async Task SmsAsync()
    {
        if (Contact == null)
            return;
        var phone = Contact.Phone?.Trim() ?? string.Empty;
        if (string.IsNullOrEmpty(phone))
        {
            await Shell.Current.DisplayAlertAsync(
                "Нет телефона",
                "Добавьте номер в режиме «Редактировать», чтобы отправить SMS.",
                "Понятно").ConfigureAwait(true);
            return;
        }

        var digits = new string(phone.Where(static c => char.IsDigit(c) || c == '+').ToArray());
        if (digits.Length == 0)
            digits = phone;

        try
        {
            await Launcher.Default.OpenAsync(new Uri("sms:" + digits)).ConfigureAwait(true);
        }
        catch
        {
            await Clipboard.Default.SetTextAsync(phone).ConfigureAwait(true);
            await Shell.Current.DisplayAlertAsync(
                "SMS",
                "Не удалось открыть приложение для сообщений. Номер скопирован в буфер обмена.",
                "OK").ConfigureAwait(true);
        }
    }
}
