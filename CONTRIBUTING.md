# Contributing

Heimdall Skills is a skills-first repository. Changes should preserve the native Agent Skills layout and keep platform-specific metadata lightweight.

## Editing Rules

- Treat `skills/heimdall-*` as the source skills.
- Keep `.agents/skills` synchronized with the source skills.
- Keep each `SKILL.md` focused on one Heimdall area.
- Prefer a new narrow skill over a broad shared reference file when the topic deserves independent routing.
- Put shared complete examples in `examples/`.
- Avoid helper scripts, generated lockfiles, or platform-only artifacts unless they are required by a skills platform.
- Use ASCII Markdown unless a referenced Heimdall API or external source requires otherwise.

## Skill Quality Bar

A good Heimdall skill change should help an agent do at least one of these things better:

- Choose Heimdall's server-rendered HTML model instead of a SPA pattern.
- Use real Heimdall APIs and package names.
- Encode dynamic text correctly.
- Wire content actions with stable invocation IDs, payloads, targets, and swap modes.
- Use response directives for multi-target updates, aborts, redirects, and explicit browser effects.
- Use Bifrost SSE for server-initiated HTML updates.
- Review or migrate existing ASP.NET Core UI code toward Heimdall idioms.

## Mirror Check

Before publishing, compare these folders:

```text
skills
.agents/skills
```

They should contain the same files with the same content.
