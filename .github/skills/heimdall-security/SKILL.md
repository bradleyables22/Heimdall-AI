---
name: heimdall-security
description: Use when reviewing or implementing Heimdall security, including encoded text, Raw safety, antiforgery, authorization metadata, request timeouts, payload validation, JavaScript directive safety, SSE topic authorization, and avoiding JSON/client-state security drift.
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

## Guidance

- Prefer server-rendered HTML over client-rendered JSON for UI state.
- Keep authorization on pages and actions.
- Re-check permissions inside actions that operate on IDs.
- Keep JavaScript interop explicit and narrow.
- Never use `Raw` for untrusted values.
- Avoid leaking sensitive data into inline payloads or state blobs.
