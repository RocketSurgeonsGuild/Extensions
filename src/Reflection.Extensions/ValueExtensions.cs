using System;
using System.Linq.Expressions;
using JetBrains.Annotations;

namespace Rocket.Surgery.Reflection
{
    /// <summary>
    /// Set backing value
    /// </summary>
    [PublicAPI]
    public static class ValueExtensions
    {
        /// <summary>
        /// Sets the backing value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TV"></typeparam>
        /// <param name="instance">The instance.</param>
        /// <param name="expression">The expression.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static T SetBackingValue<T, TV>(this T instance, Expression<Func<T, TV>> expression, TV value)
        {
            BackingFieldHelper.Instance.SetBackingField(instance, expression, value);
            return instance;
        }
    }
}
