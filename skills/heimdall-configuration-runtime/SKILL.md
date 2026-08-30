---
name: heimdall-configuration-runtime
description: Use when configuring Heimdall server options or browser runtime behavior, including AddHeimdall settings, antiforgery, runtime script loading, Heimdall.config synchronization, timeouts, client information, asynchronous request headers, detailed errors, endpoint setup, middleware order, and runtime boot issues.
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

## Antiforgery Configuration

Validation and browser token work are enabled by default. To disable them globally, align the server and browser:

```csharp
builder.Services.AddHeimdall(options =>
{
    options.EnableAntiforgery = false;
});
```

```javascript
Heimdall.config.antiforgery = false;
```

Set the browser option before content actions or SSE subscriptions start. The browser setting only suppresses token acquisition, headers, and antiforgery retries; it does not weaken server validation by itself.

When globally disabled, Heimdall does not require `AddAntiforgery()` or `UseAntiforgery()`. Keep them when other endpoints use ASP.NET Core antiforgery. Prefer `[RequireAntiforgeryToken(false)]` when only one action or declaring type should opt out.

## Browser Client Information

Opt in to a bounded browser-capability snapshot on content actions:

```javascript
Heimdall.config.clientInfo = true;
Heimdall.config.clientInfoMaxAgeMs = 60_000;
```

Collection is disabled by default and adds no extra request. Use the focused `heimdall-client-info` skill for server binding, caching, events, CORS, and trust boundaries.

## Asynchronous Request Headers

Use an async provider when credentials or other headers must be resolved just before a request attempt:

```javascript
Heimdall.config.requestHeaders = async context => {
  if (new URL(context.url).origin !== window.location.origin)
    return {};

  const token = await auth.getAccessToken(context.signal);
  if (!token)
    throw new Error("Authentication is required.");

  return { Authorization: `Bearer ${token}` };
};
```

The provider runs for `content-action`, `csrf-token`, and `bifrost-token` requests. Inspect `context.kind` and `context.url` before attaching credentials. A rejection fails closed; the request is not sent and content actions resolve with code `request-headers-failed`.

Use `heimdall:unauthorized` for raw `401` handling:

```javascript
document.addEventListener("heimdall:unauthorized", event => {
  event.preventDefault();
  window.location.assign("/sign-in");
});
```

Preventing the event suppresses Heimdall's `Location` redirect only; the request remains failed and normal error reporting continues. Use `heimdall-request-lifecycle` for provider ordering, precedence, cancellation, and full event semantics.

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
- Keep server and browser antiforgery settings aligned.
- Filter async credentials by trusted request kind and origin.
- Keep interactions declarative in markup whenever possible.
- Debug by inspecting the rendered DOM first.
