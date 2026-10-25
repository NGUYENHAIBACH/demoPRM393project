using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Dormio.Web.TagHelpers;

/// <summary>Usage: &lt;status-badge active="@Model.IsActive" /&gt; renders a Bootstrap 5 badge.</summary>
[HtmlTargetElement("status-badge")]
public class StatusBadgeTagHelper : TagHelper
{
    public bool Active { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "span";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Attributes.SetAttribute("class", Active ? "badge bg-success" : "badge bg-secondary");
        output.Content.SetContent(Active ? "Active" : "Inactive");
    }
}
