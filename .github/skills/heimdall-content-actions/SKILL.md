---
name: heimdall-content-actions
description: Use when wiring Heimdall interactions and server-side content actions, including .Heimdall() triggers, invocation IDs, payload sources, targets, swap modes, ContentInvocation attributes, payload binding, dependency injection, authorization, and request timeouts.
---

# Heimdall Content Actions

Use this skill when wiring Heimdall interactions.

Official docs: https://heimdall-framework.org/actions

Content actions are server methods that return HTML fragments for DOM updates. The browser posts to Heimdall's content action endpoint, the server invokes the action, and the returned HTML is swapped into the requested target.

## Rendered Trigger

Attach a trigger, target, swap mode, and payload source to rendered HTML:

```csharp
button.Heimdall()
    .Click("notes.save")
    .PayloadFromClosestForm()
    .Target("#notes-panel")
    .SwapOuter()
    .PreventDefault()
    .Disable();
```

Equivalent static helpers exist:

```csharp
Html.Button(
    HeimdallHtml.OnClick("notes.save"),
    HeimdallHtml.Target("#notes-panel"),
    HeimdallHtml.SwapMode(HeimdallHtml.Swap.Outer),
    "Save");
```

## Action Methods

Use `[ContentInvocation]` on static or instance methods:

```csharp
[ContentInvocation("notes.save")]
public static IHtmlContent Save([ContentPayload] NotePayload payload)
{
    return NotesPanel.Render(payload);
}
```

Use `[ContentInvocationPrefix]` on action classes:

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

    [ContentInvocation]
    public IHtmlContent Summary()
    {
        return OrderSummary.Render(orders.GetSummary());
    }
}
```

The resolved invocation IDs are `orders.filter` and `orders.Summary`.

## Supported Parameters

Content actions support:

- `HttpContext`
- `CancellationToken`
- `ClaimsPrincipal`
- constructor dependencies through DI for instance action classes
- implicit service parameters
- `[FromServices]` service parameters
- one payload parameter, optionally marked with `[ContentPayload]`

Supported return shapes:

- `IHtmlContent`
- `Task<IHtmlContent>`
- `ValueTask<IHtmlContent>`

## Payload Sources

Closest form:

```csharp
form.Heimdall()
    .Submit("todos.add")
    .PayloadFromClosestForm()
    .Target("#todo-panel")
    .SwapOuter()
    .PreventDefault();
```

Inline static payload:

```csharp
button.Heimdall()
    .Click("orders.archive")
    .Payload(new { Id = order.Id })
    .Target("#orders-list")
    .SwapOuter();
```

Empty payload:

```csharp
button.Heimdall()
    .Click("orders.refresh")
    .PayloadEmptyObject()
    .Target("#orders-list")
    .SwapOuter();
```

Closest state:

```csharp
host.Heimdall().State(new CounterState { Count = count });

button.Heimdall()
    .Click("counter.increment")
    .PayloadFromClosestState()
    .Target("#counter")
    .SwapOuter();
```

Global reference:

```csharp
button.Heimdall()
    .Click("search.run")
    .PayloadFromRef("window.App.search")
    .Target("#results")
    .SwapOuter();
```

## Triggers

Common fluent trigger helpers:

```csharp
.Load("feed.initial")
.Click("orders.refresh")
.Change("filters.changed")
.Input("search.preview")
.Submit("form.save")
.KeyDown("command.enter")
.Blur("field.validate")
.Hover("card.preview")
.Visible("feed.more")
.Scroll("feed.more")
```

Useful modifiers:

```csharp
.DebounceMs(250)
.Key("Enter")
.HoverDelayMs(300)
.VisibleOnce()
.ScrollThresholdPx(200)
.PollMs(5000)
.ScopeSelf()
.ScopeClosest()
.IgnoreAll()
```

## Authorization And Timeouts

Content actions honor ASP.NET Core authorization metadata:

```csharp
using Microsoft.AspNetCore.Authorization;

[Authorize(Roles = "Admin")]
[ContentInvocation("admin.refresh")]
public static IHtmlContent RefreshAdminPanel(HttpContext ctx)
{
    return AdminPanel.Render(ctx.User);
}
```

They also honor request timeout metadata:

```csharp
using Microsoft.AspNetCore.Http.Timeouts;

[ContentInvocation("search")]
[RequestTimeout(milliseconds: 2000)]
public static async Task<IHtmlContent> Search(SearchPayload payload, CancellationToken ct)
{
    var results = await SearchService.QueryAsync(payload.Query, ct);
    return SearchResults.Render(results);
}
```

Use `[DisableRequestTimeout]` when an action should opt out of configured timeouts.

## Naming Guidance

- Use stable dotted invocation IDs such as `orders.filter`, `todos.add`, and `profile.save`.
- Keep target IDs stable and specific, such as `#orders-list`, `#todo-panel`, and `#profile-form`.
- Use `SwapOuter()` when the action returns a replacement for the whole target.
- Use `SwapInner()` when the action returns only the target's children.
- Use `SwapBeforeEnd()` for append behavior.
- Use `SwapNone()` when response directives or side effects are the main purpose.
