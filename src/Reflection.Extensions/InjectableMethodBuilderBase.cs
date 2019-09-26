using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Rocket.Surgery.Reflection.Extensions
{
    /// <summary>
    /// Injectable method builder base
    /// </summary>
    public abstract class InjectableMethodBuilderBase
    {
        internal InjectableMethodBuilderBase(Type container, ImmutableArray<string> methodNames)
        {
            Container = container.GetTypeInfo();
            MethodNames = methodNames;
        }
        internal InjectableMethodBuilderBase(Type container)
        {
            Container = container.GetTypeInfo();
            MethodNames = ImmutableArray<string>.Empty;
        }

        /// <summary>
        /// Gets the container.
        /// </summary>
        /// <value>
        /// The container.
        /// </value>
        public TypeInfo Container { get; }

        /// <summary>
        /// Gets the method names.
        /// </summary>
        /// <value>
        /// The method names.
        /// </value>
        public ImmutableArray<string> MethodNames { get; }

        /// <summary>
        /// Gets the method information.
        /// </summary>
        /// <returns></returns>
        protected MethodInfo? GetMethodInfo()
        {
            // Make this stupid simple
            // We allow multiple method names, but beyond that overloads are not allowed
            // there are multiple methods we simply take the one with the most arguments and move on.
            return MethodNames
                .SelectMany(z => Container.DeclaredMethods.Where(x => x.Name == z))
                .OrderByDescending(x => x.GetParameters().Length)
                .FirstOrDefault();
        }

        /// <summary>
        /// Compiles the specified arguments.
        /// </summary>
        /// <param name="arguments">The arguments.</param>
        /// <returns></returns>
        protected (MethodCallExpression body, IEnumerable<ParameterExpression> parameters) Compile(params TypeInfo[] arguments)
        {
            // Make this stupid simple
            // We allow multiple method names, but beyond that overloads are not allowed
            // there are multiple methods we simply take the one with the most arguments and move on.
            var methodInfo = GetMethodInfo();

            if (methodInfo is null)
            {
                throw new MethodNotFoundException(MethodNames.ToArray());
            }

            static string? getParameterName(Type? type)
            {
                if (type is null) return null;
                if (type.GetTypeInfo().IsInterface && type.Name.StartsWith("I"))
                {
                    return type.Name.Substring(1, 1).ToLower() + type.Name.Substring(2);
                }
                return type.Name.Substring(0, 1).ToLower() + type.Name.Substring(1);
            }

            var parameters = methodInfo.GetParameters();

            var resolvedParameters = parameters
                .Select(info => new
                {
                    expression = Expression.Parameter(info.ParameterType, getParameterName(info.ParameterType)),
                    info,
                    type = info.ParameterType,
                    index = Array.IndexOf((Array)parameters, info),
                    argument = arguments.FirstOrDefault(x => info.ParameterType.GetTypeInfo().IsAssignableFrom(x))
                })
                .ToArray();

            var providerArg = Expression.Parameter(typeof(IServiceProvider), "serviceProvider");

            var methodArguments = new Expression[parameters.Length];

            foreach (var instance in resolvedParameters)
            {
                if (instance.argument is null)
                {
                    var parameterType = parameters[instance.index].ParameterType;

                    var parameterTypeExpression = new Expression[]
                    {
                        providerArg,
                        Expression.Constant(parameterType, typeof(Type)),
                        Expression.Constant(instance.info.IsOptional, typeof(bool))
                    };

                    var getServiceCall = Expression.Call(GetServiceInfo, parameterTypeExpression);
                    methodArguments[instance.index] = Expression.Convert(getServiceCall, parameterType);
                }
                else
                {
                    methodArguments[instance.index] = instance.expression;
                }
            }

            if (methodInfo.IsStatic)
            {
                var body = Expression.Call(methodInfo, methodArguments);
                return (
                    body,
                    new[]
                    {
                        providerArg
                    }.Concat(
                        resolvedParameters
                            .Where(x => !(x.argument is null))
                            .OrderBy(x => x.index)
                            .Select(x => x.expression)
                    )
                );
            }
            else
            {
                var instanceArg = Expression.Parameter(typeof(object), "instance");
                Expression middlewareInstanceArg = instanceArg;
                if (methodInfo.DeclaringType == Container.AsType())
                {
                    middlewareInstanceArg = Expression.Convert(middlewareInstanceArg, methodInfo.DeclaringType);
                }

                var body = Expression.Call(middlewareInstanceArg, methodInfo, methodArguments);
                return (
                    body,
                    new[]
                    {
                        instanceArg,
                        providerArg
                    }.Concat(
                        resolvedParameters
                            .Where(x => !(x.argument is null))
                            .OrderBy(x => x.index)
                            .Select(x => x.expression)
                    )
                );
            }
        }

        private static readonly MethodInfo GetServiceInfo = typeof(InjectableMethodBuilderBase)
            .GetTypeInfo()
            .GetDeclaredMethod(nameof(GetService));

        private static object GetService(IServiceProvider sp, Type type, bool optional)
        {
            var service = sp.GetService(type);

            if (!optional && service is null)
            {
                throw new InvalidOperationException(string.Format("No service for type '{0}' has been registered.", type.FullName));
            }

            return service;
        }
    }
}
