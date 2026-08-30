---
name: heimdall-fluent-html
description: Use when generating or editing Heimdall server-rendered markup with FluentHtml or Html, including IHtmlContent render functions, ToHtmlString output, native command/commandfor helpers, element builders, attributes, multipart forms and file inputs, local-time markup, tables, text encoding, Raw usage, and Heimdall behavior extension points.
---

# Heimdall FluentHtml

Use this skill when generating Heimdall markup.

Official docs: https://heimdall-framework.org/html-attributes

`FluentHtml` is the preferred authoring API for server-rendered Heimdall UI. It produces `IHtmlContent`, encodes text by default when `.Text(...)` is used, and composes naturally with Heimdall behavior helpers.

## Imports

```csharp
using Heimdall.Server.Rendering;
using Microsoft.AspNetCore.Html;
```

## Core Pattern

```csharp
public static partial class ProfilePanel
{
    public static class Css
    {
        public const string Root = "profile-panel";
        public const string Header = "profile-panel__header";
    }

    public static IHtmlContent Render()
        => FluentHtml.Section(section =>
        {
            section.Id("profile")
                .Class(Css.Root)
                .H2(h => h.Text("Profile"))
                .P(p => p.Text("Server-rendered HTML."));
        });
}
```

Most element helpers accept a builder callback:

```csharp
FluentHtml.Div(div =>
{
    div.Id("summary");
    div.Text("Hello");
});
```

Nested elements are available as builder methods:

```csharp
FluentHtml.Main(main =>
{
    main.Section(section =>
    {
        section.H1(h => h.Text("Orders"));
        section.Div(panel => panel.Id("orders-list"));
    });
});
```

## Common Builder Methods

Use these methods instead of string concatenation:

```csharp
b.Id("orders")
b.Class("container", "py-4")
b.Lang("en-US")
b.Attr("data-kind", "order")
b.Data("order-id", order.Id.ToString())
b.Aria("label", "Orders")
b.Text(order.Name)
b.Content(OrderSummary.Render(order))
b.Raw("<!DOCTYPE html>")
```

## CSS Class Names

Use typed constants for app-owned CSS classes. Keep them near the component that renders the markup:

```csharp
public static partial class NotesPanel
{
    public static class Css
    {
        public const string Root = "notes-panel";
        public const string Empty = "notes-panel__empty";
    }

    public static IHtmlContent Render(NotesModel model)
        => FluentHtml.Div(panel =>
        {
            panel.Class(Css.Root);
            panel.Div(empty => empty.Class(Css.Empty).Text("No notes yet."));
        });
}
```

Use raw class strings sparingly for one-off values. Prefer `Component.Css.*` for repeated project classes and framework helpers such as `Bootstrap.*` for framework classes.

Boolean attributes:

```csharp
input.Required();
input.Disabled(isBusy);
input.Checked(isSelected);
```

Use `.Lang(...)` when an element establishes a language for its content. It emits the native HTML `lang` attribute and accepts any valid BCP 47 language tag:

```csharp
html.Lang("en");
panel.Lang("fr-FR");
```

The browser-local time runtime resolves language from the localized element's nearest `lang` attribute, then the document language, then the browser locale.

Form attributes:

```csharp
input.Name("email");
input.Value(model.Email);
input.Placeholder("you@example.com");
label.For("email");
```

## Text, Content, And Raw

Use `.Text(...)` for strings:

```csharp
p.Text(user.DisplayName);
```

Use `.Content(...)` for prebuilt `IHtmlContent`:

```csharp
main.Content(UserCard.Render(user));
```

Use `.Raw(...)` only for trusted markup:

```csharp
fragment.Raw("<!DOCTYPE html>");
```

Do not pass untrusted user content to `Raw`.

## Render To A String

Use `ToHtmlString()` when HTML must be returned as data instead of written directly to an ASP.NET Core response:

```csharp
using Heimdall.Server.Rendering;

IHtmlContent document = FluentHtml.HtmlTag(html =>
{
    html.Head(head => head.Title(title => title.Text("Dashboard")))
        .Body(body => body.Content(Dashboard.Render()));
});

string markup = document.ToHtmlString();
```

The default HTML encoder is used unless another encoder is supplied. This is appropriate for static resources and embedded UI documents, including HTML resource strings consumed by another protocol.

## Browser-Native Commands

Use native HTML commands for declarative dialogs, popovers, and custom command events without requiring the Heimdall runtime:

```csharp
button.CommandFor("confirmation-dialog")
    .Command(Html.CommandType.show_modal);

closeButton.CommandFor("confirmation-dialog")
    .Command(Html.CommandType.close);

customButton.CommandFor("record-preview")
    .Command("--archive-record");
```

The same helpers are exposed through `Html.Command`, `Html.CommandFor`, `FluentHtml.Command`, and `FluentHtml.CommandFor`. Pass an element ID without `#` to `CommandFor`.

## Forms

Use `FluentHtml.Form` plus Heimdall behavior helpers:

```csharp
FluentHtml.Form(form =>
{
    form.Id("todo-form");
    form.Heimdall()
        .Submit("todos.add")
        .PayloadFromClosestForm()
        .Target("#todo-panel")
        .SwapOuter()
        .PreventDefault()
        .Disable();

    form.Label(label =>
    {
        label.For("title");
        label.Text("Title");
    });

    form.Input(Html.InputType.text, input =>
    {
        input.Id("title")
            .Name("title")
            .Required()
            .Placeholder("New todo");
    });

    form.Button(button =>
    {
        button.Type("submit");
        button.Text("Add");
    });
});
```

Known input types use `Html.InputType`, including `text`, `email`, `password`, `number`, `date`, `search`, `checkbox`, `radio`, `hidden`, `file`, and `datetime_local`.

For a file upload, add multipart encoding and use the typed file input:

```csharp
FluentHtml.Form(form =>
{
    form.MultipartFormData();
    form.Heimdall()
        .Submit("profile.upload")
        .PayloadFromClosestForm()
        .Target("#upload-result");

    form.Input(Html.InputType.file, input => input
        .Name("avatar")
        .Accept("image/png", "image/jpeg")
        .Required());
});
```

Use the focused `heimdall-forms` skill for multipart binding, request limits, and upload security.

## Browser-Local Time

Any element builder can display an absolute instant in the browser's local timezone:

```csharp
span.LocalizeTime(createdAt, "MMM d, yyyy 'at' h:mm tt");
```

The helper emits a server fallback and runtime attributes; it does not require a particular HTML tag or a server round trip. Use the focused `heimdall-time-localization` skill for supported formats, lifecycle hooks, and absolute-time rules.

## Lists And Tables

Use normal C# loops inside the builder callback:

```csharp
FluentHtml.Ul(ul =>
{
    foreach (var item in items)
    {
        ul.Li(li => li.Text(item.Title));
    }
});
```

```csharp
FluentHtml.Table(table =>
{
    table.TableHead(head =>
    {
        head.TableRow(row =>
        {
            row.TableHeaderCell(th => th.Text("Order"));
            row.TableHeaderCell(th => th.Text("Status"));
        });
    });

    table.TableBody(body =>
    {
        foreach (var order in orders)
        {
            body.TableRow(row =>
            {
                row.TableDataCell(cell => cell.Text(order.Number));
                row.TableDataCell(cell => cell.Text(order.Status));
            });
        }
    });
});
```

## Lower-Level Html Helpers

Use `Html` for compact literal composition or response directives:

```csharp
Html.Tag("main",
    Html.Attr("id", "home"),
    Html.Tag("h1", "Home"));
```

Prefer `FluentHtml` for larger markup.

## Heimdall Behavior Extension

Prefer the scoped callback overload when HTML configuration continues after Heimdall behavior. The callback temporarily exposes the Heimdall builder and then returns the original element builder:

```csharp
button
    .Id("save")
    .Heimdall(heimdall => heimdall
        .Click("orders.save")
        .Target("#orders")
        .SwapOuter())
    .Class("btn", "btn-primary")
    .Text("Save");
```

You can transition between the HTML and Heimdall builders more than once. Attributes are emitted in the order in which the methods run:

```csharp
button
    .Heimdall(h => h.Click("orders.save"))
    .Class("btn")
    .Heimdall(h => h.Disable())
    .Text("Save");
```

The original no-argument `.Heimdall()` transition remains available when the rest of the chain only needs Heimdall methods:

```csharp
button.Heimdall()
    .Click("orders.refresh")
    .PayloadEmptyObject()
    .Target("#orders-list")
    .SwapOuter();
```

Add `.SyncReplace("orders")`, `.SyncDrop()`, `.SyncQueueLatest()`, or `.SyncParallel()` only when requests need explicit overlap coordination.

The callback overload also returns the original fragment builder, so response directives can be placed naturally among ordinary fragment content:

```csharp
FluentHtml.Fragment(fragment =>
{
    fragment.Add(MainResult.Render());
    fragment.Heimdall(h => h.Invocation(
        "#sidebar",
        payload: Sidebar.Render()));
    fragment.Add(Footer.Render());
    fragment.Heimdall(h => h.Mutate("#status", mutation => mutation
        .RemoveClass("loading")
        .AddClass("ready")));
});
```

Use the focused `heimdall-mutations` skill for in-place attribute, class, and state updates.
