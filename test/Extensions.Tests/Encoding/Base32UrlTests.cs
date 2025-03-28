using Rocket.Surgery.Extensions.Encoding;
using Rocket.Surgery.Extensions.Testing;

namespace Rocket.Surgery.Extensions.Tests.Encoding;

/// <summary>
///     Basic unit tests covering key functional variants of base 32 encoding / decoding.
///     TODO: Anyone feels like it some more comprehensive testing of the crockford encoding would be helpful. Cheers, Mhano
///     TODO: Tests evolved a bit over time, refactoring to organise might be needed if adding significant test cases.
/// </summary>
public class Base32UrlTests() : AutoFakeTest(Defaults.LoggerTest)
{
    private static readonly string[][] rfc4684TestVectors =
    [
        ["f", "MY======"],
        ["fo", "MZXQ===="],
        ["foo", "MZXW6==="],
        ["foob", "MZXW6YQ="],
        ["fooba", "MZXW6YTB"],
        ["foobar", "MZXW6YTBOI======"],
        ["", ""],
    ];

    [Test]
    public async Task Rfc4648TestVectorsEncodeDecode()
    {
        var enc = new Base32Url(true, true, false);

        foreach (var s in rfc4684TestVectors)
        {
            await Assert.That(enc.Encode(System.Text.Encoding.ASCII.GetBytes(s[0]))).IsEqualTo(s[1]);
            await Assert.That(System.Text.Encoding.ASCII.GetString(enc.Decode(s[1]))).IsEqualTo(s[0]);
        }
    }

    [Test]
    public async Task IgnoreWs()
    {
        await Assert
             .That(System.Text.Encoding.ASCII.GetString(new Base32Url(false, true, true).Decode("JBS\t\r\n WY3D\tPE BLW64\n TMM\r QQQ")))
             .IsEqualTo("Hello World!");
    }

    [Test]
    public async Task NoPaddingEncodeDecode()
    {
        var enc = new Base32Url(false, true, false);

        foreach (var s in rfc4684TestVectors)
        {
            await Assert.That(System.Text.Encoding.ASCII.GetString(enc.Decode(s[1].TrimEnd('=')))).IsEqualTo(s[0]);
            await Assert.That(enc.Encode(System.Text.Encoding.ASCII.GetBytes(s[0]))).IsEqualTo(s[1].TrimEnd('='));
        }
    }

    [Test]
    public async Task CaseInsensitiveDecode()
    {
        var enc = new Base32Url(true, false, false);

        foreach (var s in rfc4684TestVectors)
        {
            #pragma warning disable CA1308 // Normalize strings to uppercase
            var encodedS1 = s[1].ToLowerInvariant();
            #pragma warning restore CA1308 // Normalize strings to uppercase
            await Assert.That(System.Text.Encoding.ASCII.GetString(enc.Decode(encodedS1))).IsEqualTo(s[0]);
        }
    }

    [Test]
    public async Task NoPaddingCaseSensitiveCustomAlphabetsEncodeDecode()
    {
        // no vowels (prevents accidental profanity)
        // no numbers or letters easily mistakable

        foreach (var alphabet in new[] { "BCDFGHKMNPQRSTVWXYZ23456789bcdfg", "BCDFGHKMNPQRSTVWXYZbcdfghkmnpqrs" })
        {
            var enc = new Base32Url(true, true, false, alphabet);

            foreach (var s in rfc4684TestVectors)
            {
                await Assert.That(System.Text.Encoding.ASCII.GetString(enc.Decode(enc.Encode(System.Text.Encoding.ASCII.GetBytes(s[0]))))).IsEqualTo(s[0]);
            }
        }
    }

    [Test]
    public async Task NoPaddingCaseInSensitiveCustomAlphabetsEncodeDecode()
    {
        // no vowels (prevents accidental profanity)
        // no numbers or letters easily mistakable

        foreach (var alphabet in new[] { "BCDFGHKMNPQRSTVWXYZ23456789~!@#$" })
        {
            var enc1 = new Base32Url(true, false, false, alphabet);
            var enc2 = new Base32Url(true, false, false, alphabet.ToLower());

            foreach (var s in rfc4684TestVectors)
            {
                await Assert.That(System.Text.Encoding.ASCII.GetString(enc2.Decode(enc1.Encode(System.Text.Encoding.ASCII.GetBytes(s[0]))))).IsEqualTo(s[0]);
                await Assert.That(System.Text.Encoding.ASCII.GetString(enc1.Decode(enc2.Encode(System.Text.Encoding.ASCII.GetBytes(s[0]))))).IsEqualTo(s[0]);
            }
        }
    }

    [Test]
    public async Task SequentialValuesSequentialLengths()
    {
        var bcv = new Base32Url(false, false, false);

        for (var j = 1; j < 11; j++)
        {
            var d = new byte[j];

            for (byte b = 0; b < 255; b++)
            {
                for (var i = 0; i < d.Length; i++)
                {
                    d[i] = (byte)( ( b + i ) % 255 );
                }

                var r = bcv.Decode(bcv.Encode(d));
                var tcnt = -1;

                var result = Array.TrueForAll(
                    d,
                    _ =>
                    {
                        tcnt++;
                        return r[tcnt] == d[tcnt];
                    }
                );

                await Assert.That(result).IsTrue();
            }
        }
    }
}
