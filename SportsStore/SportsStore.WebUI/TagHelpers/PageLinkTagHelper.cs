using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using SportsStore.WebUI.Models;

namespace SportsStore.WebUI.TagHelpers
{
    [HtmlTargetElement("div", Attributes = "page-model")]
    public class PaginationTagHelper : TagHelper
    {
        public PagingInfo? PageModel { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "div";

            var pageCount = PageModel?.TotalPages;

            for (int i = 1; i <= pageCount; i++)
            {
                var tag = new TagBuilder("a");
                var url = $"?page={i}";

                tag.Attributes["href"] = url;
                tag.InnerHtml.Append(i.ToString());

                if (i == PageModel?.CurrentPage)
                {
                    tag.AddCssClass("selected");
                }

                output.Content.AppendHtml(tag);
            }
        }
    }
}
