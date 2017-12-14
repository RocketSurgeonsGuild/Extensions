using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Rocket.Surgery.Reflection.Extensions;

namespace Rocket.Surgery.Binding
{
    /// <summary>
    /// Class PrivateSetterContractResolver.
    /// </summary>
    /// <seealso cref="Newtonsoft.Json.Serialization.DefaultContractResolver" />
    /// TODO Edit XML Comment Template for PrivateSetterContractResolver
    public class PrivateSetterContractResolver : DefaultContractResolver
    {
        /// <inheritdoc />
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var prop = base.CreateProperty(member, memberSerialization);

            if (prop.ValueProvider is BackingFieldValueProvider)
            {
                prop.Writable = true;
            }

            return prop;
        }

        /// <inheritdoc />
        protected override IValueProvider CreateMemberValueProvider(MemberInfo member)
        {
            if (member is PropertyInfo property)
            {
                var hasPrivateSetter = property.SetMethod != null;
                if (!hasPrivateSetter)
                {
                    var backingField = GetBackingField(property);
                    if (backingField != null)
                    {
                        return new BackingFieldValueProvider(member, backingField);
                    }
                }
            }
            return base.CreateMemberValueProvider(member);
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
