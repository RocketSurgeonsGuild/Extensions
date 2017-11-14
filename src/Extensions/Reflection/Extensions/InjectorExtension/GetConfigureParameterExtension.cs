using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Rocket.Surgery.Reflection.Extensions.InjectorExtension
{
    /// <summary>
    /// Class GetConfigureParameterExtension.
    /// </summary>
    /// TODO Edit XML Comment Template for GetConfigureParameterExtension
    internal static class GetConfigureParameterExtension
    {
        /// <summary>
        /// Gets the configure parameter.
        /// </summary>
        /// <param name="resolvedConfiguredParameter">The resolved configured parameter.</param>
        /// <param name="type">The type.</param>
        /// <param name="configuredParameters">The configured parameters.</param>
        /// <returns>ConfiguredParameter.</returns>
        /// TODO Edit XML Comment Template for GetConfigureParameter
        internal static ConfiguredParameter GetConfigureParameter(this IDictionary<TypeInfo, ConfiguredParameter> resolvedConfiguredParameter, Type type, IEnumerable<ConfiguredParameter> configuredParameters)
        {
            var typeInfo = type?.GetTypeInfo();
            ConfiguredParameter configuredParam = null;
            if (typeInfo != null && !resolvedConfiguredParameter.TryGetValue(typeInfo, out configuredParam))
            {
                configuredParam = configuredParameters.SingleOrDefault(x => type != typeof(object) && x.ParameterInfo.ParameterType != typeof(object) && IntrospectionExtensions.GetTypeInfo(x.ParameterInfo.ParameterType).IsAssignableFrom(typeInfo) || typeInfo == IntrospectionExtensions.GetTypeInfo(x.ParameterInfo.ParameterType));
                resolvedConfiguredParameter.Add(typeInfo, configuredParam);
            }

            return configuredParam;
        }
    }
}
