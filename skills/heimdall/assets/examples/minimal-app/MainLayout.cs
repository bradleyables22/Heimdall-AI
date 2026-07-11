using Heimdall.Server.Rendering;
using Microsoft.AspNetCore.Html;

public static class MainLayout
{
    public static IHtmlContent Render(IHtmlContent page, string title)
        => FluentHtml.Fragment(fragment =>
        {
            fragment.Raw("<!DOCTYPE html>")
                .HtmlTag(html =>
                {
                    html.Attr("lang", "en")
                        .Head(head =>
                        {
                            head.Meta(meta => meta.Attr("charset", "utf-8"));
                            head.Meta(meta =>
                            {
                                meta.Name("viewport")
                                    .ContentAttr("width=device-width, initial-scale=1");
                            });
                            head.Title(t => t.Text(title));
                            head.Script(script =>
                                script.Src("/_content/HeimdallFramework.Web/heimdall-bundle.min.js"));
                        })
                        .Body(body => body.Content(page));
                });
        });
}
