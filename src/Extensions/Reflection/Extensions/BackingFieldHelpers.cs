using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Rocket.Surgery.Reflection.Extensions
{
    public class BackingFieldHelper
    {
        public static BackingFieldHelper Instance { get; } = new BackingFieldHelper();
        private readonly ConcurrentDictionary<(Type, string), FieldInfo> _backingFields = new ConcurrentDictionary<(Type, string), FieldInfo>();
        private FieldInfo GetBackingField(PropertyInfo pi)
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

        private FieldInfo GetBackingField(Type objectType, Type interfaceType, string name)
        {
            if (!_backingFields.TryGetValue((objectType, name), out var backingField))
            {
                var property = objectType.GetTypeInfo().GetProperty($"{interfaceType.FullName.Replace("+", ".")}.{name}", BindingFlags.NonPublic | BindingFlags.Instance);
                if (property == null)
                    property = objectType.GetTypeInfo().GetProperty(name);

                backingField = GetBackingField(property);
                _backingFields.TryAdd((objectType, name), backingField);
            }
            if (backingField is null)
                throw new NotSupportedException("Given Expression is not supported");
            return backingField;
        }

        public FieldInfo GetBackingField<TInterface, TValue>(Type type, Expression<Func<TInterface, TValue>> expression)
        {
            if (expression.Body is MemberExpression exp)
            {
                return GetBackingField(type, typeof(TInterface), exp.Member.Name);
            }
            throw new NotSupportedException("Given Expression is not supported");
        }

        public void SetBackingField<TInterface, TValue>(TInterface instance, Expression<Func<TInterface, TValue>> expression, TValue value)
        {
            if (expression.Body is MemberExpression exp)
            {
                var field = GetBackingField(instance.GetType(), expression);
                field.SetValue(instance, value);
                return;
            }
            throw new NotSupportedException("Given Expression is not supported");
        }
    }
}
