---
name: dotnet-cryptography
category: security
subcategory: crypto
description: Selects crypto algorithms and usage. Hashing, AES-GCM, RSA, ECDSA, PQC key derivation.
license: MIT
targets: ['*']
tags: [security, dotnet, skill]
version: '0.0.1'
author: 'dotnet-agent-harness'
invocable: true
claudecode:
  allowed-tools: ['Read', 'Grep', 'Glob', 'Bash', 'Write', 'Edit']
codexcli:
  short-description: '.NET skill guidance for security tasks'
opencode:
  allowed-tools: ['Read', 'Grep', 'Glob', 'Bash', 'Write', 'Edit']
copilot: {}
geminicli: {}
antigravity: {}
---

# dotnet-cryptography

Modern .NET cryptography covering hashing (SHA-256/384/512), symmetric encryption (AES-GCM), asymmetric cryptography
(RSA, ECDSA), key derivation (PBKDF2, Argon2), and post-quantum algorithms (ML-KEM, ML-DSA, SLH-DSA) for .NET 10+.
Includes TFM-aware guidance: what's available on net10.0 vs fallback strategies for net8.0/net9.0.

## Scope

- Algorithm selection and correct usage of System.Security.Cryptography APIs
- Hashing for integrity (SHA-256/384/512)
- Symmetric encryption (AES-GCM)
- Asymmetric cryptography (RSA, ECDSA)
- Key derivation (PBKDF2, Argon2)
- Post-quantum cryptography (ML-KEM, ML-DSA, SLH-DSA) for .NET 10+
- Deprecated algorithm warnings

## Out of scope

- Secrets management and configuration binding -- see [skill:dotnet-secrets-management]
- OWASP vulnerability categories and deprecated security patterns -- see [skill:dotnet-security-owasp]
- Authentication/authorization implementation (JWT, OAuth, Identity) -- see [skill:dotnet-api-security] and
  [skill:dotnet-blazor-auth]
- Cloud-specific key management (Azure Key Vault, AWS KMS) -- see [skill:dotnet-advisor]
- TLS/HTTPS configuration -- see [skill:dotnet-advisor]

Cross-references: [skill:dotnet-security-owasp] for OWASP A02 (Cryptographic Failures) and deprecated pattern warnings,
[skill:dotnet-secrets-management] for storing keys and secrets securely.

---

## Prerequisites

- .NET 8.0+ (LTS baseline for classical algorithms)
- .NET 10.0+ for post-quantum algorithms (ML-KEM, ML-DSA, SLH-DSA)
- Platform support for PQC: Windows 11 (November 2025+) or OpenSSL 3.5+ on Linux/macOS

---

## Hashing (SHA-2 Family)

Use SHA-256/384/512 for integrity verification, checksums, and content-addressable storage. Never use hashing alone for
passwords (see Key Derivation below).

````csharp

using System.Security.Cryptography;

// Hash a byte array
byte[] data = "Hello, world"u8.ToArray();
byte[] hash = SHA256.HashData(data);

// Hash a stream (efficient for large files)
await using var stream = File.OpenRead("largefile.bin");
byte[] fileHash = await SHA256.HashDataAsync(stream);

// Compare hashes securely (constant-time comparison prevents timing attacks)
bool isEqual = CryptographicOperations.FixedTimeEquals(hash1, hash2);

```text

```csharp

// HMAC for authenticated hashing (message authentication codes)
byte[] key = RandomNumberGenerator.GetBytes(32); // 256-bit key
byte[] mac = HMACSHA256.HashData(key, data);

// Verify HMAC
byte[] computedMac = HMACSHA256.HashData(key, receivedData);
if (!CryptographicOperations.FixedTimeEquals(mac, computedMac))
{
    throw new CryptographicException("Message authentication failed");
}

```text

---

## Symmetric Encryption (AES-GCM)

AES-GCM is the recommended symmetric encryption for .NET. It provides both confidentiality and authenticity
(authenticated encryption with associated data -- AEAD).

```csharp

using System.Security.Cryptography;

public static class AesGcmEncryptor
{
    private const int NonceSize = 12; // 96-bit nonce (required by GCM)
    private const int TagSize = 16;   // 128-bit authentication tag

    public static byte[] Encrypt(byte[] plaintext, byte[] key)
    {
        var nonce = RandomNumberGenerator.GetBytes(NonceSize);
        var ciphertext = new byte[plaintext.Length];
        var tag = new byte[TagSize];

        using var aes = new AesGcm(key, TagSize);
        aes.Encrypt(nonce, plaintext, ciphertext, tag);

        // Prepend nonce + append tag for transport
        var result = new byte[NonceSize + ciphertext.Length + TagSize];
        nonce.CopyTo(result, 0);
        ciphertext.CopyTo(result, NonceSize);
        tag.CopyTo(result, NonceSize + ciphertext.Length);
        return result;
    }

    public static byte[] Decrypt(byte[] encryptedData, byte[] key)
    {
        var nonce = encryptedData.AsSpan(0, NonceSize);
        var ciphertext = encryptedData.AsSpan(NonceSize, encryptedData.Length - NonceSize - TagSize);
        var tag = encryptedData.AsSpan(encryptedData.Length - TagSize);
        var plaintext = new byte[ciphertext.Length];

        using var aes = new AesGcm(key, TagSize);
        aes.Decrypt(nonce, ciphertext, tag, plaintext);
        return plaintext;
    }
}

```text

```csharp

// ASP.NET Core Data Protection API -- preferred for web application scenarios
// Handles key management, rotation, and storage automatically
using Microsoft.AspNetCore.DataProtection;

public sealed class TokenProtector(IDataProtectionProvider provider)
{
    private readonly IDataProtector _protector =
        provider.CreateProtector("Tokens.V1");

    public string Protect(string plaintext) => _protector.Protect(plaintext);
    public string Unprotect(string ciphertext) => _protector.Unprotect(ciphertext);
}

// Registration:
builder.Services.AddDataProtection()
    .SetApplicationName("MyApp")
    .PersistKeysToFileSystem(new DirectoryInfo("/keys"));

```text

---

## Asymmetric Cryptography (RSA, ECDSA)

### RSA

Use RSA for encryption of small payloads (key wrapping) and digital signatures. Minimum 2048-bit keys; prefer 4096-bit
for new systems.

```csharp

using System.Security.Cryptography;

// Generate an RSA key pair
using var rsa = RSA.Create(4096);

// Sign data
byte[] signature = rsa.SignData(data, HashAlgorithmName.SHA256, RSASignaturePadding.Pss);

// Verify signature (with public key)
byte[] publicKeyBytes = rsa.ExportRSAPublicKey();
using var rsaPublic = RSA.Create();
rsaPublic.ImportRSAPublicKey(publicKeyBytes, out _);
bool valid = rsaPublic.VerifyData(data, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pss);

// Encrypt with OAEP padding (never use PKCS#1 v1.5 for new code)
byte[] encrypted = rsaPublic.Encrypt(smallPayload, RSAEncryptionPadding.OaepSHA256);
byte[] decrypted = rsa.Decrypt(encrypted, RSAEncryptionPadding.OaepSHA256);

```text

### ECDSA

Prefer ECDSA over RSA for digital signatures in new projects -- smaller keys with equivalent security.

```csharp

using System.Security.Cryptography;

// Generate ECDSA key (P-256 = NIST curve, widely supported)
using var ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);

// Sign data
byte[] signature = ecdsa.SignData(data, HashAlgorithmName.SHA256);

// Export public key for verification
byte[] publicKey = ecdsa.ExportSubjectPublicKeyInfo();

// Import and verify
using var ecdsaPublic = ECDsa.Create();
ecdsaPublic.ImportSubjectPublicKeyInfo(publicKey, out _);
bool valid = ecdsaPublic.VerifyData(data, signature, HashAlgorithmName.SHA256);

```text

---

## Key Derivation (Password Hashing)

### PBKDF2 (Built-in)

PBKDF2 is built into .NET and acceptable for password hashing. Use at least 600,000 iterations with SHA-256 (OWASP
recommendation).

```csharp

using System.Buffers.Binary;
using System.Security.Cryptography;

public static class PasswordHasher
{
    private const int SaltSize = 16;       // 128-bit salt
    private const int HashSize = 32;       // 256-bit derived key
    private const int Iterations = 600_000; // OWASP 2023 recommendation for SHA-256
    private const int PayloadSize = 4 + SaltSize + HashSize; // iteration count + salt + hash

    public static string HashPassword(string password)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            Iterations,
            HashAlgorithmName.SHA256,
            HashSize);

        // Store iteration count (fixed little-endian), salt, and hash together
        byte[] result = new byte[PayloadSize];
        BinaryPrimitives.WriteInt32LittleEndian(result, Iterations);
        salt.CopyTo(result.AsSpan(4));
        hash.CopyTo(result.AsSpan(4 + SaltSize));
        return Convert.ToBase64String(result);
    }

    public static bool VerifyPassword(string password, string stored)
    {
        // Defensive parsing: reject malformed input without exceptions
        Span<byte> decoded = stackalloc byte[PayloadSize];
        if (!Convert.TryFromBase64String(stored, decoded, out int bytesWritten)
            || bytesWritten != PayloadSize)
        {
            return false;
        }

        int iterations = BinaryPrimitives.ReadInt32LittleEndian(decoded);
        if (iterations <= 0)
            return false;

        var salt = decoded.Slice(4, SaltSize);
        var expectedHash = decoded.Slice(4 + SaltSize, HashSize);

        byte[] actualHash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            iterations,
            HashAlgorithmName.SHA256,
            HashSize);

        return CryptographicOperations.FixedTimeEquals(expectedHash, actualHash);
    }
}

```text

### Argon2 (via NuGet)

Argon2id is the recommended algorithm for password hashing when a NuGet dependency is acceptable. It is memory-hard,
resisting GPU/ASIC attacks better than PBKDF2.

```csharp

// Requires: <PackageReference Include="Konscious.Security.Cryptography.Argon2" Version="1.*" />
using Konscious.Security.Cryptography;

public static byte[] HashWithArgon2(string password, byte[] salt)
{
    using var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
    {
        Salt = salt,
        DegreeOfParallelism = 4,  // threads
        MemorySize = 65536,       // 64 MB
        Iterations = 3
    };
    return argon2.GetBytes(32); // 256-bit hash
}

```text

> Prefer ASP.NET Core Identity's `PasswordHasher<T>` for web applications -- it handles PBKDF2 with correct parameters
> and format versioning automatically. Use custom hashing only for non-Identity scenarios.

---

## Post-Quantum Cryptography (.NET 10+)

.NET 10 introduces post-quantum cryptography (PQC) through the `System.Security.Cryptography` namespace. These
algorithms resist attacks from both classical and quantum computers.

### Platform Requirements

PQC APIs require OS-level support:

- **Windows:** Windows 11 (November 2025 update) or Windows Server 2025 with PQC updates
- **Linux/macOS:** OpenSSL 3.5 or newer

Always check `IsSupported` before using PQC types. On unsupported platforms, fall back to classical algorithms.

### ML-KEM (FIPS 203) -- Key Encapsulation

ML-KEM replaces classical key exchange (ECDH) for establishing shared secrets. It is the most mature .NET 10 PQC API
(not marked `[Experimental]` at class level).

```csharp

#if NET10_0_OR_GREATER
using System.Security.Cryptography;

if (!MLKem.IsSupported)
{
    Console.WriteLine("ML-KEM not available on this platform");
    return;
}

// Generate a key pair
using MLKem privateKey = MLKem.GenerateKey(MLKemAlgorithm.MLKem768);

// Export public encapsulation key (share with peer)
byte[] publicKeyBytes = privateKey.ExportEncapsulationKey();

// Peer: import public key and encapsulate a shared secret
using MLKem publicKey = MLKem.ImportEncapsulationKey(
    MLKemAlgorithm.MLKem768, publicKeyBytes);
publicKey.Encapsulate(out byte[] ciphertext, out byte[] sharedSecret1);

// Original holder: decapsulate to recover the same shared secret
byte[] sharedSecret2 = privateKey.Decapsulate(ciphertext);

// Both parties now have the same shared secret for symmetric encryption
bool match = sharedSecret1.AsSpan().SequenceEqual(sharedSecret2);
#endif

```text

**Parameter sets:**

| Parameter Set              | Security Level         | Encapsulation Key | Ciphertext  |
| -------------------------- | ---------------------- | ----------------- | ----------- |
| `MLKemAlgorithm.MLKem512`  | NIST Level 1 (128-bit) | 800 bytes         | 768 bytes   |
| `MLKemAlgorithm.MLKem768`  | NIST Level 3 (192-bit) | 1,184 bytes       | 1,088 bytes |
| `MLKemAlgorithm.MLKem1024` | NIST Level 5 (256-bit) | 1,568 bytes       | 1,568 bytes |

Prefer `MLKem768` for general use (balances security and performance).

### ML-DSA (FIPS 204) -- Digital Signatures

ML-DSA replaces RSA/ECDSA for quantum-resistant digital signatures.

```csharp

#if NET10_0_OR_GREATER
using System.Security.Cryptography;

if (!MLDsa.IsSupported)
{
    Console.WriteLine("ML-DSA not available on this platform");
    return;
}

// Generate signing key
using MLDsa key = MLDsa.GenerateKey(MLDsaAlgorithm.MLDsa65);

// Sign data
byte[] data = "Document to sign"u8.ToArray();

## Detailed Examples

See [references/detailed-examples.md](references/detailed-examples.md) for complete code samples and advanced patterns.
