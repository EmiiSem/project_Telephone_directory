using project_Telephone_directory.Models;

namespace project_Telephone_directory.Services;

public interface IContactRepository
{
    Task EnsureReadyAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ContactDisplayModel>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ContactDisplayModel>> SearchAsync(string query, CancellationToken cancellationToken = default);

    Task<ContactDisplayModel?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<int> InsertAsync(ContactDisplayModel model, CancellationToken cancellationToken = default);

    Task UpdateAsync(ContactDisplayModel model, CancellationToken cancellationToken = default);

    Task DeleteAsync(int id, CancellationToken cancellationToken = default);

    Task<int> UpsertManyAsync(IEnumerable<ContactDisplayModel> items, CancellationToken cancellationToken = default);

    Task SetAvatarFromFileAsync(int contactId, string sourceFilePath, CancellationToken cancellationToken = default);

    Task ClearAvatarAsync(int contactId, CancellationToken cancellationToken = default);
}
