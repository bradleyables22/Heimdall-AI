---
name: heimdall-assets-templates
description: Use when working with Heimdall static assets, templates, layouts, web root assets, Razor Class Library static web assets, shared template layers, copied SSG assets, and path-base-aware asset URLs.
---

# Heimdall Assets And Templates

Use this skill when handling assets, layouts, template layers, and path-aware URLs in Heimdall apps.

Official docs: https://heimdall-framework.org/assets

Heimdall can compose pages from FluentHtml, MVC partials, Bootstrap helpers, static generation, and your own template layer. Assets should remain predictable ASP.NET Core static assets.

## Runtime Asset

Load Heimdall's browser runtime from the web package:

```csharp
head.Script(script =>
{
    script.Src("/_content/HeimdallFramework.Web/heimdall-bundle.min.js");
});
```

Debug build:

```text
/_content/HeimdallFramework.Web/heimdall-bundle.js
```

## ASP.NET Core Static Assets

Use normal static file middleware:

```csharp
app.MapStaticAssets();
app.UseStaticFiles();
```

For static site output, copy assets through SSG options:

```csharp
options.CopyWebRootAssets = true;
options.CopyStaticWebAssets = true;
```

## Layouts

Keep layouts as ordinary render functions:

```csharp
public static IHtmlContent Render(IHtmlContent page, string title)
    => FluentHtml.Fragment(fragment =>
    {
        fragment.Raw("<!DOCTYPE html>")
            .HtmlTag(html =>
            {
                html.Head(head =>
                {
                    head.Title(t => t.Text(title));
                    head.Script(script =>
                        script.Src("/_content/HeimdallFramework.Web/heimdall-bundle.min.js"));
                });
                html.Body(body => body.Content(page));
            });
    });
```

## Path Base

For static generation or subdirectory hosting, use path-base-aware helpers when available, such as `ctx.ToSitePath("/css/site.css")` in static page render callbacks.

## Guidance

- Use standard ASP.NET Core static asset conventions.
- Keep layout rendering centralized.
- Keep asset URLs path-base aware for static/subdirectory deployments.
- Copy web root and static web assets during SSG when the output needs them.
- Do not invent custom runtime asset paths.
- Do not inline large scripts or styles into Heimdall response fragments unless a specific use case demands it.
