using System.Reflection;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Rocket.Surgery.Binding;

/// <summary>
///     Allows Newtonsoft.Json to set the underlying backing field for a given readonly autoprop
/// </summary>
/// <remarks>
///     Initializes a new instance of the <see cref="BackingFieldValueProvider" /> class.
/// </remarks>
/// <param name="memberInfo">The member information.</param>
/// <param name="backingField">The backing field.</param>
[PublicAPI]
public class BackingFieldValueProvider(MemberInfo memberInfo, FieldInfo backingField) : IValueProvider
{
    /// <inheritdoc />
    public object? GetValue(object target)
    {
        ArgumentNullException.ThrowIfNull(target);

        try
        {
            return _backingField.GetValue(target);
        }
        catch (Exception ex)
        {
            throw new JsonSerializationException($"Error getting value from '{_memberInfo.Name}' on '{target.GetType()}'.", ex);
        }
    }

    /// <inheritdoc />
    public void SetValue(object target, object? value)
    {
        ArgumentNullException.ThrowIfNull(target);

        try
        {
            _backingField.SetValue(target, value);
        }
        catch (Exception ex)
        {
            throw new JsonSerializationException($"Error setting value to '{_memberInfo.Name}' on '{target.GetType()}'.", ex);
        }
    }

    private readonly FieldInfo _backingField = backingField;
    private readonly MemberInfo _memberInfo = memberInfo;
}
