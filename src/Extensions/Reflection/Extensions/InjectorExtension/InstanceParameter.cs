using System;
using System.Reflection;

namespace Rocket.Surgery.Reflection.Extensions.InjectorExtension
{
    /// <summary>
    /// Class InstanceParameter.
    /// </summary>
    /// <seealso cref="ConfiguredParameter" />
    /// TODO Edit XML Comment Template for InstanceParameter
    class InstanceParameter : ConfiguredParameter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InstanceParameter"/> class.
        /// </summary>
        /// <param name="parameterInfo">The parameter information.</param>
        /// <param name="optional">if set to <c>true</c> [optional].</param>
        /// TODO Edit XML Comment Template for #ctor
        internal InstanceParameter(ParameterInfo parameterInfo, bool optional)
            : base(parameterInfo, optional)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InstanceParameter"/> class.
        /// </summary>
        /// <param name="desiredType">Type of the desired.</param>
        /// <param name="predicate">The predicate.</param>
        /// <param name="optional">if set to <c>true</c> [optional].</param>
        /// TODO Edit XML Comment Template for #ctor
        internal InstanceParameter(Type desiredType, Func<Type, Func<ParameterInfo, bool>> predicate, bool optional = false)
            : base(null, optional)
        {
            TypeInfo = desiredType?.GetTypeInfo();
            Predicate = predicate;
        }

        /// <summary>
        /// Gets the type information.
        /// </summary>
        /// <value>The type information.</value>
        /// TODO Edit XML Comment Template for TypeInfo
        public TypeInfo TypeInfo { get; }
        /// <summary>
        /// Gets the predicate.
        /// </summary>
        /// <value>The predicate.</value>
        /// TODO Edit XML Comment Template for Predicate
        public Func<Type, Func<ParameterInfo, bool>> Predicate { get; }
    }
}
