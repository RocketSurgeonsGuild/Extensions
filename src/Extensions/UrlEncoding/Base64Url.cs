using System.Text;

namespace Rocket.Surgery.Extensions.Encoding;

/// <summary>
///     Modified Base64 for URL applications ('base64url' encoding)
///     See http://tools.ietf.org/html/rfc4648
///     For more information see http://en.wikipedia.org/wiki/Base64
/// </summary>
[PublicAPI]
public static class Base64Url
{
    #pragma warning disable CA1055 // Uri return values should not be strings
    /// <summary>
    ///     Modified Base64 for URL applications ('base64url' encoding)
    ///     See http://tools.ietf.org/html/rfc4648
    ///     For more information see http://en.wikipedia.org/wiki/Base64
    /// </summary>
    /// <param name="input"></param>
    /// <returns>Input byte array converted to a base64ForUrl encoded string</returns>
    public static string ToBase64ForUrlString(byte[] input)
        #pragma warning restore CA1055 // Uri return values should not be strings
    {
        if (input == null)
        {
            throw new ArgumentNullException(nameof(input));
        }

        var result = new StringBuilder(Convert.ToBase64String(input).TrimEnd('='));

        result.Replace('+', '-');
        result.Replace('/', '_');

        return result.ToString();
    }

    #pragma warning disable CA1054 // Uri parameters should not be strings
    /// <summary>
    ///     Modified Base64 for URL applications ('base64url' encoding)
    ///     See http://tools.ietf.org/html/rfc4648
    ///     For more information see http://en.wikipedia.org/wiki/Base64
    /// </summary>
    /// <param name="base64ForUrlInput"></param>
    /// <returns>Input base64ForUrl encoded string as the original byte array</returns>
    public static byte[] FromBase64ForUrlString(string base64ForUrlInput)
        #pragma warning restore CA1054 // Uri parameters should not be strings
    {
        if (base64ForUrlInput is null)
        {
            throw new ArgumentNullException(nameof(base64ForUrlInput));
        }

        var padChars = base64ForUrlInput.Length % 4 == 0 ? 0 : 4 - ( base64ForUrlInput.Length % 4 );

        var result = new StringBuilder(base64ForUrlInput, base64ForUrlInput.Length + padChars);
        result.Append(string.Empty.PadRight(padChars, '='));

        result.Replace('-', '+');
        result.Replace('_', '/');

        return Convert.FromBase64String(result.ToString());
    }
}
