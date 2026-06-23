# Heimdall Instructions

When generating or modifying Heimdall code, use Heimdall's native authoring model.

Heimdall is an HTML-first ASP.NET Core framework:

```text
event -> server action -> HTML -> targeted DOM update
```

Rules:

- Use `IHtmlContent` render functions.
- Use `FluentHtml` for server-side markup.
- Use `.Text(...)` for dynamic text and `.Raw(...)` only for trusted markup.
- Use `.Heimdall()` for triggers, targets, swaps, payloads, state, and SSE.
- Use `[ContentInvocation]` and `[ContentInvocationPrefix]` for server actions.
- Return HTML fragments from actions, not JSON UI state.
- Use response directives for out-of-band updates, aborts, redirects, and explicit JavaScript void calls.
- Use Bifrost for server-sent event HTML streaming.
- Use `Heimdall.Bootstrap` strongly typed helpers when Bootstrap classes are desired.

Common imports:

```csharp
using Heimdall.Server;
using Heimdall.Server.Rendering;
using Microsoft.AspNetCore.Html;
using Bs = Heimdall.Bootstrap.Bootstrap;
```

Preferred form interaction:

```csharp
form.Heimdall()
    .Submit("todos.add")
    .PayloadFromClosestForm()
    .Target("#todo-panel")
    .SwapOuter()
    .PreventDefault()
    .Disable();
```

Preferred content action:

```csharp
[ContentInvocationPrefix("todos")]
public sealed class TodoActions(ITodoRepository todos)
{
    [ContentInvocation("add")]
    public async Task<IHtmlContent> Add([ContentPayload] AddTodoRequest request, CancellationToken ct)
    {
        await todos.AddAsync(request.Title.Trim(), ct);
        return TodoPanel.Render(await todos.GetAllAsync(ct), error: null);
    }
}
```
