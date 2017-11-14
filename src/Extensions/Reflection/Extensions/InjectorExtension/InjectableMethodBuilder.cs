using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Rocket.Surgery.Reflection.Extensions.InjectorExtension
{
    /// <summary>
    /// Class InjectableMethodBuilder.
    /// </summary>
    /// TODO Edit XML Comment Template for InjectableMethodBuilder
    public partial class InjectableMethodBuilder
    {
        private readonly MethodInfo _methodInfo;
        private readonly IEnumerable<ParameterInfo> _parameterInfos;
        private InstanceParameter _instanceParameter;
        private readonly IList<ConfiguredParameter> _configuredParameters = new List<ConfiguredParameter>();
        private readonly IList<Type> _returnTypes;

        /// <summary>
        /// Initializes a new instance of the <see cref="InjectableMethodBuilder"/> class.
        /// </summary>
        /// <param name="methodInfo">The method information.</param>
        /// TODO Edit XML Comment Template for #ctor
        internal InjectableMethodBuilder(MethodInfo methodInfo)
        {
            _methodInfo = methodInfo;
            _parameterInfos = methodInfo.GetParameters();
            _returnTypes = new List<Type>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InjectableMethodBuilder"/> class.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// TODO Edit XML Comment Template for #ctor
        protected InjectableMethodBuilder(InjectableMethodBuilder builder)
        {
            _methodInfo = builder._methodInfo;
            _parameterInfos = builder._parameterInfos.ToArray();
            _configuredParameters = builder._configuredParameters.ToList();
            _returnTypes = builder._returnTypes.ToList();
            _instanceParameter = builder._instanceParameter;
        }

        /// <summary>
        /// Configures the parameter.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <param name="optional">if set to <c>true</c> [optional].</param>
        /// <returns>InjectableMethodBuilder.</returns>
        /// <exception cref="System.MissingMemberException"></exception>
        /// TODO Edit XML Comment Template for ConfigureParameter
        public InjectableMethodBuilder ConfigureParameter(Func<ParameterInfo, bool> predicate, bool optional = false)
        {
            // lack of optional means its required.
            var param = _parameterInfos.SingleOrDefault(predicate);
            if (param == null && !optional)
            {
                throw new MissingMemberException($"Could not find injectable property for predicate '{predicate}'");
            }

            if (param != null)
                _configuredParameters.Add(new ConfiguredParameter(param, optional));

            return new InjectableMethodBuilder(this);
        }

        /// <summary>
        /// Configures the parameter.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="optional">if set to <c>true</c> [optional].</param>
        /// <returns>InjectableMethodBuilder.</returns>
        /// <exception cref="System.MissingMemberException"></exception>
        /// TODO Edit XML Comment Template for ConfigureParameter
        public InjectableMethodBuilder ConfigureParameter(Type type, bool optional = false)
        {
            // lack of optional means its required.
            var param = _parameterInfos.SingleOrDefault(x => x.ParameterType == type);
            if (param == null && !optional)
            {
                throw new MissingMemberException($"Could not find injectable property for type '{type.FullName}'");
            }

            if (param != null)
                _configuredParameters.Add(new ConfiguredParameter(param, optional));

            return new InjectableMethodBuilder(this);
        }

        /// <summary>
        /// Configures the parameter.
        /// </summary>
        /// <param name="optional">if set to <c>true</c> [optional].</param>
        /// <returns>InjectableMethodBuilder.</returns>
        /// <exception cref="System.MissingMemberException"></exception>
        /// TODO Edit XML Comment Template for ConfigureParameter
        public InjectableMethodBuilder ConfigureParameter<T>(bool optional = false)
        {
            return ConfigureParameter(typeof(T), optional);
        }

        /// <summary>
        /// Configures the instance parameter.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="predicate">The predicate.</param>
        /// <param name="optional">if set to <c>true</c> [optional].</param>
        /// <returns>InjectableMethodBuilder.</returns>
        /// TODO Edit XML Comment Template for ConfigureInstanceParameter
        public InjectableMethodBuilder ConfigureInstanceParameter(Type type, Func<object, Func<ParameterInfo, bool>> predicate, bool optional = false)
        {
            // lack of optional means its required.
            var param = _parameterInfos.SingleOrDefault(x => x.ParameterType == type);
            if (param != null)
            {
                _configuredParameters.Add(new ConfiguredParameter(param, optional));
            }
            else
            {
                _instanceParameter = new InstanceParameter(type, predicate, optional);
            }

            return new InjectableMethodBuilder(this);
        }

        /// <summary>
        /// Configures the instance parameter.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <param name="optional">if set to <c>true</c> [optional].</param>
        /// <returns>InjectableMethodBuilder.</returns>
        /// TODO Edit XML Comment Template for ConfigureInstanceParameter
        public InjectableMethodBuilder ConfigureInstanceParameter<T>(Func<object, Func<ParameterInfo, bool>> predicate, bool optional = false)
        {
            return ConfigureInstanceParameter(typeof(T), predicate, optional);
        }

        /// <summary>
        /// Configures the instance parameter.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <param name="optional">if set to <c>true</c> [optional].</param>
        /// <returns>InjectableMethodBuilder.</returns>
        /// TODO Edit XML Comment Template for ConfigureInstanceParameter
        public InjectableMethodBuilder ConfigureInstanceParameter(Func<object, Func<ParameterInfo, bool>> predicate, bool optional = false)
        {
            _instanceParameter = new InstanceParameter(null, predicate, optional);
            return new InjectableMethodBuilder(this);
        }

        /// <summary>
        /// Configures the instance parameter.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="optional">if set to <c>true</c> [optional].</param>
        /// <returns>InjectableMethodBuilder.</returns>
        /// TODO Edit XML Comment Template for ConfigureInstanceParameter
        public InjectableMethodBuilder ConfigureInstanceParameter(Type type, bool optional = false)
        {
            ConfigureParameter(type, optional);
            return new InjectableMethodBuilder(this);
        }

        /// <summary>
        /// Configures the instance parameter.
        /// </summary>
        /// <param name="optional">if set to <c>true</c> [optional].</param>
        /// <returns>InjectableMethodBuilder.</returns>
        /// TODO Edit XML Comment Template for ConfigureInstanceParameter
        public InjectableMethodBuilder ConfigureInstanceParameter<T>(bool optional = false)
        {
            return ConfigureInstanceParameter(typeof(T), optional);
        }

        /// <summary>
        /// Returns the type.
        /// </summary>
        /// <param name="returnTypes">The return types.</param>
        /// <returns>InjectableMethodBuilder.</returns>
        /// TODO Edit XML Comment Template for ReturnType
        public InjectableMethodBuilder ReturnType(params Type[] returnTypes)
        {
            foreach (var item in returnTypes)
                _returnTypes.Add(item);

            return new InjectableMethodBuilder(this);
        }

        /// <summary>
        /// Validates the type of the return.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">Method must return at least one of the declared return types.</exception>
        /// TODO Edit XML Comment Template for ValidateReturnType
        internal void ValidateReturnType()
        {
            if (!_returnTypes.Any(z => z.GetTypeInfo().IsAssignableFrom(_methodInfo.ReturnType.GetTypeInfo())))
            {
                throw new InvalidOperationException("Method must return at least one of the declared return types.");
            }
        }
    }
}
