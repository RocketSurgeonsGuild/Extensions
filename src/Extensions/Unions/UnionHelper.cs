using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;

namespace Rocket.Surgery.Unions
{
    public static class UnionHelper
    {
        public static Type GetUnionEnumType(Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            return GetUnionEnumType(type.GetTypeInfo());
        }

        public static Type GetUnionEnumType(TypeInfo type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

            type = GetRootType(type);

            if (type.GetCustomAttribute<UnionKeyAttribute>(false) == null)
                throw new ArgumentOutOfRangeException(nameof(type), "type must have a union key attribute");
            return type.GetDeclaredProperty(type.GetCustomAttribute<UnionKeyAttribute>(false).Key).PropertyType;
        }

        public static Type GetUnionEnumType(Type type, string propertyName)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            if (string.IsNullOrWhiteSpace(propertyName))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(propertyName));
            return GetUnionEnumType(type.GetTypeInfo(), propertyName);
        }

        public static Type GetUnionEnumType(TypeInfo type, string propertyName)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

            type = GetRootType(type);

            if (string.IsNullOrWhiteSpace(propertyName))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(propertyName));
            return type.GetDeclaredProperty(propertyName).PropertyType;
        }

        public static TypeInfo GetRootType(Type type)
        {
            return GetRootType(type.GetTypeInfo());
        }

        public static TypeInfo GetRootType(TypeInfo typeInfo)
        {
            var rootType = typeInfo;
            while (rootType != null)
            {
                if (rootType.GetCustomAttributes<UnionKeyAttribute>(false)?.Any() == true) break;
                rootType = rootType.BaseType?.GetTypeInfo();
            }

            return rootType;
        }

        /// <summary>
        /// Get's all union paires for a given union enum
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static IReadOnlyDictionary<object, Type> GetUnion(Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            return GetUnion(type.GetTypeInfo());
        }

        /// <summary>
        /// Get's all union paires for a given union enum
        /// </summary>
        /// <param name="typeInfo"></param>
        /// <returns></returns>
        public static IReadOnlyDictionary<object, Type> GetUnion(TypeInfo typeInfo)
        {
            if (typeInfo == null) throw new ArgumentNullException(nameof(typeInfo));
            return typeInfo.Assembly.DefinedTypes
                .Where(typeInfo.IsAssignableFrom)
                .Where(x => !x.IsAbstract)
                .Select(type => (type, @enum: type.GetCustomAttribute<UnionAttribute>()?.Value))
                .ToDictionary(
                    x => x.@enum,
                    x => x.type.AsType()
                );
        }

        ///  <summary>
        ///  Get's a list of all the unions in a set of assemblies
        ///
        ///  Used to create a unit tests that ensures all unions are configured properly
        ///  </summary>
        /// <param name="assembly"></param>
        /// <param name="rest"></param>
        /// <returns></returns>
        public static IEnumerable<(TypeInfo enumType, TypeInfo rootType, bool allImplemented)> GetAll(
            Assembly assembly, params Assembly[] rest)
        {
            if (assembly == null) throw new ArgumentNullException(nameof(assembly));
            return GetAll(new[] { assembly }.Concat(rest));
        }

        /// <summary>
        /// Get's a list of all the unions in a set of assemblies
        ///
        /// Used to create a unit tests that ensures all unions are configured properly
        /// </summary>
        /// <param name="assemblies"></param>
        /// <returns></returns>
        public static IEnumerable<(TypeInfo enumType, TypeInfo rootType, bool allImplemented)> GetAll(IEnumerable<Assembly> assemblies)
        {
            foreach (var rootType in assemblies.SelectMany(x =>
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
                .Where(x => x.GetCustomAttributes<UnionKeyAttribute>().Any())
                .Where(x => x.IsAbstract)
            )
            {
                var enumType = GetUnionEnumType(rootType);

                var types = rootType.Assembly.DefinedTypes
                    .Where(rootType.IsAssignableFrom)
                    .Where(x => x.GetCustomAttributes<UnionAttribute>().Any())
                    .Where(x => !x.IsAbstract)
                    .Select(x => x.GetCustomAttribute<UnionAttribute>().Value)
                    .Distinct()
                    .ToArray();

                var values = Enum.GetValues(enumType)
                    .Cast<object>()
                    .ToArray();

                yield return (enumType.GetTypeInfo(), rootType, types.Length == values.Length);
            }
        }
    }
}
