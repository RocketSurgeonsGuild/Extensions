namespace Rocket.Surgery;

/// <summary>
///     RequestException.
/// </summary>
/// <seealso cref="System.Exception" />
public class RequestException : Exception
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="RequestException" /> class.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public RequestException(string message) : base(message)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="RequestException" /> class.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">
    ///     The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is
    ///     specified.
    /// </param>
    public RequestException(string message, Exception innerException) : base(message, innerException)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="RequestException" /> class.
    /// </summary>
    public RequestException()
    {
    }
}
