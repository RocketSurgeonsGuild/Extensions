using System;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Rocket.Surgery.Binding
{
    /// <summary>
    /// Class BackingFieldValueProvider.
    /// </summary>
    /// <seealso cref="Newtonsoft.Json.Serialization.IValueProvider" />
    /// TODO Edit XML Comment Template for BackingFieldValueProvider
    public class BackingFieldValueProvider : IValueProvider
    {
        private readonly FieldInfo _backingField;
        private readonly MemberInfo _memberInfo;

        /// <summary>
        /// Initializes a new instance of the <see cref="BackingFieldValueProvider"/> class.
        /// </summary>
        /// <param name="memberInfo">The member information.</param>
        /// <param name="backingField">The backing field.</param>
        /// TODO Edit XML Comment Template for #ctor
        public BackingFieldValueProvider(MemberInfo memberInfo, FieldInfo backingField)
        {
            _backingField = backingField;
            _memberInfo = memberInfo;
        }

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="target">The target to set the value on.</param>
        /// <param name="value">The value to set on the target.</param>
        /// <exception cref="Newtonsoft.Json.JsonSerializationException"></exception>
        /// TODO Edit XML Comment Template for SetValue
        public void SetValue(object target, object value)
        {
            try
            {
                _backingField.SetValue(target, value);
            }
            catch (Exception ex)
            {
                throw new JsonSerializationException($"Error setting value to '{_memberInfo.Name}' on '{target.GetType()}'.", ex);
            }
        }

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <param name="target">The target to get the value from.</param>
        /// <returns>The value.</returns>
        /// <exception cref="Newtonsoft.Json.JsonSerializationException"></exception>
        /// TODO Edit XML Comment Template for GetValue
        public object GetValue(object target)
        {
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
