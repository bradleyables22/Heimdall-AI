# Heimdall.AI

Heimdall.AI is an Agent Skills repository for building Heimdall HTML-first ASP.NET Core systems with skills-compatible AI coding agents.

The repo now follows the Agent Skills shape directly: a skill is a folder with `SKILL.md`, optional `references/`, optional `scripts/`, optional `assets/`, and any platform-specific metadata that does not break the open standard.

## Available Skill

```text
skills/
  heimdall/
    SKILL.md
    references/
    assets/examples/
    agents/openai.yaml
```

The `heimdall` skill teaches agents the Heimdall authoring model:

- `FluentHtml` and `Html` for server-side HTML composition.
- `HeimdallHtml` and `.Heimdall()` helpers for triggers, targets, swaps, payloads, state, SSE, and response directives.
- `ContentInvocation`, payload binding, DI activation, authorization, and request timeouts.
- Bifrost server-sent events for streaming HTML.
- `Heimdall.Bootstrap` strongly typed Bootstrap helpers.
- Heimdall's HTML-first loop: event to server action to HTML to targeted DOM update.

## Platform Use

Use the source skill at `skills/heimdall` when installing from this repository or packaging for a marketplace.

With the Agent Skills CLI, install from the published GitHub repository:

```powershell
npx skills add bradleyables22/Heimdall-AI
```

For project-level discovery by OpenAI Codex, VS Code/GitHub Copilot, GitHub Copilot CLI, Gemini CLI, and other `.agents`-aware clients, this repo also includes a synchronized mirror at:

```text
.agents/skills/heimdall/
```

For Claude Code, this repository can be loaded as a plugin because it includes:

```text
.claude-plugin/plugin.json
skills/heimdall/
```

During local development:

```powershell
claude --plugin-dir .
```

For direct project-skill installation, copy `skills/heimdall` into the target platform's skills directory. Common destinations include:

- `.agents/skills/heimdall`
- `.claude/skills/heimdall`
- `.github/skills/heimdall`
- `~/.agents/skills/heimdall`
- `~/.claude/skills/heimdall`
- `~/.copilot/skills/heimdall`

## Maintenance

Edit the source skill at `skills/heimdall`. Keep `.agents/skills/heimdall` synchronized when the source skill changes so clients that discover project skills from `.agents/skills` see the same content.
