using System;
using System.Collections.Generic;
using System.Linq;
using HtmlTags.Conventions;
using VolunteerConvergence.Models;

namespace VolunteerConvergence.Infrastructure.Tags.SelectBuilders
{
    public class OrganizationTypeSelectElementBuilder : EntitySelectElementBuilder<OrganizationType>
    {
        protected override Guid GetValue(OrganizationType instance) => instance.Id;

        protected override string GetDisplayValue(OrganizationType instance) => instance.Name;
        protected override IEnumerable<OrganizationType> Source(ElementRequest request)
        {
            return request.Get<VolConContext>().Set<OrganizationType>().OrderBy(x => x.Name);
        }
    }
}