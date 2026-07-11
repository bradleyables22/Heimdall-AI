---
name: heimdall-app-structure
description: Use when setting up, scaffolding, or reviewing Heimdall ASP.NET Core application structure, including AddHeimdall, UseHeimdall, MapHeimdallPage, layouts, static assets, static site generation, and where pages, fragments, actions, and models belong.
---

# Heimdall App Structure

Use this skill when building the shell of a Heimdall application or deciding where Heimdall code belongs.

Official docs: https://heimdall-framework.org/pages

Heimdall is HTML-first. The application loop is:

```text
event -> server action -> HTML -> targeted DOM update
```

Prefer normal ASP.NET Core services, normal C# render functions, and server-rendered HTML. Do not create a SPA shell unless the user explicitly asks for one.

## Minimal Setup

```csharp
using Heimdall.Server;
using Heimdall.Server.Rendering;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAntiforgery();
builder.Services.AddHeimdall(options =>
{
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
});

var app = builder.Build();

app.UseAntiforgery();

app.MapStaticAssets();
app.UseStaticFiles();

app.UseHeimdall();

app.MapHeimdallPage("/", () =>
    Html.Tag("main",
        Html.Tag("h1", "Hello Heimdall"),
        Html.Tag("p", "HTML-first server-driven UI.")));

app.Run();
```

Reference the browser runtime from layouts or pages:

```csharp
head.Script(script =>
{
    script.Src("/_content/HeimdallFramework.Web/heimdall-bundle.min.js");
});
```

For debugging, use:

```text
/_content/HeimdallFramework.Web/heimdall-bundle.js
```

## Pages

A Heimdall page is a function that returns `IHtmlContent`.

```csharp
app.MapHeimdallPage("/", () =>
    MainLayout.Render(HomePage.Render(), "Home"));
```

Pages can use `HttpContext`:

```csharp
app.MapHeimdallPage("/profile", ctx =>
    MainLayout.Render(ProfilePage.Render(ctx.User), "Profile"));
```

Pages can use DI through the `(IServiceProvider, HttpContext)` overload:

```csharp
app.MapHeimdallPage("/dashboard", async (sp, ctx) =>
{
    var repo = sp.GetRequiredService<IDashboardRepository>();
    var model = await repo.GetAsync(ctx.RequestAborted);
    return MainLayout.Render(DashboardPage.Render(model), "Dashboard");
});
```

## Layouts

Layouts are normal functions. Use them to wrap page content, include static assets, and reference the Heimdall browser runtime.

```csharp
using Heimdall.Server.Rendering;
using Microsoft.AspNetCore.Html;

public static class MainLayout
{
    public static IHtmlContent Render(IHtmlContent page, string title)
        => FluentHtml.Fragment(fragment =>
        {
            fragment.Raw("<!DOCTYPE html>")
                .HtmlTag(html =>
                {
                    html.Attr("lang", "en")
                        .Head(head =>
                        {
                            head.Meta(meta => meta.Attr("charset", "utf-8"));
                            head.Meta(meta =>
                            {
                                meta.Name("viewport")
                                    .ContentAttr("width=device-width, initial-scale=1");
                            });
                            head.Title(t => t.Text(title));
                            head.Script(script =>
                                script.Src("/_content/HeimdallFramework.Web/heimdall-bundle.min.js"));
                        })
                        .Body(body => body.Content(page));
                });
        });
}
```

## Suggested File Shape

```text
Rendering/
  Layouts/
    MainLayout.cs
  Pages/
    HomePage.cs
    OrdersPage.cs
  Fragments/
    OrderList.cs
Actions/
  OrderActions.cs
Models/
  OrderFilter.cs
```

Keep rendering close to the UI it represents. Keep content action methods close to the domain interaction they handle.

## Static Site Generation

Use static site generation for explicit page output:

```csharp
builder.Services
    .AddHeimdallStaticSiteGeneration(options =>
    {
        options.OutputPath = "dist";
        options.CleanOutputPath = true;
        options.CopyWebRootAssets = true;
        options.CopyStaticWebAssets = true;
        options.UseSitemap("https://example.com");
        options.UseRobotsTxt();
    })
    .WithStaticPage("/", () => MainLayout.Render(HomePage.Render(), "Home"))
    .WithNotFoundPage(() => MainLayout.Render(NotFoundPage.Render(), "Not Found"));

var app = builder.Build();

app.MapStaticAssets();
app.UseDefaultFiles();
app.UseStaticFiles();

await app.RunWithHeimdallStaticSiteGenerationAsync(args);
```

Use `UsePathBase("/portal")` for subdirectory deployments. In static page render callbacks, use `ctx.ToSitePath("/css/site.css")` for rooted links that need to honor the configured path base.
