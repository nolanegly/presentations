using System;
using System.Linq.Expressions;
using HtmlTags;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace VolunteerConvergence.Infrastructure.Tags
{
    public static class HtmlHelperExtensions
    {
        public static HtmlTag ValidationDiv(this IHtmlHelper helper)
        {
            var outerDiv = new HtmlTag("div")
                .Id("validationSummary")
                .AddClasses("validation-summary-valid", "text-danger")
                .Data("valmsg-summary", true);

            var ul = new HtmlTag("ul");
            ul.Add("li", li => li.Style("display", "none"));

            outerDiv.Children.Add(ul);

            return outerDiv;
        }

        public static HtmlTag FormBlock<T, TMember>(this IHtmlHelper<T> helper,
            Expression<Func<T, TMember>> expression,
            Action<HtmlTag> labelModifier = null,
            Action<HtmlTag> inputModifier = null
        ) where T : class
        {
            labelModifier ??= _ => { };
            inputModifier ??= _ => { };

            var divTag = new HtmlTag("div");
            divTag.AddClass("form-group");

            var labelTag = helper.Label(expression);
            labelModifier(labelTag);

            divTag.Children.Add(labelTag);

            var inputDivTag = new DivTag().AddClass("col-md-10");
            divTag.Children.Add(inputDivTag);

            var inputTag = helper.Input(expression);
            inputModifier(inputTag);
            inputTag.Attr("autocomplete", "off");
            inputDivTag.Children.Add(inputTag);

            var valSpanTag = new HtmlTag("span").AddClasses("text-danger", "field-validation-valid")
                .Attr("data-valmsg-for", inputTag.Attr("name"))
                .Attr("data-valmsg-replace", "true");
            inputDivTag.Children.Add(valSpanTag);

            return divTag;
        }

        public static HtmlTag DisplayBlock<T, TMember>(this IHtmlHelper<T> helper, 
            Expression<Func<T, TMember>> expression) where T : class
        {
            var labelTag = helper.DisplayLabel(expression).AddClass("data-label");
            labelTag.Text(labelTag.Text() + ": ");
            var valueTag = helper.Display(expression).AddClass("data-value");
            return new HtmlTag("div").Append(new[] { labelTag, valueTag });
        }
    }
}