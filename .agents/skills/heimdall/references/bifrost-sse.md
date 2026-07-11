# Bifrost SSE

Use this reference when building real-time Heimdall features.

Bifrost streams HTML over server-sent events. The server publishes `IHtmlContent`; the browser receives it and applies normal Heimdall swap and directive processing.

## Subscribe From Rendered HTML

```csharp
FluentHtml.Div(div =>
{
    div.Id("orders-stream");
    div.Heimdall()
        .SseTopic("orders")
        .SseTarget("#orders-list")
        .SseSwap(HeimdallHtml.Swap.BeforeEnd);
});
```

Shortcut:

```csharp
div.Heimdall().Sse("orders", "#orders-list", HeimdallHtml.Swap.BeforeEnd);
```

Named event filtering:

```csharp
div.Heimdall()
    .SseTopic("orders")
    .SseEvent("order.updated")
    .SseTarget("#orders-list")
    .SseSwap(HeimdallHtml.Swap.Outer);
```

## Publish From Server Code

Inject or request `Bifrost` from DI. Use `Microsoft.AspNetCore.Mvc` for `[FromServices]`.

```csharp
using Microsoft.AspNetCore.Mvc;

[ContentInvocation("orders.publish")]
public static async Task<IHtmlContent> Publish([FromServices] Bifrost bifrost)
{
    await bifrost.PublishAsync(
        topic: "orders",
        content: OrderToast.Render("Order updated"),
        ttl: TimeSpan.FromSeconds(10));

    return HtmlString.Empty;
}
```

Named event:

```csharp
await bifrost.PublishAsync(
    topic: "orders",
    eventName: "order.updated",
    content: OrderRow.Render(order),
    ttl: TimeSpan.FromSeconds(10),
    ct: ct);
```

## SSE With Out-Of-Band Updates

Bifrost content can include response directives:

```csharp
var message = FluentHtml.Fragment(fragment =>
{
    fragment.Add(OrderToast.Render(order));
    fragment.Heimdall().Invocation(
        targetSelector: "#order-count",
        payload: OrderCount.Render(count));
});

await bifrost.PublishAsync("orders", message, TimeSpan.FromSeconds(10), ct);
```

## Authorization

Topic subscription can be authorized before a subscribe token is issued:

```csharp
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("BifrostTopic", policy =>
        policy.RequireAuthenticatedUser());
});

builder.Services.AddHeimdall(options =>
{
    options.BifrostTopicPolicy = "BifrostTopic";
    options.AuthorizeBifrostTopic = (ctx, topic) =>
        ValueTask.FromResult(
            topic.StartsWith($"user:{ctx.User.Identity?.Name}:", StringComparison.Ordinal));
});
```

If both `BifrostTopicPolicy` and `AuthorizeBifrostTopic` are configured, both must allow the topic.

## Guidance

- Use SSE for server-initiated updates, live feeds, notifications, dashboards, and progress streams.
- Publish HTML fragments, not JSON that the client must render.
- Use stable topic names such as `orders`, `alerts`, or `user:{id}:notifications`.
- Use named events when multiple UI regions share one topic but need different handling.
- Keep TTLs short enough for stale messages to expire naturally.
