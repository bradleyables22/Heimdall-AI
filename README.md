# Heimdall.AI

Heimdall.AI is an AI authoring pack for building Heimdall systems with general-purpose AI assistants.

It is not a Codex-only project. The `canonical/` folder contains the tool-neutral guidance. The `adapters/` folder packages that guidance for specific assistants such as Codex, Claude, Cursor, and GitHub Copilot.

## Purpose

AI models usually know C# and ASP.NET Core, but they do not know Heimdall's authoring dialect by default. This pack teaches the primitives that matter:

- `FluentHtml` and `Html` for server-side HTML composition.
- `HeimdallHtml` and `.Heimdall()` helpers for triggers, targets, swaps, payloads, state, SSE, and response directives.
- `ContentInvocation`, payload binding, DI activation, authorization, and request timeouts.
- Bifrost server-sent events for streaming HTML.
- `Heimdall.Bootstrap` strongly typed Bootstrap helpers.
- Heimdall's HTML-first architecture: event to server action to HTML to targeted DOM update.

## Layout

```text
canonical/
  heimdall-authoring.md
  references/
    app-structure.md
    fluent-html.md
    content-actions.md
    response-directives.md
    bifrost-sse.md
    bootstrap-helpers.md
  examples/
    minimal-app/
    form-content-action/
    live-sse-feed/

adapters/
  codex/heimdall/
  claude/CLAUDE.md
  cursor/.cursor/rules/heimdall.mdc
  copilot/.github/copilot-instructions.md
```

## Using The Pack

For any AI assistant, start with `canonical/heimdall-authoring.md`, then load the references that match the task.

Use the adapter that matches your tool:

- Codex: install or copy `adapters/codex/heimdall` as a Codex skill.
- Claude: use `adapters/claude/CLAUDE.md` as project instructions.
- Cursor: copy `adapters/cursor/.cursor/rules/heimdall.mdc` into a project.
- Copilot: copy `adapters/copilot/.github/copilot-instructions.md` into a project.

The canonical docs are the source of truth. Adapter files should stay short and point the assistant toward the same Heimdall authoring rules.
