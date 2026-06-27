    {
        InstanceState(x => x.CurrentState);

        Event(() => OrderSubmitted,
            x => x.CorrelateById(ctx => ctx.Message.OrderId));
        Event(() => PaymentReceived,
            x => x.CorrelateById(ctx => ctx.Message.OrderId));
        Event(() => PaymentFailed,
            x => x.CorrelateById(ctx => ctx.Message.OrderId));

        Initially(
            When(OrderSubmitted)
                .Then(ctx =>
                {
                    ctx.Saga.OrderId = ctx.Message.OrderId;
                    ctx.Saga.Total = ctx.Message.Total;
                })
                .Publish(ctx => new RequestPayment(
                    ctx.Saga.OrderId, ctx.Saga.Total))
                .TransitionTo(PaymentPending));

        During(PaymentPending,
            When(PaymentReceived)
                .Then(ctx =>
                    ctx.Saga.PaymentReceivedAt = DateTime.UtcNow)
                .Publish(ctx => new FulfillOrder(ctx.Saga.OrderId))
                .TransitionTo(Completed),
            When(PaymentFailed)
                .Publish(ctx => new CancelOrder(ctx.Saga.OrderId))
                .TransitionTo(Faulted));
    }
}

// Registration -- requires MassTransit.EntityFrameworkCore package for EF persistence
// NuGet: MassTransit.EntityFrameworkCore Version="8.*"
builder.Services.AddMassTransit(x =>
{
    x.AddSagaStateMachine<OrderStateMachine, OrderState>()
        .EntityFrameworkRepository(r =>
        {
            r.ExistingDbContext<SagaDbContext>();
            r.UsePostgres();
        });

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.ConfigureEndpoints(context);
    });
});

```text

### Saga Persistence

| Store                 | Package                           | Use when                                 |
| --------------------- | --------------------------------- | ---------------------------------------- |
| Entity Framework Core | `MassTransit.EntityFrameworkCore` | Already using EF Core; need transactions |
| MongoDB               | `MassTransit.MongoDb`             | Document-oriented state; high throughput |
| Redis                 | `MassTransit.Redis`               | Ephemeral sagas; low latency             |
| In-Memory             | Built-in                          | Testing only -- state lost on restart    |

### Compensation Pattern

When a saga step fails, publish compensating commands to undo prior steps:

```bash

OrderSubmitted -> RequestPayment -> PaymentReceived -> ReserveInventory
                                                          |
                                                     InventoryFailed
                                                          |
                                                    RefundPayment (compensation)
                                                          |
                                                    CancelOrder (compensation)

```text

---

## Idempotent Consumers

At-least-once delivery means consumers may receive the same message multiple times. Idempotent consumers ensure repeated
processing produces the same result.

### Database-Based Deduplication

```csharp

public sealed class IdempotentOrderConsumer(
    AppDbContext db,
    ILogger<IdempotentOrderConsumer> logger)
    : IConsumer<OrderPlaced>
{
    public async Task Consume(ConsumeContext<OrderPlaced> context)
    {
        var messageId = context.MessageId
            ?? throw new InvalidOperationException("Missing MessageId");

        // Check if already processed
        var exists = await db.ProcessedMessages
            .AnyAsync(m => m.MessageId == messageId);

        if (exists)
        {
            logger.LogInformation(
                "Duplicate message {MessageId}, skipping", messageId);
            return;
        }

        // Process the message
        await ProcessOrderAsync(context.Message);

        // Record as processed
        db.ProcessedMessages.Add(new ProcessedMessage
        {
            MessageId = messageId,
            ProcessedAt = DateTime.UtcNow,
            ConsumerType = nameof(IdempotentOrderConsumer)
        });

        await db.SaveChangesAsync();
    }
}

```text

### Natural Idempotency

Prefer operations that are naturally idempotent:

- **Upserts** (`INSERT ... ON CONFLICT UPDATE`) instead of blind inserts
- **Conditional updates** (`UPDATE ... WHERE Status = 'Pending'`) instead of unconditional
- **Deterministic IDs** derived from message content instead of auto-generated

---

## Message Envelope Pattern

Wrap message payloads in a standard envelope with metadata for tracing, versioning, and routing.

```csharp

public sealed record MessageEnvelope<T>(
    string MessageId,
    string MessageType,
    DateTimeOffset Timestamp,
    string CorrelationId,
    string Source,
    int Version, // Schema version for backward-compatible deserialization
    T Payload);

```text

MassTransit provides this automatically via `ConsumeContext` (MessageId, CorrelationId, Headers). When using raw broker
clients, implement envelopes explicitly.

---

## Agent Gotchas

1. **Do not use auto-complete with Azure Service Bus** -- set `AutoCompleteMessages = false` and call
   `CompleteMessageAsync` after successful processing. Auto-complete acknowledges before processing finishes, risking
   data loss on failure.
2. **Do not forget to handle poison messages** -- always configure max delivery count and DLQ monitoring. Without these,
   a single bad message blocks the entire queue indefinitely.
3. **Do not use in-memory saga persistence in production** -- saga state is lost on restart, leaving business processes
   in unknown states. Use Entity Framework, MongoDB, or Redis persistence.
4. **Do not assume message ordering across partitions** -- competing consumers and topic subscriptions deliver messages
   out of order by default. Use sessions or partitioning when order matters.
5. **Do not skip idempotency for at-least-once consumers** -- brokers may redeliver on timeout, network glitch, or
   consumer restart. Every consumer must handle duplicate messages safely.
6. **Do not hardcode connection strings** -- use environment variables or Azure Key Vault references. For local
   development, use user secrets or `.env` files excluded from source control.

---

## References

- [Azure Service Bus documentation](https://learn.microsoft.com/en-us/azure/service-bus-messaging/)
- [Azure Service Bus client library for .NET](https://learn.microsoft.com/en-us/dotnet/api/overview/azure/messaging.servicebus-readme)
- [RabbitMQ .NET client documentation](https://www.rabbitmq.com/client-libraries/dotnet-api-guide)
- [MassTransit documentation](https://masstransit.io/documentation/concepts)
- [MassTransit sagas](https://masstransit.io/documentation/patterns/saga)
- [Enterprise Integration Patterns](https://www.enterpriseintegrationpatterns.com/)
````

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