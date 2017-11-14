using System.Reflection;

namespace Rocket.Surgery.Reflection.Extensions.InjectorExtension
{
    /// <summary>
    /// Class ConfiguredParameter.
    /// </summary>
    /// TODO Edit XML Comment Template for ConfiguredParameter
    internal class ConfiguredParameter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConfiguredParameter"/> class.
        /// </summary>
        /// <param name="parameterInfo">The parameter information.</param>
        /// <param name="optional">if set to <c>true</c> [optional].</param>
        /// TODO Edit XML Comment Template for #ctor
        internal ConfiguredParameter(ParameterInfo parameterInfo, bool optional)
        {
            ParameterInfo = parameterInfo;
            Optional = optional;
        }

        /// <summary>
        /// Gets the parameter information.
        /// </summary>
        /// <value>The parameter information.</value>
        /// TODO Edit XML Comment Template for ParameterInfo
        public ParameterInfo ParameterInfo { get; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="ConfiguredParameter"/> is optional.
        /// </summary>
        /// <value><c>true</c> if optional; otherwise, <c>false</c>.</value>
        /// TODO Edit XML Comment Template for Optional
        public bool Optional { get; }
    }
}
