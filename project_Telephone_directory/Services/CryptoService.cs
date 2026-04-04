using System.Security.Cryptography;
using System.Text;
using Microsoft.Maui.Storage;

namespace project_Telephone_directory.Services;

public sealed class CryptoService : ICryptoService
{
    private const string KeyName = "contact_db_field_key_v1";
    private const string FileKeyName = ".contact_field_key.txt";
    private byte[]? _key;

    public async Task EnsureKeyAsync(CancellationToken cancellationToken = default)
    {
        if (_key != null)
            return;

        try
        {
            var existing = await SecureStorage.Default.GetAsync(KeyName).ConfigureAwait(false);
            if (!string.IsNullOrEmpty(existing))
            {
                _key = Convert.FromBase64String(existing);
                return;
            }

            var key = RandomNumberGenerator.GetBytes(32);
            var b64 = Convert.ToBase64String(key);
            await SecureStorage.Default.SetAsync(KeyName, b64).ConfigureAwait(false);
            _key = key;
        }
        catch
        {
            await EnsureKeyFromAppDataFileAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    private async Task EnsureKeyFromAppDataFileAsync(CancellationToken cancellationToken)
    {
        var path = Path.Combine(FileSystem.AppDataDirectory, FileKeyName);
        if (File.Exists(path))
        {
            var text = await File.ReadAllTextAsync(path, cancellationToken).ConfigureAwait(false);
            if (!string.IsNullOrWhiteSpace(text))
            {
                _key = Convert.FromBase64String(text.Trim());
                return;
            }
        }

        var key = RandomNumberGenerator.GetBytes(32);
        var outB64 = Convert.ToBase64String(key);
        await File.WriteAllTextAsync(path, outB64, cancellationToken).ConfigureAwait(false);
        _key = key;
    }

    public async Task<string> EncryptAsync(string plainText, CancellationToken cancellationToken = default)
    {
        await EnsureKeyAsync(cancellationToken).ConfigureAwait(false);
        if (string.IsNullOrEmpty(plainText))
            return string.Empty;

        var nonce = RandomNumberGenerator.GetBytes(12);
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var tag = new byte[16];
        var cipher = new byte[plainBytes.Length];

        using (var aes = new AesGcm(_key!, 16))
        {
            aes.Encrypt(nonce, plainBytes, cipher, tag);
        }

        var payload = new byte[nonce.Length + tag.Length + cipher.Length];
        Buffer.BlockCopy(nonce, 0, payload, 0, nonce.Length);
        Buffer.BlockCopy(tag, 0, payload, nonce.Length, tag.Length);
        Buffer.BlockCopy(cipher, 0, payload, nonce.Length + tag.Length, cipher.Length);

        return Convert.ToBase64String(payload);
    }

    public async Task<string> DecryptAsync(string cipherText, CancellationToken cancellationToken = default)
    {
        await EnsureKeyAsync(cancellationToken).ConfigureAwait(false);
        if (string.IsNullOrEmpty(cipherText))
            return string.Empty;

        var payload = Convert.FromBase64String(cipherText);
        if (payload.Length < 12 + 16)
            return string.Empty;

        var nonce = payload.AsSpan(0, 12);
        var tag = payload.AsSpan(12, 16);
        var cipher = payload.AsSpan(28);

        var plain = new byte[cipher.Length];
        using (var aes = new AesGcm(_key!, 16))
        {
            aes.Decrypt(nonce, cipher, tag, plain);
        }

        return Encoding.UTF8.GetString(plain);
    }
}
