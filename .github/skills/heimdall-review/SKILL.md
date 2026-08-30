---
name: heimdall-review
description: Use when reviewing Heimdall code or generated output for correctness, including real APIs, rendering, local time, forms and uploads, client information, antiforgery and request authentication, synchronized lifecycle, mutations and swaps, Bifrost, observability, architecture fit, and anti-patterns.
---

# Heimdall Review

Use this skill when reviewing Heimdall code or checking generated output.

Official docs: https://heimdall-framework.org/security

Prioritize correctness, security, and whether the code actually follows Heimdall's native HTML-first model:

```text
event -> server action -> HTML -> targeted DOM update
```

## Imports And Packages

- Uses `Heimdall.Server` for page mapping, content actions, services, and Bifrost.
- Uses `Heimdall.Server.Rendering` for `Html`, `FluentHtml`, and `HeimdallHtml`.
- Uses `Microsoft.AspNetCore.Html` for `IHtmlContent` return values.
- Uses `Heimdall.Bootstrap.Bootstrap` only when the Bootstrap package is installed.
- Does not invent Heimdall package names, endpoint paths, helper names, or browser-runtime paths.

## Rendering

- Pages, layouts, fragments, and action responses return `IHtmlContent`.
- New markup uses `FluentHtml` unless local code clearly prefers lower-level `Html` helpers.
- Dynamic or user-provided strings use `.Text(...)`.
- `.Raw(...)` is limited to trusted static markup such as `<!DOCTYPE html>`.
- Rendering modules stay as ordinary C# functions, not Razor components or SPA state machines.
- App-owned CSS classes are typed constants near the owning component, such as `NotesPanel.Css.Root`.
- Framework classes use typed helpers such as `Bootstrap.*` when available.
- Repeated raw app class strings are treated as maintainability issues.
- Embedded HTML resources use `IHtmlContent.ToHtmlString()` instead of duplicated serializers.
- `ToHtmlString()` is not assumed to fetch or inline referenced CSS and JavaScript assets.
- Native dialog/popover commands use typed `CommandFor` and `Command` helpers where practical.
- Browser-local times use `.LocalizeTime(...)` only for absolute instants; `DateTimeKind.Unspecified` is rejected.
- Predetermined timezones use `TimeZoneInfo` and normal text rendering instead of browser localization.

## Interactions

- User interactions are expressed on rendered HTML with `.Heimdall()`.
- Invocation IDs are stable and dotted, such as `orders.filter` or `todos.add`.
- Targets are stable and specific, such as `#orders-list` or `#todo-panel`.
- Swap mode matches the returned fragment:
  - `SwapOuter()` when replacing the selected target.
  - `SwapInner()` when replacing children.
  - `SwapBeforeEnd()` when appending.
  - `SwapNone()` when directives or side effects are the main result.
- Forms use `PayloadFromClosestForm()` when submitting user input.
- File-upload forms use `Html.InputType.file` and explicit `MultipartFormData()` markup.
- Programmatic file uploads pass `FormData`, not JSON-serialized `File` objects.
- Buttons with static context use `.Payload(...)` or `.PayloadEmptyObject()`.
- No synchronization attribute is added when parallel requests are correct.
- Search, filter, preview, and navigation-like replacement use `SyncReplace()` when stale responses must not win.
- Shared synchronization groups are used only when separate elements must coordinate.
- Expected request cancellation is handled as a normal `cancelled` result, not a Heimdall error.
- Lifecycle hooks remain narrow integrations rather than hidden client-side UI ownership.
- Queue-latest preserves submitted form/file snapshots while refreshing closest-state and selector targets when execution begins.
- Async request headers are resolved at attempt time, filtered by trusted kind/origin, and fail closed when required credentials cannot be obtained.
- `heimdall:unauthorized` handles raw `401` challenges without treating `403` as unauthenticated.

## Content Actions

- Content actions use `[ContentInvocation]` or `[ContentInvocationPrefix]`.
- Action methods return `IHtmlContent`, `Task<IHtmlContent>`, or `ValueTask<IHtmlContent>`.
- Action responses return HTML fragments, not JSON UI state.
- Payload binding uses one payload object, optionally marked with `[ContentPayload]`.
- Multipart actions bind `IFormFile` or supported file collections by field name and may combine them with one payload parameter.
- `[FromForm]` is used when a form-only payload or explicit field alias is intended.
- Upload actions have finite native ASP.NET Core request/form limits and validate file content and storage names.
- `HeimdallClientInfo` is treated as a framework parameter, not the payload.
- Dependencies are supplied by constructor injection, implicit service parameters, or `[FromServices]`.
- Long-running actions honor `CancellationToken` and request timeout metadata.
- Authorization uses ASP.NET Core metadata such as `[Authorize]`.
- Per-action antiforgery opt-out uses native `[RequireAntiforgeryToken(false)]`; global server/browser settings remain aligned.
- Actions are colocated with the rendered component or partial they primarily update.
- Component-owned actions use a nested `ComponentNameActions` class, split to `Component.Actions.cs` with partial classes when needed.
- Static action methods are limited to tiny, pure, or demo interactions.
- Real persistence, repositories, Bifrost, MVC rendering, logging, and other services use instance action classes with constructor DI.
- Top-level action classes are reserved for cross-component or domain workflows.
- MVC controller-local content methods use `[NonAction]` as a safety convention.
- Static mutable state is flagged unless it is clearly demo-only.

## Response Directives

- Multi-target updates use Heimdall response directives instead of ad hoc JavaScript rendering.
- Validation failures use `Abort(...)` when the main swap should be suppressed.
- Redirects use Heimdall redirect directives.
- JavaScript void invocation is limited to explicit browser effects.
- JavaScript function paths are rooted at `window.`, `globalThis.`, or `document.`.
- Responses do not evaluate JavaScript source.
- Attribute, class, or state-only changes use mutation directives when preserving node identity is valuable.
- Mutation root count and `Self`/`Subtree`/`Matching` scope are deliberate.
- Invocations and mutations that depend on one another appear in the required response order.
- Mutation lifecycle hooks remain narrow and malformed directives do not leak into the DOM.

## Bifrost SSE

- SSE subscriptions use stable topic names and explicit targets.
- Published SSE payloads are HTML fragments.
- Named events are used when one topic feeds multiple UI regions.
- Bifrost authorization is configured for user-specific or sensitive topics.
- TTL values are short enough for stale messages to expire naturally.
- `HasSubscribers(topic)` is used only as a local instantaneous optimization hint, not a delivery or authorization guarantee.
- Bearer authentication is attached to Bifrost token minting, not assumed to be available as an `EventSource` header.

## Runtime Configuration And Client Data

- `EnableAntiforgery` and `Heimdall.config.antiforgery` agree.
- `Heimdall.config.clientInfo` is enabled only when actions use the bounded presentation snapshot.
- `HeimdallClientInfo` values never control authorization, pricing, auditing, or identity.
- Cross-origin policies explicitly allow trusted origins and every Heimdall/application header in use.
- The current `credentials: "same-origin"` behavior is considered before claiming cross-domain cookie authentication.

## Observability

- OpenTelemetry registers `HeimdallDiagnostics.ActivitySourceName` and `MeterName`.
- Dashboards and tests use public diagnostic constants instead of duplicated private strings.
- Payloads, user identities, Bifrost topic names, URLs, and other high-cardinality data are not added as metric dimensions.

## Architecture

- The UI follows `event -> server action -> HTML -> targeted DOM update`.
- Server rendering owns UI state wherever practical.
- JavaScript stays small and explicit.
- MVC controllers are not used as the normal route for Heimdall content actions.
- Component or partial boundaries own their host IDs, action IDs, payload models, CSS constants, render methods, and local actions.
- SPA shells are introduced only when explicitly requested.
