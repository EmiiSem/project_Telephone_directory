using project_Telephone_directory.Models;

namespace project_Telephone_directory.Services;

public sealed class ContactImportExportService : IContactImportExportService
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public Task<string> ExportToJsonAsync(IReadOnlyList<ContactDisplayModel> contacts, CancellationToken cancellationToken = default)
    {
        var list = contacts.Select(c => new ContactExportDto
        {
            Name = c.Name,
            Phone = c.Phone,
            Email = c.Email,
            Notes = c.Notes,
            Social = c.Social.Count > 0 ? new Dictionary<string, string>(c.Social) : null
        }).ToList();

        var json = JsonSerializer.Serialize(list, Options);
        return Task.FromResult(json);
    }

    public Task<IReadOnlyList<ContactDisplayModel>> ImportFromJsonAsync(string json, CancellationToken cancellationToken = default)
    {
        var dtos = JsonSerializer.Deserialize<List<ContactExportDto>>(json, Options) ?? new List<ContactExportDto>();
        var result = new List<ContactDisplayModel>(dtos.Count);
        foreach (var d in dtos)
        {
            if (string.IsNullOrWhiteSpace(d.Name))
                continue;
            var social = d.Social ?? new Dictionary<string, string>();
            result.Add(new ContactDisplayModel
            {
                Id = 0,
                Name = d.Name.Trim(),
                Phone = d.Phone ?? string.Empty,
                Email = d.Email ?? string.Empty,
                Notes = d.Notes,
                Social = social,
                FromSystem = false
            });
        }

        return Task.FromResult<IReadOnlyList<ContactDisplayModel>>(result);
    }
}
