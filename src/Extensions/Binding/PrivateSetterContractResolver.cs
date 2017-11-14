using System.Reflection;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Rocket.Surgery.Binding
{
    /// <summary>
    /// Class PrivateSetterContractResolver.
    /// </summary>
    /// <seealso cref="Newtonsoft.Json.Serialization.DefaultContractResolver" />
    /// TODO Edit XML Comment Template for PrivateSetterContractResolver
    public class PrivateSetterContractResolver : DefaultContractResolver
    {
        /// <summary>
        /// Creates a <see cref="T:Newtonsoft.Json.Serialization.JsonProperty" /> for the given <see cref="T:System.Reflection.MemberInfo" />.
        /// </summary>
        /// <param name="member">The member to create a <see cref="T:Newtonsoft.Json.Serialization.JsonProperty" /> for.</param>
        /// <param name="memberSerialization">The member's parent <see cref="T:Newtonsoft.Json.MemberSerialization" />.</param>
        /// <returns>A created <see cref="T:Newtonsoft.Json.Serialization.JsonProperty" /> for the given <see cref="T:System.Reflection.MemberInfo" />.</returns>
        /// TODO Edit XML Comment Template for CreateProperty
        protected override JsonProperty CreateProperty(
            MemberInfo member,
            MemberSerialization memberSerialization)
        {
            var prop = base.CreateProperty(member, memberSerialization);
            if (!prop.Writable)
            {
                if (member is PropertyInfo property)
                {
                    var hasPrivateSetter = property.SetMethod != null;
                    if (!hasPrivateSetter)
                    {
                        var backingField = GetBackingField(property);
                        if (backingField != null)
                        {
                            hasPrivateSetter = true;
                            prop.ValueProvider = new BackingFieldValueProvider(member, backingField);
                        }
                    }
                    prop.Writable = hasPrivateSetter;
                }
            }

            return prop;
        }

        private static FieldInfo GetBackingField(PropertyInfo pi)
        {
            if (!pi.CanRead || !pi.GetMethod.IsDefined(typeof(CompilerGeneratedAttribute), inherit: true))
                return null;
            var backingField = pi.DeclaringType.GetTypeInfo().GetDeclaredField($"<{pi.Name}>k__BackingField");
            if (backingField == null)
                return null;
            if (!backingField.IsDefined(typeof(CompilerGeneratedAttribute), inherit: true))
                return null;
            return backingField;
        }
    }
}
