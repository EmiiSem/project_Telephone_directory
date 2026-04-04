namespace project_Telephone_directory.Services;

public sealed class DefaultSystemContactsService : ISystemContactsService
{
    public Task<bool> EnsureReadPermissionAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(false);

    public Task<IReadOnlyList<SystemContactRow>> FetchAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<SystemContactRow>>(Array.Empty<SystemContactRow>());
}
