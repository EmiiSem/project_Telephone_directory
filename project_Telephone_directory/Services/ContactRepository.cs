using project_Telephone_directory.Database;
using project_Telephone_directory.Models;
using SQLite;

namespace project_Telephone_directory.Services;

public sealed class ContactRepository : IContactRepository
{
    private static string AvatarsDirectory =>
        Path.Combine(FileSystem.AppDataDirectory, "contact_avatars");

    private readonly ContactDatabase _database;
    private readonly ICryptoService _crypto;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private readonly SemaphoreSlim _cacheLoadLock = new(1, 1);
    private List<ContactDisplayModel>? _allCache;
    private int _cacheStamp;

    public ContactRepository(ContactDatabase database, ICryptoService crypto)
    {
        _database = database;
        _crypto = crypto;
    }

    public async Task EnsureReadyAsync(CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            await _database.OpenAsync().ConfigureAwait(false);
            await _crypto.EnsureKeyAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<IReadOnlyList<ContactDisplayModel>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await EnsureReadyAsync(cancellationToken).ConfigureAwait(false);
        if (_allCache != null)
            return _allCache;

        await _cacheLoadLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (_allCache != null)
                return _allCache;

            var stampAtLoad = Volatile.Read(ref _cacheStamp);
            var db = await _database.OpenAsync().ConfigureAwait(false);
            var rows = await db.Table<ContactEntity>().ToListAsync().ConfigureAwait(false);
            rows.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase));
            var list = new List<ContactDisplayModel>(rows.Count);
            foreach (var r in rows)
                list.Add(await MapAsync(r, cancellationToken).ConfigureAwait(false));

            if (Volatile.Read(ref _cacheStamp) != stampAtLoad)
            {
                _allCache = null;
                return await GetAllAsync(cancellationToken).ConfigureAwait(false);
            }

            _allCache = list;
            return _allCache;
        }
        finally
        {
            _cacheLoadLock.Release();
        }
    }

    public async Task<IReadOnlyList<ContactDisplayModel>> SearchAsync(string query, CancellationToken cancellationToken = default)
    {
        var q = query?.Trim() ?? string.Empty;
        var all = await GetAllAsync(cancellationToken).ConfigureAwait(false);
        if (string.IsNullOrEmpty(q))
            return all;

        var lower = q.ToLowerInvariant();
        var matches = new List<ContactDisplayModel>();
        foreach (var c in all)
        {
            if (c.Name.Contains(lower, StringComparison.OrdinalIgnoreCase) ||
                c.Phone.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                c.Email.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                (!string.IsNullOrEmpty(c.Notes) && c.Notes.Contains(q, StringComparison.OrdinalIgnoreCase)))
            {
                matches.Add(c);
            }
        }

        matches.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase));
        return matches;
    }

    public async Task<ContactDisplayModel?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        await EnsureReadyAsync(cancellationToken).ConfigureAwait(false);
        var db = await _database.OpenAsync().ConfigureAwait(false);
        var row = await db.Table<ContactEntity>().Where(c => c.Id == id).FirstOrDefaultAsync().ConfigureAwait(false);
        return row == null ? null : await MapAsync(row, cancellationToken).ConfigureAwait(false);
    }

    public async Task<int> InsertAsync(ContactDisplayModel model, CancellationToken cancellationToken = default)
    {
        await EnsureReadyAsync(cancellationToken).ConfigureAwait(false);
        var db = await _database.OpenAsync().ConfigureAwait(false);
        var now = DateTime.UtcNow;
        var entity = new ContactEntity
        {
            Name = model.Name.Trim(),
            PhoneCipher = await _crypto.EncryptAsync(model.Phone, cancellationToken).ConfigureAwait(false),
            EmailCipher = await _crypto.EncryptAsync(model.Email, cancellationToken).ConfigureAwait(false),
            Notes = model.Notes,
            SocialJson = SerializeSocial(model.Social),
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
            FromSystem = model.FromSystem,
            AvatarStorageName = model.AvatarStorageName
        };
        await db.InsertAsync(entity).ConfigureAwait(false);
        InvalidateCache();
        return entity.Id;
    }

    public async Task UpdateAsync(ContactDisplayModel model, CancellationToken cancellationToken = default)
    {
        await EnsureReadyAsync(cancellationToken).ConfigureAwait(false);
        var db = await _database.OpenAsync().ConfigureAwait(false);
        var existing = await db.Table<ContactEntity>().Where(c => c.Id == model.Id).FirstOrDefaultAsync().ConfigureAwait(false)
            ?? throw new InvalidOperationException("Contact not found");
        existing.Name = model.Name.Trim();
        existing.PhoneCipher = await _crypto.EncryptAsync(model.Phone, cancellationToken).ConfigureAwait(false);
        existing.EmailCipher = await _crypto.EncryptAsync(model.Email, cancellationToken).ConfigureAwait(false);
        existing.Notes = model.Notes;
        existing.SocialJson = SerializeSocial(model.Social);
        existing.UpdatedAtUtc = DateTime.UtcNow;
        existing.FromSystem = model.FromSystem;
        await db.UpdateAsync(existing).ConfigureAwait(false);
        InvalidateCache();
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        await EnsureReadyAsync(cancellationToken).ConfigureAwait(false);
        var db = await _database.OpenAsync().ConfigureAwait(false);
        var row = await db.Table<ContactEntity>().Where(c => c.Id == id).FirstOrDefaultAsync().ConfigureAwait(false);
        if (row == null)
            return;
        TryDeleteStoredAvatar(row.AvatarStorageName);
        await db.DeleteAsync(row).ConfigureAwait(false);
        InvalidateCache();
    }

    public async Task<int> UpsertManyAsync(IEnumerable<ContactDisplayModel> items, CancellationToken cancellationToken = default)
    {
        await EnsureReadyAsync(cancellationToken).ConfigureAwait(false);
        var db = await _database.OpenAsync().ConfigureAwait(false);
        var count = 0;
        foreach (var model in items)
        {
            if (string.IsNullOrWhiteSpace(model.Name))
                continue;

            var now = DateTime.UtcNow;
            var entity = new ContactEntity
            {
                Name = model.Name.Trim(),
                PhoneCipher = await _crypto.EncryptAsync(model.Phone, cancellationToken).ConfigureAwait(false),
                EmailCipher = await _crypto.EncryptAsync(model.Email, cancellationToken).ConfigureAwait(false),
                Notes = model.Notes,
                SocialJson = SerializeSocial(model.Social),
                CreatedAtUtc = now,
                UpdatedAtUtc = now,
                FromSystem = model.FromSystem
            };
            await db.InsertAsync(entity).ConfigureAwait(false);
            count++;
        }

        if (count > 0)
            InvalidateCache();
        return count;
    }

    public async Task SetAvatarFromFileAsync(int contactId, string sourceFilePath, CancellationToken cancellationToken = default)
    {
        await EnsureReadyAsync(cancellationToken).ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(sourceFilePath) || !File.Exists(sourceFilePath))
            return;

        EnsureAvatarsDirectoryExists();
        var db = await _database.OpenAsync().ConfigureAwait(false);
        var row = await db.Table<ContactEntity>().Where(c => c.Id == contactId).FirstOrDefaultAsync().ConfigureAwait(false);
        if (row == null)
            return;

        var ext = Path.GetExtension(sourceFilePath);
        if (string.IsNullOrEmpty(ext) || ext.Length > 8)
            ext = ".jpg";
        var storageName = $"{contactId}_{Guid.NewGuid():N}{ext}";
        var dest = Path.Combine(AvatarsDirectory, storageName);
        TryDeleteStoredAvatar(row.AvatarStorageName);

        await using (var src = new FileStream(sourceFilePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true))
        await using (var dst = new FileStream(dest, FileMode.CreateNew, FileAccess.Write, FileShare.None, 4096, true))
            await src.CopyToAsync(dst, cancellationToken).ConfigureAwait(false);

        row.AvatarStorageName = storageName;
        await db.UpdateAsync(row).ConfigureAwait(false);
        InvalidateCache();
    }

    public async Task ClearAvatarAsync(int contactId, CancellationToken cancellationToken = default)
    {
        await EnsureReadyAsync(cancellationToken).ConfigureAwait(false);
        var db = await _database.OpenAsync().ConfigureAwait(false);
        var row = await db.Table<ContactEntity>().Where(c => c.Id == contactId).FirstOrDefaultAsync().ConfigureAwait(false);
        if (row == null)
            return;
        TryDeleteStoredAvatar(row.AvatarStorageName);
        row.AvatarStorageName = null;
        await db.UpdateAsync(row).ConfigureAwait(false);
        InvalidateCache();
    }

    private void InvalidateCache()
    {
        Interlocked.Increment(ref _cacheStamp);
        _allCache = null;
    }

    private static void EnsureAvatarsDirectoryExists()
    {
        Directory.CreateDirectory(AvatarsDirectory);
    }

    private static void TryDeleteStoredAvatar(string? storageName)
    {
        if (string.IsNullOrEmpty(storageName))
            return;
        try
        {
            var p = Path.Combine(AvatarsDirectory, storageName);
            if (File.Exists(p))
                File.Delete(p);
        }
        catch
        {
            // ignore
        }
    }

    private async Task<ContactDisplayModel> MapAsync(ContactEntity row, CancellationToken cancellationToken)
    {
        var phone = await _crypto.DecryptAsync(row.PhoneCipher, cancellationToken).ConfigureAwait(false);
        var email = await _crypto.DecryptAsync(row.EmailCipher, cancellationToken).ConfigureAwait(false);
        string? avatarPath = null;
        if (!string.IsNullOrEmpty(row.AvatarStorageName))
        {
            var p = Path.Combine(AvatarsDirectory, row.AvatarStorageName);
            if (File.Exists(p))
                avatarPath = p;
        }

        return new ContactDisplayModel
        {
            Id = row.Id,
            Name = row.Name,
            Phone = phone,
            Email = email,
            Notes = row.Notes,
            Social = DeserializeSocial(row.SocialJson),
            FromSystem = row.FromSystem,
            AvatarStorageName = row.AvatarStorageName,
            AvatarLocalPath = avatarPath
        };
    }

    private static string? SerializeSocial(IReadOnlyDictionary<string, string> social)
    {
        if (social.Count == 0)
            return null;
        return JsonSerializer.Serialize(social);
    }

    private static IReadOnlyDictionary<string, string> DeserializeSocial(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return new Dictionary<string, string>();

        try
        {
            var dict = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
            return dict ?? new Dictionary<string, string>();
        }
        catch
        {
            return new Dictionary<string, string>();
        }
    }
}
