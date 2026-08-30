---
name: heimdall-security
description: Use when reviewing or implementing Heimdall security, including encoding and Raw safety, global and per-action antiforgery, JWT or asynchronous request headers, unauthorized handling, CORS, upload validation, untrusted client information, authorization metadata, JavaScript directives, and SSE topic authorization.
---

# Heimdall Security

Use this skill when implementing or reviewing security-sensitive Heimdall code.

Official docs: https://heimdall-framework.org/security

Heimdall is server-driven, but normal web security still applies. Treat content actions like endpoints that accept input and return HTML.

## Encoding

Use `.Text(...)` for dynamic or user-provided text:

```csharp
p.Text(user.DisplayName);
```

Use `.Raw(...)` only for trusted static markup:

```csharp
fragment.Raw("<!DOCTYPE html>");
```

Do not pass user content, database content, or request values to `Raw`.

## Antiforgery

Configure antiforgery for interactions that mutate state:

```csharp
builder.Services.AddAntiforgery();

var app = builder.Build();

app.UseAntiforgery();
```

Validation is enabled by default. A specific action or declaring type can opt out with native ASP.NET Core metadata:

```csharp
using Microsoft.AspNetCore.Antiforgery;

[RequireAntiforgeryToken(false)]
[ContentInvocation("webhook.receive")]
public static IHtmlContent Receive(WebhookPayload payload)
    => Html.Span("Accepted");
```

Method metadata overrides type metadata. Disable globally only when another model makes cross-site requests harmless, such as a bearer-token API without ambient cookie authentication:

```csharp
builder.Services.AddHeimdall(options =>
{
    options.EnableAntiforgery = false;
});
```

Also set `Heimdall.config.antiforgery = false` before browser actions or SSE subscriptions begin. The server setting is authoritative; the browser setting only avoids unnecessary token work.

## Bearer Tokens And Request Headers

Resolve short-lived credentials asynchronously at request-attempt time:

```javascript
Heimdall.config.requestHeaders = async context => {
  const url = new URL(context.url);
  if (url.origin !== window.location.origin)
    return {};

  const token = await auth.getAccessToken(context.signal);
  if (!token)
    throw new Error("Authentication is required.");

  return { Authorization: `Bearer ${token}` };
};
```

The provider runs for content actions, CSRF-token requests, and Bifrost subscribe-token requests. Filter by `context.kind` and trusted origin before attaching credentials. Provider rejection fails closed and prevents the request.

Use the cancellable `heimdall:unauthorized` event to show login UI or navigate after raw `401` responses. Preventing default suppresses only a `Location`-based redirect; it does not convert failure into success. Do not treat `403` as an authentication challenge.

Native `EventSource` cannot receive arbitrary headers. Authenticate the Bifrost token-minting request; the signed subscribe token then protects the stream connection.

## File Uploads

Uploaded files are untrusted input:

- Set finite request and multipart section limits.
- Validate content signatures rather than trusting extension or `ContentType`.
- Generate storage names instead of using `IFormFile.FileName`.
- Store files outside executable/static roots unless controlled serving is intended.
- Use a dedicated streaming endpoint for files too large for buffered `IFormFile` handling.

## Client Information

`HeimdallClientInfo` values come from the `X-Heimdall-Client-Info` request header. Never use timezone, locale, viewport, device category, theme, or capability values for authorization, pricing, auditing, or identity. Render a safe default when `IsAvailable` is false.

## Authorization

Content actions honor ASP.NET Core authorization metadata:

```csharp
[Authorize(Roles = "Admin")]
[ContentInvocation("admin.refresh")]
public static IHtmlContent RefreshAdminPanel(HttpContext ctx)
{
    return AdminPanel.Render(ctx.User);
}
```

Page routes can also require authorization:

```csharp
app.MapHeimdallPage("/admin", ctx => AdminPage.Render(ctx))
   .RequireAuthorization();
```

## Payload Validation

- Validate payloads server-side.
- Normalize strings before use.
- Use typed payload models.
- Treat inline payload IDs as untrusted and re-check permissions server-side.
- Do not trust client state as authoritative domain state.

## Request Timeouts

Use request timeout metadata for long-running or externally dependent actions:

```csharp
[ContentInvocation("search")]
[RequestTimeout(milliseconds: 2000)]
public static async Task<IHtmlContent> Search(SearchPayload payload, CancellationToken ct)
{
    var results = await SearchService.QueryAsync(payload.Query, ct);
    return SearchResults.Render(results);
}
```

## JavaScript Directives

Heimdall JavaScript directives name existing browser functions. They do not evaluate JavaScript source.

Rules:

- Function paths must start with `window.`, `globalThis.`, or `document.`.
- Bare global names are invalid.
- Bracket expressions are invalid.
- Arguments are serialized as JSON.
- Return values are ignored.

## Bifrost Topic Authorization

Authorize sensitive topics:

```csharp
builder.Services.AddHeimdall(options =>
{
    options.BifrostTopicPolicy = "BifrostTopic";
    options.AuthorizeBifrostTopic = (ctx, topic) =>
        ValueTask.FromResult(
            topic.StartsWith($"user:{ctx.User.Identity?.Name}:", StringComparison.Ordinal));
});
```

`Bifrost.HasSubscribers(topic)` reveals only an instantaneous local-instance boolean. It does not identify users, replace authorization, or guarantee delivery.

## Cross-Origin Frontends

Cross-origin Heimdall requests generally require CORS preflight because they use non-safelisted headers. Explicitly allow trusted frontend origins, methods, and the headers the application enables, including:

- `X-Heimdall-Content-Action`
- `RequestVerificationToken`
- `X-Heimdall-Client-Info`
- application headers such as `Authorization`
- JSON and multipart content types

Do not recommend unrestricted origins for authenticated applications. The runtime currently uses `credentials: "same-origin"`; verify the application's supported authentication model before promising cross-domain cookie authentication.

## Guidance

- Prefer server-rendered HTML over client-rendered JSON for UI state.
- Keep authorization on pages and actions.
- Re-check permissions inside actions that operate on IDs.
- Keep JavaScript interop explicit and narrow.
- Never use `Raw` for untrusted values.
- Avoid leaking sensitive data into inline payloads or state blobs.
- Keep async credential providers origin-aware and fail closed when a required token is unavailable.
- Treat uploads and client-information headers as untrusted input.
