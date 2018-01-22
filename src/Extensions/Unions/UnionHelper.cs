using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;

namespace Rocket.Surgery.Unions
{
    public static class UnionHelper
    {
        public static Type GetUnionType(Type type, string propertyName = null)
        {
            return GetUnionType(type.GetTypeInfo(), propertyName);
        }

        public static Type GetUnionType(TypeInfo type, string propertyName = null)
        {
            return (type.IsEnum ? type : type.GetDeclaredProperty(propertyName).PropertyType.GetTypeInfo()).AsType();
        }

        /// <summary>
        /// Get's all union paires for a given union enum
        /// </summary>
        /// <param name="enumType"></param>
        /// <returns></returns>
        public static IReadOnlyDictionary<object, Type> GetUnion(Type enumType)
        {
            return GetUnion(enumType.GetTypeInfo());
        }
        /// <summary>
        /// Get's all union paires for a given union enum
        /// </summary>
        /// <param name="enumType"></param>
        /// <returns></returns>
        public static IReadOnlyDictionary<object, Type> GetUnion(TypeInfo enumType)
        {
            if (!enumType.IsEnum) throw new NotSupportedException("Type must be an enum");
            if (!enumType.GetCustomAttributes<UnionAttribute>().Any()) throw new NotSupportedException("Union enumType must have a union attribute to point at the base class");


            return enumType.DeclaredFields
                .Where(x => x.IsStatic)
                .ToDictionary(
                    x => Enum.Parse(enumType.AsType(), x.Name),
                    x => x.GetCustomAttribute<UnionAttribute>().Type);
        }

        ///  <summary>
        ///  Get's a list of all the unions in a set of assemblies
        ///
        ///  Used to create a unit tests that ensures all unions are configured properly
        ///  </summary>
        /// <param name="assembly"></param>
        /// <param name="rest"></param>
        /// <returns></returns>
        public static IEnumerable<(TypeInfo enumType, bool allImplemented)> GetAll(
            Assembly assembly, params Assembly[] rest)
        {
            return GetAll(new[] { assembly }.Concat(rest));
        }

        /// <summary>
        /// Get's a list of all the unions in a set of assemblies
        ///
        /// Used to create a unit tests that ensures all unions are configured properly
        /// </summary>
        /// <param name="assemblies"></param>
        /// <returns></returns>
        public static IEnumerable<(TypeInfo enumType, bool allImplemented)> GetAll(IEnumerable<Assembly> assemblies)
        {
            foreach (var type in assemblies.SelectMany(x =>
                {
                    try
                    {
                        return x.DefinedTypes;
                    }
                    catch
                    {
                        return Enumerable.Empty<TypeInfo>();
                    }
                })
                .Where(x => x.IsEnum && x.GetCustomAttributes<UnionAttribute>().Any())
            )
            {
                var enumType = type;
                var rootType = type.GetCustomAttribute<UnionAttribute>().Type.GetTypeInfo();

                var enumTypes = enumType
                    .DeclaredFields
                    .Where(x => x.IsStatic)
                    .Select(field => new { field, type = field.GetCustomAttribute<UnionAttribute>()?.Type?.GetTypeInfo() })
                    .ToArray();

                var allImplemented = enumTypes.All(x => x.type != null);

                yield return (enumType, allImplemented);
            }
        }
    }
}
