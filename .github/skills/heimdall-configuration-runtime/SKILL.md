---
name: heimdall-configuration-runtime
description: Use when configuring Heimdall server options or browser runtime behavior, including AddHeimdall settings, runtime script loading, Heimdall.config defaults, request synchronization and timeout defaults, detailed errors, endpoint setup, middleware order, and diagnosing runtime boot issues.
---

# Heimdall Configuration And Runtime

Use this skill when configuring Heimdall server options or browser runtime behavior.

Official docs: https://heimdall-framework.org/configuration

Heimdall has two cooperating sides:

- ASP.NET Core services and middleware discover actions, map endpoints, and render pages.
- The browser runtime observes Heimdall attributes, sends requests, processes responses, and swaps HTML.

## Server Setup

```csharp
using Heimdall.Server;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAntiforgery();
builder.Services.AddHeimdall(options =>
{
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
});

var app = builder.Build();

app.UseAntiforgery();
app.UseHeimdall();
```

Keep normal ASP.NET Core middleware explicit:

```csharp
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();
app.UseHeimdall();
```

## Browser Runtime Script

Layouts should load the Heimdall browser runtime:

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

## Runtime Defaults

Use runtime configuration for client-wide defaults, not for hiding core interaction behavior. The interaction should still be visible in rendered attributes.

Prefer local attributes or fluent helpers for:

- invocation IDs
- payload sources
- targets
- swap modes
- trigger modifiers

Use global runtime defaults only when an application-wide default is truly desired.

Request coordination defaults preserve existing behavior:

```javascript
Heimdall.config.requestSync = "parallel";
Heimdall.config.requestTimeoutMs = 0;
```

`parallel` and a timeout of `0` require no migration. Override them per element with `heimdall-sync` and `heimdall-sync-group`, or per programmatic invocation with `sync`, `syncGroup`, `timeoutMs`, and `signal`.

Use `heimdall:request-config`, `heimdall:request-before`, `heimdall:request-after`, `heimdall:request-finally`, `heimdall:request-cancel`, and `heimdall:request-timeout` for request integration. Use the focused `heimdall-request-lifecycle` skill for strategy selection and event ordering.

## Middleware Order

Use the normal ASP.NET Core ordering rules:

- Static files before page rendering.
- Authentication before authorization.
- Authorization before protected endpoints.
- Antiforgery configured before interactions that require it.
- Heimdall middleware/endpoints included before the app starts accepting requests.

## Runtime Boot Checklist

When an interaction does not fire:

1. Confirm the runtime script is loaded.
2. Confirm there is exactly one trigger attribute with an action ID.
3. Confirm the action ID matches the resolved `[ContentInvocation]` ID.
4. Confirm the payload source points to real data.
5. Confirm the target selector matches an element.
6. Confirm the swap mode is valid.
7. Confirm synchronization did not intentionally replace, drop, or queue the request.
8. Confirm the element is not disabled or `aria-disabled="true"`.

## Guidance

- Do not invent endpoint paths or runtime script paths.
- Do not use JavaScript config as a substitute for visible Heimdall attributes.
- Keep detailed errors development-only.
- Keep interactions declarative in markup whenever possible.
- Debug by inspecting the rendered DOM first.
