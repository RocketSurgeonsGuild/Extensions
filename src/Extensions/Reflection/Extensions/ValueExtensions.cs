using System;
using System.Linq.Expressions;

namespace Rocket.Surgery.Reflection.Extensions
{
    /// <summary>
    /// Set backing value
    /// </summary>
    public static class ValueExtensions
    {
        /// <summary>
        /// Sets the backing value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="V"></typeparam>
        /// <param name="instance">The instance.</param>
        /// <param name="expression">The expression.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static T SetBackingValue<T, V>(this T instance, Expression<Func<T, V>> expression, V value)
        {
            BackingFieldHelper.Instance.SetBackingField(instance, expression, value);
            return instance;
        }
    }
}
