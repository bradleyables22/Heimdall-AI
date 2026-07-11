---
name: heimdall-state
description: Use when modeling Heimdall state in the DOM or server-rendered UI, including closest state payloads, state boundaries, server-owned state, avoiding client-side state managers, and passing current interaction state back to content actions.
---

# Heimdall State

Use this skill when an interaction needs to carry current UI state back to the server.

Official docs: https://heimdall-framework.org/state

Heimdall state is not a client-side state manager. It is a way to keep interaction state visible at the DOM boundary and send it with a content action when needed. The server should remain the source of truth.

## Closest State Pattern

Render state near the UI that owns it:

```csharp
FluentHtml.Div(host =>
{
    host.Id("counter");
    host.Heimdall().State(new CounterState { Count = count });

    host.P(p => p.Text($"Count: {count}"));

    host.Button(button =>
    {
        button.Text("Increment");
        button.Heimdall()
            .Click("counter.increment")
            .PayloadFromClosestState()
            .Target("#counter")
            .SwapOuter();
    });
});
```

Receive it on the server:

```csharp
[ContentInvocationPrefix("counter")]
public static class CounterActions
{
    [ContentInvocation("increment")]
    public static IHtmlContent Increment([ContentPayload] CounterState state)
    {
        return CounterPanel.Render(state.Count + 1);
    }
}
```

## Guidance

- Use state for small interaction context, not for replacing server persistence.
- Keep state close to the UI boundary that consumes it.
- Prefer stable render functions that can re-render the whole state host.
- Use `PayloadFromClosestState()` when a child control should send the nearest state object.
- Use forms for user-editable field sets.
- Use services or persistence for authoritative domain state.
- Do not build a parallel client state machine when the server can render the current truth.
