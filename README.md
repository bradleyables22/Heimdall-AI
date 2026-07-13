# Heimdall UI Framework Skills

Heimdall UI Framework Skills is an Agent Skills pack for building Heimdall HTML-first ASP.NET Core systems with skills-compatible AI coding agents.

The repo follows the Agent Skills shape directly: each capability is a folder with its own `SKILL.md`. The pack is split into narrow Heimdall skills so agents can route to the exact area they need instead of loading one broad internal manual.

## Why This Exists

General-purpose coding agents know C# and ASP.NET Core, but they do not know Heimdall's authoring dialect by default. This repository gives them the reusable context they need to generate Heimdall code that stays server-rendered, HTML-first, and aligned with the framework's native interaction model.

## Available Skills

```text
skills/
  heimdall-getting-started/
  heimdall-app-structure/
  heimdall-pages-routing-middleware/
  heimdall-fluent-html/
  heimdall-html-attributes/
  heimdall-content-actions/
  heimdall-response-directives/
  heimdall-swaps/
  heimdall-state/
  heimdall-forms/
  heimdall-payloads/
  heimdall-triggers/
  heimdall-modifiers/
  heimdall-polling/
  heimdall-lazy-loading/
  heimdall-bifrost-sse/
  heimdall-bootstrap/
  heimdall-static-site-generation/
  heimdall-mvc-integration/
  heimdall-configuration-runtime/
  heimdall-assets-templates/
  heimdall-security/
  heimdall-javascript-runtime/
  heimdall-javascript-interop/
  heimdall-patterns/
  heimdall-migration/
  heimdall-review/
```

Together, these skills teach agents the Heimdall authoring model:

- `FluentHtml` and `Html` for server-side HTML composition.
- `HeimdallHtml` and `.Heimdall()` helpers for triggers, targets, swaps, payloads, state, SSE, and response directives.
- `ContentInvocation`, payload binding, DI activation, authorization, and request timeouts.
- Bifrost server-sent events for streaming HTML.
- `Heimdall.Bootstrap` strongly typed Bootstrap helpers.
- Heimdall's HTML-first loop: event to server action to HTML to targeted DOM update.
- Migration and review patterns for existing ASP.NET Core UI.
- Static site generation, MVC integration, security, runtime configuration, JavaScript runtime/interop, assets/templates, and interaction patterns.

## Repository Shape

```text
.agents/
  skills/                   Project-discovery mirror for .agents-aware clients.
  plugins/marketplace.json  Repo-local ChatGPT/Codex plugin marketplace.
.claude-plugin/
  marketplace.json          Claude Code marketplace catalog.
  plugin.json               Claude Code plugin metadata.
.codex-plugin/
  plugin.json               ChatGPT/Codex plugin metadata.
.cursor/
  rules/                    Cursor project rule.
.github/
  skills/                   GitHub/Copilot-facing skills mirror.
examples/                   Small Heimdall examples shared by the pack.
skills/
  heimdall-*/               Source skills used for packaging and install.
AGENTS.md                   Cross-agent repository instructions.
```

## Platform Use

Use the source skills under `skills/` when installing from this repository or packaging for a marketplace.

With the Agent Skills CLI, install from the published GitHub repository:

```powershell
npx skills add bradleyables22/Heimdall-AI
```

For project-level discovery by OpenAI Codex, VS Code/GitHub Copilot, GitHub Copilot CLI, Gemini CLI, and other `.agents`-aware clients, this repo also includes a synchronized mirror at:

```text
.agents/skills/
```

For GitHub and Copilot users who expect repository customizations under `.github`, this repo also mirrors the skills at:

```text
.github/skills/
```

For Cursor, this repo includes:

```text
.cursor/rules/heimdall-ui-framework.mdc
```

For Claude Code, this repository can be loaded as a plugin because it includes:

```text
.claude-plugin/marketplace.json
.claude-plugin/plugin.json
skills/
```

From Claude Code, add the marketplace and install the plugin:

```text
/plugin marketplace add bradleyables22/Heimdall-AI
/plugin install heimdall-ai@heimdall-ui-framework
```

During local development:

```powershell
claude --plugin-dir .
```

For ChatGPT Work/Codex plugin testing, this repository includes:

```text
.codex-plugin/plugin.json
.agents/plugins/marketplace.json
skills/
```

After opening this repo in the ChatGPT desktop app, the local marketplace can expose the `heimdall-ai` plugin for install from the plugin directory.

For direct project-skill installation, copy the desired `skills/heimdall-*` folders into the target platform's skills directory. Common destinations include:

- `.agents/skills/`
- `.claude/skills/`
- `.github/skills/`
- `~/.agents/skills/`
- `~/.claude/skills/`
- `~/.copilot/skills/`

## Maintenance

Edit the source skills under `skills/`. Keep `.agents/skills` and `.github/skills` synchronized when source skills change so clients that discover project skills from either location see the same content.

When changing guidance, prefer creating or improving a focused `heimdall-*` skill over adding a large shared reference file. Each skill should be useful by itself when selected.
