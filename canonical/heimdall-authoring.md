# Heimdall Authoring Guide For AI Assistants

Use this guide when building or modifying Heimdall applications.

Heimdall is an HTML-first framework for ASP.NET Core applications. The central loop is:

```text
event -> server action -> HTML -> targeted DOM update
```

Prefer server-rendered HTML and small interaction islands over client-side application state machines. The server owns rendering and orchestration. The browser runtime resolves triggers and payloads, posts to Heimdall endpoints, and swaps returned HTML into the DOM.

## Package Model

Typical applications use:

- `HeimdallFramework.Server` for ASP.NET Core services, middleware, pages, content actions, Bifrost SSE, MVC partial rendering, static generation, and rendering helpers.
- `HeimdallFramework.Web` for the JavaScript browser runtime served as Razor Class Library static web assets.
- `HeimdallFramework.Bootstrap` when strongly typed Bootstrap classes are desired.

Common namespaces:

```csharp
using Heimdall.Server;
using Heimdall.Server.Rendering;
using Microsoft.AspNetCore.Html;
using Bs = Heimdall.Bootstrap.Bootstrap;
```

## Default Authoring Rules

- Return `IHtmlContent` from pages, layouts, fragments, and content actions.
- Use `FluentHtml` for new markup unless there is a strong reason to use the lower-level `Html` helpers.
- Use `.Text(...)` for user-provided or dynamic text so content is encoded.
- Use `.Raw(...)` only for trusted markup such as `<!DOCTYPE html>` or known static snippets.
- Keep pages and partials as normal C# functions. Heimdall does not require Razor, components, or a SPA framework.
- Use content actions for server-side interaction. Return HTML fragments, not JSON view models, for UI updates.
- Use `.Heimdall()` fluent helpers to attach triggers, targets, swaps, payload sources, state, and SSE behavior.
- Use response directives for out-of-band updates, aborts, redirects, and explicit JavaScript void calls.
- Use Bifrost for real-time HTML streaming over server-sent events.
- Keep JavaScript small and explicit. Do not generate JavaScript source from server responses.

## Reference Routing

Read only the references needed for the task:

- `references/app-structure.md`: application setup, layout shape, page mapping, static generation.
- `references/fluent-html.md`: `FluentHtml`, `Html`, builders, encoding, forms, tables, attributes.
- `references/content-actions.md`: action discovery, invocation IDs, payload binding, DI, authorization, request timeouts.
- `references/response-directives.md`: out-of-band invocations, aborts, redirects, JavaScript void invocations.
- `references/bifrost-sse.md`: SSE subscription markup and server publishing with Bifrost.
- `references/bootstrap-helpers.md`: strongly typed Bootstrap class helpers.

## Good Heimdall Shape

Prefer small modules:

```text
Rendering/
  Layouts/
    MainLayout.cs
  Pages/
    DashboardPage.cs
    OrdersPage.cs
  Fragments/
    OrderList.cs
Actions/
  OrderActions.cs
Models/
  OrderFilter.cs
```

Render modules should be ordinary static methods:

```csharp
public static class OrdersPage
{
    public static IHtmlContent Render(IReadOnlyList<Order> orders)
        => FluentHtml.Main(main =>
        {
            main.Id("orders-page");
            main.Content(OrderList.Render(orders));
        });
}
```

Content actions should line up with the rendered invocation IDs:

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

Buttons and forms should declare behavior on the HTML they render:

```csharp
form.Heimdall()
    .Submit("orders.filter")
    .PayloadFromClosestForm()
    .Target("#orders-list")
    .SwapOuter()
    .PreventDefault()
    .Disable();
```

## Anti-Patterns

- Do not create a SPA shell unless the user explicitly asks for one.
- Do not return JSON and ask the browser to render it for normal Heimdall UI updates.
- Do not hand-build HTML strings when `FluentHtml` or `Html` helpers can express the markup.
- Do not use `Raw` for untrusted values.
- Do not invent attributes, endpoints, helper names, or package names. Use the Heimdall helpers from this pack.
- Do not route normal Heimdall content actions through MVC controllers.
- Do not put app state primarily in JavaScript when the server can render the current HTML.
