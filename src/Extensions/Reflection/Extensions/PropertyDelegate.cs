using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using FastExpressionCompiler;

namespace Rocket.Surgery.Reflection.Extensions
{
    /// <summary>
    /// Property delegate
    /// </summary>
    /// <seealso cref="IEquatable{PropertyDelegate}" />
    public class PropertyDelegate : IEquatable<PropertyDelegate?>
    {
        private static readonly MethodInfo CreateStronglyTypedExpressionMethod = typeof(PropertyDelegate)
            .GetTypeInfo()
            .GetDeclaredMethod(nameof(CreateStronglyTypedExpression));

        private readonly TypeDelegate _root;
        private readonly Expression _bodyExpression;
        private readonly ParameterExpression _parameterExpression;
        private Expression? _stronglyTypedExpression;
        private Delegate? _delegate;

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyDelegate" /> class.
        /// </summary>
        /// <param name="root">The root.</param>
        /// <param name="path">The path.</param>
        /// <param name="propertyType">Type of the property.</param>
        /// <param name="bodyExpression">The body expression.</param>
        /// <param name="parameterExpression">The parameter expression.</param>
        internal PropertyDelegate(TypeDelegate root, string path, Type propertyType, Expression bodyExpression, ParameterExpression parameterExpression)
        {
            _root = root;
            _bodyExpression = bodyExpression;
            _parameterExpression = parameterExpression;
            Path = path;
            PropertyType = propertyType;
        }

        /// <summary>
        /// Gets the type of the root.
        /// </summary>
        /// <value>
        /// The type of the root.
        /// </value>
        public Type RootType => _root.Type;

        /// <summary>
        /// Gets the path.
        /// </summary>
        /// <value>
        /// The path.
        /// </value>
        public string Path { get; }

        /// <summary>
        /// Gets the type of the property.
        /// </summary>
        /// <value>
        /// The type of the property.
        /// </value>
        public Type PropertyType { get; }

        /// <summary>
        /// Gets the delegate.
        /// </summary>
        /// <value>
        /// The delegate.
        /// </value>
        public Delegate Delegate => _delegate ?? (_delegate = Expression.Lambda(_bodyExpression, _parameterExpression).CompileFast());

        /// <summary>
        /// Gets the strongly typed expression.
        /// </summary>
        /// <value>
        /// The strongly typed expression.
        /// </value>
        public Expression StronglyTypedExpression =>
                    _stronglyTypedExpression ??
                    (_stronglyTypedExpression = (Expression)CreateStronglyTypedExpressionMethod
                        .MakeGenericMethod(RootType, PropertyType)
                        .Invoke(this, new object[0]));

        private Expression CreateStronglyTypedExpression<TType, TProperty>()
        {
            return Expression.Lambda<Func<TType, TProperty>>(_bodyExpression, _parameterExpression);
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object? obj)
        {
            return Equals(obj as PropertyDelegate);
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, false.
        /// </returns>
        public bool Equals(PropertyDelegate? other)
        {
            return other != null &&
                   EqualityComparer<Type>.Default.Equals(RootType, other.RootType) &&
                   Path == other.Path;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            var hashCode = -1835866237;
            hashCode = (hashCode * -1521134295) + EqualityComparer<Type>.Default.GetHashCode(RootType);
            hashCode = (hashCode * -1521134295) + EqualityComparer<string>.Default.GetHashCode(Path);
            return hashCode;
        }

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="delegate1">The delegate1.</param>
        /// <param name="delegate2">The delegate2.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator ==(PropertyDelegate? delegate1, PropertyDelegate? delegate2)
        {
            return EqualityComparer<PropertyDelegate>.Default.Equals(delegate1!, delegate2!);
        }

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="delegate1">The delegate1.</param>
        /// <param name="delegate2">The delegate2.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator !=(PropertyDelegate? delegate1, PropertyDelegate? delegate2)
        {
            return !(delegate1 == delegate2);
        }
    }
}
