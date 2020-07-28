using System;
using System.Threading.Tasks;
using Shouldly;
using VolunteerConvergence.Features.Applicants;

namespace VolunteerConvergence.IntegrationTests.Features.Applicants
{
    using static SliceFixture;

    class CreateTests
    {
        public async Task Should_create_new_applicant()
        {
            // arrange
            var applicant = new CreateModel.Command()
            {
                FirstName = "Snoopy",
                LastName = "Beagle",
                Email = $"snoopy{Guid.NewGuid()}@example.com"
            };

            // act
            var result = await SendAsync(applicant);

            // assert
            var actual = await ExecuteDbContextAsync(db => 
                db.Applicants.FindAsync(result));

            actual.FirstName.ShouldBe(applicant.FirstName);
            actual.LastName.ShouldBe(applicant.LastName);
            actual.Email.ShouldBe(applicant.Email);
        }
    }
}
