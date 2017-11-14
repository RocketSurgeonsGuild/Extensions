using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Rocket.Surgery.Reflection.Extensions.InjectorExtension
{
    /// <summary>
    /// Class InjectableMethod.
    /// </summary>
    /// TODO Edit XML Comment Template for InjectableMethod
    internal partial class InjectableMethod
    {
        private readonly bool _voidReturnType;
        private readonly MethodInfo _methodInfo;
        private InstanceParameter _instanceParameter;
        private IEnumerable<ParameterInfo> _injectableParameters;
        private IEnumerable<ConfiguredParameter> _configuredParameters;
        private readonly TypeInfo _methodReturnTypeInfo;
        private readonly int _numParams;
        private readonly IDictionary<TypeInfo, ConfiguredParameter> _resolvedConfiguredParameter;

        /// <summary>
        /// Initializes a new instance of the <see cref="InjectableMethod"/> class.
        /// </summary>
        /// <param name="methodInfo">The method information.</param>
        /// <param name="instanceParameter">The instance parameter.</param>
        /// <param name="configuredParameters">The configured parameters.</param>
        /// <param name="resolvedConfiguredParameter">The resolved configured parameter.</param>
        /// <exception cref="System.Exception">Instance param requires a predicate if it is not well defined.</exception>
        /// TODO Edit XML Comment Template for #ctor
        internal InjectableMethod(
             MethodInfo methodInfo,
             InstanceParameter instanceParameter,
             IEnumerable<ConfiguredParameter> configuredParameters,
             IDictionary<TypeInfo, ConfiguredParameter> resolvedConfiguredParameter)
        {
            _voidReturnType = methodInfo.ReturnType == typeof(void);
            _methodInfo = methodInfo;
            _instanceParameter = instanceParameter;
            _configuredParameters = configuredParameters as ConfiguredParameter[] ?? configuredParameters.ToArray();
            _numParams = methodInfo.GetParameters().Count();
            _resolvedConfiguredParameter = resolvedConfiguredParameter;
            _methodReturnTypeInfo = _methodInfo.ReturnType.GetTypeInfo();

            if (_instanceParameter?.ParameterInfo != null)
            {
                _configuredParameters = _configuredParameters.Union(new[] { _instanceParameter });
                _instanceParameter = null;
            }
            if (_instanceParameter != null && _instanceParameter.Predicate == null)
                throw new Exception("Instance param requires a predicate if it is not well defined.");

            _injectableParameters = methodInfo.GetParameters().Except(_configuredParameters.Select(x => x.ParameterInfo)).ToArray();
        }

        private void UpdateInstanceParameter<T>(T instance)
        {
            if (_instanceParameter == null || instance == null) return;
            if (_instanceParameter.Predicate != null)
            {
                var type = instance?.GetType() ?? typeof(T);
                var typeInfo = type.GetTypeInfo();

                var param = _injectableParameters.SingleOrDefault(_instanceParameter.Predicate(type));

                if (param != null)
                {
                    _configuredParameters = _configuredParameters.Union(new[] { new InstanceParameter(param, param.IsOptional) });
                    _injectableParameters = _injectableParameters.Except(_configuredParameters.Select(x => x.ParameterInfo));

                    if (_resolvedConfiguredParameter.ContainsKey(typeInfo))
                        _resolvedConfiguredParameter.Remove(typeInfo);
                }

                if (param == null)
                {
                    if (!_instanceParameter.Optional)
                        throw new InvalidOperationException(
                            $"Missing required parameter type '{_instanceParameter.TypeInfo.FullName}'");
                }
            }

            _instanceParameter = null;
        }

        /// <summary>
        /// Injects the parameters.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        /// <param name="parameters">The parameters.</param>
        /// <exception cref="System.Exception">Could not resolve required service</exception>
        /// <exception cref="System.InvalidOperationException"></exception>
        /// TODO Edit XML Comment Template for InjectParameters
        protected virtual void InjectParameters(IServiceProvider serviceProvider, object[] parameters)
        {
            foreach (var parameterInfo in _injectableParameters)
            {
                if (parameterInfo.IsOptional)
                {
                    parameters[parameterInfo.Position] = serviceProvider.GetService(parameterInfo.ParameterType);
                }
                else
                {
                    try
                    {
                        // ReSharper disable once SuspiciousTypeConversion.Global
                            var service = serviceProvider.GetService(parameterInfo.ParameterType);
                            if (service == null)
                                throw new Exception("Could not resolve required service");
                            parameters[parameterInfo.Position] = serviceProvider.GetService(parameterInfo.ParameterType);
                    }
                    catch (Exception)
                    {
                        throw new InvalidOperationException(
                            $"Unable to resolve required service for {_methodInfo.Name} method {parameterInfo.Name} {parameterInfo.ParameterType.FullName}");
                    }
                }
            }
        }

        private void SetConfiguredParameter<T>(object[] parameters, T value)
        {
            UpdateInstanceParameter(value);
            var configuredParam = _resolvedConfiguredParameter.GetConfigureParameter(value?.GetType() ?? typeof(T), _configuredParameters);
            if (configuredParam != null)
            {
                parameters[configuredParam.ParameterInfo.Position] = value;
            }
        }

        private static readonly MethodInfo EmptyMethod = typeof(Enumerable).GetTypeInfo().DeclaredMethods.Single(x => x.Name == nameof(Enumerable.Empty));
        private static TReturn GetDefaultReturnValue<TReturn>(Type type)
        {
            return GetDefaultReturnValue<TReturn>(type.GetTypeInfo());
        }

        private static TReturn GetDefaultReturnValue<TReturn>(TypeInfo typeInfo)
        {
            // type is IEnumerable<T>;
            if (typeInfo.IsGenericType && typeInfo.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                var enumType = typeInfo.GenericTypeArguments[0];
                return (TReturn)EmptyMethod.MakeGenericMethod(enumType).Invoke(null, new object[] { });
            }

            return default(TReturn);
        }

        private TReturn GetResult<TReturn>(object container, object[] parameters, TReturn returnDefault, TypeInfo returnTypeInfo)
        {
            if (!_voidReturnType && returnTypeInfo.IsInterface && _methodReturnTypeInfo.ImplementedInterfaces.Contains(returnTypeInfo.AsType()) || _methodReturnTypeInfo.IsAssignableFrom(returnTypeInfo))
            {
                return (TReturn)_methodInfo.Invoke(container, parameters);
            }

            _methodInfo.Invoke(container, parameters);
            return returnDefault;
        }

        private void ExecuteResult(object container, object[] parameters)
        {
            try
            {
                _methodInfo.Invoke(container, parameters);
            }
            catch (TargetInvocationException e)
            {
                throw e.InnerException;
            }
        }
    }
}
