namespace SystemLayer.Application.Interfaces;

/// <summary>
/// Low-level CAFM client interface for SOAP/XML operations
/// </summary>
public interface ICafmClient
{
    /// <summary>
    /// Authenticates with CAFM system and returns session token
    /// </summary>
    /// <param name="correlationId">Correlation ID for tracking</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Session token for subsequent operations</returns>
    Task<string> LoginAsync(string correlationId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Logs out from CAFM system and invalidates session token
    /// </summary>
    /// <param name="sessionToken">Session token to invalidate</param>
    /// <param name="correlationId">Correlation ID for tracking</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task LogoutAsync(string sessionToken, string correlationId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Executes a SOAP operation with login/logout wrapper
    /// </summary>
    /// <typeparam name="TRequest">Request type</typeparam>
    /// <typeparam name="TResponse">Response type</typeparam>
    /// <param name="operation">Operation name</param>
    /// <param name="request">Request object</param>
    /// <param name="correlationId">Correlation ID for tracking</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Response object</returns>
    Task<TResponse> ExecuteWithSessionAsync<TRequest, TResponse>(
        string operation,
        TRequest request,
        string correlationId,
        CancellationToken cancellationToken = default)
        where TRequest : class
        where TResponse : class;
    
    /// <summary>
    /// Sends raw SOAP request and returns raw XML response
    /// </summary>
    /// <param name="soapAction">SOAP action header</param>
    /// <param name="soapBody">SOAP request body XML</param>
    /// <param name="sessionToken">Session token for authentication</param>
    /// <param name="correlationId">Correlation ID for tracking</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Raw XML response</returns>
    Task<string> SendSoapRequestAsync(
        string soapAction,
        string soapBody,
        string sessionToken,
        string correlationId,
        CancellationToken cancellationToken = default);
}