using Android.Content;
using Android.Provider;
using Microsoft.Maui.ApplicationModel;
using project_Telephone_directory.Services;

namespace project_Telephone_directory.Platforms.Android.Services;

public sealed class AndroidContactsService : ISystemContactsService
{
    public async Task<bool> EnsureReadPermissionAsync(CancellationToken cancellationToken = default)
    {
        var status = await Permissions.RequestAsync<Permissions.ContactsRead>().ConfigureAwait(false);
        return status == PermissionStatus.Granted;
    }

    public async Task<IReadOnlyList<SystemContactRow>> FetchAsync(CancellationToken cancellationToken = default)
    {
        if (!await EnsureReadPermissionAsync(cancellationToken).ConfigureAwait(false))
            return Array.Empty<SystemContactRow>();

        var activity = Platform.CurrentActivity;
        var resolver = activity?.ContentResolver;
        if (resolver == null)
            return Array.Empty<SystemContactRow>();

        return await Task.Run(() => ReadContacts(resolver), cancellationToken).ConfigureAwait(false);
    }

    private static IReadOnlyList<SystemContactRow> ReadContacts(ContentResolver resolver)
    {
        var list = new List<SystemContactRow>();
        var uri = ContactsContract.Contacts.ContentUri;
        var projection = new[]
        {
            ContactsContract.Contacts.InterfaceConsts.Id,
            ContactsContract.Contacts.InterfaceConsts.DisplayName
        };

        using var cursor = resolver.Query(uri, projection, null, null, null);
        if (cursor == null)
            return list;

        var idIdx = cursor.GetColumnIndex(ContactsContract.Contacts.InterfaceConsts.Id);
        var nameIdx = cursor.GetColumnIndex(ContactsContract.Contacts.InterfaceConsts.DisplayName);

        while (cursor.MoveToNext())
        {
            var id = cursor.GetString(idIdx);
            if (string.IsNullOrEmpty(id))
                continue;

            var name = cursor.GetString(nameIdx) ?? string.Empty;
            var phone = ReadFirstPhone(resolver, id);
            var email = ReadFirstEmail(resolver, id);

            list.Add(new SystemContactRow
            {
                Name = name.Trim(),
                Phone = phone,
                Email = email
            });
        }

        return list;
    }

    private static string ReadFirstPhone(ContentResolver resolver, string contactId)
    {
        var uri = ContactsContract.CommonDataKinds.Phone.ContentUri;
        var projection = new[] { ContactsContract.CommonDataKinds.Phone.Number };
        var selection = $"{ContactsContract.CommonDataKinds.Phone.InterfaceConsts.ContactId} = ?";
        using var cursor = resolver.Query(uri, projection, selection, new[] { contactId }, null);
        if (cursor == null || !cursor.MoveToFirst())
            return string.Empty;

        var idx = cursor.GetColumnIndex(ContactsContract.CommonDataKinds.Phone.Number);
        return idx < 0 ? string.Empty : (cursor.GetString(idx) ?? string.Empty);
    }

    private static string ReadFirstEmail(ContentResolver resolver, string contactId)
    {
        var uri = ContactsContract.CommonDataKinds.Email.ContentUri;
        var projection = new[] { ContactsContract.CommonDataKinds.Email.Address };
        var selection = $"{ContactsContract.CommonDataKinds.Email.InterfaceConsts.ContactId} = ?";
        using var cursor = resolver.Query(uri, projection, selection, new[] { contactId }, null);
        if (cursor == null || !cursor.MoveToFirst())
            return string.Empty;

        var idx = cursor.GetColumnIndex(ContactsContract.CommonDataKinds.Email.Address);
        return idx < 0 ? string.Empty : (cursor.GetString(idx) ?? string.Empty);
    }
}
