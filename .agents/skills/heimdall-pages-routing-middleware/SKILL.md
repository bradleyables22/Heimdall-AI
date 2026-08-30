---
name: heimdall-pages-routing-middleware
description: Use when mapping Heimdall pages and configuring ASP.NET Core routing or middleware, including MapHeimdallPage overloads, HttpContext, dependency injection, route-level rendering, authorization, pages vs content invocations, and pipeline order.
---

# Heimdall Pages, Routing, And Middleware

Use this skill when mapping route-level Heimdall pages or configuring the ASP.NET Core pipeline.

Official docs: https://heimdall-framework.org/pages

Pages are for browser routes. Content invocations are for triggered interactions. Keeping that distinction clear prevents messy architecture.

## Pages

A Heimdall page returns `IHtmlContent` for a route:

```csharp
app.MapHeimdallPage("/", () =>
    MainLayout.Render(HomePage.Render(), "Home"));
```

Pages can use `HttpContext`:

```csharp
app.MapHeimdallPage("/profile", ctx =>
    MainLayout.Render(ProfilePage.Render(ctx.User), "Profile"));
```

Pages can use dependency injection:

```csharp
app.MapHeimdallPage("/dashboard", async (sp, ctx) =>
{
    var repo = sp.GetRequiredService<IDashboardRepository>();
    var model = await repo.GetAsync(ctx.RequestAborted);
    return MainLayout.Render(DashboardPage.Render(model), "Dashboard");
});
```

## Middleware

Use normal ASP.NET Core middleware:

```csharp
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

app.MapHeimdallPage("/", () => HomePage.Render());
app.MapHeimdallPage("/pages", () => RoutingPage.Render());
app.MapHeimdallPage("/admin", ctx => AdminPage.Render(ctx))
   .RequireAuthorization();
```

## Pages vs Content Invocations

Pages:

- mapped with `MapHeimdallPage(...)`
- used for route-level rendering when the browser requests a URL
- handle URLs such as `/`, `/pages`, or `/forms`
- return full HTML for the request
- usually wrapped in a layout
- participate in the normal ASP.NET Core endpoint pipeline

Content invocations:

- marked with `[ContentInvocation]`
- used for triggered interactions that return fresh HTML fragments
- triggered by Heimdall attributes
- typically swapped into an existing target
- run inside ASP.NET Core but serve interaction flows instead of route-level page loads

## Canonical Routes From Action History

A successful content action may push or replace the address-bar URL with a history response directive. The action remains an RPC interaction; the history URL must also map to a normal page because browser Back, Forward, refresh, sharing, and direct entry use GET.

```csharp
app.MapHeimdallPage("/orders/{id:int}", (route, ctx) =>
    MainLayout.Render(
        ctx,
        OrderPage.Render(route.GetInt32("id")),
        "Order"));

[ContentInvocation("orders.open")]
public static IHtmlContent Open([ContentPayload] OpenOrder payload)
    => FluentHtml.Fragment(fragment =>
    {
        fragment.Add(OrderPanel.Render(payload.Id));
        fragment.Heimdall().HistoryPush($"orders/{payload.Id}");
    });
```

`orders/42` and `/orders/42` intentionally resolve to the same origin-rooted URL. Do not depend on the current page directory. Use push for a new navigation state and replace when correcting or refining the current entry. There is no server-issued pop command.

## Guidance

- Use pages for route-level rendering.
- Use content actions for interaction-level fragments.
- Keep page mapping in `Program.cs` or a clear endpoint module.
- Apply endpoint authorization to protected pages.
- Keep layouts responsible for shared chrome and assets.
- Do not use content actions as full-page routes.
- Map every URL emitted by `HistoryPush` or `HistoryReplace` as a page route.
- Do not use pages as ad hoc action endpoints for dynamic fragments.
