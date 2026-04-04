namespace project_Telephone_directory.Services;

public sealed class SystemContactRow
{
    public string Name { get; init; } = string.Empty;

    public string Phone { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;
}

public interface ISystemContactsService
{
    Task<bool> EnsureReadPermissionAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SystemContactRow>> FetchAsync(CancellationToken cancellationToken = default);
}
