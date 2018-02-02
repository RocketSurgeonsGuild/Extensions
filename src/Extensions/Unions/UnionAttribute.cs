using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Rocket.Surgery.Unions
{
    [AttributeUsage(AttributeTargets.Class)]
    public class UnionAttribute : Attribute
    {
        public object Value { get; }

        public UnionAttribute(object value)
        {
            if (value == null || !value.GetType().GetTypeInfo().IsEnum)
                throw new ArgumentOutOfRangeException(nameof(value), $"value must be an enum, got type {value.GetType().FullName}");

            Value = value;
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class UnionKeyAttribute : Attribute
    {
        public string Key { get; }

        public UnionKeyAttribute(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentOutOfRangeException(nameof(key), "key must be a string");

            Key = key;
        }
    }
}
