using System;
using System.Text;

namespace Rocket.Surgery.Unions
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Enum)]
    public class UnionAttribute : Attribute
    {
        public Type Type { get; }
        public UnionAttribute(Type type)
        {
            Type = type;
        }
    }
}
