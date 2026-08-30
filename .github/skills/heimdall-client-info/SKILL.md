---
name: heimdall-client-info
description: Use when sending browser presentation and capability information with Heimdall content actions, including HeimdallClientInfo binding, runtime opt-in, caching and invalidation, request customization events, viewport and preference fields, CORS, privacy, and untrusted-input rules.
---

# Heimdall Client Information

Use client information when a content action can improve presentation from browser capabilities that are unavailable in the normal HTTP request.

Official docs: https://heimdall-framework.org/configuration

Collection is opt-in and applies only to content-action requests. It does not make an additional network request.

## Enable In The Browser

Configure the runtime before Heimdall actions begin:

```javascript
Heimdall.config.clientInfo = true;
Heimdall.config.clientInfoMaxAgeMs = 60_000;
```

The runtime collects lazily on the first action and caches the serialized snapshot. Set `clientInfoMaxAgeMs` to `0` to collect on every action.

The cache is invalidated for relevant browser changes including resize, orientation, language, online status, display preferences, pointer, and hover capability. Maximum age also refreshes timezone and UTC offset when no browser event is emitted.

## Bind In A Content Action

`HeimdallClientInfo` is a framework parameter and does not consume the action's single payload slot:

```csharp
[ContentInvocation("dashboard.render")]
public static IHtmlContent Render(
    DashboardRequest request,
    HeimdallClientInfo client)
{
    if (!client.IsAvailable)
        return Dashboard.Render(request, ClientPresentation.Default);

    var presentation = new ClientPresentation(
        client.TimeZone,
        client.ViewportWidth,
        client.ColorScheme,
        client.PrefersReducedMotion);

    return Dashboard.Render(request, presentation);
}
```

When collection is disabled or the header is omitted, binding still succeeds with `IsAvailable == false`.

## Available Information

The bounded model includes:

- locale and ordered languages
- IANA timezone and current UTC offset
- viewport and screen dimensions
- device-pixel ratio and orientation
- light/dark color preference
- reduced-motion, contrast, and forced-color preferences
- touch points, pointer precision, and hover capability
- online status
- a mobile/tablet/desktop `DeviceCategory` heuristic

It deliberately excludes user-agent strings, device model, hardware capacity, network type, and geolocation.

`DeviceCategory` and every other property are client-provided hints. Prefer capability fields such as pointer, hover, and viewport over device-category branching.

## Per-Request Customization

The cancellable event runs immediately before serialization for each enabled action attempt:

```javascript
document.addEventListener("heimdall:client-info-before", event => {
  event.detail.info.locale = getApplicationLocale();

  if (event.detail.actionId === "telemetry.ignore")
    event.preventDefault();
});
```

The event exposes `info`, `actionId`, `requestId`, `attempt`, and `sourceElement`.

- Mutate or replace `event.detail.info` for that request.
- Call `preventDefault()` or assign `null` to omit the header without cancelling the action.
- Request-local changes do not alter the cached browser snapshot.

The normal cancellable `heimdall:request-before` event runs later and exposes the final `X-Heimdall-Client-Info` header. It may replace or remove the header or cancel the request.

## Transport And Security

The fixed JSON schema is sent in `X-Heimdall-Client-Info` for JSON and multipart content actions. The server caps the serialized header at 4096 characters; malformed or oversized values receive `400 Bad Request`.

The value is ordinary JSON, not encrypted or signed. HTTPS protects it in transit, but the browser controls it and can replace it. Signing client-authored capability data would not make the claims authoritative.

Client information is not sent on Bifrost connections.

For cross-origin applications, allow `X-Heimdall-Client-Info` in the ASP.NET Core CORS policy. Heimdall action requests normally require preflight because they already use non-safelisted headers.

Treat every property as untrusted input. Never use it for:

- authorization or access control
- pricing or entitlement decisions
- audit identity
- fraud or security guarantees
- permanent device identification

Use it only for presentation, accessibility defaults, responsive server rendering, diagnostics hints, or similar non-authoritative behavior.

The browser timezone is an IANA identifier. Validate it before server use and account for `TimeZoneInfo` platform/ICU mapping differences rather than assuming every deployment accepts the identifier directly.

## Guidance

- Leave collection disabled unless a content action uses the information.
- Use a reasonable maximum age instead of collecting on every interaction.
- Prefer CSS media queries when no server-rendering decision depends on the value.
- Honor reduced-motion, contrast, and forced-color preferences when customizing presentation.
- Always render a safe default when `IsAvailable` is false.
- Document the header in cross-origin CORS configuration.
