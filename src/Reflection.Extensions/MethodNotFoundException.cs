using System;
using JetBrains.Annotations;

#pragma warning disable RCS1194 // Implement exception constructors.

namespace Rocket.Surgery.Reflection
{
#pragma warning disable CA1032 // Implement standard exception constructors
    /// <summary>
    /// MethodNotFoundException
    /// </summary>
    /// <seealso cref="System.Exception" />
    [PublicAPI]
    public class MethodNotFoundException : Exception
#pragma warning restore CA1032 // Implement standard exception constructors
    {
#pragma warning disable CA1819 // Properties should not return arrays
        /// <summary>
        /// Gets the method names.
        /// </summary>
        /// <value>
        /// The method names.
        /// </value>
        public string[] MethodNames { get; }
#pragma warning restore CA1819 // Properties should not return arrays

        /// <summary>
        /// Initializes a new instance of the <see cref="MethodNotFoundException" /> class.
        /// </summary>
        /// <param name="methodNames">The method names.</param>
        public MethodNotFoundException(string[] methodNames) : base("Method not found! Looking for methods: " + string.Join(", ", methodNames)) => MethodNames = methodNames;

        /// <summary>
        /// Initializes a new instance of the <see cref="MethodNotFoundException" /> class.
        /// </summary>
        /// <param name="methodNames">The method names.</param>
        /// <param name="innerException">The inner exception.</param>
        public MethodNotFoundException(string[] methodNames, Exception innerException) : base("Method not found! Looking for methods: " + string.Join(", ", methodNames), innerException) => MethodNames = methodNames;
    }
}
