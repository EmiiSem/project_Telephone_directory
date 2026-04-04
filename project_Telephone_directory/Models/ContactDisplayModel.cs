namespace project_Telephone_directory.Models;

public sealed class ContactDisplayModel
{
    public int Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public string Phone { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public string? Notes { get; init; }

    public IReadOnlyDictionary<string, string> Social { get; init; } =
        new Dictionary<string, string>();

    public bool FromSystem { get; init; }

    /// <summary>Имя файла в каталоге аватаров (только для слоя данных).</summary>
    public string? AvatarStorageName { get; init; }

    /// <summary>Полный путь к файлу аватара, если он есть на диске.</summary>
    public string? AvatarLocalPath { get; init; }

    public bool HasAvatar => !string.IsNullOrEmpty(AvatarLocalPath);

    public string Initials
    {
        get
        {
            if (string.IsNullOrWhiteSpace(Name))
                return "?";
            var t = Name.Trim();
            var parts = t.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (parts.Length >= 2)
                return string.Concat(char.ToUpperInvariant(parts[0][0]), char.ToUpperInvariant(parts[1][0]));
            return t.Length >= 2
                ? t[..2].ToUpperInvariant()
                : char.ToUpperInvariant(t[0]).ToString();
        }
    }
}
