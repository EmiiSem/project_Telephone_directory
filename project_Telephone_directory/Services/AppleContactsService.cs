#if IOS || MACCATALYST

using Contacts;
using Foundation;
using Microsoft.Maui.ApplicationModel;

namespace project_Telephone_directory.Services;

public sealed class AppleContactsService : ISystemContactsService
{
    public async Task<bool> EnsureReadPermissionAsync(CancellationToken cancellationToken = default)
    {
        var status = await Permissions.RequestAsync<Permissions.ContactsRead>().ConfigureAwait(false);
        return status == PermissionStatus.Granted;
    }

    public Task<IReadOnlyList<SystemContactRow>> FetchAsync(CancellationToken cancellationToken = default)
    {
        return MainThread.InvokeOnMainThreadAsync(() =>
        {
            var list = new List<SystemContactRow>();
            var store = new CNContactStore();
            var keys = new[]
            {
                CNContactKey.GivenName,
                CNContactKey.FamilyName,
                CNContactKey.PhoneNumbers,
                CNContactKey.EmailAddresses
            };

            var request = new CNContactFetchRequest(keys);
            NSError? error;
            store.EnumerateContacts(request, out error, (CNContact contact, ref bool stop) =>
            {
                stop = false;
                var given = contact.GivenName ?? string.Empty;
                var family = contact.FamilyName ?? string.Empty;
                var name = $"{given} {family}".Trim();
                if (string.IsNullOrEmpty(name))
                    name = contact.OrganizationName ?? "Контакт";

                var phone = string.Empty;
                if (contact.PhoneNumbers != null && contact.PhoneNumbers.Length > 0)
                {
                    var first = contact.PhoneNumbers[0];
                    phone = first?.Value?.StringValue ?? string.Empty;
                }

                var email = string.Empty;
                if (contact.EmailAddresses != null && contact.EmailAddresses.Length > 0)
                {
                    var first = contact.EmailAddresses[0];
                    email = first?.Value ?? string.Empty;
                }

                list.Add(new SystemContactRow
                {
                    Name = name,
                    Phone = phone,
                    Email = email
                });
            });

            return (IReadOnlyList<SystemContactRow>)list;
        });
    }
}

#endif
