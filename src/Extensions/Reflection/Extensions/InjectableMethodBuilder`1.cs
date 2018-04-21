using FastExpressionCompiler;
using System;
using System.Collections.Immutable;
using System.Linq.Expressions;

namespace Rocket.Surgery.Reflection.Extensions
{
    public class InjectableMethodBuilder : InjectableMethodBuilderBase
    {
        public static InjectableMethodBuilder Create<TContainer>(string methodName)
        {
            return new InjectableMethodBuilder(typeof(TContainer)).ForMethod(methodName);
        }

        public static InjectableMethodBuilder Create(Type container, string methodName)
        {
            return new InjectableMethodBuilder(container).ForMethod(methodName);
        }

        public InjectableMethodBuilder(Type container) : base(container) { }
        public InjectableMethodBuilder(Type container, ImmutableArray<string> methodNames) : base(container, methodNames) { }

        public InjectableMethodBuilder<T> WithParameter<T>()
        {
            return new InjectableMethodBuilder<T>(Container, MethodNames);
        }

        public InjectableMethodBuilder ForMethod(string methodName)
        {
            return new InjectableMethodBuilder(Container.AsType(), MethodNames.Add(methodName));
        }

        public Func<object, IServiceProvider, TResult> Compile<TResult>()
        {
            if (GetMethodInfo()?.IsStatic == true)
                throw new NotSupportedException("Method must not be a static method to compile as an instance methods!");
            var (body, parameters) = base.Compile();
            var lambda = Expression.Lambda<Func<object, IServiceProvider, TResult>>(body, parameters);
            return ExpressionCompiler.CompileFast(lambda);
        }

        public Action<object, IServiceProvider> Compile()
        {
            if (GetMethodInfo()?.IsStatic == true)
                throw new NotSupportedException("Method must not be a static method to compile as an instance methods!");
            var (body, parameters) = base.Compile();
            var lambda = Expression.Lambda<Action<object, IServiceProvider>>(body, parameters);
            return ExpressionCompiler.CompileFast(lambda);
        }

        public Func<IServiceProvider, TResult> CompileStatic<TResult>()
        {
            if (GetMethodInfo()?.IsStatic != true)
                throw new NotSupportedException("Method must be a static method to compile as an static methods!");
            var (body, parameters) = base.Compile();
            var lambda = Expression.Lambda<Func<IServiceProvider, TResult>>(body, parameters);
            return ExpressionCompiler.CompileFast(lambda);
        }

        public Action<IServiceProvider> CompileStatic()
        {
            if (GetMethodInfo()?.IsStatic != true)
                throw new NotSupportedException("Method must be a static method to compile as an static methods!");
            var (body, parameters) = base.Compile();
            var lambda = Expression.Lambda<Action<IServiceProvider>>(body, parameters);
            return ExpressionCompiler.CompileFast(lambda);
        }
    }
}
