using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using FastExpressionCompiler;

namespace Rocket.Surgery.Reflection.Extensions
{
    public class PropertyDelegate : IEquatable<PropertyDelegate>
    {
        private static readonly MethodInfo CreateStronglyTypedExpressionMethod = typeof(PropertyDelegate)
            .GetTypeInfo()
            .GetDeclaredMethod(nameof(CreateStronglyTypedExpression));

        private readonly TypeDelegate _root;
        private readonly Expression _bodyExpression;
        private readonly ParameterExpression _parameterExpression;
        private Expression _stronglyTypedExpression;
        private Delegate _delegate;

        internal PropertyDelegate(TypeDelegate root, string path, Type propertyType, Expression bodyExpression, ParameterExpression parameterExpression)
        {
            _root = root;
            _bodyExpression = bodyExpression;
            _parameterExpression = parameterExpression;
            Path = path;
            PropertyType = propertyType;
        }

        public Type RootType => _root.Type;
        public string Path { get; }
        public Type PropertyType { get; }
        public Delegate Delegate => _delegate ?? (_delegate = Expression.Lambda(_bodyExpression, _parameterExpression).CompileFast());
        public Expression StronglyTypedExpression => 
            _stronglyTypedExpression ?? 
            (_stronglyTypedExpression = (Expression)CreateStronglyTypedExpressionMethod
                .MakeGenericMethod(RootType, PropertyType)
                .Invoke(this, new object[0]));

        private Expression CreateStronglyTypedExpression<TType, TProperty>()
        {
            return Expression.Lambda<Func<TType, TProperty>>(_bodyExpression, _parameterExpression);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as PropertyDelegate);
        }

        public bool Equals(PropertyDelegate other)
        {
            return other != null &&
                   EqualityComparer<Type>.Default.Equals(RootType, other.RootType) &&
                   Path == other.Path;
        }

        public override int GetHashCode()
        {
            var hashCode = -1835866237;
            hashCode = hashCode * -1521134295 + EqualityComparer<Type>.Default.GetHashCode(RootType);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Path);
            return hashCode;
        }

        public static bool operator ==(PropertyDelegate delegate1, PropertyDelegate delegate2)
        {
            return EqualityComparer<PropertyDelegate>.Default.Equals(delegate1, delegate2);
        }

        public static bool operator !=(PropertyDelegate delegate1, PropertyDelegate delegate2)
        {
            return !(delegate1 == delegate2);
        }
    }
}