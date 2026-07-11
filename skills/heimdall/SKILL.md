---
name: heimdall
description: Use this skill when creating, editing, reviewing, or explaining Heimdall HTML-first ASP.NET Core apps. Covers FluentHtml markup, HeimdallHtml behavior attributes, content actions, payload binding, response directives, Bifrost SSE, static site generation, MVC partial rendering, and Heimdall Bootstrap helpers.
compatibility: Agent Skills open standard. Designed for OpenAI Codex, Claude Code, GitHub Copilot/VS Code, Gemini CLI, and compatible file-reading agents.
metadata:
  version: "0.1.0"
  package: "Heimdall.AI"
---

# Heimdall

Use this skill to write idiomatic Heimdall systems.

Heimdall is an HTML-first framework for ASP.NET Core applications. The central loop is:

```text
event -> server action -> HTML -> targeted DOM update
```

Prefer server-rendered HTML, `IHtmlContent`, `FluentHtml`, content actions, and targeted DOM swaps. Do not invent a SPA architecture unless the user explicitly asks for one.

## Workflow

1. Confirm the task is really about Heimdall authoring or migration.
2. Inspect the target project for existing Heimdall packages, namespaces, helpers, and local conventions.
3. Read only the reference files needed for the current task.
4. Implement with Heimdall's native primitives.
5. Verify generated code uses real Heimdall APIs, stable invocation IDs, encoded dynamic text, and HTML fragments for UI updates.
6. Run the project's normal build or tests when available.

## Reference Routing

Resolve paths relative to this skill directory.

- For app setup, pages, layouts, static generation, static assets, and the browser runtime, read `references/app-structure.md`.
- For markup generation, forms, attributes, encoding, tables, and builder idioms, read `references/fluent-html.md`.
- For interactions, triggers, payloads, targets, swaps, DI, authorization, and timeouts, read `references/content-actions.md`.
- For out-of-band updates, aborts, redirects, and JavaScript void invocations, read `references/response-directives.md`.
- For real-time updates over server-sent events, read `references/bifrost-sse.md`.
- For strongly typed Bootstrap classes, read `references/bootstrap-helpers.md`.
- For complete working patterns, inspect `assets/examples/minimal-app`, `assets/examples/form-content-action`, or `assets/examples/live-sse-feed` only when examples would clarify the implementation.

## Default Rules

- Use `using Heimdall.Server;` for server services, page mapping, content actions, and Bifrost.
- Use `using Heimdall.Server.Rendering;` for `Html`, `FluentHtml`, and `HeimdallHtml`.
- Use `using Microsoft.AspNetCore.Html;` when returning or composing `IHtmlContent`.
- Use `using Bs = Heimdall.Bootstrap.Bootstrap;` when Bootstrap helpers are installed.
- Return `IHtmlContent` from render functions and content actions.
- Use `FluentHtml` for new markup unless the surrounding code uses lower-level `Html` helpers.
- Use `.Text(...)` for user-provided or dynamic text.
- Use `.Raw(...)` only for trusted markup such as `<!DOCTYPE html>` or known static snippets.
- Use `.Heimdall()` on element builders for triggers, targets, swaps, payload sources, state, and SSE behavior.
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

## Anti-Patterns

- Do not create a SPA shell unless the user explicitly asks for one.
- Do not return JSON and ask the browser to render it for normal Heimdall UI updates.
- Do not hand-build HTML strings when `FluentHtml` or `Html` helpers can express the markup.
- Do not use `Raw` for untrusted values.
- Do not invent attributes, endpoints, helper names, package names, or browser-runtime paths.
- Do not route normal Heimdall content actions through MVC controllers.
- Do not put app state primarily in JavaScript when the server can render the current HTML.
