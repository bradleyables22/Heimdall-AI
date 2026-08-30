---
name: heimdall-bifrost-sse
description: Use when building Heimdall real-time features with Bifrost server-sent events, including subscriptions, topics, named events, IHtmlContent publication, response directives and mutations, TTLs, HasSubscribers hints, antiforgery, request authentication, and topic authorization.
---

# Heimdall Bifrost SSE

Use this skill when building real-time Heimdall features.

Official docs: https://heimdall-framework.org/sse

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
public static partial class OrderNotifications
{
    [ContentInvocationPrefix("orders")]
    public sealed class OrderNotificationsActions(Bifrost bifrost)
    {
        [ContentInvocation("publish")]
        public async Task<IHtmlContent> Publish(CancellationToken ct)
        {
            await bifrost.PublishAsync(
                topic: "orders",
                content: OrderToast.Render("Order updated"),
                ttl: TimeSpan.FromSeconds(10),
                ct: ct);

            return HtmlString.Empty;
        }
    }
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

## Subscriber Hint

Background services can avoid expensive rendering when nobody is currently listening on this application instance:

```csharp
if (!bifrost.HasSubscribers("orders"))
    return;

await bifrost.PublishAsync(
    "orders",
    OrdersPanel.Render(model),
    TimeSpan.FromSeconds(10),
    ct);
```

`HasSubscribers(topic)` is an instantaneous in-memory hint for the current process only. A subscriber can connect or disconnect immediately afterward. It does not list topics or users, account for other application instances, reserve delivery, or replace publishing correctness.

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

Streamed content may also contain ordered mutation directives. Use `heimdall-mutations` when a live update should change attributes, classes, or state without replacing a node.

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

## Antiforgery And Authentication

Bifrost first requests a signed subscribe token, then opens the native `EventSource` connection. Antiforgery applies to token minting by default.

When antiforgery is globally disabled, align the server's `EnableAntiforgery = false` with browser `Heimdall.config.antiforgery = false`. Signed Bifrost subscribe tokens still use data protection independently.

`Heimdall.config.requestHeaders` runs for the `bifrost-token` request, so bearer authentication can protect token minting. Native `EventSource` cannot receive arbitrary application headers; the async provider does not run on the stream connection itself. Raw `401` token responses emit `heimdall:unauthorized`.

## Guidance

- Use SSE for server-initiated updates, live feeds, notifications, dashboards, and progress streams.
- Publish HTML fragments, not JSON that the client must render.
- Keep publish actions beside the notification, stream, or live component that owns the topic when practical.
- Use stable topic names such as `orders`, `alerts`, or `user:{id}:notifications`.
- Use named events when multiple UI regions share one topic but need different handling.
- Keep TTLs short enough for stale messages to expire naturally.
- Use `HasSubscribers` only to skip optional expensive work, never as a delivery guarantee.
- Authenticate and authorize subscribe-token minting for protected topics.
