using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Rocket.Surgery.Unions
{
    /// <summary>
    /// union
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.Class)]
    public class UnionAttribute : Attribute
    {
        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public object Value { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnionAttribute" /> class.
        /// </summary>
        /// <param name="value">The value.</param>
        public UnionAttribute(object value)
        {
            if (value == null || !value.GetType().GetTypeInfo().IsEnum)
                throw new ArgumentOutOfRangeException(nameof(value), $"value must be an enum, got type {value?.GetType().FullName}");

            Value = value;
        }
    }
}
