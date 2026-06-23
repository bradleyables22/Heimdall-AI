using Heimdall.Server;
using Heimdall.Server.Rendering;
using Microsoft.AspNetCore.Html;

public sealed record TodoItem(int Id, string Title);

public sealed class AddTodoRequest
{
    public string Title { get; set; } = string.Empty;
}

public static class TodoPage
{
    public static IHtmlContent Render(IReadOnlyList<TodoItem> todos)
        => TodoPanel.Render(todos, error: null);
}

public static class TodoPanel
{
    public static IHtmlContent Render(IReadOnlyList<TodoItem> todos, string? error)
        => FluentHtml.Section(section =>
        {
            section.Id("todo-panel");

            section.H1(h => h.Text("Todos"));

            if (!string.IsNullOrWhiteSpace(error))
            {
                section.P(p =>
                {
                    p.Id("todo-error");
                    p.Text(error);
                });
            }

            section.Form(form =>
            {
                form.Heimdall()
                    .Submit("todos.add")
                    .PayloadFromClosestForm()
                    .Target("#todo-panel")
                    .SwapOuter()
                    .PreventDefault()
                    .Disable();

                form.Input(Html.InputType.text, input =>
                {
                    input.Name("title")
                        .Required()
                        .Placeholder("New todo");
                });

                form.Button(button =>
                {
                    button.Type("submit");
                    button.Text("Add");
                });
            });

            section.Content(TodoList.Render(todos));
        });
}

public static class TodoList
{
    public static IHtmlContent Render(IReadOnlyList<TodoItem> todos)
        => FluentHtml.Ul(list =>
        {
            list.Id("todo-list");

            foreach (var todo in todos)
            {
                list.Li(item => item.Text(todo.Title));
            }
        });
}

[ContentInvocationPrefix("todos")]
public sealed class TodoActions(ITodoRepository todos)
{
    [ContentInvocation("add")]
    public async Task<IHtmlContent> Add([ContentPayload] AddTodoRequest request, CancellationToken ct)
    {
        var title = request.Title.Trim();
        if (string.IsNullOrWhiteSpace(title))
        {
            var current = await todos.GetAllAsync(ct);
            return TodoPanel.Render(current, "Title is required.");
        }

        await todos.AddAsync(title, ct);
        return TodoPanel.Render(await todos.GetAllAsync(ct), error: null);
    }
}

public interface ITodoRepository
{
    Task<IReadOnlyList<TodoItem>> GetAllAsync(CancellationToken ct);
    Task AddAsync(string title, CancellationToken ct);
}
