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

## Guidance

- Use pages for route-level rendering.
- Use content actions for interaction-level fragments.
- Keep page mapping in `Program.cs` or a clear endpoint module.
- Apply endpoint authorization to protected pages.
- Keep layouts responsible for shared chrome and assets.
- Do not use content actions as full-page routes.
- Do not use pages as ad hoc action endpoints for dynamic fragments.
