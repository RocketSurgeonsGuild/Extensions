using System.Linq;
using System.Reflection;

namespace Rocket.Surgery.Reflection.Extensions
{
    /// <summary>
    /// TypeInfoExtensions.
    /// </summary>
    /// TODO Edit XML Comment Template for TypeInfoExtensions
    public static class TypeInfoExtensions
    {
        /// <summary>
        /// Finds the declared property.
        /// </summary>
        /// <param name="typeInfo">The type information.</param>
        /// <param name="name">The name.</param>
        /// <returns>PropertyInfo.</returns>
        /// TODO Edit XML Comment Template for FindDeclaredProperty
        public static PropertyInfo? FindDeclaredProperty(this TypeInfo? typeInfo, string name)
        {
            while (typeInfo != null)
            {
                var property = typeInfo.GetDeclaredProperty(name);
                if (property != null)
                    return property;

                typeInfo = typeInfo?.BaseType?.GetTypeInfo();
            }

            return null;
        }
    }
}
