namespace project_Telephone_directory.Services;

public interface ICryptoService
{
    Task EnsureKeyAsync(CancellationToken cancellationToken = default);

    Task<string> EncryptAsync(string plainText, CancellationToken cancellationToken = default);

    Task<string> DecryptAsync(string cipherText, CancellationToken cancellationToken = default);
}
