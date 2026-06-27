---
name: dotnet-microsoft-agent-framework
category: developer-experience
subcategory: cli
description: 'Integrates AI/LLM via Microsoft Agent Framework. Agents, workflows, tools, MCP servers, multi-agent orchestration.'
license: MIT
targets: ['*']
tags: [foundation, dotnet, skill]
version: '0.0.1'
author: 'dotnet-agent-harness'
invocable: true
claudecode:
  allowed-tools: ['Read', 'Grep', 'Glob', 'Bash', 'Write', 'Edit']
codexcli:
  short-description: '.NET skill guidance for AI agent development'
opencode:
  allowed-tools: ['Read', 'Grep', 'Glob', 'Bash', 'Write', 'Edit']
copilot: {}
geminicli: {}
antigravity: {}
---

# dotnet-microsoft-agent-framework

Microsoft Agent Framework for building agentic AI applications in .NET. Covers agents, workflows, multi-agent
orchestration, tool integration (MCP servers, function calling), provider configuration (Azure OpenAI, OpenAI, Microsoft
Foundry, Anthropic, Ollama), chat history management, middleware, and enterprise features (observability,
authentication, responsible AI).

## Scope

- Agent creation and configuration for all supported providers
- Workflow orchestration (sequential, concurrent, group chat, handoff, magentic)
- Tool integration (MCP servers, function calling, hosted tools)
- Chat history and session management
- Middleware for intercepting agent actions
- Enterprise observability with OpenTelemetry
- Authentication with Microsoft Entra
- Responsible AI features (prompt injection protection, content safety)

## Out of scope

- General async/await patterns and cancellation token propagation -- see [skill:dotnet-csharp-async-patterns]
- DI container mechanics and service lifetime management -- see [skill:dotnet-csharp-dependency-injection]
- HTTP client resilience and retry policies -- see [skill:dotnet-resilience]
- Configuration binding (options pattern, secrets) -- see [skill:dotnet-csharp-configuration]

Cross-references: [skill:dotnet-csharp-async-patterns] for async streaming patterns used with chat completions,
[skill:dotnet-csharp-dependency-injection] for agent service registration in ASP.NET Core, [skill:dotnet-resilience] for
retry policies on AI service calls, [skill:dotnet-csharp-configuration] for managing API keys and model configuration.

---

## Package Landscape

| Package                                  | Purpose                                                          |
| ---------------------------------------- | ---------------------------------------------------------------- |
| `Microsoft.Agents.AI`                    | Core abstractions, AIAgent base class                            |
| `Microsoft.Agents.AI.OpenAI`             | OpenAI provider support (Chat Completion, Responses, Assistants) |
| `Microsoft.Agents.AI.AzureAI`            | Azure OpenAI provider support                                    |
| `Microsoft.Agents.AI.AzureAI.Persistent` | Microsoft Foundry Agent Service integration                      |
| `Microsoft.Agents.AI.Anthropic`          | Anthropic provider support                                       |
| `Microsoft.Agents.AI.Ollama`             | Ollama local model integration                                   |
| `Microsoft.Agents.Hosting`               | Hosting abstractions for agent applications                      |
| `Microsoft.Agents.AI.Hosting.OpenAI`     | OpenAI-compatible endpoint hosting                               |
| `Microsoft.Agents.Middleware`            | Middleware pipeline for agent interception                       |
| `Azure.Identity`                         | Azure authentication (DefaultAzureCredential, ManagedIdentity)   |

---

## Agents

### OpenAI

The OpenAI provider supports three client types with different capabilities:

| Client Type         | API                  | Best For                                      |
| ------------------- | -------------------- | --------------------------------------------- |
| **Chat Completion** | Chat Completions API | Simple agents, broad model support            |
| **Responses**       | Responses API        | Full-featured agents with hosted tools        |
| **Assistants**      | Assistants API       | Server-managed agents with persistent threads |

#### Chat Completion Agent

```csharp
using Microsoft.Agents.AI;
using OpenAI;

OpenAIClient client = new OpenAIClient("<your_api_key>");
var chatClient = client.GetChatClient("gpt-4o-mini");

AIAgent agent = chatClient.AsAIAgent(
    instructions: "You are a helpful assistant specialized in data analysis.",
    name: "DataAnalyst");

var response = await agent.RunAsync("Analyze Q3 sales trends.");
Console.WriteLine(response);
```

#### Responses API Agent

```csharp
using Microsoft.Agents.AI.OpenAI;
using OpenAI;

OpenAIClient client = new OpenAIClient("<your_api_key>");
var responsesClient = client.GetResponsesClient("gpt-4o");

AIAgent agent = responsesClient.AsAIAgent(
    instructions: "You are a research assistant with access to web search.",
    name: "ResearchAssistant");

// Responses API supports hosted tools (web search, file search, code interpreter)
var response = await agent.RunAsync("Find recent articles about .NET 10 features.");
```

#### OpenAI Assistants

```csharp
using Microsoft.Agents.AI.OpenAI;
using OpenAI;

OpenAIClient client = new OpenAIClient("<your_api_key>");
var assistantClient = client.GetAssistantClient();

// Create a persistent assistant with built-in tools
var assistant = await assistantClient.CreateAssistantAsync(
    model: "gpt-4o",
    name: "CodeReviewer",
    instructions: "You review code for best practices and potential issues.",
    tools: ["code_interpreter"]);

AIAgent agent = assistant.AsAIAgent();
```

### Azure OpenAI

Azure OpenAI provides enterprise-grade AI with managed endpoints, private networking, and Microsoft Entra
authentication.

```csharp
using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;

AzureOpenAIClient client = new AzureOpenAIClient(
    new Uri("https://<resource>.openai.azure.com"),
    new DefaultAzureCredential());

var chatClient = client.GetChatClient("gpt-4o");

AIAgent agent = chatClient.AsAIAgent(
    instructions: "You are a customer support agent for Contoso.");

var response = await agent.RunAsync("I need help with my order.");
```

**Azure OpenAI also supports Responses and Assistants APIs:**

```csharp
// Responses API
var responsesClient = client.GetResponsesClient("gpt-4o");
AIAgent agent = responsesClient.AsAIAgent(instructions: "...");

// Assistants API
var assistantClient = client.GetAssistantClient();
var assistant = await assistantClient.CreateAssistantAsync(model: "gpt-4o", ...);
AIAgent agent = assistant.AsAIAgent();
```

### Microsoft Foundry Agent Service

Foundry Agent Service provides managed, scalable agents with built-in content safety, persistent storage, and Microsoft
365 integration.

```csharp
using Azure.AI.Agents.Persistent;
using Azure.Identity;
using Microsoft.Agents.AI;

var persistentAgentsClient = new PersistentAgentsClient(
    "https://<resource>.services.ai.azure.com/api/projects/<project>",
    new DefaultAzureCredential());

// Create a new agent in the service
AIAgent agent = await persistentAgentsClient.CreateAIAgentAsync(
    model: "gpt-4o-mini",
    name: "SupportBot",
    instructions: "You handle customer support inquiries.");

// Use the agent
var response = await agent.RunAsync("My account is locked.");

// Or retrieve an existing agent
AIAgent existingAgent = await persistentAgentsClient.GetAIAgentAsync("<agent-id>");
```

### Anthropic

```csharp
using Microsoft.Agents.AI.Anthropic;
using Anthropic;

var client = new AnthropicClient("<api_key>");
var agent = client.AsAIAgent(
    model: "claude-3-opus-20240229",
    instructions: "You are a helpful assistant.");

var response = await agent.RunAsync("Explain quantum computing.");
```

### Ollama (Local Models)

```csharp
using Microsoft.Agents.AI.Ollama;

var client = new OllamaClient(new Uri("http://localhost:11434"));
var agent = client.AsAIAgent(
    model: "llama3.2",
    instructions: "You are a local AI assistant.");

var response = await agent.RunAsync("Summarize this document.");
```

### Generic IChatClient Agent

Any service implementing `Microsoft.Extensions.AI.IChatClient` can be used with `ChatClientAgent`:

```csharp
using Microsoft.Agents.AI;

// IChatClient from any provider
IChatClient chatClient = GetChatClientFromAnyProvider();

AIAgent agent = new ChatClientAgent(
    chatClient,
    instructions: "You are a helpful assistant.");

var response = await agent.RunAsync("Hello!");
```

---

## Workflows

Workflows provide graph-based orchestration for multi-step tasks with type-safe routing, checkpointing, and
human-in-the-loop support.

### Sequential Workflow

```csharp
using Microsoft.Agents.AI.Workflows;

var workflow = WorkflowBuilder.CreateSequential()
    .AddStep<ResearchStep>("research")
    .AddStep<WriteStep>("write")
    .AddStep<ReviewStep>("review")
    .Build();

var result = await workflow.ExecuteAsync(new ResearchInput { Topic = "AI in Healthcare" });
```

### Conditional/Branching Workflow

```csharp
var workflow = WorkflowBuilder.Create()
    .AddStep<AnalyzeIntentStep>("analyze")
    .AddBranch(
        condition: ctx => ctx.Get<Intent>("intent") == Intent.Support,
        thenBranch: b => b.AddStep<HandleSupportStep>("support"),
        elseBranch: b => b.AddStep<HandleSalesStep>("sales"))
    .Build();
```

### Agent Group Chat

Multiple agents collaborating with termination conditions:

```csharp
using Microsoft.Agents.AI.Workflows;

var analyst = new ChatClientAgent(chatClient,
    instructions: "You analyze data and provide insights. Be concise.");

var writer = new ChatClientAgent(chatClient,
    instructions: "You take analysis and write clear reports.");

var workflow = WorkflowBuilder.CreateGroupChat()
    .AddAgent(analyst, "analyst")
    .AddAgent(writer, "writer")
    .WithTerminationCondition(ctx =>
        ctx.Messages.Last().Content.Contains("[COMPLETE]"))
    .WithMaxIterations(10)
    .Build();

var result = await workflow.ExecuteAsync("Analyze Q4 sales and write a summary report.");
```

### Handoff Pattern

Transfer control between specialized agents:

```csharp
var triage = new ChatClientAgent(chatClient,
    instructions: "You triage requests and hand off to specialists.");

var billing = new ChatClientAgent(chatClient,
    instructions: "You handle billing questions.");

var technical = new ChatClientAgent(chatClient,
    instructions: "You handle technical support.");

var workflow = WorkflowBuilder.Create()
    .AddStep<AgentStep>("triage", triage)
    .AddHandoff(
        from: "triage",
        condition: ctx => ctx.Get<string>("department") == "billing",
        to: billing)
    .AddHandoff(
        from: "triage",
        condition: ctx => ctx.Get<string>("department") == "technical",
        to: technical)
    .Build();
```

### Magentic Pattern (Lead Agent)

A lead agent directs other agents:

```csharp
var orchestrator = new ChatClientAgent(chatClient,
    instructions: "You are an orchestrator. Delegate tasks to specialists and synthesize results.");

var workflow = WorkflowBuilder.CreateMagentic()
    .WithLeadAgent(orchestrator)
    .AddSpecialist("researcher", researchAgent, "Research specialist")
    .AddSpecialist("coder", codeAgent, "Code specialist")
    .Build();

var result = await workflow.ExecuteAsync("Build a web scraper that extracts product prices.");
```

---

## Tools and Function Calling

### Function Tools

```csharp
public sealed class OrderTools
{
    [Function("get_order_status")]
    [Description("Retrieves the status of an order by ID")]
    public async Task<OrderStatus> GetOrderStatusAsync(
        [Description("The order ID to look up")] string orderId,
        CancellationToken ct = default)
    {
        // Implementation
        return await _orderService.GetStatusAsync(orderId, ct);
    }

    [Function("cancel_order")]
    [Description("Cancels an order if eligible")]
    public async Task<CancelResult> CancelOrderAsync(
        [Description("The order ID to cancel")] string orderId,
        CancellationToken ct = default)
    {
        // Implementation
        return await _orderService.CancelAsync(orderId, ct);
    }
}

// Register tools with agent
var agent = chatClient.AsAIAgent(
    instructions: "You help customers with their orders.",
    tools: new OrderTools());
```

### MCP (Model Context Protocol) Tools

Connect to MCP servers for external tools:

```csharp
using Microsoft.Agents.AI.Tools.MCP;

// Connect to an MCP server
var mcpClient = new MCPClient("https://api.example.com/mcp");

// Get available tools from the server
var tools = await mcpClient.ListToolsAsync();

// Create agent with MCP tools
var agent = chatClient.AsAIAgent(
    instructions: "You have access to external data sources via MCP.",
    tools: tools);

// Or connect to hosted MCP servers (Azure OpenAI, OpenAI Responses API)
var responsesClient = client.GetResponsesClient("gpt-4o");
var agent = responsesClient.AsAIAgent(
    instructions: "You have access to hosted tools.",

## Detailed Examples

See [references/detailed-examples.md](references/detailed-examples.md) for complete code samples and advanced patterns.
