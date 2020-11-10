using System;
using System.Collections.Generic;
using System.Linq;
using HtmlTags.Conventions;
using VolunteerConvergence.Models;

namespace VolunteerConvergence.Infrastructure.Tags.SelectBuilders
{
    public class OrganizationSelectElementBuilder : EntitySelectElementBuilder<Organization>
    {
        protected override Guid GetValue(Organization instance) => instance.Id;

        protected override string GetDisplayValue(Organization instance) => instance.Name;
        protected override IEnumerable<Organization> Source(ElementRequest request)
        {
            return request.Get<VolConContext>().Set<Organization>().OrderBy(x => x.Name);
        }
    }
}