using System;
using VolunteerConvergence.Models;

namespace VolunteerConvergence.IntegrationTests.Volunteers
{
    class VolunteerBuilder
    {
        public Volunteer Build(string first, string last, int joinYear, int joinMonth, int joinDay)
        {
            var id = Guid.NewGuid();
            return new Volunteer()
            {
                Id = id,
                FirstName = first,
                LastName = last,
                Email = first + $" {id}", // generate unique email
                JoinDate = new DateTime(joinYear, joinMonth, joinDay)
            };
        }
    }
}