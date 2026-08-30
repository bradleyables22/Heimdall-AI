# Changelog

## Unreleased

- Updated the skill pack for Heimdall Server and Web 3.0.8.
- Documented the native `.Lang(...)` helper and scoped `.Heimdall(h => ...)` element/fragment callbacks that return to the original FluentHtml builder.
- Added focused `heimdall-mutations`, `heimdall-time-localization`, `heimdall-client-info`, and `heimdall-observability` skills.
- Added multipart form and file-upload guidance covering `IFormFile`, `[FromForm]`, native request/form limits, programmatic `FormData`, queue snapshots, and upload security.
- Added global and per-action antiforgery guidance, asynchronous request-header resolution and precedence, JWT examples, raw `401` handling, cross-origin header/CORS considerations, and Bifrost token authentication.
- Added Bifrost `HasSubscribers(topic)` guidance with its local-instance optimization-only semantics.
- Expanded queue-latest guidance for state rebinding, form/file snapshots, target re-resolution, cancellation reasons, and retry stability.
- Expanded the review, runtime, response-directive, swaps, state, payload, FluentHtml, app-structure, security, patterns, and migration skills for the new capabilities.
- Added the focused `heimdall-request-lifecycle` skill for parallel, replace, drop, and queue-latest coordination, named groups, cancellation, client timeouts, programmatic invocation, and request/swap lifecycle events.
- Added native HTML `command` and `commandfor` guidance across the attributes, FluentHtml, and review skills.
- Added `IHtmlContent.ToHtmlString()` guidance for static and embedded HTML resources without implying asset inlining.
- Updated configuration, runtime, swaps, modifiers, and review guidance for migration-safe defaults and stale-response protection.

## 0.1.0

- Converted the Heimdall skills pack into a native Agent Skills repository.
- Replaced the broad `heimdall` skill with narrow `heimdall-*` skills.
- Expanded coverage to mirror the Heimdall documentation map, including SSG, MVC integration, configuration/runtime, swaps, state, forms, payloads, triggers, modifiers, polling, lazy loading, assets/templates, security, JavaScript runtime/interop, pages/routing, and patterns.
- Added `.agents/skills` as the project-discovery mirror.
- Added `.github/skills` as a GitHub/Copilot-facing skills mirror.
- Added `AGENTS.md` and a Cursor project rule.
- Added Claude Code plugin metadata.
- Added shared Heimdall examples.
- Added Heimdall-native best-practice guidance for component-owned actions, nested `ComponentNameActions`, and typed component CSS class constants.
