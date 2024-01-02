using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using HtmlTags;
using HtmlTags.Reflection;
using VolunteerConvergence.Infrastructure.Tags.SelectBuilders;

namespace VolunteerConvergence.Infrastructure.Tags
{
    public class TagConventions : HtmlConventionWithDisplayLabelRegistry
    {
        public TagConventions()
        {
            ConfigureDisplayLabelsAndDisplays();
            ConfigureLabels();
            ConfigureEditors();
            AddSelectElementBuilders();
        }

        private void ConfigureDisplayLabelsAndDisplays()
        {
            DisplayLabels.Always.BuildBy<DefaultDisplayLabelBuilder>();
            DisplayLabels.ModifyForAttribute<DisplayAttribute>((t, a) => t.Text(a.Name));

            Displays.IfPropertyIs<DateTime>().ModifyWith(m => m.CurrentTag.Text(m.Value<DateTime>().ToShortDateString()));
            Displays.IfPropertyIs<DateTime?>().ModifyWith(m => m.CurrentTag.Text(m.Value<DateTime?>()?.ToShortDateString()));
            Displays.IfPropertyIs<DateTimeOffset>().ModifyWith(m => m.CurrentTag.Text(m.Value<DateTimeOffset>().LocalDateTime.ToShortDateString()));
            Displays.IfPropertyIs<DateTimeOffset?>().ModifyWith(m => m.CurrentTag.Text(m.Value<DateTimeOffset?>()?.LocalDateTime.ToShortDateString()));
            
            Displays.IfPropertyIs<decimal>().ModifyWith(m => m.CurrentTag.Text(m.Value<decimal>().ToString("C")));
            
            Displays.IfPropertyTypeIs(t => t.IsEnum).ModifyWith(m => m.CurrentTag.Text(m.Value<Enum>().GetDisplayName()));
        }

        private void ConfigureLabels()
        {
            Labels.Always.AddClass("control-label");

            // If the property has a Display attribute, use that value for for the rendered text instead of the property name
            Labels.ModifyForAttribute<DisplayAttribute>((t, a) => t.Text(a.Name));

            // Just assume a "Data." prefix for attributes, since we'll always use a single bound property on page models called "Data"
            Labels.Always.ModifyWith(er => er.CurrentTag.Text(er.CurrentTag.Text().Replace("Data ", "")));
        }

        private void ConfigureEditors()
        {
            Editors.Always.AddClass("form-control");

            Editors.Modifier<EnumDropDownModifier>();

            // By default, render properties ending in "id" as hidden. These are usually database keys needed for operations
            // but should not be seen or edited by the user directly
            Editors.If(er => er.Accessor.InnerProperty.Name.EndsWith("id", StringComparison.OrdinalIgnoreCase) 
                             && !er.Accessor.HasAttribute<VisibleIdFieldAttribute>())
                .BuildBy(a => new HiddenTag());

            Editors.IfPropertyIs<DateTime?>().ModifyWith(m => m.CurrentTag
                .AddPattern("9{1,2}/9{1,2}/9999")
                //.AddPlaceholder("MM/DD/YYYY")
                .AddPlaceholder("YYYY/MM/DD")
                .Attr("data-provide", "supercool-datepicker")
                .Value(m.Value<DateTime?>() != null ? m.Value<DateTime>().ToShortDateString() : string.Empty)
            );

            SetupEditorsForRowVersions();

            void SetupEditorsForRowVersions()
            {
                Editors.If(er => er.Accessor.InnerProperty.Name.EndsWith("RowVersion", StringComparison.OrdinalIgnoreCase))
                    .BuildBy(a => new HiddenTag(), @"Don't render a visible element for RowVersion database columns");

                Editors.If(er => er.Accessor.InnerProperty.Name.EndsWith("RowVersion", StringComparison.OrdinalIgnoreCase))
                    .ModifyWith(er => er.CurrentTag.Value(er.RawValue != null ? Convert.ToBase64String(er.Value<byte[]>()) : null)
                        , @"Render the byte array contents of RowVersion properties, not just the string 'System.Byte[]'");
            }
        }

        private void AddSelectElementBuilders()
        {
            Editors.BuilderPolicy<OrganizationSelectElementBuilder>();
            Editors.BuilderPolicy<OrganizationTypeSelectElementBuilder>();
        }
    }

    public static class EnumExtensions
    {
        public static string GetDisplayName(this Enum enumValue)
        {
            return enumValue.GetType()
                .GetMember(enumValue.ToString())
                .First()
                .GetCustomAttribute<DisplayAttribute>()?
                .GetName() ?? enumValue.ToString().BreakUpCamelCase();
        }
    }

    /// <summary>
    /// Indicates a field ending in "id" should not be rendered as a hidden
    /// </summary>
    public class VisibleIdFieldAttribute : Attribute
    {
    }
}