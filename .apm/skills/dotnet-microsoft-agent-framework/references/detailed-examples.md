    instructions: "You have access to hosted tools.",
    hostedTools: ["web_search", "file_search", "code_interpreter"]);
```

### Hosted Tools (Responses API)

```csharp
using Microsoft.Agents.AI.OpenAI;

var responsesClient = client.GetResponsesClient("gpt-4o");

var agent = responsesClient.AsAIAgent(
    instructions: "You are a research assistant.",
    hostedTools: new HostedTools
    {
        WebSearch = new WebSearchTool(),
        FileSearch = new FileSearchTool { VectorStoreIds = ["vs_123"] },
        CodeInterpreter = new CodeInterpreterTool()
    });

var response = await agent.RunAsync("Search for recent papers on climate change and analyze the data.");
```

---

## Chat History and Sessions

### Session Management

```csharp
using Microsoft.Agents.AI.Sessions;

// Create a new session
var session = new AgentSession();

// Multi-turn conversation
await agent.RunAsync("What's the weather?", session);
await agent.RunAsync("Will it rain tomorrow?", session); // Has context from previous turn

// Persist session for later
var sessionData = session.Serialize();
// ... save to database ...

// Restore session later
var restoredSession = AgentSession.Deserialize(sessionData);
```

### Chat History Providers

Use Redis or other providers for distributed session storage:

```csharp
using Microsoft.Agents.AI.Sessions.Redis;

builder.Services.AddRedisChatHistoryProvider(
    connectionString: "localhost:6379");

// Agent automatically uses Redis for session persistence
var agent = chatClient.AsAIAgent(
    instructions: "You remember previous conversations.",
    chatHistoryProvider: provider);
```

### Custom Context Providers

```csharp
public class RAGContextProvider : IContextProvider
{
    private readonly IVectorStore _vectorStore;

    public async Task<IEnumerable<ChatMessage>> GetContextAsync(
        string userMessage,
        CancellationToken ct)
    {
        // Retrieve relevant documents
        var embedding = await GenerateEmbeddingAsync(userMessage, ct);
        var docs = await _vectorStore.SearchAsync(embedding, top: 5, ct);

        return docs.Select(d => new ChatMessage(
            Role.System,
            $"Context: {d.Content}"));
    }
}

var agent = chatClient.AsAIAgent(
    instructions: "You answer based on the provided context.",
    contextProviders: [new RAGContextProvider(vectorStore)]);
```

---

## Middleware

Middleware intercepts agent actions for logging, authorization, rate limiting, and modification.

### Authorization Middleware

```csharp
public class AuthorizationMiddleware : IAgentMiddleware
{
    public async Task<AgentResponse> InvokeAsync(
        AgentContext context,
        Func<AgentContext, Task<AgentResponse>> next)
    {
        // Check authorization before processing
        if (context.FunctionName == "cancel_order")
        {
            var userId = context.User.Identity?.Name;
            var orderId = context.Arguments["orderId"]?.ToString();

            if (!await _authService.CanCancelOrderAsync(userId, orderId))
            {
                return new AgentResponse("You are not authorized to cancel this order.");
            }
        }

        return await next(context);
    }
}

// Register middleware
var agent = chatClient.AsAIAgent(instructions: "...")
    .UseMiddleware<AuthorizationMiddleware>();
```

### Logging Middleware

```csharp
public class LoggingMiddleware : IAgentMiddleware
{
    private readonly ILogger<LoggingMiddleware> _logger;

    public async Task<AgentResponse> InvokeAsync(
        AgentContext context,
        Func<AgentContext, Task<AgentResponse>> next)
    {
        _logger.LogInformation(
            "Agent {AgentName} processing: {Message}",
            context.AgentName,
            context.UserMessage);

        var stopwatch = Stopwatch.StartNew();
        var response = await next(context);
        stopwatch.Stop();

        _logger.LogInformation(
            "Agent {AgentName} completed in {ElapsedMs}ms",
            context.AgentName,
            stopwatch.ElapsedMilliseconds);

        return response;
    }
}
```

### Rate Limiting Middleware

```csharp
public class RateLimitMiddleware : IAgentMiddleware
{
    private readonly IRateLimiter _rateLimiter;

    public async Task<AgentResponse> InvokeAsync(
        AgentContext context,
        Func<AgentContext, Task<AgentResponse>> next)
    {
        var userId = context.User.Identity?.Name;

        if (!await _rateLimiter.TryAcquireAsync(userId))
        {
            return new AgentResponse("Rate limit exceeded. Please try again later.");
        }

        return await next(context);
    }
}
```

---

## Enterprise Features

### Observability with OpenTelemetry

```csharp
using Microsoft.Agents.AI.Telemetry;

// Configure OpenTelemetry
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing =>
    {
        tracing.AddAgentFrameworkInstrumentation();
        tracing.AddAzureMonitorTraceExporter();
    })
    .WithMetrics(metrics =>
    {
        metrics.AddAgentFrameworkMetrics();
        metrics.AddAzureMonitorMetricExporter();
    });

// All agent interactions automatically emit traces and metrics
```

### Authentication with Microsoft Entra

```csharp
using Azure.Identity;
using Microsoft.Agents.AI;

// Use managed identity in production
AzureOpenAIClient client = new AzureOpenAIClient(
    new Uri("https://<resource>.openai.azure.com"),
    new ManagedIdentityCredential());

// Or use Entra ID for user-delegated access
var credential = new InteractiveBrowserCredential();
```

### Responsible AI

```csharp
using Microsoft.Agents.AI.Safety;

var agent = chatClient.AsAIAgent(instructions: "...")
    .UseSafetyFilters(new SafetyOptions
    {
        // Prompt injection protection
        PromptInjectionDetection = true,

        // Content safety
        ContentSafety = new ContentSafetyOptions
        {
            HateSpeech = FilterSeverity.Medium,
            SelfHarm = FilterSeverity.High,
            Violence = FilterSeverity.Medium
        },

        // Task adherence monitoring
        TaskAdherenceMonitoring = true
    });
```

---

## Streaming Responses

```csharp
var agent = chatClient.AsAIAgent(instructions: "...");
var session = new AgentSession();

await foreach (var chunk in agent.RunStreamingAsync("Tell me a story.", session))
{
    Console.Write(chunk.Content);
}
```

---

## Key Principles

- **Prefer Agents for open-ended tasks** -- Use agents when the task is conversational or requires autonomous tool use.
  If you can write a deterministic function, do that instead.
- **Use Workflows for structured processes** -- When the process has well-defined steps or requires explicit control
  over execution order, use workflows over single agents.
- **Choose the right API** -- Chat Completion for simple cases, Responses API for hosted tools, Assistants for
  persistent stateful conversations.
- **Design focused tools** -- Each tool should represent a single domain. Use `[Description]` attributes so the model
  knows when and how to call them.
- **Do not store API keys in code** -- Use environment variables, Azure Key Vault, or Managed Identity (see
  [skill:dotnet-csharp-configuration])
- **Implement middleware for cross-cutting concerns** -- Use middleware for logging, authorization, and rate limiting
  rather than duplicating logic in tools.
- **Use chat history providers for distributed apps** -- In multi-instance deployments, use Redis or other providers for
  session persistence.
- **Enable observability in production** -- Always configure OpenTelemetry tracing and metrics for production
  deployments.
- **Validate inputs in tools** -- AI models may call tools with unexpected arguments. Validate all inputs before
  executing operations.
- **Handle cancellation tokens** -- Always propagate `CancellationToken` through tool methods to support timeouts and
  user cancellations.

---

## Agent Gotchas

1. **Do not hardcode API keys** -- Use `DefaultAzureCredential` for development and `ManagedIdentityCredential` for
   production. Hardcoded secrets leak into source control.
2. **Do not ignore function return types** -- The model receives the serialized result. Return summary DTOs, not full
   entity graphs, to avoid exceeding token limits.
3. **Do not create agents per request** -- Agents are designed to be long-lived. Register in DI and reuse across
   requests.
4. **Do not forget middleware ordering** -- Middleware executes in registration order. Place authorization before
   logging if you want to log only authorized requests.
5. **Do not assume MCP availability** -- Always handle cases where MCP servers are unavailable or return errors.
6. **Do not store sensitive data in chat history** -- Filter out PII before persisting chat history. Use context
   providers for sensitive data instead.
7. **Do not skip cancellation token propagation** -- AI calls can hang or take too long. Always propagate
   `CancellationToken` to allow cancellation.
8. **Do not mix streaming and non-streaming inconsistently** -- Choose one pattern per endpoint to avoid confusing
   client behavior.

---

## Prerequisites

- .NET 8.0 or later
- `Microsoft.Agents.AI` NuGet package (prerelease)
- Provider-specific packages (e.g., `Microsoft.Agents.AI.OpenAI`, `Microsoft.Agents.AI.AzureAI`)
- Azure subscription (for Azure OpenAI or Foundry)
- OpenAI API key (for OpenAI direct)
- Ollama installation (for local models)

---

## References

- [Microsoft Agent Framework documentation](https://learn.microsoft.com/agent-framework/overview/)
- [.NET AI ecosystem tools](https://learn.microsoft.com/dotnet/ai/dotnet-ai-ecosystem)
- [Agent Framework samples](https://github.com/microsoft/agent-framework/tree/main/dotnet/samples)
- [MCP Protocol](https://modelcontextprotocol.io/)
- [Azure OpenAI documentation](https://learn.microsoft.com/azure/ai-services/openai/)
- [Microsoft Foundry Agent Service](https://learn.microsoft.com/azure/ai-foundry/agents/overview)

## Code Navigation (Serena MCP)

**Primary approach:** Use Serena symbol operations for efficient code navigation:

1. **Find definitions**: `serena_find_symbol` instead of text search
2. **Understand structure**: `serena_get_symbols_overview` for file organization
3. **Track references**: `serena_find_referencing_symbols` for impact analysis
4. **Precise edits**: `serena_replace_symbol_body` for clean modifications

**When to use Serena vs traditional tools:**

- **Use Serena**: Navigation, refactoring, dependency analysis, precise edits
- **Use Read/Grep**: Reading full files, pattern matching, simple text operations
- **Fallback**: If Serena unavailable, traditional tools work fine

**Example workflow:**

```text
# Instead of:
Read: src/Services/OrderService.cs
Grep: "public void ProcessOrder"

# Use:
serena_find_symbol: "OrderService/ProcessOrder"
serena_get_symbols_overview: "src/Services/OrderService.cs"
```
