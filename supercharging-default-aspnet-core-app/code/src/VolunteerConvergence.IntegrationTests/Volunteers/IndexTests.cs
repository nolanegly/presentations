using System;
using System.Linq;
using System.Threading.Tasks;
using Shouldly;
using VolunteerConvergence.Features.Volunteers;
using VolunteerConvergence.Models;

namespace VolunteerConvergence.IntegrationTests.Volunteers
{
    using static SliceFixture;

    class IndexTests
    {
        public async Task Should_load_all_volunteers()
        {
            // arrange
            var volunteerOne = new VolunteerBuilder().Build("Charlie", "Brown", 2018, 07, 04);
            var volunteerTwo = new VolunteerBuilder().Build("Sally", "Brown", 2020, 01, 01);
            
            await InsertAsync(volunteerOne, volunteerTwo);

            // act
            var result = await SendAsync(new IndexModel.Query());

            // assert
            AssertContains(result, volunteerOne);
            AssertContains(result, volunteerTwo);
        }

        private void AssertContains(IndexModel.Result result, Volunteer expected)
        {
            var actual = result.Volunteers.Single(a =>
                a.Email.Equals(expected.Email, StringComparison.InvariantCultureIgnoreCase));

            actual.FirstName.ShouldBe(expected.FirstName);
            actual.LastName.ShouldBe(expected.LastName);
            actual.JoinDate.ShouldBe(expected.JoinDate.Date);
        }
    }
}
