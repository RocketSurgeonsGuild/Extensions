namespace Rocket.Surgery.Unions;

/// <summary>
///     Union key
/// </summary>
/// <seealso cref="System.Attribute" />
[AttributeUsage(AttributeTargets.Class)]
public sealed class UnionKeyAttribute : Attribute
{
    /// <summary>
    ///     Gets the key.
    /// </summary>
    /// <value>
    ///     The key.
    /// </value>
    public string Key { get; }

    /// <summary>
    ///     Initializes a new instance of the <see cref="UnionKeyAttribute" /> class.
    /// </summary>
    /// <param name="key">The key.</param>
    public UnionKeyAttribute(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentOutOfRangeException(nameof(key), "key must be a string");

        Key = key;
    }
}
