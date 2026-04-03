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
        return _connection;
    }

    public async Task<SQLiteAsyncConnection> OpenAsync() => await GetConnectionAsync();
}
