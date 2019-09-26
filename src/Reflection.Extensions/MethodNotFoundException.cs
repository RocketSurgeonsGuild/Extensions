using System;

namespace Rocket.Surgery.Reflection.Extensions
{
    /// <summary>
    /// MethodNotFoundException
    /// </summary>
    /// <seealso cref="System.Exception" />
    public class MethodNotFoundException : Exception
    {
        /// <summary>
        /// Gets the method names.
        /// </summary>
        /// <value>
        /// The method names.
        /// </value>
        public string[] MethodNames { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MethodNotFoundException" /> class.
        /// </summary>
        /// <param name="methodNames">The method names.</param>
        public MethodNotFoundException(string[] methodNames) : base("Method not found! Looking for methods: " + string.Join(", ", methodNames))
        {
            MethodNames = methodNames;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MethodNotFoundException" /> class.
        /// </summary>
        /// <param name="methodNames">The method names.</param>
        /// <param name="innerException">The inner exception.</param>
        public MethodNotFoundException(string[] methodNames, Exception innerException) : base("Method not found! Looking for methods: " + string.Join(", ", methodNames), innerException)
        {
            MethodNames = methodNames;
        }
    }
}
