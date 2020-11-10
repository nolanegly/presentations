using HtmlTags;
using HtmlTags.Conventions;
using HtmlTags.Conventions.Elements;
using HtmlTags.Reflection;
using System;
using System.Collections.Generic;

namespace VolunteerConvergence.Infrastructure.Tags
{
    public class ExcludeFromOptionsAttribute : Attribute
    {
        public List<int> ExcludedValues { get; }

        public ExcludeFromOptionsAttribute(params int[] values)
        {
            ExcludedValues = new List<int>(values);
        }
    }

    public class EnumDropDownModifier : IElementModifier
    {
        public bool Matches(ElementRequest token)
        {
            var propertyType = Nullable.GetUnderlyingType(token.Accessor.PropertyType);
            return propertyType != null && propertyType.IsEnum || token.Accessor.PropertyType.IsEnum;
        }

        public void Modify(ElementRequest request)
        {
            var propertyType = Nullable.GetUnderlyingType(request.Accessor.PropertyType);

            var enumType = (propertyType != null && propertyType.IsEnum)
                ? propertyType
                : request.Accessor.PropertyType;

            var currentValue = request.RawValue;

            var excludedValues = request.Accessor.HasAttribute<ExcludeFromOptionsAttribute>()
                ? request.Accessor.GetAttribute<ExcludeFromOptionsAttribute>().ExcludedValues
                : new List<int>();

            request.CurrentTag.RemoveAttr("type");
            request.CurrentTag.RemoveAttr("value");
            request.CurrentTag.TagName("select");

            foreach (var value in Enum.GetValues(enumType))
            {
                if (excludedValues.Contains((int)value))
                    continue;

                var optionTag = new HtmlTag("option")
                    .Value(((int)value).ToString())
                    .Text(((Enum)value).GetDisplayName());

                if (currentValue != null && value.ToString() == currentValue.ToString())
                    optionTag.Attr("selected");

                request.CurrentTag.Append(optionTag);
            }

            if (propertyType != null)
            {
                var emptyOption = new HtmlTag("option").Value("").Text("");
                if (currentValue == null) emptyOption.Attr("selected");
                request.CurrentTag.InsertFirst(emptyOption);
            }
        }
    }
}
