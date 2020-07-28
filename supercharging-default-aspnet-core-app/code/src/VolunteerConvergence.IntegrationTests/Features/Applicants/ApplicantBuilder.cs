using System;
using VolunteerConvergence.Models;

namespace VolunteerConvergence.IntegrationTests.Features.Applicants
{
    class ApplicantBuilder
    {
        public Applicant Build(string first, string last)
        {
            var id = Guid.NewGuid();
            return new Applicant
            {
                Id = id,
                FirstName = first,
                LastName = last,
                Email = $"{first}-{id}@example.com" // generate unique email
            };
        }
    }
}