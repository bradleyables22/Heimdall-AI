# Changelog

## Unreleased

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
