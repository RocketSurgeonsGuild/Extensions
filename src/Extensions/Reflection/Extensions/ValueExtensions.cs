using System;
using System.Linq.Expressions;

namespace Rocket.Surgery.Reflection.Extensions
{
    public static class ValueExtensions
    {
        public static T SetBackingValue<T, V>(this T instance, Expression<Func<T, V>> expression, V value)
        {
            BackingFieldHelper.Instance.SetBackingField(instance, expression, value);
            return instance;
        }
    }
}
