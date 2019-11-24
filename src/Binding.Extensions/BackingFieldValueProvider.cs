using System;
using System.Reflection;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Rocket.Surgery.Binding
{
    /// <summary>
    /// Allows Newtonsoft.Json to set the underlying backing field for a given readonly autoprop
    /// </summary>
    [PublicAPI]
    public class BackingFieldValueProvider : IValueProvider
    {
        private readonly FieldInfo _backingField;
        private readonly MemberInfo _memberInfo;

        /// <summary>
        /// Initializes a new instance of the <see cref="BackingFieldValueProvider"/> class.
        /// </summary>
        /// <param name="memberInfo">The member information.</param>
        /// <param name="backingField">The backing field.</param>
        public BackingFieldValueProvider(MemberInfo memberInfo, FieldInfo backingField)
        {
            _backingField = backingField;
            _memberInfo = memberInfo;
        }

        /// <inheritdoc />
        public void SetValue([NotNull] object target, object? value)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            try
            {
                _backingField.SetValue(target, value);
            }
            catch (Exception ex)
            {
                throw new JsonSerializationException($"Error setting value to '{_memberInfo.Name}' on '{target.GetType()}'.", ex);
            }
        }

        /// <inheritdoc />
        public object GetValue([NotNull] object target)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            try
            {
                return _backingField.GetValue(target);
            }
            catch (Exception ex)
            {
                throw new JsonSerializationException($"Error getting value from '{_memberInfo.Name}' on '{target.GetType()}'.", ex);
            }
        }
    }
}
