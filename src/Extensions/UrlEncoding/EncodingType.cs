#pragma warning disable CA1008
namespace Rocket.Surgery.Extensions.Encoding;

/// <summary>
///     EncodingType
/// </summary>
public enum EncodingType
{
    /// <summary>
    ///     The base64
    /// </summary>
    Base64 = 1,

    /// <summary>
    ///     The base64 URL
    /// </summary>
    Base64Url = 2,

    /// <summary>
    ///     The base32 URL
    /// </summary>
    Base32Url = 3,

    /// <summary>
    ///     The z base32
    /// </summary>
    ZBase32 = 4,

    /// <summary>
    ///     The base32 low profanity
    /// </summary>
    Base32LowProfanity = 5,

    /// <summary>
    ///     The base32 crockford
    /// </summary>
    Base32Crockford = 6,
}
