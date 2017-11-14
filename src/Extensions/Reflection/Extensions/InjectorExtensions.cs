using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Rocket.Surgery.Reflection.Extensions.InjectorExtension;

namespace Rocket.Surgery.Reflection.Extensions
{
    /// <summary>
    /// Class InjectorExtensions.
    /// </summary>
    /// TODO Edit XML Comment Template for InjectorExtensions
    public static class InjectorExtensions
    {
        /// <summary>
        /// Creates the injectable method.
        /// </summary>
        /// <param name="type">The type information.</param>
        /// <param name="methodName">Name of the method.</param>
        /// <param name="predicate">The predicate.</param>
        /// <returns>InjectableMethodBuilder.</returns>
        /// TODO Edit XML Comment Template for CreateInjectableMethod
        public static InjectableMethodBuilder CreateInjectableMethod(this Type type, string methodName, Func<MethodInfo, bool> predicate = null)
        {
            return CreateInjectableMethod(type.GetTypeInfo(), methodName, predicate);
        }

        /// <summary>
        /// Creates the injectable method.
        /// </summary>
        /// <param name="typeInfo">The type information.</param>
        /// <param name="methodName">Name of the method.</param>
        /// <param name="predicate">The predicate.</param>
        /// <returns>InjectableMethodBuilder.</returns>
        /// TODO Edit XML Comment Template for CreateInjectableMethod
        public static InjectableMethodBuilder CreateInjectableMethod(this TypeInfo typeInfo, string methodName, Func<MethodInfo, bool> predicate = null)
        {
            // Warn that there is no execute method.
            var methodInfos = typeInfo.DeclaredMethods
                .Where(x => x.Name == methodName);

            if (predicate != null)
                methodInfos = methodInfos.Where(predicate);

            var methodInfo = methodInfos.FirstOrDefault();

            return methodInfo?.CreateInjectableMethod();
        }

        /// <summary>
        /// Creates the injectable method.
        /// </summary>
        /// <param name="methodInfo">The method information.</param>
        /// <returns>InjectableMethodBuilder.</returns>
        /// TODO Edit XML Comment Template for CreateInjectableMethod
        public static InjectableMethodBuilder CreateInjectableMethod(this MethodInfo methodInfo)
        {
            return new InjectableMethodBuilder(methodInfo);
        }
    }

    public abstract class InjectableMethodBuilder2Base
    {
        internal InjectableMethodBuilder2Base(Type container, ImmutableArray<string> methodNames)
        {
            Container = container.GetTypeInfo();
            MethodNames = methodNames;
        }
        internal InjectableMethodBuilder2Base(Type container)
        {
            Container = container.GetTypeInfo();
            MethodNames = ImmutableArray<string>.Empty;
        }

        public TypeInfo Container { get; }
        public ImmutableArray<string> MethodNames { get; }

        protected (MethodCallExpression body, IEnumerable<ParameterExpression> parameters) Compile(params TypeInfo[] arguments)
        {
            var middleware = Container;
            // TODO: Make binding flags configurable
            var methodInfos = MethodNames
                .SelectMany(z => middleware.DeclaredMethods.Where(x => x.Name == z))
                .Select(method =>
                {
                    if (arguments.Length == 0)
                    {
                        return new { method, matches = 0, exact = method.GetParameters().Length == 0 };
                    }

                    var matches = arguments
                        .Join(method.GetParameters(),
                            x => x,
                            x => x.ParameterType.GetTypeInfo(),
                            (a, y) => a
                        ).Count();

                    var exact = matches == arguments.Length;
                    return new { method, matches, exact };
                })
                .ToArray();

            var methodInfo = methodInfos.FirstOrDefault(z => z.exact)?.method;
            if (methodInfos.Count(z => z.exact) > 1)
            {
                var max = methodInfos
                    .Where(z => z.exact)
                    .Max(z => z.method.GetParameters().Length);

                methodInfo = methodInfos
                    .FirstOrDefault(z => z.exact && z.method.GetParameters().Length == max)
                    ?.method;
            }
            if (methodInfo == null)
            {
                var max = methodInfos.Max(z => z.matches);
                var methodMax = methodInfos.Where(z => z.matches == max).Max(z => z.matches);

                methodInfo = methodInfos
                    .FirstOrDefault(z => z.matches == max && z.method.GetParameters().Length == methodMax)
                    ?.method;
            }

            var parameters = methodInfo.GetParameters();

            var providerArg = Expression.Parameter(typeof(IServiceProvider), "serviceProvider");
            var instanceArg = Expression.Parameter(middleware.AsType(), "instance");

            var instanceParameters = parameters
                .Join(arguments,
                      z => z.ParameterType,
                      z => z.AsType(),
                      (info, type) => new
                      {
                          expression = Expression.Parameter(type.AsType(), type.Name.Substring(0, 1).ToLower() + type.Name.Substring(1)),
                          info,
                          type,
                          index = Array.IndexOf(parameters, info)
                      });
            var otherArgs = instanceParameters.Select(z => new
            {
                expression = Expression.Parameter(z.type.AsType(), z.type.Name.Substring(0, 1).ToLower() + z.type.Name.Substring(1)),
                z.index,
                z.info,
                z.type
            });

            var methodArguments = new Expression[parameters.Length];

            foreach (var instance in instanceParameters)
            {

            }

            foreach (var arg in otherArgs)
            {
                var parameterType = parameters[arg.index].ParameterType;

                var parameterTypeExpression = new Expression[]
                {
                    providerArg,
                    Expression.Constant(parameterType, typeof(Type)),
                    Expression.Constant(methodInfo.DeclaringType, typeof(Type))
                };

                var getServiceCall = Expression.Call(GetServiceInfo, parameterTypeExpression);
                methodArguments[arg.index] = Expression.Convert(getServiceCall, parameterType);
            }

            Expression middlewareInstanceArg = instanceArg;
            if (methodInfo.DeclaringType == middleware.AsType())
            {
                middlewareInstanceArg = Expression.Convert(middlewareInstanceArg, methodInfo.DeclaringType);
            }

            var body = Expression.Call(middlewareInstanceArg, methodInfo, methodArguments);
            return (body, new[] { instanceArg, providerArg }.Concat(otherArgs.Select(x => x.expression)));
        }

        private static readonly MethodInfo GetServiceInfo = typeof(InjectableMethodBuilder2Base)
            .GetTypeInfo()
            .GetDeclaredMethod(nameof(GetService));

        private static object GetService(IServiceProvider sp, Type type, Type middleware)
        {
            var service = sp.GetService(type);

            // TODO: Make this configurable
            //if (service == null)
            //{
            //    throw new InvalidOperationException($"No service defined for type {type.FullName} for middleware {middleware.FullName}.");
            //}

            return service;
        }
    }

    public static class InjectableMethodBuilder2
    {
        public static InjectableMethodBuilder2<C> Create<C>(string methodName)
        {
            return new InjectableMethodBuilder2<C>().ForMethod(methodName);
        }
        public static InjectableMethodBuilder2<object> Create(Type container, string methodName)
        {
            return new InjectableMethodBuilder2<object>(container).ForMethod(methodName);
        }
    }

    public class InjectableMethodBuilder2<C> : InjectableMethodBuilder2Base
    {

        public InjectableMethodBuilder2() : base(typeof(C)) { }
        public InjectableMethodBuilder2(Type container) : base(container) { }


        public InjectableMethodBuilder2(ImmutableArray<string> methodNames) : base(typeof(C), methodNames)
        {
        }

        public InjectableMethodBuilder2<C, T> WithParameter<T>()
        {
            return new InjectableMethodBuilder2<C, T>(MethodNames);
        }

        public InjectableMethodBuilder2<C> ForMethod(string methodName)
        {
            return new InjectableMethodBuilder2<C>(MethodNames.Add(methodName));
        }

        public Func<C, IServiceProvider, TResult> Compile<TResult>()
        {
            var (body, parameters) = base.Compile();
            var lambda = Expression.Lambda<Func<C, IServiceProvider, TResult>>(body, parameters);
            return lambda.Compile();
        }

        public Action<C, IServiceProvider> Compile()
        {
            var (body, parameters) = base.Compile();
            var lambda = Expression.Lambda<Action<C, IServiceProvider>>(body, parameters);
            return lambda.Compile();
        }
    }

    public class InjectableMethodBuilder2<C, T> : InjectableMethodBuilder2Base
    {
        public InjectableMethodBuilder2(): base(typeof(C)) { }
        public InjectableMethodBuilder2(ImmutableArray<string> methodNames) : base(typeof(C), methodNames) { }

        public InjectableMethodBuilder2<C, T, T2> WithParameter<T2>()
        {
            return new InjectableMethodBuilder2<C, T, T2>(MethodNames);
        }

        public InjectableMethodBuilder2<C, T> ForMethod(string methodName)
        {
            return new InjectableMethodBuilder2<C, T>(MethodNames.Add(methodName));
        }

        public Func<C, IServiceProvider, T, TResult> Compile<TResult>()
        {
            var (body, parameters) = base.Compile(
                typeof(T).GetTypeInfo());
            var lambda = Expression.Lambda<Func<C, IServiceProvider, T, TResult>>(body, parameters);
            return lambda.Compile();
        }

        public Action<C, IServiceProvider, T> Compile()
        {
            var (body, parameters) = base.Compile(
                typeof(T).GetTypeInfo());
            var lambda = Expression.Lambda<Action<C, IServiceProvider, T>>(body, parameters);
            return lambda.Compile();
        }
    }

    public class InjectableMethodBuilder2<C, T, T2> : InjectableMethodBuilder2Base
    {
        public InjectableMethodBuilder2(): base(typeof(C)) { }
        public InjectableMethodBuilder2(ImmutableArray<string> methodNames) : base(typeof(C), methodNames) { }

        public InjectableMethodBuilder2<C, T, T2, T3> WithParameter<T3>()
        {
            return new InjectableMethodBuilder2<C, T, T2, T3>(MethodNames);
        }

        public InjectableMethodBuilder2<C, T, T2> ForMethod(string methodName)
        {
            return new InjectableMethodBuilder2<C, T, T2>(MethodNames.Add(methodName));
        }

        public Func<C, IServiceProvider, T, T2, TResult> Compile<TResult>()
        {
            var (body, parameters) = base.Compile(
                typeof(T).GetTypeInfo(),
                typeof(T2).GetTypeInfo());
            var lambda = Expression.Lambda<Func<C, IServiceProvider, T, T2, TResult>>(body, parameters);
            return lambda.Compile();
        }

        public Action<C, IServiceProvider, T, T2> Compile()
        {
            var (body, parameters) = base.Compile(
                typeof(T).GetTypeInfo(),
                typeof(T2).GetTypeInfo());
            var lambda = Expression.Lambda<Action<C, IServiceProvider, T, T2>>(body, parameters);
            return lambda.Compile();
        }
    }
    public class InjectableMethodBuilder2<C, T, T2, T3> : InjectableMethodBuilder2Base
    {
        public InjectableMethodBuilder2(): base(typeof(C)) { }
        public InjectableMethodBuilder2(ImmutableArray<string> methodNames) : base(typeof(C), methodNames) { }

        public InjectableMethodBuilder2<C, T, T2, T3, T4> WithParameter<T4>()
        {
            return new InjectableMethodBuilder2<C, T, T2, T3, T4>(MethodNames);
        }

        public InjectableMethodBuilder2<C, T, T2, T3> ForMethod(string methodName)
        {
            return new InjectableMethodBuilder2<C, T, T2, T3>(MethodNames.Add(methodName));
        }

        public Func<C, IServiceProvider, T, T2, T3, TResult> Compile<TResult>()
        {
            var (body, parameters) = base.Compile(
                typeof(T).GetTypeInfo(),
                typeof(T2).GetTypeInfo(),
                typeof(T3).GetTypeInfo());
            var lambda = Expression.Lambda<Func<C, IServiceProvider, T, T2, T3, TResult>>(body, parameters);
            return lambda.Compile();
        }

        public Action<C, IServiceProvider, T, T2, T3> Compile()
        {
            var (body, parameters) = base.Compile(
                typeof(T).GetTypeInfo(),
                typeof(T2).GetTypeInfo(),
                typeof(T3).GetTypeInfo());
            var lambda = Expression.Lambda<Action<C, IServiceProvider, T, T2, T3>>(body, parameters);
            return lambda.Compile();
        }
    }
    public class InjectableMethodBuilder2<C, T, T2, T3, T4> : InjectableMethodBuilder2Base
    {
        public InjectableMethodBuilder2(): base(typeof(C)) { }
        public InjectableMethodBuilder2(ImmutableArray<string> methodNames) : base(typeof(C), methodNames) { }

        public InjectableMethodBuilder2<C, T, T2, T3, T4, T5> WithParameter<T5>()
        {
            return new InjectableMethodBuilder2<C, T, T2, T3, T4, T5>(MethodNames);
        }

        public InjectableMethodBuilder2<C, T, T2, T3, T4> ForMethod(string methodName)
        {
            return new InjectableMethodBuilder2<C, T, T2, T3, T4>(MethodNames.Add(methodName));
        }

        public Func<C, IServiceProvider, T, T2, T3, T4, TResult> Compile<TResult>()
        {
            var (body, parameters) = base.Compile(
                typeof(T).GetTypeInfo(),
                typeof(T2).GetTypeInfo(),
                typeof(T3).GetTypeInfo(),
                typeof(T4).GetTypeInfo());
            var lambda = Expression.Lambda<Func<C, IServiceProvider, T, T2, T3, T4, TResult>>(body, parameters);
            return lambda.Compile();
        }

        public Action<C, IServiceProvider, T, T2, T3, T4> Compile()
        {
            var (body, parameters) = base.Compile(
                typeof(T).GetTypeInfo(),
                typeof(T2).GetTypeInfo(),
                typeof(T3).GetTypeInfo(),
                typeof(T4).GetTypeInfo());
            var lambda = Expression.Lambda<Action<C, IServiceProvider, T, T2, T3, T4>>(body, parameters);
            return lambda.Compile();
        }
    }
    public class InjectableMethodBuilder2<C, T, T2, T3, T4, T5> : InjectableMethodBuilder2Base
    {
        public InjectableMethodBuilder2(): base(typeof(C)) { }
        public InjectableMethodBuilder2(ImmutableArray<string> methodNames) : base(typeof(C), methodNames) { }


        public InjectableMethodBuilder2<C, T, T2, T3, T4, T5, T6> WithParameter<T6>()
        {
            return new InjectableMethodBuilder2<C, T, T2, T3, T4, T5, T6>(MethodNames);
        }

        public InjectableMethodBuilder2<C, T, T2, T3, T4, T5> ForMethod(string methodName)
        {
            return new InjectableMethodBuilder2<C, T, T2, T3, T4, T5>(MethodNames.Add(methodName));
        }

        public Func<C, IServiceProvider, T, T2, T3, T4, T5, TResult> Compile<TResult>()
        {
            var (body, parameters) = base.Compile(
                typeof(T).GetTypeInfo(),
                typeof(T2).GetTypeInfo(),
                typeof(T3).GetTypeInfo(),
                typeof(T4).GetTypeInfo(),
                typeof(T5).GetTypeInfo());
            var lambda = Expression.Lambda<Func<C, IServiceProvider, T, T2, T3, T4, T5, TResult>>(body, parameters);
            return lambda.Compile();
        }

        public Action<C, IServiceProvider, T, T2, T3, T4, T5> Compile()
        {
            var (body, parameters) = base.Compile(
                typeof(T).GetTypeInfo(),
                typeof(T2).GetTypeInfo(),
                typeof(T3).GetTypeInfo(),
                typeof(T4).GetTypeInfo(),
                typeof(T5).GetTypeInfo());
            var lambda = Expression.Lambda<Action<C, IServiceProvider, T, T2, T3, T4, T5>>(body, parameters);
            return lambda.Compile();
        }
    }

    public class InjectableMethodBuilder2<C, T, T2, T3, T4, T5, T6> : InjectableMethodBuilder2Base
    {
        public InjectableMethodBuilder2(): base(typeof(C)) { }
        public InjectableMethodBuilder2(ImmutableArray<string> methodNames) : base(typeof(C), methodNames) { }

        public InjectableMethodBuilder2<C, T, T2, T3, T4, T5, T6> ForMethod(string methodName)
        {
            return new InjectableMethodBuilder2<C, T, T2, T3, T4, T5, T6>(MethodNames.Add(methodName));
        }

        public Func<C, IServiceProvider, T, T2, T3, T4, T5, T6, TResult> Compile<TResult>()
        {
            var (body, parameters) = base.Compile(
                typeof(T).GetTypeInfo(),
                typeof(T2).GetTypeInfo(),
                typeof(T3).GetTypeInfo(),
                typeof(T4).GetTypeInfo(),
                typeof(T5).GetTypeInfo(),
                typeof(T6).GetTypeInfo());
            var lambda = Expression.Lambda<Func<C, IServiceProvider, T, T2, T3, T4, T5, T6, TResult>>(body, parameters);
            return lambda.Compile();
        }

        public Action<C, IServiceProvider, T, T2, T3, T4, T5, T6> Compile()
        {
            var (body, parameters) = base.Compile(
                typeof(T).GetTypeInfo(),
                typeof(T2).GetTypeInfo(),
                typeof(T3).GetTypeInfo(),
                typeof(T4).GetTypeInfo(),
                typeof(T5).GetTypeInfo(),
                typeof(T6).GetTypeInfo());
            var lambda = Expression.Lambda<Action<C, IServiceProvider, T, T2, T3, T4, T5, T6>>(body, parameters);
            return lambda.Compile();
        }
    }
}
