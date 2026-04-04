using project_Telephone_directory.Models;

namespace project_Telephone_directory.Services;

public interface IContactImportExportService
{
    Task<string> ExportToJsonAsync(IReadOnlyList<ContactDisplayModel> contacts, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ContactDisplayModel>> ImportFromJsonAsync(string json, CancellationToken cancellationToken = default);
}
