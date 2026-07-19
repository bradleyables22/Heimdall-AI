---
name: heimdall-modifiers
description: Use when refining Heimdall trigger behavior with modifiers such as debounce, key filters, hover delay, visible once, scroll threshold, polling interval, scope self/closest, ignore regions, prevent default, disable, and request synchronization.
---

# Heimdall Modifiers

Use this skill when a Heimdall trigger needs behavior refinement.

Official docs: https://heimdall-framework.org/modifiers

A trigger defines when an interaction starts. A modifier adjusts how that trigger behaves.

```text
Trigger  -> when an interaction starts
Modifier -> how that trigger behaves
Payload  -> what data is sent
Swap     -> how returned HTML is applied
```

## Core Pattern

```csharp
input.Heimdall()
    .Input("search.query")
    .PayloadFromClosestForm()
    .DebounceMs(300)
    .Target("#search-results")
    .SwapInner();
```

## Common Modifiers

```csharp
.DebounceMs(250)
.Key("Enter")
.HoverDelayMs(300)
.VisibleOnce()
.ScrollThresholdPx(200)
.PollMs(5000)
.ScopeSelf()
.ScopeClosest()
.IgnoreAll()
.PreventDefault()
.Disable()
.SyncReplace("search")
```

Use the focused `heimdall-request-lifecycle` skill when choosing among `SyncParallel`, `SyncReplace`, `SyncDrop`, and `SyncQueueLatest`, coordinating groups, or handling cancellation and lifecycle events.

## Debounce

Use debounce for noisy input or change events:

```csharp
input.Heimdall()
    .Input("search.query")
    .PayloadFromClosestForm()
    .DebounceMs(300)
    .Target("#search-results")
    .SwapInner();
```

## Key

Use key filters for keyboard shortcuts:

```csharp
input.Heimdall()
    .KeyDown("command.submit")
    .Key("Enter")
    .PayloadFromClosestForm()
    .Target("#result")
    .SwapInner();
```

## Visible Once

Use visible-once for lazy or infinite-load sentinels:

```csharp
sentinel.Heimdall()
    .Visible("feed.loadMore")
    .VisibleOnce()
    .Payload(new { Cursor = cursor })
    .Target("#feed")
    .SwapBeforeEnd();
```

## Scope And Ignore

Use scope and ignore when nested elements would otherwise trigger parent handlers.

Ignore blocks parent trigger resolution, while triggers inside the ignored region can still work normally. Scope narrows how a trigger matches.

## Guidance

- Add modifiers only to an existing trigger.
- Keep modifiers on the same element as the trigger they affect.
- Prefer debounce to custom JavaScript for noisy events.
- Prefer `PreventDefault()` on Heimdall-managed forms.
- Prefer `Disable()` for submit buttons or commands that should not double-fire.
- Prefer `SyncReplace()` for searches or previews where stale results must never win.
- Keep the default parallel strategy when requests are genuinely independent.
- Use ignore/scope to clarify nested interaction boundaries.
