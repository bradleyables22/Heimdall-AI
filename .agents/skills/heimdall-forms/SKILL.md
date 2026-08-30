---
name: heimdall-forms
description: Use when building Heimdall forms, including traditional and interactive validation flows, PayloadFromClosestForm, multipart encoding, file inputs and uploads, IFormFile binding, request limits, SwapNone, status regions, server validation, and returned HTML.
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

## File Uploads

Use normal HTML file inputs. Add multipart encoding to make the form contract explicit:

```csharp
return FluentHtml.Form(form =>
{
    form.Id("profile-upload");
    form.MultipartFormData();
    form.Heimdall()
        .Submit("profile.save")
        .PayloadFromClosestForm()
        .Target("#upload-result")
        .SwapInner()
        .Disable();

    form.Input(Html.InputType.text, input => input
        .Name("DisplayName")
        .Required());

    form.Input(Html.InputType.file, input => input
        .Name("avatar")
        .Accept("image/png", "image/jpeg")
        .Required());

    form.Button(button => button.Type("submit").Text("Upload"));
});
```

The runtime automatically sends forms containing file inputs as `multipart/form-data`. Forms without files continue to use JSON. Programmatic callers can pass a browser `FormData` instance:

```javascript
const data = new FormData(document.querySelector("#profile-upload"));
await Heimdall.invoke("profile.save", data, {
  target: "#upload-result",
  swap: "inner"
});
```

Bind normal payload fields and files together:

```csharp
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

[RequestSizeLimit(10_000_000)]
[RequestFormLimits(MultipartBodyLengthLimit = 8_000_000)]
[ContentInvocation("profile.save")]
public static IHtmlContent Save(
    [FromForm] ProfileRequest request,
    [FromForm(Name = "avatar")] IFormFile avatar)
{
    return UploadResult.Render(request.DisplayName, avatar.Length);
}
```

Supported file shapes include `IFormFile`, `IFormFileCollection`, `IFormFile[]`, and common generic file collection interfaces. File parameters bind by parameter or `[FromForm(Name = ...)]` name.

`[FromForm]` on the payload is optional for an ordinary multipart request, but it makes the form-only contract explicit and prevents a registered payload type from being mistaken for a service. A form-only payload rejects JSON with `415 Unsupported Media Type`.

ASP.NET Core's native `[RequestSizeLimit]`, `[DisableRequestSizeLimit]`, and `[RequestFormLimits]` metadata are honored on an action method or declaring type. Method metadata overrides type metadata; configured `FormOptions` remain the baseline. Limit violations return `413 Payload Too Large`.

Uploads use ASP.NET Core's buffered `IFormFile` pipeline. For large streaming uploads, use a dedicated endpoint instead of a Heimdall content action.

## Upload Security

- Keep finite request and multipart limits.
- Treat `FileName` and `ContentType` as untrusted metadata.
- Validate file signatures/content, not only extensions or MIME labels.
- Generate storage names instead of using the supplied file name.
- Store uploads outside executable/static roots unless serving is intentional and controlled.
- Apply authorization and malware scanning appropriate to the application.
- Leave room for ordinary fields and multipart framing when setting request limits.

With `queue-latest`, form fields and selected files are snapshotted when the submit occurs. Later edits do not change the queued upload.

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
- Use `MultipartFormData()` and `Html.InputType.file` for explicit file-upload markup.
- Use native ASP.NET Core form and request-size metadata for upload limits.
- Do not return JSON and ask the client to render validation UI.
