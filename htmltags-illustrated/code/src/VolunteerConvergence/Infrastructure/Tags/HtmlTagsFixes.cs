using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using HtmlTags;
using HtmlTags.Conventions;
using HtmlTags.Conventions.Elements;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.DependencyInjection;

namespace VolunteerConvergence.Infrastructure.Tags
{
    /// <summary>
    /// This class exists solely to help surface some HtmlTag members for display labels that are currently inaccessible
    /// </summary>
    public class HtmlConventionWithDisplayLabelRegistry : HtmlConventionRegistry
    {
        public ElementCategoryExpression DisplayLabels => new ElementCategoryExpression(Library.TagLibrary.Category(nameof(DisplayLabels)).Profile(TagConstants.Default));
    }

    /// <summary>
    /// This class exists solely to expose a DisplayLabel HtmlTag out of the library, where it is currently inaccessible
    /// </summary>
    public static class HtmlHelperLibraryExtensions
    {

        public static HtmlTag DisplayLabel<T, TMember>(this IHtmlHelper<T> helper, Expression<Func<T, TMember>> expression)
            where T : class
        {
            return helper.Tag(expression, nameof(HtmlConventionWithDisplayLabelRegistry.DisplayLabels));
        }

        public static HtmlTag DisplayLabel<T, TMember>(this IHtmlHelper<List<T>> helper, Expression<Func<T, TMember>> expression)
            where T : class
        {
            var library = helper.ViewContext.HttpContext.RequestServices.GetService<HtmlConventionLibrary>();
            var generator = ElementGenerator<T>.For(library, t => helper.ViewContext.HttpContext.RequestServices.GetService(t));
            return generator.TagFor(expression, nameof(TagConventions.DisplayLabels));
        }
    }

    /// <summary>
    /// Provide a default builder for DisplayLabels
    /// </summary>
    public class DefaultDisplayLabelBuilder : IElementBuilder
    {
        public bool Matches(ElementRequest subject)
        {
            return true;
        }

        public HtmlTag Build(ElementRequest request)
        {
            return new HtmlTag("span").Text(request.Accessor.InnerProperty.Name.BreakUpCamelCase());
        }
    }

    public static class StringExtensions
    {
        public static string BreakUpCamelCase(this string text)
        {
            var patterns = new[]
            {
                "([a-z])([A-Z])",
                "([0-9])([a-zA-Z])",
                "([a-zA-Z])([0-9])"
            };
            var output = patterns.Aggregate(text,
                (current, pattern) => Regex.Replace(current, pattern, "$1 $2", RegexOptions.IgnorePatternWhitespace));
            return output.Replace('_', ' ');
        }
    }
}