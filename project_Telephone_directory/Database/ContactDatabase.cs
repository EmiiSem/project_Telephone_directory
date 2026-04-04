using project_Telephone_directory.Models;
using SQLite;

namespace project_Telephone_directory.Database;

public sealed class ContactDatabase
{
    private SQLiteAsyncConnection? _connection;

    private async Task<SQLiteAsyncConnection> GetConnectionAsync()
    {
        if (_connection != null)
            return _connection;

        var path = Path.Combine(FileSystem.AppDataDirectory, "contacts_secure.db3");
        _connection = new SQLiteAsyncConnection(path, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.SharedCache);
        await _connection.CreateTableAsync<ContactEntity>();
        await MigrateAvatarColumnAsync(_connection).ConfigureAwait(false);
        return _connection;
    }

    public async Task<SQLiteAsyncConnection> OpenAsync() => await GetConnectionAsync();

    private static async Task MigrateAvatarColumnAsync(SQLiteAsyncConnection db)
    {
        var count = await db.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM pragma_table_info('contacts') WHERE name='AvatarStorageName'").ConfigureAwait(false);
        if (count > 0)
            return;
        await db.ExecuteAsync("ALTER TABLE contacts ADD COLUMN AvatarStorageName TEXT").ConfigureAwait(false);
    }
}
