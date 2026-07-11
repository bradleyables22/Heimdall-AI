---
name: heimdall-static-site-generation
description: Use when generating static Heimdall sites, configuring AddHeimdallStaticSiteGeneration, registering static pages, output paths, copied assets, sitemap, robots.txt, path base handling, SSG templates, and build-time generation commands.
---

# Heimdall Static Site Generation

Use this skill when a Heimdall app should render selected routes to static HTML.

Official docs: https://heimdall-framework.org/static-site-generation

Heimdall SSG uses the same server rendering code and dependency injection container as the running app. Use it when pages can be built ahead of time but still need FluentHtml, layouts, services, assets, sitemap, robots.txt, and normal ASP.NET Core startup.

## Template

Use the SSG template for a new static-first app:

```bash
dotnet new install HeimdallFramework.Templates.SsgApp
dotnet new heimdall-ssg -n MyHeimdallDocs
cd MyHeimdallDocs
dotnet run
```

Generate static output:

```bash
dotnet run -- --heimdall-generate-static
```

## Basic Model

Static generation is opt-in. Register the pages to generate, then run the app with the generation command.

```text
Normal app run:
dotnet run

Static generation run:
dotnet run -- --heimdall-generate-static

Output:
dist/
  index.html
  docs/index.html
  404.html
  heimdall.static.manifest.json
```

## Register Generation

Register static generation before `builder.Build()`:

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
    .WithStaticPage("/", () =>
        MainLayout.Render(HomePage.Render(), "Home"))
    .WithStaticPage("/docs", ctx =>
        MainLayout.Render(DocsPage.Render(ctx), "Docs"))
    .WithNotFoundPage(() =>
        MainLayout.Render(NotFoundPage.Render(), "Not Found"));
```

Then keep the app pipeline normal:

```csharp
var app = builder.Build();

app.MapStaticAssets();
app.UseDefaultFiles();
app.UseStaticFiles();

await app.RunWithHeimdallStaticSiteGenerationAsync(args);
```

## Path Base

Use `UsePathBase("/portal")` for subdirectory deployments. In static page render callbacks, use `ctx.ToSitePath("/css/site.css")` for rooted links that must honor the configured path base.

## Guidance

- Generate only explicit pages.
- Keep render code shared between static and runtime modes when practical.
- Use DI-backed services during generation for content catalogs, markdown, docs metadata, or navigation.
- Copy static web assets when the output needs Heimdall or framework-provided assets.
- Prefer static generation for docs, marketing pages, content sites, and hybrid apps with mostly build-time pages.
- Do not use SSG for pages that require per-request authenticated state unless that state is rendered later by runtime interactions.
