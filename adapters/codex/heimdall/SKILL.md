---
name: heimdall
description: Build and modify Heimdall HTML-first ASP.NET Core systems. Use when Codex is asked to create, scaffold, edit, review, or explain Heimdall applications, including FluentHtml markup, HeimdallHtml behavior attributes, content actions, payload binding, response directives, Bifrost SSE, static site generation, MVC partial rendering, or Heimdall Bootstrap helpers.
---

# Heimdall

Use this skill to write idiomatic Heimdall systems.

Heimdall's central loop is:

```text
event -> server action -> HTML -> targeted DOM update
```

Prefer server-rendered HTML, `IHtmlContent`, `FluentHtml`, content actions, and targeted DOM swaps. Do not invent a SPA architecture unless the user explicitly asks for one.

## Workflow

1. Identify the task type.
2. Read only the relevant references.
3. Implement with Heimdall's native primitives.
4. Verify generated code uses real Heimdall APIs and package names.

## Reference Routing

- For app setup, pages, layouts, and static generation, read `references/app-structure.md`.
- For markup generation, forms, attributes, encoding, and builder idioms, read `references/fluent-html.md`.
- For interactions, triggers, payloads, target selectors, swap modes, DI, authorization, and timeouts, read `references/content-actions.md`.
- For out-of-band updates, aborts, redirects, and JavaScript void invocation, read `references/response-directives.md`.
- For real-time updates over server-sent events, read `references/bifrost-sse.md`.
- For strongly typed Bootstrap classes, read `references/bootstrap-helpers.md`.

## Default Rules

- Use `using Heimdall.Server;` for server services, page mapping, content actions, and Bifrost.
- Use `using Heimdall.Server.Rendering;` for `Html`, `FluentHtml`, and `HeimdallHtml`.
- Use `using Microsoft.AspNetCore.Html;` when returning or composing `IHtmlContent`.
- Use `using Bs = Heimdall.Bootstrap.Bootstrap;` when Bootstrap helpers are installed.
- Return `IHtmlContent` from render functions and content actions.
- Use `.Text(...)` for dynamic text and `.Raw(...)` only for trusted markup.
- Use `.Heimdall()` on element builders for triggers, targets, swaps, payloads, state, and SSE behavior.
- Use `.Heimdall()` on fragment builders for response directives.
- Return HTML fragments from content actions, not JSON UI state.
- Keep JavaScript explicit and small; use `JsInvokeVoid` only for browser effects.

## Common Shape

```csharp
public static IHtmlContent Render(IReadOnlyList<Order> orders)
    => FluentHtml.Section(section =>
    {
        section.Id("orders-panel");

        section.Form(form =>
        {
            form.Heimdall()
                .Submit("orders.filter")
                .PayloadFromClosestForm()
                .Target("#orders-panel")
                .SwapOuter()
                .PreventDefault()
                .Disable();

            form.Input(Html.InputType.search, input =>
            {
                input.Name("query")
                    .Placeholder("Search orders");
            });
        });

        section.Content(OrderList.Render(orders));
    });
```

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
