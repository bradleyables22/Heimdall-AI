# Heimdall Project Instructions

Use these instructions when building or modifying Heimdall applications.

Heimdall is an HTML-first ASP.NET Core framework. Its main loop is:

```text
event -> server action -> HTML -> targeted DOM update
```

Prefer server-rendered HTML, `IHtmlContent`, `FluentHtml`, content actions, and targeted DOM swaps. Do not create a SPA shell unless the user explicitly asks for one.

Use these imports when needed:

```csharp
using Heimdall.Server;
using Heimdall.Server.Rendering;
using Microsoft.AspNetCore.Html;
using Bs = Heimdall.Bootstrap.Bootstrap;
```

Authoring rules:

- Use `FluentHtml` for markup.
- Use `.Text(...)` for dynamic text and `.Raw(...)` only for trusted markup.
- Use `.Heimdall()` on element builders for triggers, targets, swaps, payloads, state, and SSE.
- Use `.Heimdall()` on fragment builders for response directives.
- Use `[ContentInvocation]` and `[ContentInvocationPrefix]` for server actions.
- Return HTML fragments from content actions, not JSON UI state.
- Use Bifrost for server-sent event HTML streaming.
- Use `Heimdall.Bootstrap` helpers when Bootstrap styling is desired.

Preferred interaction pattern:

```csharp
form.Heimdall()
    .Submit("orders.filter")
    .PayloadFromClosestForm()
    .Target("#orders-panel")
    .SwapOuter()
    .PreventDefault()
    .Disable();
```

Preferred action pattern:

```csharp
[ContentInvocationPrefix("orders")]
public sealed class OrderActions(IOrderRepository orders)
{
    [ContentInvocation("filter")]
    public async Task<IHtmlContent> Filter([ContentPayload] OrderFilter filter, CancellationToken ct)
    {
        var results = await orders.SearchAsync(filter, ct);
        return OrdersPanel.Render(results);
    }
}
```

For full guidance, load the `canonical/` folder from the Heimdall.AI pack, especially `canonical/heimdall-authoring.md` and the references under `canonical/references/`.
