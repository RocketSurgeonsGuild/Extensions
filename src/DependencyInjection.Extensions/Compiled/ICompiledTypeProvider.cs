using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.DependencyInjection;

namespace Rocket.Surgery.DependencyInjection.Compiled;

/// <summary>
///     A provider that gets a list of assemblies for a given context
/// </summary>
[PublicAPI]
public interface ICompiledTypeProvider
{
    /// <summary>
    ///     Method used to ensure the argument expression is hashed correctly each time.
    /// </summary>
    /// <param name="argumentExpression"></param>
    /// <returns></returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static string GetArgumentExpressionHash(string argumentExpression)
    {
        var expression = argumentExpression.Replace("\r", "");
        expression = string.Join("", expression.Split('\n', StringSplitOptions.RemoveEmptyEntries).Select(z => z.Trim()));
        return Convert.ToBase64String(MD5.HashData(Encoding.UTF8.GetBytes(expression)));
    }

    /// <summary>
    ///     Get the full list of assemblies
    /// </summary>
    /// <returns>IEnumerable{Assembly}.</returns>
    IEnumerable<Assembly> GetAssemblies(
        Action<IReflectionAssemblySelector> action,
        [CallerLineNumber]
        int lineNumber = 0,
        [CallerFilePath]
        string filePath = "",
        [CallerArgumentExpression(nameof(action))]
        string argumentExpression = ""
    );

    /// <summary>
    ///     Get the full list of types using the given selector
    /// </summary>
    /// <returns>IEnumerable{Type}.</returns>
    IEnumerable<Type> GetTypes(
        Func<IReflectionTypeSelector, IEnumerable<Type>> selector,
        [CallerLineNumber]
        int lineNumber = 0,
        [CallerFilePath]
        string filePath = "",
        [CallerArgumentExpression(nameof(selector))]
        string argumentExpression = ""
    );

    /// <summary>
    ///     Scan for types using the given selector
    /// </summary>
    /// <returns>IEnumerable{Type}.</returns>
    IServiceCollection Scan(
        IServiceCollection services,
        Action<IServiceDescriptorAssemblySelector> selector,
        [CallerLineNumber]
        int lineNumber = 0,
        [CallerFilePath]
        string filePath = "",
        [CallerArgumentExpression(nameof(selector))]
        string argumentExpression = ""
    );
}
