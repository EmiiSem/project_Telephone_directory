namespace project_Telephone_directory.Models;

public sealed class ContactExportDto
{
    public string Name { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? Notes { get; set; }

    public Dictionary<string, string>? Social { get; set; }
}
