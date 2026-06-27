byte[] data = "Document to sign"u8.ToArray();
byte[] signature = new byte[key.Algorithm.SignatureSizeInBytes];
key.SignData(data, signature);

// Export public key for verification
byte[] publicKeyBytes = key.ExportMLDsaPublicKey();

// Verify with public key
using MLDsa publicKey = MLDsa.ImportMLDsaPublicKey(
    MLDsaAlgorithm.MLDsa65, publicKeyBytes);
bool valid = publicKey.VerifyData(data, signature);
#endif

```text

**Parameter sets:**

| Parameter Set            | Security Level | Public Key  | Signature   |
| ------------------------ | -------------- | ----------- | ----------- |
| `MLDsaAlgorithm.MLDsa44` | NIST Level 2   | 1,312 bytes | 2,420 bytes |
| `MLDsaAlgorithm.MLDsa65` | NIST Level 3   | 1,952 bytes | 3,309 bytes |
| `MLDsaAlgorithm.MLDsa87` | NIST Level 5   | 2,592 bytes | 4,627 bytes |

### SLH-DSA (FIPS 205) -- Hash-Based Signatures

SLH-DSA (Stateless Hash-Based Digital Signature Algorithm) provides extremely conservative long-term signatures. Use
when mathematical structure of lattice-based schemes (ML-DSA) is a concern. The entire `SlhDsa` class is
`[Experimental]` (SYSLIB5006) -- Windows has not yet added native support.

```csharp

#if NET10_0_OR_GREATER
using System.Security.Cryptography;

// SlhDsa is [Experimental] -- suppress SYSLIB5006 only when intentional
#pragma warning disable SYSLIB5006
if (SlhDsa.IsSupported)
{
    using SlhDsa key = SlhDsa.GenerateKey(SlhDsaAlgorithm.SlhDsaSha2_128s);
    byte[] data = "Long-term document"u8.ToArray();
    byte[] signature = new byte[key.Algorithm.SignatureSizeInBytes];
    key.SignData(data, signature);
    bool valid = key.VerifyData(data, signature);
}
#pragma warning restore SYSLIB5006
#endif

```text

### Fallback Strategy for net8.0/net9.0

Post-quantum algorithms are only available in .NET 10+. For applications targeting earlier TFMs:

1. **Use classical algorithms now:** ECDSA (P-256/P-384) for signatures, ECDH + AES-GCM for key exchange/encryption.
   These remain secure against classical attacks.
2. **Prepare for migration:** Isolate cryptographic operations behind interfaces so algorithm swaps require minimal code
   changes.
3. **Multi-target when ready:** Use `#if NET10_0_OR_GREATER` conditionals or separate assemblies per TFM to add PQC
   support alongside classical fallbacks.
4. **Harvest-now-decrypt-later:** For data that must remain confidential for 10+ years, consider migrating to .NET 10
   sooner to protect against future quantum decryption of captured ciphertext.

### Interoperability Caveats

- **Key and signature sizes:** PQC keys and signatures are significantly larger than classical equivalents (e.g.,
  ML-DSA-65 signature is 3,309 bytes vs ECDSA P-256 at 64 bytes). This affects storage, bandwidth, and protocol message
  sizes.
- **No cross-platform PQC yet:** PQC APIs depend on OS crypto libraries. An app compiled for net10.0 will fail at
  runtime on older OS versions. Always gate behind `IsSupported`.
- **PKCS#8/X.509 formats are experimental:** Import/export of PQC keys in standard certificate formats is
  `[Experimental]` pending IETF RFC finalization. Do not persist PQC keys in PKCS#8 format in production yet.
- **Composite/hybrid signatures:** `CompositeMLDsa` (hybrid ML-DSA + classical) is fully `[Experimental]` with no native
  OS support. Use it only for prototyping.
- **TLS integration:** ML-DSA and SLH-DSA certificates work in TLS 1.3+ via `SslStream`, but only when the OS crypto
  library supports PQC in TLS. Verify with your deployment target.
- **Performance:** ML-KEM and ML-DSA are fast. SLH-DSA is significantly slower for signing (seconds, not milliseconds)
  -- use it only when hash-based security guarantees are required.

---

## Deprecated Cryptographic APIs

The following cryptographic algorithms are broken or obsolete. Do not use them in new code.

| Algorithm                  | Replacement | Reason                                               |
| -------------------------- | ----------- | ---------------------------------------------------- |
| MD5                        | SHA-256+    | Collision attacks since 2004; trivially broken       |
| SHA-1                      | SHA-256+    | Collision attacks demonstrated (SHAttered, 2017)     |
| DES                        | AES-GCM     | 56-bit key; brute-forceable in hours                 |
| 3DES (TripleDES)           | AES-GCM     | Deprecated by NIST (2023); Sweet32 attack            |
| RC2                        | AES-GCM     | Weak key schedule; effective key length < advertised |
| RSA PKCS#1 v1.5 encryption | RSA-OAEP    | Bleichenbacher padding oracle attacks                |

For the full list of deprecated security patterns beyond cryptography (CAS, APTCA, .NET Remoting, DCOM,
BinaryFormatter), see [skill:dotnet-security-owasp] which is the canonical owner of deprecated security pattern
warnings.

---

## Agent Gotchas

1. **Never reuse a nonce with AES-GCM** -- reusing a nonce with the same key breaks both confidentiality and
   authenticity. Always generate a fresh random nonce per encryption operation.
2. **Never use ECB mode** -- ECB encrypts identical plaintext blocks to identical ciphertext blocks, leaking patterns.
   .NET's `Aes.Create()` defaults to CBC, but prefer AES-GCM for authenticated encryption.
3. **Never compare hashes with `==`** -- use `CryptographicOperations.FixedTimeEquals` to prevent timing side-channel
   attacks.
4. **Never use MD5 or SHA-1 for security purposes** -- they are broken. SHA-1 is acceptable only for non-security
   checksums (e.g., git object hashes) where collision resistance is not a security requirement.
5. **Never hardcode encryption keys** -- use [skill:dotnet-secrets-management] for key storage. Generate keys with
   `RandomNumberGenerator.GetBytes`.
6. **Minimum RSA key size is 2048 bits** -- NIST deprecated 1024-bit RSA keys. Use 4096 for new systems.
7. **PBKDF2 iteration count must be high** -- OWASP recommends 600,000 iterations with SHA-256 (as of 2023). Lower
   counts are brute-forceable.
8. **PQC `IsSupported` checks are mandatory** -- calling PQC APIs on unsupported platforms throws
   `PlatformNotSupportedException`. Always check before use.
9. **Do not suppress SYSLIB5006 globally** -- suppress the experimental diagnostic only at the specific call site where
   you intentionally use experimental PQC APIs.

---



## Code Navigation (Serena MCP)

**Primary approach:** Use Serena symbol operations for efficient code navigation:

1. **Find definitions**: `serena_find_symbol` instead of text search
2. **Understand structure**: `serena_get_symbols_overview` for file organization
3. **Track references**: `serena_find_referencing_symbols` for impact analysis
4. **Precise edits**: `serena_replace_symbol_body` for clean modifications

**When to use Serena vs traditional tools:**
- ✅ **Use Serena**: Navigation, refactoring, dependency analysis, precise edits
- ✅ **Use Read/Grep**: Reading full files, pattern matching, simple text operations
- ✅ **Fallback**: If Serena unavailable, traditional tools work fine

**Example workflow:**
```text
# Instead of:
Read: src/Services/OrderService.cs
Grep: "public void ProcessOrder"

# Use:
serena_find_symbol: "OrderService/ProcessOrder"
serena_get_symbols_overview: "src/Services/OrderService.cs"
```
## References

- [ASP.NET Core Security](https://learn.microsoft.com/en-us/aspnet/core/security/?view=aspnetcore-10.0)
- [Security in .NET](https://learn.microsoft.com/en-us/dotnet/standard/security/)
- [Secure Coding Guidelines for .NET](https://learn.microsoft.com/en-us/dotnet/standard/security/secure-coding-guidelines)
- [Cryptography Model in .NET](https://learn.microsoft.com/en-us/dotnet/standard/security/cryptography-model)
- [Post-Quantum Cryptography in .NET](https://devblogs.microsoft.com/dotnet/post-quantum-cryptography-in-dotnet/)
- [ASP.NET Core Data Protection](https://learn.microsoft.com/en-us/aspnet/core/security/data-protection/introduction?view=aspnetcore-10.0)
- [OWASP Password Storage Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/Password_Storage_Cheat_Sheet.html)
- [NIST FIPS 203 (ML-KEM)](https://csrc.nist.gov/pubs/fips/203/final)
- [NIST FIPS 204 (ML-DSA)](https://csrc.nist.gov/pubs/fips/204/final)
- [NIST FIPS 205 (SLH-DSA)](https://csrc.nist.gov/pubs/fips/205/final)
````
