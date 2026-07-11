---
name: heimdall-getting-started
description: Use when starting a new Heimdall app or explaining the framework basics, including installing templates, choosing web app vs MVC templates, first page, first interaction, server-owned UI, and what Heimdall is not.
---

# Heimdall Getting Started

Use this skill when starting from zero or orienting someone to Heimdall.

Official docs: https://heimdall-framework.org/docs

Heimdall is an HTML-first, server-driven UI framework for ASP.NET Core. It keeps the browser simple, keeps the server in control, and removes the need for a separate client-side application layer in many cases.

## What Heimdall Solves

Traditional stack:

```text
UI -> JSON API -> client state -> DOM updates
```

Heimdall:

```text
UI -> HTML -> DOM updates
```

Heimdall helps:

- remove separate JSON API layers for many UI interactions
- avoid client-side state synchronization problems
- keep UI rendering and business logic together on the server
- support partial updates, live updates, and streaming without a SPA

## What Heimdall Is Not

- Not a SPA framework.
- Not a client-side state manager.
- Not a replacement for APIs in all scenarios.
- Not a virtual DOM or component runtime.

## Install Templates

Choose the starter that matches the rendering style.

HTML-first web app:

```bash
dotnet new install HeimdallFramework.Templates.WebApp
dotnet new heimdall-webapp -n MyHeimdallApp
```

MVC/Razor app:

```bash
dotnet new install HeimdallFramework.Templates.MvcApp
dotnet new heimdall-mvc -n MyHeimdallMvcApp
```

Static site app:

```bash
dotnet new install HeimdallFramework.Templates.SsgApp
dotnet new heimdall-ssg -n MyHeimdallDocs
```

## First Page

A Heimdall page is a route that returns HTML:

```csharp
app.MapHeimdallPage("/", ctx =>
{
    return FluentHtml.Div(d =>
    {
        d.Text("Hello from Heimdall");
    });
});
```

## First Interaction

A Heimdall interaction sends a request to a server action, gets HTML back, and swaps that HTML into the DOM.

```csharp
button.Text("Click Me");
button.Heimdall()
    .Click("counter.increment")
    .Target("#result")
    .SwapInner();
```

Rendered HTML mental model:

```html
<button
  heimdall-content-click="counter.increment"
  heimdall-content-target="#result"
  heimdall-content-swap="inner">
  Click Me
</button>

<div id="result"></div>
```

## Guidance

- Start with one route and one interaction.
- Let pages and actions return HTML.
- Keep the server as the source of truth.
- Add MVC, SSG, Bootstrap, Bifrost, and JavaScript interop only when the app needs them.
- Do not begin by creating a SPA shell.
