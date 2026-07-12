---
name: heimdall-forms
description: Use when building Heimdall forms, including traditional submit flows, interactive validation loops, PayloadFromClosestForm, SwapNone, status regions, server validation, re-rendered form hosts, and returning HTML instead of JSON.
---

# Heimdall Forms

Use this skill when building forms with Heimdall.

Official docs: https://heimdall-framework.org/forms

Heimdall forms stay HTML-first. The browser sends form data to a server action, the server validates and re-renders the current truth, and Heimdall updates the UI with returned HTML.

## Two Common Styles

Traditional form:

- submit once
- return success or error UI
- optionally update other regions out-of-band

Interactive form:

- validate during input or change
- re-render the form host as needed
- keep the server as the source of truth throughout

## Shared Foundation

Both styles use a form boundary, payload binding from the closest form, a content action, and returned HTML.

```csharp
form.Heimdall()
    .Submit("contact.submit")
    .PayloadFromClosestForm();
```

Prefer owning the form, host IDs, payload model, validation helpers, and content actions from the component that renders the form:

```csharp
public static partial class ContactForm
{
    public const string HostId = "contact-form-host";

    public static class ActionIds
    {
        public const string Submit = "contact.submit";
    }

    public sealed class ContactRequest
    {
        public string Email { get; set; } = string.Empty;
    }

    [ContentInvocationPrefix("contact")]
    public sealed class ContactFormActions(ContactService contacts)
    {
        [ContentInvocation("submit")]
        public async Task<IHtmlContent> Submit([ContentPayload] ContactRequest request)
        {
            var result = await contacts.SubmitAsync(request);
            return ContactForm.Render(result);
        }
    }
}
```

## Traditional Submit

```csharp
return FluentHtml.Form(form =>
{
    form.Id("contactForm");
    form.Heimdall()
        .Submit("contact.submit")
        .PayloadFromClosestForm()
        .SwapNone();

    form.Input(Html.InputType.text, input =>
    {
        input.Name("Name").Required();
    });

    form.Div(status => status.Id("statusMessage"));

    form.Button(button =>
    {
        button.Type("submit");
        button.Text("Submit");
    });
});
```

The action can decide which region to update:

```csharp
[ContentInvocation("contact.submit")]
public static async Task<IHtmlContent> SubmitMessageAsync(
    EmailService email,
    ContactFormSubmission submission)
{
    var isSuccess = await email.SendAsync(submission);
    var target = isSuccess ? "#contactForm" : "#statusMessage";

    return HeimdallHtml.Invocation(
        targetSelector: target,
        swap: HeimdallHtml.Swap.Inner,
        payload: ContactStatus.Render(isSuccess));
}
```

## Interactive Validation

Use input/change triggers when the form should validate as the user edits:

```csharp
input.Heimdall()
    .Input("checkout.validate")
    .PayloadFromClosestForm()
    .DebounceMs(300)
    .Target("#checkout-form")
    .SwapOuter();
```

The action should normalize and validate the submitted values, then return the form or the relevant fragment.

## Validation Loop

1. The browser sends values from the closest form.
2. The server normalizes and validates the request.
3. The server returns HTML representing the current truth.
4. Heimdall updates the relevant UI boundary.

## Guidance

- Prefer `PayloadFromClosestForm()` for form submit and validation interactions.
- Keep form actions beside the component that renders the form when that component owns the target boundary.
- Use an instance `ComponentNameActions` class with constructor DI for real persistence, email, repository, or validation services.
- Avoid static mutable collections for submitted data except in tiny demos.
- Use `PreventDefault()` for Heimdall-handled form submissions.
- Use `Disable()` to prevent duplicate submissions when appropriate.
- Return HTML for success and error states.
- Use `SwapNone()` when the action will return directives that choose targets.
- Use `SwapOuter()` when re-rendering the full form host.
- Do not return JSON and ask the client to render validation UI.
