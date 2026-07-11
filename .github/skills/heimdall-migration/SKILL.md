---
name: heimdall-migration
description: Use when migrating existing ASP.NET Core UI to Heimdall, including Razor or MVC views, controller partials, client-side rendering, polling, WebSockets, incremental conversion strategy, and preserving services while moving toward server-rendered HTML fragments.
---

# Heimdall Migration

Use this skill when moving an existing ASP.NET Core UI toward Heimdall.

Official docs: https://heimdall-framework.org/patterns

## Migration Goal

Move interactive UI toward Heimdall's native loop:

```text
event -> server action -> HTML -> targeted DOM update
```

The target is not "rewrite everything." Prefer incremental conversion of high-value interactions while preserving existing domain services, authorization, validation, and data access code.

## From Razor Or MVC Views

1. Identify a page, partial, or repeated UI region that can become an `IHtmlContent` render function.
2. Move markup into a static renderer such as `OrdersPage.Render(...)`, `OrderList.Render(...)`, or `MainLayout.Render(...)`.
3. Replace Razor interpolation with `.Text(...)` for dynamic values.
4. Replace trusted static markup only when needed with `.Raw(...)`.
5. Keep models and services intact.
6. Map pages with `MapHeimdallPage`.
7. Move form posts and button interactions into content actions that return fragments.

Before:

```csharp
return View(new OrdersViewModel(orders));
```

After:

```csharp
app.MapHeimdallPage("/orders", async (sp, ctx) =>
{
    var repo = sp.GetRequiredService<IOrderRepository>();
    var orders = await repo.GetRecentAsync(ctx.RequestAborted);
    return MainLayout.Render(OrdersPage.Render(orders), "Orders");
});
```

## From Controller Actions Returning Partials

Convert partial-returning endpoints into content actions.

Before:

```csharp
public async Task<IActionResult> Filter(OrderFilter filter)
{
    var results = await orders.SearchAsync(filter);
    return PartialView("_OrderList", results);
}
```

After:

```csharp
[ContentInvocationPrefix("orders")]
public sealed class OrderActions(IOrderRepository orders)
{
    [ContentInvocation("filter")]
    public async Task<IHtmlContent> Filter([ContentPayload] OrderFilter filter, CancellationToken ct)
    {
        var results = await orders.SearchAsync(filter, ct);
        return OrderList.Render(results);
    }
}
```

Wire the rendered form:

```csharp
form.Heimdall()
    .Submit("orders.filter")
    .PayloadFromClosestForm()
    .Target("#orders-list")
    .SwapOuter()
    .PreventDefault()
    .Disable();
```

## From Client-Side Rendering

1. Find the state transition the client performs.
2. Move the state transition to an application service or content action.
3. Render the resulting HTML on the server.
4. Swap the target DOM region with Heimdall.
5. Keep JavaScript only for browser-only effects such as focus, clipboard, or analytics.

Do not return JSON for normal UI updates unless an existing API must remain for non-Heimdall clients.

## From Polling Or WebSockets

Use Bifrost SSE when the server needs to initiate HTML updates.

```csharp
feed.Heimdall()
    .SseTopic("orders")
    .SseTarget("#orders-feed")
    .SseSwap(HeimdallHtml.Swap.BeforeEnd);
```

Publish HTML:

```csharp
await bifrost.PublishAsync(
    topic: "orders",
    content: OrderToast.Render(order),
    ttl: TimeSpan.FromSeconds(10),
    ct: ct);
```

## Incremental Strategy

- Start with one page or interaction.
- Keep stable CSS selectors for target regions.
- Preserve existing service boundaries.
- Convert repeated markup into fragments before converting full pages.
- Use response directives when one action must update multiple targets.
- Add Bifrost only for live updates, notifications, progress, or dashboards.

## Things To Avoid

- Do not wrap Heimdall inside a new SPA shell.
- Do not convert server-rendered forms into JSON-driven client renderers.
- Do not create generic MVC controller endpoints for normal Heimdall content actions.
- Do not hand-build HTML strings during migration.
- Do not use `.Raw(...)` for old Razor values unless the value is trusted markup.
