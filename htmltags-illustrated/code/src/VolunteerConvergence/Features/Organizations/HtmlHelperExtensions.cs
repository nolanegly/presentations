using HtmlTags;
using Microsoft.AspNetCore.Mvc.Rendering;
using VolunteerConvergence.Infrastructure.Tags;
using VolunteerConvergence.Models;

namespace VolunteerConvergence.Features.Organizations
{
    public static class HtmlHelperExtensions
    {
        public static HtmlTag NeedLevelSpan(this IHtmlHelper htmlHelper, NeedLevel level)
        {
            var colorStyle = GetColorStyleForNeedLevel(level);
            var styles = new[] { "alert", colorStyle };

            return new HtmlTag("span").AddClasses(styles).Text(level.GetDisplayName());
        }

        private static string GetColorStyleForNeedLevel(NeedLevel level)
        {
            if (level == NeedLevel.Critical) return "alert-danger";
            if (level == NeedLevel.High) return "alert-warning";
            if (level == NeedLevel.Medium) return "alert-info";
            return string.Empty;
        }
    }
}
