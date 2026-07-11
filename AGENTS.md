# Agent Instructions

This repository is a native Agent Skills pack for the Heimdall UI Framework.

## Repository Purpose

The source of truth is the focused skill pack under `skills/`. Each `heimdall-*` directory contains one independently routable `SKILL.md` for a specific Heimdall UI Framework area.

The repo also carries platform discovery mirrors and plugin metadata:

- `.agents/skills/`: interoperable project-skill mirror for Codex, VS Code/GitHub Copilot, Gemini CLI, and other Agent Skills clients.
- `.github/skills/`: GitHub/Copilot-facing mirror for users who expect skills under GitHub configuration.
- `.codex-plugin/plugin.json`: ChatGPT/Codex plugin metadata.
- `.claude-plugin/plugin.json`: Claude Code plugin metadata.
- `.cursor/rules/heimdall-ui-framework.mdc`: Cursor project rule that points Cursor toward the skill pack.

## Editing Rules

- Edit source skills in `skills/` first.
- Keep `.agents/skills/` and `.github/skills/` synchronized with `skills/`.
- Keep each `SKILL.md` focused on one Heimdall area.
- Prefer a new focused skill over a broad shared reference file when a topic deserves independent routing.
- Do not add helper scripts, generated lockfiles, or platform-only artifacts unless a platform requires them.
- Use ASCII Markdown unless a referenced Heimdall API, product name, or source asset requires otherwise.

## Validation

Before finishing a repo change:

- Confirm every skill folder has `SKILL.md`.
- Confirm every `SKILL.md` frontmatter `name` matches its parent folder name.
- Confirm every `SKILL.md` has a non-empty `description`.
- Confirm `skills/`, `.agents/skills/`, and `.github/skills/` contain the same skill folders and skill files.
- Confirm JSON manifests parse:
  - `.codex-plugin/plugin.json`
  - `.claude-plugin/plugin.json`
  - `.agents/plugins/marketplace.json`

## Branding

Use the user-facing name `Heimdall UI Framework`.

Keep the stable plugin identifier `heimdall-ai` unless the user explicitly asks for a breaking rename and understands existing installs may need to be removed and re-added.

Use the current favicon copied from the Heimdall documentation project for plugin icon assets:

```text
assets/icon.png
assets/logo.png
assets/logo-dark.png
```

## Git Notes

The branch in this repo is `master` unless the user changes it. If giving marketplace install instructions for GitHub, use `master` unless a real `main` branch exists.
