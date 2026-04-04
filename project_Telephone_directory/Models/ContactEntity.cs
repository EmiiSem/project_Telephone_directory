using SQLite;

namespace project_Telephone_directory.Models;

[Table("contacts")]
public sealed class ContactEntity
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed]
    public string Name { get; set; } = string.Empty;

    public string PhoneCipher { get; set; } = string.Empty;

    public string EmailCipher { get; set; } = string.Empty;

    public string? Notes { get; set; }

    /// <summary>Optional JSON map of social network id -> handle.</summary>
    public string? SocialJson { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime UpdatedAtUtc { get; set; }

    /// <summary>True if imported from device address book.</summary>
    public bool FromSystem { get; set; }

    /// <summary>File name only, relative to app avatars directory.</summary>
    public string? AvatarStorageName { get; set; }
}
